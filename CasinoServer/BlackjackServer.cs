using System.Net;
using CardGamesLibrary;
using CardGamesLibrary.Blackjack;
using BlackJackDealer;
using CardGamesLibrary.Security;

partial class BlackjackServer
{
	private readonly object _lockObj = new();

	public BlackjackServer(int port, int seats = 5)
	{
		_playerLimit = seats;
		_gameState = new(seats);

		_listener = new(IPAddress.Any, port);

		_lobbyPasscode = RoomCodeGenerator.Generate();

		_phaseCommandHandlers = new() {
			{(GamePhase.Betting, GameCommandType.Bet), HandleBetAsync},

			{(GamePhase.Insurance,GameCommandType.Bet), HandleInsuranceAsync},

			{(GamePhase.PlayerTurns,GameCommandType.Hit), HandleHitAsync},
			{(GamePhase.PlayerTurns,GameCommandType.Stand), HandleStandAsync},
			{(GamePhase.PlayerTurns,GameCommandType.Split), HandleSplitAsync},
			{(GamePhase.PlayerTurns,GameCommandType.Double), HandleDoubleDownAsync},
			{(GamePhase.PlayerTurns,GameCommandType.Surrender), HandleSurrenderAsync},

			{(GamePhase.PreGame,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.Betting,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.Drawing,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.Insurance,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.PlayerTurns,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.DealerTurn,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.Resolve,GameCommandType.Chat), HandleChatAsync},
			{(GamePhase.EndGame,GameCommandType.Chat), HandleChatAsync},

			{(GamePhase.PreGame,GameCommandType.Refill), HandleRefillAsync},
			{(GamePhase.Betting,GameCommandType.Refill), HandleRefillAsync},
			{(GamePhase.EndGame,GameCommandType.Refill), HandleRefillAsync},
		};
	}

	public async Task StartAsync(CancellationToken token)
	{
		_listener.Start();
		Logger.LogNormal($"Server started on port {_listener.LocalEndpoint.ToString()?.Split(":")[1]}");
		Logger.LogNormal($"The lobby code is : {_lobbyPasscode}");

		_ = AcceptClientsAsync(token);

		await GameLoopAsync(token);
	}
}
