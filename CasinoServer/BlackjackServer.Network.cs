using System.Net.Sockets;
using CardGamesLibrary;
using CardGamesLibrary.Blackjack;
using BlackJackDealer;

partial class BlackjackServer
{
	private readonly TcpListener _listener;
	private readonly Dictionary<Identifier, (TcpClient client, CancellationTokenSource cts)> _clients = [];
	private int playerCount;
	private readonly int _playerLimit;
	private readonly string _lobbyPasscode;

	// ============ Client connection and handling ============
	async Task AcceptClientsAsync(CancellationToken ct)
	{
		while(!ct.IsCancellationRequested)
		{
			var client = await _listener.AcceptTcpClientAsync(ct);

			try
			{
				{
					var stream = client.GetStream();
					using var reader = new StreamReader(stream, leaveOpen: true);
					using var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };

					string? code = await reader.ReadLineAsync(ct);
					if(code is null || code != _lobbyPasscode)
					{
						await writer.WriteLineAsync("AUTH_FAILED");
						throw new IOException();//throw this simply because I know it is already handled as wanted.
					}

					await writer.WriteLineAsync("AUTH_OK");
				}

				ConnectionPaquet paquet = await MessageFactory.ReadFrameAsync<ConnectionPaquet>(client.GetStream(), 15_000) ?? throw new("The connection information is incomplete or invalid.");

				lock(_lockObj)
				{
					if(playerCount > 5)
						_ = UnicastAsync(MessageFactory.Wrap(MessageType.REJECT, "This game is full."), paquet.Id);

					_clients.Add(paquet.Id, (client, new()));
					var p = new Player() { Id = paquet.Id, Name = paquet.Name, Chips = paquet.Chips };

					int key = _gameState.Players.Where(
						x => x.Value is null
					).First().Key;

					_gameState.Players[key] = p;
					playerCount++;

					Message msg = MessageFactory.Wrap(MessageType.ACCEPT, new LobbyInformation() { LobbySize = _playerLimit, Game = LobbyGame.BLACKJACK });
					_ = UnicastAsync(msg, paquet.Id);

					Logger.LogWarning($"Client Connected : {p.Name}, {p.Id}.");

					BroadcastStateAndEvent($"{p.Name} has joined the game");
					msg = MessageFactory.Wrap(MessageType.INFO, $"Waiting for players to join, game will start soon.");
					_ = UnicastAsync(msg, paquet.Id);
				}
				_ = Task.Run(() => HandleClient(paquet.Id));
			}
			catch(IOException)
			{
				client.Close();
			}
			catch(Exception)
			{
				//Message msg = MessageFactory.Wrap(MessageType.REJECT, ex.Message);
				//UnicastAsync(msg, client);
			}
		}
	}

	async Task HandleClient(Identifier identifier)
	{
		if(!_clients.TryGetValue(identifier, out var value))
		{
			return;
		}

		var client = value.client;
		var stream = client.GetStream();
		var token = value.cts.Token;

		try
		{
			while(!token.IsCancellationRequested && client.Connected)
			{
				GameCommand? obj;
				try
				{
					obj = await MessageFactory.ReadFrameAsync<GameCommand>(stream, 30_000);
				}
				catch(IOException)
				{
					continue;
				}
				catch(Exception ex)
				{

					Logger.LogError($"[Client {identifier}] Error reading: {ex.Message}");
					break;
				}

				if(obj is null) continue;

				GameCommandType cmd = obj.Type;
				string arg = obj.Arg ?? "";

				if(_phaseCommandHandlers.TryGetValue((_gameState.CurrentPhase, cmd), out var handler))
				{
					await handler(identifier, arg);
				}
				else
				{
					var ev = MessageFactory.Wrap(MessageType.ERROR, $"Command {cmd} not valid during {_gameState.CurrentPhase}.");
					_ = UnicastAsync(ev, identifier);
				}
			}
		}
		finally { RemoveClient(identifier); }
	}

	// ============ Sending Messages ============
	private void BroadcastStateAndEvent(string message)
	{
		Message state = MessageFactory.Wrap(MessageType.STATE, BuildState());
		Message ev = MessageFactory.Wrap(MessageType.EVENT, message);
		List<Task> tasks = [BroadcastAsync(state), BroadcastAsync(ev)];

		Task.WhenAll(tasks);
	}

	private async Task BroadcastAsync<T>(T message)
	{
		MessageFactory.ConstructFrame(message, out byte[] frame);

		List<Task> tasks = [];

		foreach(var kvp in _clients.ToArray())
		{
			var (client, _) = kvp.Value;
			tasks.Add(SendSafeAsync(client, frame, kvp.Key));
		}

		await Task.WhenAll(tasks);
	}

	private async Task UnicastAsync<T>(T message, Identifier id)
	{
		MessageFactory.ConstructFrame(message, out byte[] frame);

		if(_clients.TryGetValue(id, out var value))
		{
			await SendSafeAsync(value.client, frame, id);
		}
	}

	private async Task SendSafeAsync(TcpClient client, byte[] frame, Identifier id)
	{
		try
		{
			if(client.Connected)
				await client.GetStream().WriteAsync(frame);
			else
				RemoveClient(id);
		}
		catch
		{
			RemoveClient(id);
		}
	}

	private void RemoveClient(Identifier id)
	{
		if(_clients.Remove(id, out (TcpClient, CancellationTokenSource) value))
		{
			try
			{
				value.Item2.Cancel();
				value.Item2.Dispose();
				value.Item1.Close();
			}
			catch { }

			lock(_lockObj)
			{
				var KV = _gameState.Players
				.FirstOrDefault(x => x.Value?.Id == id);

				var key = KV.Key;
				var name = KV.Value?.Name ?? "";

				if(_gameState.Players.ContainsKey(key))
				{
					_gameState.Players[key] = null;
					playerCount--;
					BroadcastStateAndEvent($"{_gameState.Players[key]?.Name} has left the game");
					Logger.LogWarning($"Client Disconnected : {name}, {id}.");
				}
			}
		}
	}

	DisplayGameState BuildState()
	{
		DisplayGameState state;
		List<DisplayPlayer> displays = [];
		foreach(var kvp in _gameState.Players.ToArray())
		{
			//if(kvp.Value is null)
			//displays.Add(new DisplayPlayer());
			//else
			displays.Add(new DisplayPlayer(kvp.Value));
		}
		lock(_lockObj)
		{
			state = new(Players: displays, Dealer: new(_gameState.Dealer.ToString(), _gameState.Dealer.PrintHandScore()), _gameState.CurrentTurn);
		}
		return state;
	}

	private CancellationToken CreateAllPlayersLeftToken()
	{
		var allCts = new CancellationTokenSource();

		void CheckIfAllGone()
		{
			if(_clients.Values.All(c => c.cts.IsCancellationRequested))
				allCts.Cancel();
		}

		// subscribe to each client's cancellation
		foreach(var (id, (_, cts)) in _clients)
		{
			if(_inactivePlayers.Contains(id)) continue;
			cts.Token.Register(CheckIfAllGone);
		}

		return allCts.Token;
	}
}
