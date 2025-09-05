using System.Net.Sockets;
using CardGamesLibrary;
using CasinoPlayerClient.Blackjack;
using Terminal.Gui;

namespace CasinoPlayerClient
{
	class Program
	{
		public static string Username { get; set; } = "";
		public static int Chips { get; set; }
		public static string Address { get; set; } = "";
		public static string Roomcode { get; set; } = "";
		public static TcpClient? TcpClient { get; set; }
		public static Identifier GlobalId { get; set; } = Identifier.New();

		static void Main()
		{
			Application.Init();
			Application.QuitKey = Key.Q.WithCtrl;

			Application.Force16Colors = true;

			var app = new Toplevel() {
				X = Pos.Center(),
				Y = 0,
				Width = 128,
				Height = Dim.Fill(),
				ColorScheme = Schemes.TopLevel,
			};

			var menuBar = new MenuBar() {
				X = 0,
				Y = 0,
				Visible = true,
				ColorScheme = Schemes.MenuBar,
			};
			menuBar.HotKeyBindings.Clear();

			var adjust = new MenuBarItem {
				Title = "_Adjust Screen",
				Action = () => {
					AdjustView dialog = new();
					Application.Top?.Add(dialog);
					dialog.SetFocus();
				},
			};

			var help = new MenuBarItem {
				Title = "?",
				Children = [
					new MenuItem {
						Title = "_Server Address",
						Data="address",
						Action = () => {
							MessageBox.Query("Server Address", $"\t\t{
								(string.IsNullOrWhiteSpace(Address) ? "<empty>" : Address)
							}\t\t", "OK");
						}
					},
				],
			};
			menuBar.Menus = [adjust, help];

			View bottomBar = new() {
				X = 0,
				Y = 44,
				Width = Dim.Percent(100),
				Height = 1,
				ColorScheme = Schemes.MenuBar,

			};

			var elem1 = new Label() { X = 0, Text = $" <{Application.QuitKey}> Quit " };
			var elem2 = new LineView(Orientation.Vertical) { X = Pos.Right(elem1) };
			//var elem3 = new Label() { X = Pos.Right(elem2), Text = $" <{Application.PrevTabGroupKey}>/<{Application.NextTabGroupKey}> Prev/Next Tab Group " };
			//var elem4 = new LineView(Orientation.Vertical) { X = Pos.Right(elem3) };
			var elem5 = new Label() { X = Pos.AnchorEnd(), Text = $" {Environment.OSVersion}, {Application.Driver?.GetType().Name ?? ""} " };
			bottomBar.Add(elem1, elem2, /*elem3, elem4,*/ elem5);

			app.Add(menuBar, new ConnexionView(), bottomBar);

			Application.Run(app);
			Application.Shutdown();
		}

		public static void SwitchToGame(LobbyInformation info)
		{
			if(TcpClient is null) throw new Exception("the tcp client is null.");

			var cts = new CancellationTokenSource();

			FrameView view = info.Game switch {
				LobbyGame.BLACKJACK => new BlackjackView(info.LobbySize, cts),
				_ => throw new ArgumentException("un-recognized game type."),
			};

			GameClient client = info.Game switch {
				LobbyGame.BLACKJACK when view is BlackjackView bjview => new BlackjackClient(TcpClient.GetStream(), bjview, cts.Token),
				_ => throw new ArgumentException("un-recognized game type."),
			};

			Application.Top?.Add(view);
			client.Start();
		}

		public static void SwitchToConnection()
		{
			TcpClient?.Close();
			TcpClient = null;

			FrameView view = new ConnexionView();
			Application.Top?.Add(view);
		}
	}
}