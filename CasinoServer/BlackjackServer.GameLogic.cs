using System.Net;
using CardGamesLibrary;
using CardGamesLibrary.Blackjack;
using BlackJackDealer;

partial class BlackjackServer
{
	private static readonly int sMinimumBet = 100;
	private readonly GameState _gameState;
	private static readonly StandardDeck sDeck = new(4);
	private readonly Dictionary<Identifier, TaskCompletionSource<bool>> _turnCompletionSources = [];
	private readonly List<Identifier> _inactivePlayers = [];

	private static PhaseTimer? sPhaseTimer;

	private bool cutCardDrawn = true;
	private bool currentIsSplit;

	private async Task GameLoopAsync(CancellationToken ct)
	{
		try
		{
			while(!ct.IsCancellationRequested)
			{
				if(_clients.Count > 0)
				{
					RefreshGameState();
					await WaitForPlayersAsync();
					await StartRoundAsync();

					if(playerCount == 0) continue;

					await ValidateBetsAsync();

					if(_inactivePlayers.Count == playerCount) continue;

					await DealInitialCardsAsync();
					await Task.Delay(2000, ct);

					if(_gameState.Dealer.HandScore == 11)
					{
						await RunInsurranceAsync();
					}

					if(_gameState.Dealer.HasBlackjack())
					{
						await HandleDealerBlackjack();
						await ResolveBetsAsync();
						await EndRoundAsync();
						continue;
					}

					if(playerCount == 0) continue;

					await RunPlayerTurnsAsync();

					await Task.Delay(2_000, ct);

					await RunDealerTurnAsync();

					if(playerCount == 0) continue;

					await ResolveBetsAsync();

					await EndRoundAsync();
				}
			}
		}
		catch(Exception ex)
		{
			Logger.LogError(ex.Message);
			await BroadcastAsync(MessageFactory.Wrap(MessageType.UNKNOWN, "A fatal error occured during the game loop, the server was forced to shutdown."));
		}
	}


