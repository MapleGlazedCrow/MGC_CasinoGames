using CardGamesLibrary;
using CardGamesLibrary.Blackjack;

partial class BlackjackServer
{
	private readonly Dictionary<(GamePhase, GameCommandType), Func<Identifier, string, Task>> _phaseCommandHandlers;

	private async Task HandleRefillAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(!int.TryParse(arg, out int bet) || bet <= 0)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Invalid bet amount."), identifier);
				return;
			}

			if(player.Chips + bet < 999_999_999)
			{
				player.AddChips(bet);
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.INFO, $"{bet} chips were succesfully added to your balance."), identifier);
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.STATE, BuildState()), identifier);
			}
		}
		await Task.CompletedTask;
	}

	public async Task HandleSurrenderAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(player.Hand.TrySurrender())
			{
				player.AddChips(player.MainBet / 2);
				BroadcastStateAndEvent($"{player.Name} surrendered.");

				if(_turnCompletionSources.TryGetValue(identifier, out var tcs))
					tcs.TrySetResult(true);
			}
			else
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, $"You can not surrender at this stage."), identifier);
			}
		}
		await Task.CompletedTask;
	}

	public async Task HandleInsuranceAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(!int.TryParse(arg, out int bet) || bet <= 0 || bet < player.MainBet / 2)
			{

				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Insurrance bet must be up to half your main bet."), identifier);
				return;
			}

			if(!player.TryTakeChips(bet))
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Insufficient funds."), identifier);
				return;
			}

			player.InsurranceBet = bet;
			BroadcastStateAndEvent($"{player.Name} bet {bet} as insurrance.");
		}
		await Task.CompletedTask;
	}

	public async Task HandleChatAsync(Identifier identifier, string arg)
	{
		var user = _gameState.Players.Values.Where(p => p?.Id == identifier).FirstOrDefault()?.Name ?? "<unknown>";
		Message msg = MessageFactory.Wrap(MessageType.MESSAGE, user + " : " + arg);
		await BroadcastAsync(msg);
		await Task.CompletedTask;
	}

	private async Task HandleHitAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(_gameState.Players[_gameState.CurrentTurn]?.Id != identifier)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "It's not your turn!"), identifier);
				return;
			}

			Card card = DrawCard();
			if(currentIsSplit)
			{
				player.Hand.AddCardToSplit(card);
			}
			else
			{
				player.Hand.AddCardToHand(card);
			}

			BroadcastStateAndEvent($"{player.Name} drew a {card.ToCompactString(true)}.");

			int score = currentIsSplit ? player.Hand.SplitScore : player.Hand.HandScore;
			int count = currentIsSplit ? player.Hand.SplitCount : player.Hand.CardCount;

			{
				bool busted = score > 21;
				bool handComplete = busted || count == 7;

				if(busted)
				{
					_ = BroadcastAsync(MessageFactory.Wrap(MessageType.EVENT, $"{player.Name} busted!"));
				}

				if(handComplete)
				{
					if(player.Hand.HasSplit && !currentIsSplit)
					{
						currentIsSplit = true;
					}
					else
					{
						if(_turnCompletionSources.TryGetValue(identifier, out var tcs))
							tcs.TrySetResult(true);

						currentIsSplit = false;
					}
				}
			}
		}
		sPhaseTimer?.ResetTime();
		await Task.CompletedTask;
	}

	private async Task HandleStandAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(_gameState.Players[_gameState.CurrentTurn]?.Id != identifier)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "It's not your turn!"), identifier);
				return;
			}

			BroadcastStateAndEvent($"{player.Name} stands");

			if(player.Hand.HasSplit && !currentIsSplit)
			{
				currentIsSplit = true;
			}
			else
			{
				if(_turnCompletionSources.TryGetValue(identifier, out var tcs))
					tcs.TrySetResult(true);
				currentIsSplit = false;
			}
		}
		sPhaseTimer?.ResetTime();
		await Task.CompletedTask;
	}

	private async Task HandleSplitAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			try
			{
				var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
					?? throw new NullReferenceException("Command was received from a null player.");

				if(_gameState.Players[_gameState.CurrentTurn]?.Id != identifier)
				{
					_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "It's not your turn!"), identifier);
					return;
				}

				if(!player.TryTakeChips(player.MainBet))
				{
					_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Insufficient funds."), identifier);
					return;
				}

				player.Hand.SplitCards();
				player.SplitBet = player.MainBet;
				_ = BroadcastAsync(MessageFactory.Wrap(MessageType.EVENT, $"{player.Name} split their hand"));

				Card c1 = DrawCard();
				player.Hand.AddCardToHand(c1);
				Card c2 = DrawCard();
				player.Hand.AddCardToSplit(c2);

				BroadcastStateAndEvent($"{player.Name} drew a {c1.ToCompactString(true)} and a {c2.ToCompactString(true)}.");
			}
			catch(Exception ex)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, ex.Message), identifier);
				return;
			}
		}
		sPhaseTimer?.ResetTime();
		await Task.CompletedTask;
	}

	private async Task HandleDoubleDownAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(_gameState.Players[_gameState.CurrentTurn]?.Id != identifier)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "It's not your turn!"), identifier);
				return;
			}

			var bet = currentIsSplit ? player.SplitBet : player.MainBet;
			if(!player.TryTakeChips(bet))
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Insufficient funds."), identifier);
				return;
			}
			_ = BroadcastAsync(MessageFactory.Wrap(MessageType.EVENT, $"{player.Name} is doubling down!"));

			Card card = DrawCard();
			if(currentIsSplit)
			{
				player.Hand.AddCardToSplit(card);
				player.SplitBet *= 2;
			}
			else
			{
				player.Hand.AddCardToHand(card);
				player.MainBet *= 2;
			}

			BroadcastStateAndEvent($"{player.Name} drew a {card.ToCompactString(true)}");

			int score = currentIsSplit ? player.Hand.SplitScore : player.Hand.HandScore;
			if(score > 21)
			{
				_ = BroadcastAsync(MessageFactory.Wrap(MessageType.EVENT, $"{player.Name} busted!"));
			}

			if(player.Hand.HasSplit && !currentIsSplit)
			{
				currentIsSplit = true;
			}
			else
			{
				if(_turnCompletionSources.TryGetValue(identifier, out var tcs))
					tcs.TrySetResult(true);
				currentIsSplit = false;
			}
		}
		sPhaseTimer?.ResetTime();
		await Task.CompletedTask;
	}

	private async Task HandleBetAsync(Identifier identifier, string arg)
	{
		lock(_lockObj)
		{
			var player = _gameState.Players.Values.FirstOrDefault(p => p?.Id == identifier)
				?? throw new NullReferenceException("Command was received from a null player.");

			if(!int.TryParse(arg, out int bet) || bet <= sMinimumBet)
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Invalid bet amount."), identifier);
				return;
			}

			string ev = player.MainBet != 0 ? $"{player.Name} changed their bet to {bet}!" : $"{player.Name} bet {bet}";

			if(!player.TryTakeChips(bet - player.MainBet))
			{
				_ = UnicastAsync(MessageFactory.Wrap(MessageType.ERROR, "Insufficient funds."), identifier);
				return;
			}

			player.MainBet = bet;
			BroadcastStateAndEvent(ev);
		}
		await Task.CompletedTask;
	}
}
