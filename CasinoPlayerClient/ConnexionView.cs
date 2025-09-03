using CardGamesLibrary;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace CasinoPlayerClient
{
	internal class ConnexionView : FrameView
	{
		private int page = 0;
		private ConnexionFrame currentFrame = new NameFrameView();

		public ConnexionView()
		{
			Width = 128;
			Height = 42;
			X = Pos.Center();
			Y = 1;
			BorderStyle = LineStyle.None;
			ColorScheme = Schemes.Default;

			var centerBox = new FrameView() {
				Title = "MapleGlazedCrow's Little Casino",
				Width = 50,
				Height = 16,
				X = Pos.Center(),
				Y = Pos.Center(),
				CanFocus = true,
				ShadowStyle = ShadowStyle.Transparent,
				BorderStyle = LineStyle.Rounded,
			};

			var inputField = new TextField() {
				X = 5,
				Y = Pos.Percent(50),
				Width = Dim.Fill(5),
				Height = 3,
				CanFocus = true,
				HasFocus = true,
				BorderStyle = LineStyle.Heavy,
				CaptionColor = Color.DarkGray,
				ColorScheme = Schemes.TextField,
				TabStop = TabBehavior.TabStop,
				Text = Program.Username,
			};

			var prevButton = new Button {
				BorderStyle = LineStyle.None,
				Text = "Prev",
				X = 2,
				Y = Pos.Percent(100) - 2,
				ColorScheme = Schemes.Button,
				Enabled = false,
				TabStop = TabBehavior.TabStop,
			};

			var connectButton = new Button {
				BorderStyle = LineStyle.None,
				Text = "Connect",
				X = Pos.Center(),
				Y = Pos.Percent(100) - 2,
				ColorScheme = Schemes.Button,
				Enabled = false,
				TabStop = TabBehavior.TabStop,
			};

			var nextButton = new Button {
				BorderStyle = LineStyle.None,
				Text = "Next",
				X = Pos.Percent(100) - 10,
				Y = Pos.Percent(100) - 2,
				ColorScheme = Schemes.Button,
				Enabled = true,
				TabStop = TabBehavior.TabStop,
			};

			prevButton.Accepting += (_, e) => {
				if(page > 0)
				{
					centerBox.Remove(currentFrame);
					switch(page)
					{
						case 1:
							prevButton.Enabled = false;
							_ = int.TryParse(inputField.Text.ToString(), out int num);
							Program.Chips = num;
							currentFrame.Dispose();
							currentFrame = new NameFrameView();
							inputField.Text = Program.Username;
							inputField.CursorPosition = inputField.Text.Length;
							break;
						case 2:
							Program.Roomcode = inputField.Text.ToString();
							currentFrame.Dispose();
							currentFrame = new ChipsFrameView();
							inputField.Text = Program.Chips.ToString();
							inputField.CursorPosition = inputField.Text.Length;
							break;
						case 3:
							connectButton.Enabled = false;
							nextButton.Enabled = true;
							Program.Address = inputField.Text.ToString();
							currentFrame.Dispose();
							currentFrame = new CodeFrameView();
							inputField.Text = Program.Roomcode;
							inputField.CursorPosition = inputField.Text.Length;
							break;
						default:
							break;
					}
					centerBox.Add(currentFrame);
					page--;
				}
			};

			nextButton.Accepting += (_, e) => {
				if(page < 3)
				{
					centerBox.Remove(currentFrame);
					switch(page)
					{
						case 0:
							prevButton.Enabled = true;
							Program.Username = inputField.Text.ToString();
							currentFrame.Dispose();
							currentFrame = new ChipsFrameView();
							inputField.Text = Program.Chips.ToString();
							inputField.CursorPosition = inputField.Text.Length;
							break;
						case 1:
							_ = int.TryParse(inputField.Text.ToString(), out int num);
							Program.Chips = num;
							currentFrame.Dispose();
							currentFrame = new CodeFrameView();
							inputField.Text = Program.Roomcode;
							inputField.CursorPosition = inputField.Text.Length;
							break;
						case 2:
							nextButton.Enabled = false;
							connectButton.Enabled = true;
							Program.Roomcode = inputField.Text.ToString().ToUpperInvariant();
							currentFrame.Dispose();
							currentFrame = new AddressFrameView();
							inputField.Text = Program.Address;
							inputField.CursorPosition = inputField.Text.Length;
							break;
						default:
							break;
					}
					centerBox.Add(currentFrame);
					page++;
					nextButton.HasFocus = false;
				}
			};

			connectButton.Accepting += async (_, e) => {
				Program.Address = inputField.Text.ToString();

				TcpClient tcpClient = new();
				try
				{
					ValidateInputs();

					var parts = Program.Address.Split(':');
					IPAddress addr = IPAddress.Parse(parts[0]);
					int port = int.Parse(parts[1]);

					await tcpClient.ConnectAsync(addr, port);

					if(tcpClient.Connected)
					{
						{
							var stream = tcpClient.GetStream();
							using var reader = new StreamReader(stream, leaveOpen: true);
							using var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };

							await writer.WriteLineAsync(Program.Roomcode);

							string? response = await reader.ReadLineAsync();
							if(response != "AUTH_OK")
							{
								throw new Exception("Failed to authenticate!"); //throw simply because it displays a popup with the message
							}
						}

						MessageFactory.ConstructFrame(
							new ConnectionPaquet(
								id: Program.GlobalId,
								name: Program.Username,
								chips: Program.Chips
							),
							out byte[] frame
						);
						tcpClient.GetStream().Write(frame);

						Message msg = await MessageFactory.ReadFrameAsync<Message>(tcpClient.GetStream()) ??
						throw new("Response from server was null.");

						LobbyInformation info = msg.Type switch {
							MessageType.REJECT =>
								throw new(msg.Content.ToString()),

							MessageType.ACCEPT =>
								JsonSerializer.Deserialize<LobbyInformation>(msg.Content) ??
								throw new("Content from server was invalid for the response type."),

							MessageType.TURN or
							MessageType.ERROR or
							MessageType.TIME or
							MessageType.UNKNOWN or
							MessageType.MESSAGE or
							MessageType.EVENT or
							MessageType.STATE or
							MessageType.INFO or
							MessageType.BELL or
							_ =>
								throw new("Response type invalid at this time."),
						};

						Program.TcpClient = tcpClient;
						Application.Top?.Remove(this);
						Program.SwitchToGame(info);
						Dispose();
					}
				}
				catch(Exception ex)
				{
					Application.Invoke(() => {
						Console.Write('\a');
						MessageBox.ErrorQuery($"<{Application.QuitKey}> to continue", ex.Message);
					});
					tcpClient.Close();
				}
				e.Cancel = true;
				return;
			};

			centerBox.Add(currentFrame, prevButton, inputField, connectButton, nextButton);

			Add(centerBox);
		}

		private static void ValidateInputs()
		{
			if(!Regex.IsMatch(Program.Username, @"^[a-zA-Z][a-zA-Z0-9_]{1,14}$"))
			{
				throw new("Name must start with a letter and be 2–15 characters long.");
			}
			if(!Regex.IsMatch(Program.Chips.ToString(), @"^[0]*[1-9]{1}[0-9]{2,5}$"))
			{
				throw new("The amount of chips you bring in must be between 100 and 999 999.");
			}
			if(!Regex.IsMatch(Program.Address, @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):(?:\d{1,5})\b"))
			{
				throw new("Address must be a valid IPv4 followed by a port");
			}
		}
	}

	public abstract class ConnexionFrame : FrameView
	{
		public Label Label { get; private set; } = new() {
			X = 0,
			Y = Pos.Center(),
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			TextAlignment = Alignment.Center,
			VerticalTextAlignment = Alignment.Center,
		};

		public ConnexionFrame()
		{
			X = 5;
			Y = 0;
			Width = Dim.Fill(5);
			Height = Dim.Percent(50);
			BorderStyle = LineStyle.None;

			Add(Label);

			VisibleChanged += (_, _) => {
				Label.Draw();
			};
		}
	}

	public class NameFrameView : ConnexionFrame
	{
		public NameFrameView()
		{
			Label.Text = "Greetings,\n Please enter your <b>name</b>.";
		}
	}

	public class ChipsFrameView : ConnexionFrame
	{

		public ChipsFrameView()
		{
			Label.Text = $"How many <b>chips</b> would\n you like to bring in?";
		}
	}

	public class AddressFrameView : ConnexionFrame
	{

		public AddressFrameView()
		{
			Label.Text = "Please enter the <b>address\n and port</b> of the lobby.";
		}
	}

	public class CodeFrameView : ConnexionFrame
	{

		public CodeFrameView()
		{
			Label.Text = "Please enter the <b>room\n code</b> for the lobby.";
		}
	}
}