	// ============ Game Phase Logics ============
	private async Task WaitForPlayersAsync()
	{
		Logger.LogInfo("Waiting for players.");
		await SetPhaseAsync(GamePhase.PreGame);

		sPhaseTimer = new PhaseTimer(
		durationSeconds: 20,
		tickIntervalMs: 1000,
		onTick: async remaining => {
			await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, (float)remaining / 20));
		});

		var task = sPhaseTimer.RunAsync(CreateAllPlayersLeftToken());

		//_ = Task.Run(async () => {
		//	//condition for  waiting
		//	timer.CompleteEarly(); // stop the timer if everyone bet
		//});

		await task;

		//if(!completedNormally)
		//{
		//	// Phase was cancelled → maybe player disconnected or game reset
		//}

		sPhaseTimer = null;
	}

	private async Task StartRoundAsync()
	{
		Logger.LogInfo("Starting a round.");
		await SetPhaseAsync(GamePhase.Betting);
		_gameState.CurrentTurn = 0;

		Message msg = MessageFactory.Wrap(MessageType.BELL, "The game is starting, place your bets.");
		_ = BroadcastAsync(msg);

		sPhaseTimer = new PhaseTimer(
			durationSeconds: 30,
			tickIntervalMs: 1000,
			onTick: async remaining => {
				await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, (float)remaining / 30));
			});

		foreach(var item in _gameState.Players)
		{
			if(item.Value is not null)
			{
				_turnCompletionSources.Add(item.Value.Id, new TaskCompletionSource<bool>());
			}
		}

		await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, 0f));

		var task = sPhaseTimer.RunAsync(CreateAllPlayersLeftToken());

		_ = Task.Run(async () => {
			await Task.WhenAll(_turnCompletionSources.Select(tcs => tcs.Value.Task));
			sPhaseTimer.CompleteEarly();
		});

		await task;

		_turnCompletionSources.Clear();
		sPhaseTimer = null;
	}

	private async Task ValidateBetsAsync()
	{
		Logger.LogInfo("Validating all bets.");
		await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, 0f));
		foreach(var player in _gameState.Players.Values)
		{
			if(player is not null && player.MainBet <= 0)
			{
				_inactivePlayers.Add(player.Id);
			}
		}
	}

	private async Task DealInitialCardsAsync()
	{
		Logger.LogInfo("Dealing initial cards.");
		await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, 0f));
		await SetPhaseAsync(GamePhase.Drawing);
		_gameState.CurrentTurn = 0;

		for(int i = 0; i < 2; i++)
		{
			foreach(Player? player in _gameState.Players.Values)
			{
				if(player is null || _inactivePlayers.Contains(player.Id)) continue;

				Card card = DrawCard();
				player.Hand.AddCardToHand(card);
				BroadcastStateAndEvent($"{player.Name} drew a {card.ToCompactString(true)}");
				await Task.Delay(1_000);
			}

			Card dealerCard = DrawCard();
			string message;
			if(i == 0)
			{
				_gameState.Dealer.AddCardToHand(dealerCard);
				message = $"Dealer drew a {dealerCard.ToCompactString(true)}";
			}
			else
			{
				_gameState.Dealer.AddCardToHole(dealerCard);
				message = $"Dealer drew the hole card";
			}

			BroadcastStateAndEvent(message);

			await Task.Delay(1_000);
		}
	}

	private async Task RunInsurranceAsync()
	{
		Logger.LogInfo("Proposing insurrance bets.");
		await SetPhaseAsync(GamePhase.Insurance);
		_gameState.CurrentTurn = 0;

		Message msg = MessageFactory.Wrap(MessageType.INFO, "The dealer is showing an Ace, you may place an Insurrance Bet.");
		_ = BroadcastAsync(msg);

		sPhaseTimer = new PhaseTimer(
			durationSeconds: 30,
			tickIntervalMs: 1000,
			onTick: async remaining => {
				await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, (float)remaining / 30));
			});

		foreach(Player? player in _gameState.Players.Values)
		{
			if(player is not null && !_inactivePlayers.Contains(player.Id))
			{
				_turnCompletionSources.Add(player.Id, new TaskCompletionSource<bool>());
			}
		}

		var task = sPhaseTimer.RunAsync(CreateAllPlayersLeftToken());

		_ = Task.Run(async () => {
			await Task.WhenAll(_turnCompletionSources.Select(tcs => tcs.Value.Task));
			sPhaseTimer.CompleteEarly();
		});

		await task;

		_turnCompletionSources.Clear();
		sPhaseTimer = null;
	}

	private async Task HandleDealerBlackjack()
	{
		Logger.LogInfo("Dealer has blackjack.");
		_gameState.Dealer.RevealHole(out _);
		BroadcastStateAndEvent($"Dealer has Blackjack");
		await Task.Delay(5_000);
	}

	private async Task RunPlayerTurnsAsync()
	{
		Logger.LogInfo("Running player turns.");
		await SetPhaseAsync(GamePhase.PlayerTurns);

		for(int currentTurn = 1; currentTurn <= _gameState.Players.Count; currentTurn++)
		{
			var player = _gameState.Players[currentTurn];
			if(player is null || _inactivePlayers.Contains(player.Id)) continue;

			_gameState.CurrentTurn = currentTurn;
			_ = BroadcastAsync(MessageFactory.Wrap(MessageType.TURN, currentTurn));
			BroadcastStateAndEvent($"It's {player.Name}'s turn.");

			Logger.LogInfo($"Current turn : {player.Name}, {player.Id}.");

			// Create a completion source for this player
			var tcs = new TaskCompletionSource<bool>();
			_turnCompletionSources[player.Id] = tcs;

			sPhaseTimer = new PhaseTimer(
			durationSeconds: 30,
			tickIntervalMs: 1000,
			onTick: async remaining => {
				await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, (float)remaining / 30));
			});

			var task = sPhaseTimer.RunAsync(_clients[player.Id].cts.Token);

			_ = Task.Run(async () => {
				await tcs.Task;
				Logger.LogInfo("Player turn completed early");
				sPhaseTimer.CompleteEarly();
			});

			await task;

			// Cleanup
			_turnCompletionSources.Remove(player.Id);
			sPhaseTimer = null;
			await Task.Delay(1000);
		}
	}

	private async Task RunDealerTurnAsync()
	{
		Logger.LogInfo("Dealer turn.");
		_ = BroadcastAsync(MessageFactory.Wrap(MessageType.TURN, 0));
		await BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, 0f));
		await SetPhaseAsync(GamePhase.DealerTurn);

		_gameState.Dealer.RevealHole(out string str);
		BroadcastStateAndEvent($"Hole card was a {str}");
		await Task.Delay(1000);

		while(_gameState.Dealer.HandScore < 17)
		{
			Card card = DrawCard();
			_gameState.Dealer.AddCardToHand(card);

			BroadcastStateAndEvent($"Dealer drew a {card.ToCompactString(true)}");
			await Task.Delay(1000);
		}
	}

	private async Task ResolveBetsAsync()
	{
		Logger.LogInfo("Resolving Bets");
		await SetPhaseAsync(GamePhase.Resolve);
		int dScore = _gameState.Dealer.HandScore;
		int dCount = _gameState.Dealer.CardCount;
		bool dBlackjack = dCount == 2 && dScore == 21;
		foreach(var player in _gameState.Players.Values.ToArray())
		{
			if(player is null || _inactivePlayers.Contains(player.Id)) continue;

			int pScore = player.Hand.HandScore;
			int pCount = player.Hand.CardCount;
			int pBet = player.MainBet;
			bool pBlackjack = pCount == 2 && pScore == 21 && !player.Hand.HasSplit;

			int winnings = 0;
			if(pBet > 0 && pScore <= 21)
			{
				if(pCount == 7)
				{
					winnings += BlackjackPayouts.Charlie(pBet);
				}
				else if(pScore > dScore || dScore > 21)
				{
					if(pBlackjack)
					{
						winnings += BlackjackPayouts.Blackjack(pBet);
					}
					else
					{
						winnings += BlackjackPayouts.Win(pBet);
					}
				}
				else if(pScore == dScore)
				{
					if(pBlackjack && !dBlackjack)
					{
						winnings += BlackjackPayouts.Blackjack(pBet);
					}
					else
					{
						winnings += BlackjackPayouts.Push(pBet);
					}
				}
			}

			if(dBlackjack)
			{
				winnings += player.InsurranceBet * 2;
			}

			pScore = player.Hand.SplitScore;
			pCount = player.Hand.SplitCount;
			pBet = player.SplitBet;

			if(pBet > 0 && pScore <= 21)
			{
				if(pCount == 7)
				{
					winnings += BlackjackPayouts.Charlie(pBet);
				}
				else if(pScore > dScore || dScore > 21)
				{
					winnings += BlackjackPayouts.Win(pBet);
				}
				else if(pScore == dScore)
				{
					winnings += BlackjackPayouts.Push(pBet);
				}
			}
			player.AddChips(winnings);
			await UnicastAsync(MessageFactory.Wrap(MessageType.INFO, $"You have won {winnings} chips"), player.Id);
		}
	}

	private async Task EndRoundAsync()
	{
		Logger.LogInfo("End of round");
		await SetPhaseAsync(GamePhase.EndGame);
		BroadcastStateAndEvent("This round is over.");
		await Task.Delay(5_000);
	}

	private void RefreshGameState()
	{
		Logger.LogInfo("Reseting Game State.");
		lock(_lockObj)
		{
			_gameState.ResetDealer();
			foreach(var item in _gameState.Players)
			{
				if(item.Value is null) continue;
				item.Value.CleanHand();
			}
			_gameState.CurrentTurn = 0;
			_inactivePlayers.Clear();

			if(cutCardDrawn)
			{
				sDeck.ShuffleRoutine();
				sDeck.InsertAt(new Random().Next(140, 160), new Card(0, 0));
				cutCardDrawn = false;
			}
			_ = BroadcastAsync(MessageFactory.Wrap(MessageType.TIME, 0f));
			_ = BroadcastAsync(MessageFactory.Wrap(MessageType.STATE, BuildState()));
		}
	}

	private Card DrawCard()
	{
		Card card = sDeck.DrawCard();
		if(card.Symbol == ((int)Symbols.BLANK))
		{
			Message msg = MessageFactory.Wrap(MessageType.EVENT, "The cut card has been drawn");
			_ = BroadcastAsync(msg);

			cutCardDrawn = true;
			card = sDeck.DrawCard();
		}

		sDeck.PutBack(card);
		return card;
	}

	private async Task SetPhaseAsync(GamePhase phase)
	{
		Logger.LogInfo($"Game changed to phase : {phase}");
		_gameState.CurrentPhase = phase;
		await BroadcastAsync(MessageFactory.Wrap(MessageType.PHASE, phase));
	}
}
