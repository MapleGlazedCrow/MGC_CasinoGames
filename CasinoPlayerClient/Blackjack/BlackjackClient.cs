using CardGamesLibrary;
using CardGamesLibrary.Blackjack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Gui;

namespace CasinoPlayerClient.Blackjack
{
	class BlackjackClient(NetworkStream _stream, BlackjackView _view, CancellationToken _ct) : GameClient<BlackjackView>(_stream, _view, _ct)
	{
		public override void Start()
		{
			try
			{
				View.CommandInputField.KeyDown += (object? sender, Key keyEvent) => {
					if(keyEvent.KeyCode == KeyCode.Enter)
					{
						var cmd = View.CommandInputField.Text.ToString();

						SendCommand(cmd);

						View.CommandInputField.Text = "";
						keyEvent.Handled = true;
					}
				};

				View.ChatInputField.KeyDown += (object? sender, Key keyEvent) => {
					if(keyEvent.KeyCode == KeyCode.Enter)
					{
						var msg = View.ChatInputField.Text.ToString();

						if(msg.Trim() != "")
						{
							SendMessage(msg);

							View.ChatInputField.Text = "";
							keyEvent.Handled = true;
						}
					}
				};

				_ = Task.Run(ReceiveLoop);
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error : " + ex.Message);
			}
		}

		protected override async Task ReceiveLoop()
		{
			while(!CT.IsCancellationRequested)
			{
				try
				{
					Message? msg = await MessageFactory.ReadFrameAsync<Message>(Stream);
					if(msg == null) break;

					//received a message, now analyse everything
					switch(msg.Type)
					{
						case MessageType.MESSAGE:
							Application.Invoke(() => { View.AddToChat(msg.Content.ToString()); });
							break;

						case MessageType.EVENT:
							Application.Invoke(() => { View.AddEvent(msg.Content.ToString()); });
							break;
						case MessageType.INFO:
							Application.Invoke(() => { View.AddInfo(msg.Content.ToString()); });
							break;
						case MessageType.BELL:
							Application.Invoke(() => {
								Console.Write('\a');
								View.AddInfo(msg.Content.ToString());
							});
							break;
						case MessageType.ERROR:
							Application.Invoke(() => { View.AddError(msg.Content.ToString()); });
							break;

						case MessageType.TIME:
							Application.Invoke(() => { View.UpdateTimer(JsonSerializer.Deserialize<float>(msg.Content)); });
							break;
						case MessageType.PHASE:
							Application.Invoke(() => { View.UpdatePhase(JsonSerializer.Deserialize<GamePhase>(msg.Content)); });
							break;

						case MessageType.STATE:
							DisplayGameState state = JsonSerializer.Deserialize<DisplayGameState>(msg.Content) ??
								throw new("Content from server was invalid for the response type.");
							Application.Invoke(() => {
								View.UpdatePlayerCards(state.Players);

								View.UpdateDealer(state.Dealer);
							});
							break;
						case MessageType.TURN:
							int turnIndex = JsonSerializer.Deserialize<int>(msg.Content);
							Application.Invoke(() => {
								View.ChangeCurrentTurn(turnIndex);
							});
							break;

						case MessageType.UNKNOWN:
							Application.Invoke(() => {
								Console.Write('\a');
								MessageBox.ErrorQuery($"<{Application.QuitKey}> to continue", JsonSerializer.Deserialize<string>(msg.Content));
							});
							Application.RequestStop();
							break;

						default:
						case MessageType.REJECT:
						case MessageType.ACCEPT:
							throw new("Response from server was invalid.");
					}
				}
				catch(IOException ex)
				{
					if(!CT.IsCancellationRequested)
					{
						Application.Invoke(() => {
							Console.Write('\a');
							MessageBox.ErrorQuery($"<{Application.QuitKey}> to continue", ex.Message);
						});
					}
					break;
				}
				catch(Exception ex)
				{
					Application.Invoke(() => {
						Console.Write('\a');
						MessageBox.ErrorQuery($"<{Application.QuitKey}> to continue", ex.Message);
					});
				}

				//Application.Invoke(() => {
				//	view.playersBox.Text = $"Status: {state.Status}";
				//});
			}
		}

		void SendCommand(string input)
		{
			if(!CommandParser.TryParse(input, out var command, out var error))
			{
				Console.Write('\a');
				MessageBox.ErrorQuery($"<{Application.QuitKey}> to continue", error ?? "Invalid command.");
				return;
			}

			MessageFactory.ConstructFrame(command, out byte[] frame);
			Stream.Write(frame);
		}

		void SendMessage(string message)
		{
			var obj = $"MSG {message}";
			SendCommand(obj);
			//MessageFactory.ConstructFrame(obj, out byte[] frame);
			//client.GetStream().Write(frame);
		}
	}
}
