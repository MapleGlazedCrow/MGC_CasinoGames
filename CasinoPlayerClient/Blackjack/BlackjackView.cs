using CardGamesLibrary;
using CardGamesLibrary.Blackjack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui;

namespace CasinoPlayerClient.Blackjack
{
	internal class BlackjackView : FrameView
	{
		public TextField CommandInputField;
		public TextField ChatInputField;

		private readonly FrameView _chipsBox;

		private readonly ScrollableTextView _infoTextArea;
		private readonly ScrollableTextView _chatTextArea;

		private readonly FrameView _dealerHand;
		private readonly Label _dealerScore;

		private readonly Dictionary<int, (Tab, Label)> _myTabs;
		private readonly List<PlayerView> _pBoxes;

		private readonly ProgressBar _timerBar;

		private readonly MenuBarItem _leaveGame = new() {
			Title = "_Leave Lobby",
		};

		public BlackjackView(int playerCount, CancellationTokenSource cts)
		{
			X = Pos.Center();
			Y = 1;
			Width = 128;
			Height = 42;
			BorderStyle = LineStyle.None;
			ColorScheme = Schemes.Default;

			// ============ Dealer Box ============
			var dealerBox = new FrameView() {
				Title = "Dealer",
				X = 0,
				Y = 1,
				CanFocus = false,
				Width = 71,
				Height = 8,
				BorderStyle = LineStyle.Single,
			};
			// ============ Dealer Hand ============
			_dealerHand = new FrameView() {
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = 6,
				TextAlignment = Alignment.Center,
				CanFocus = false,
				BorderStyle = LineStyle.None,
				Text = "",
			};
			// ============ Dealer Score ============
			_dealerScore = new Label() {
				X = 0,
				Y = Pos.Bottom(_dealerHand) - 1,
				Width = Dim.Fill(),
				Height = 1,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "[ 0 ]"
			};
			dealerBox.Add(_dealerHand, _dealerScore);

			// ============ Game Info ============
			var infoBox = new FrameView() {
				X = Pos.Right(dealerBox),
				Y = Pos.Top(dealerBox),
				Width = Dim.Fill(),
				Height = 8,
				Title = "Lobby Events",
			};
			_infoTextArea = new ScrollableTextView() {
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Fill(1),
				TextDirection = TextDirection.LeftRight_TopBottom,
				TextAlignment = Alignment.Start,
				Text = "",
				WordWrap = true,
				ReadOnly = true,
				CanFocus = false,
				ColorScheme = Schemes.TextBox,
			};
			infoBox.Add(_infoTextArea);

			// ============ Display (Main TextView) ============

			var displayBox = new TabView() {

				X = 0,
				Y = Pos.Bottom(dealerBox),
				Width = 48,
				Height = 27,
				BorderStyle = LineStyle.None,
				CanFocus = false,
			};

			string[] colors = ["#ff0000", "#ffff00", "#00ff00", "#00ffff", "#0000ff"];
			_pBoxes = [.. Enumerable.Range(1, playerCount).Select(i => new PlayerView(color: colors[i - 1]))];

			_myTabs = Enumerable.Range(1, playerCount)
			.ToDictionary(i => i, i => (new Tab() {
				View = _pBoxes[i - 1],
				DisplayText = $" Seat {i} ",
				ColorScheme = Schemes.Tabs,
				Id = $"pl{i}",
			}, new Label() { Text = $" Seat {i} ", ColorScheme = Schemes.TabLabel, }));

			foreach(var item in _myTabs.Reverse())
			{
				item.Value.Item1.Add(item.Value.Item2);
				displayBox.AddTab(item.Value.Item1, item.Key == 1);
			}

			// ============ Chat Box ============
			var chatBox = new FrameView() {
				X = Pos.Right(displayBox),
				Y = Pos.Bottom(infoBox),
				Width = 48,
				Height = Dim.Height(displayBox),
				Title = "Message Log (be kind)"
			};
			_chatTextArea = new ScrollableTextView() {
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Fill(3),
				TextDirection = TextDirection.LeftRight_TopBottom,
				TextAlignment = Alignment.Start,
				Text = "",
				WordWrap = true,
				ReadOnly = true,
				CanFocus = false,
				ColorScheme = Schemes.TextBox,
			};

			var chatInputZone = new FrameView {
				X = 0,
				Y = Pos.Bottom(_chatTextArea),
				Width = Dim.Fill(),
				Height = 3,
				BorderStyle = LineStyle.Single,
			};
			chatInputZone.Add(new Label() { Text = ">/", X = 0, Y = 0 });
			ChatInputField = new TextField() {
				X = 2,
				Y = 0,
				Width = Dim.Fill(),
				ColorScheme = Schemes.TextField,
			};
			chatInputZone.Add(ChatInputField);
			chatBox.Add(_chatTextArea, chatInputZone);

			// ============ Rules Box ============
			var rulesAndPayouts = new TabView() {
				X = Pos.Right(chatBox),
				Y = Pos.Bottom(infoBox),
				Width = Dim.Fill(),
				Height = Dim.Height(displayBox),
				BorderStyle = LineStyle.None,
				ColorScheme = Schemes.Default,
				CanFocus = false,
			};
			var rulesBox1 = new FrameView() {
				Width = Dim.Fill(),
				Height = Dim.Fill(),
				BorderStyle = LineStyle.None,
				Text =
				" ● The dealer must stand on\n   17 or above.\n\n" +
				" ● \"Blackjack\" is only\n   obtained with a hand of\n   two cards.\n\n" +
				" ● If a hand is \"Split\", it\n   can no longer win by\n   \"Blackjack\".\n\n" +
				" ● Max. one \"Split\" per hand.\n\n" +
				" ● A \"Split\" may only happen\n   when a hand is a pair.\n\n" +
				" ● If the dealer is showing\n   an \"Ace\", players may\n   place an Insurance bet.\n\n"
			};
			var rulesBox2 = new FrameView() {
				Width = Dim.Fill(),
				Height = Dim.Fill(),
				BorderStyle = LineStyle.None,
				Text =
				" ● On a \"Double Down\", the\n   hand gets one additionnal\n   card and the game moves on\n   to the next hand.\n\n" +
				" ● A \"Split\" or \"Double Down\"\n   can only happen if enough\n   chips are available.\n\n" +
				" ● If a hand reaches 7 cards\n   without a bust, the hand\n   wins by \"7-Card Charlie\".\n\n"
			};
			var rulesTab1 = new Tab() {
				View = rulesBox1,
				DisplayText = "Rules 1/2",
			};
			var rulesTab2 = new Tab() {
				View = rulesBox2,
				DisplayText = "Rules 2/2",
			};
			rulesAndPayouts.AddTab(rulesTab1, true);
			rulesAndPayouts.AddTab(rulesTab2, false);

			var payoutsBox = new FrameView() {
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Fill(),
				BorderStyle = LineStyle.None,
			};
			payoutsBox.Add(new Label() {
				Width = Dim.Fill(),
				TextAlignment = Alignment.Center,
				Text =
				"[ 7-Card Charlie ].......4:1\n\n" +
				"[ Blackjack ]............3:2\n\n" +
				"[ Winner ]...............1:1\n\n" +
				"[ Push ].................0:1\n\n" +
				"[ Loser ]...............-1:1\n\n" +
				"[ Surrender ]...........-1:2\n\n" +
				"[ Insurance ]............2:1\n\n" +
				"\nFor odds at λ:μ,\nthe payout is\ncalculated as such:\n\n" +
				"<your bet>\n+\nλ * (<your bet> / μ)"

			}, new Line() { Y = 13 });
			var payoutsTab = new Tab() {
				View = payoutsBox,
				DisplayText = "Payouts",
			};
			rulesAndPayouts.AddTab(payoutsTab, false);

			// ============ quick commands ============
			var availableComm = new Label() {
				Y = Pos.Bottom(displayBox),
				Text = "Quick Commands (click to fill) :"
			};

			var betComm = new Label() {
				X = Pos.Right(availableComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "BET <amount>",
			};

			var hitComm = new Label() {
				X = Pos.Right(betComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "HIT",
			};

			var standComm = new Label() {
				X = Pos.Right(hitComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "STAND",
			};

			var splitComm = new Label() {
				X = Pos.Right(standComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "SPLIT",
			};

			var doubleComm = new Label() {
				X = Pos.Right(splitComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "DOUBLE",
			};

			var surrenderComm = new Label() {
				X = Pos.Right(doubleComm) + 4,
				Y = Pos.Top(availableComm),
				Text = "SURRENDER",
			};
			Add(availableComm, hitComm, standComm, splitComm, doubleComm, betComm, surrenderComm);

			// ============ Command Input ============
			var commandBox = new FrameView() {
				Title = "Command",
				X = 0,
				Y = Pos.Bottom(availableComm) + 1,
				Width = 51,
				Height = 3,
			};
			CommandInputField = new TextField() {
				X = 2,
				Y = 0,
				Width = Dim.Fill(),
				ColorScheme = Schemes.TextField,
			};
			commandBox.Add(new Label() { Text = ">/", X = 0, Y = 0 });
			commandBox.Add(CommandInputField);

			// ============ Add events to Quick Command ============
			betComm.MouseClick += (_, _) => {
				CommandInputField.Text = "BET ";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 4;
			};
			hitComm.MouseClick += (_, _) => {
				CommandInputField.Text = "HIT";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 3;
			};
			standComm.MouseClick += (_, _) => {
				CommandInputField.Text = "STAND";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 5;
			};
			splitComm.MouseClick += (_, _) => {
				CommandInputField.Text = "SPLIT";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 5;
			};
			doubleComm.MouseClick += (_, _) => {
				CommandInputField.Text = "DOUBLE";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 6;
			};
			surrenderComm.MouseClick += (_, _) => {
				CommandInputField.Text = "SURRENDER";
				CommandInputField.SetFocus();
				CommandInputField.CursorPosition = 9;
			};

			// ============ Chips Box ============
			_chipsBox = new FrameView() {
				Title = "Chips",
				X = Pos.Percent(100) - 20,
				Y = Pos.Top(commandBox),
				Width = 20,
				Height = 3,
			};

			// ============ timer bar ============
			_timerBar = new ProgressBar() {
				X = 0,
				Y = Pos.Percent(100) - 1,
				Height = 1,
				Width = Dim.Fill(),
				Fraction = 0.30f,
				ProgressBarStyle = ProgressBarStyle.MarqueeBlocks,
				ColorScheme = BarColors.ProgressBarPassive,
			};
			Add(_timerBar);

			// ============ Add all views ============
			Add(dealerBox, infoBox, chatBox, displayBox, _chipsBox, commandBox, rulesAndPayouts);

			if(Application.Top is not null && Application.Top.MenuBar is not null)
			{
				_leaveGame.Action = async () => {
					Application.Top?.Remove(this);
					_leaveGame.RemoveMenuItem();
					await cts.CancelAsync();
					Program.SwitchToConnection();
					Dispose();
				};
				Application.Top.MenuBar.Menus = [_leaveGame, .. Application.Top.MenuBar.Menus];
			}
		}

		public void UpdateChips(string amount) => _chipsBox.Text = amount;

		public void AddInfo(string s)
		{
			//in blue
			_infoTextArea.Text += $"INFO : {s}\n";
			_infoTextArea.ScrollToEnd();
		}
		public void AddEvent(string s)
		{
			//in white
			_infoTextArea.Text += $"EVENT : {s}\n";
			_infoTextArea.ScrollToEnd();
		}

		public void AddError(string s)
		{
			//in red
			_infoTextArea.Text += $"ERROR : {s}\n";
			_infoTextArea.ScrollToEnd();
		}

		public void AddToChat(string s)
		{
			_chatTextArea.Text += $"-> {s}\n";
			_chatTextArea.ScrollToEnd();
		}

		public void UpdatePlayerCards(List<CardGamesLibrary.Blackjack.DisplayPlayer> players)
		{
			if(players.Count != _pBoxes.Count) throw new Exception("Mismatched player count");
			for(int i = 0; i < players.Count; ++i)
			{
				if(players[i].IsNull)
				{
					_myTabs[i + 1].Item1.Enabled = false;
				}
				else
				{
					PlayerView view = _pBoxes[i];

					view.MainBet.Text = $"[ {players[i].MainBet} ]".PadLeft(19, '.');
					view.MainScore.Text = $"[ {players[i].MainScore} ]".PadLeft(22, '.');
					view.SplitBet.Text = $"[ {players[i].SplitBet} ]".PadLeft(18, '.');
					view.SplitScore.Text = $"[ {players[i].SplitScore} ]".PadLeft(22, '.');

					view.BottomPanel.Text = players[i].CompleteVisual;
					view.SetName(players[i].Name);

					if(Identifier.Parse(players[i].Id) == Program.GlobalId)
					{
						UpdateChips(players[i].Chips);
					}
				}
			}
		}

		public void UpdateDealer(DisplayDealer dealer)
		{
			_dealerHand.Text = dealer.Visual;
			_dealerScore.Text = $"[ {dealer.Score} ]";
		}

		public void UpdateTimer(float fraction)
		{
			_timerBar.Fraction = fraction;
			_timerBar.SetNeedsDraw();
		}

		public void UpdatePhase(GamePhase phase)
		{
			_timerBar.ColorScheme = BarColors.TryParse(phase);
			_timerBar.SetNeedsDraw();
		}

		public void ChangeCurrentTurn(int index)
		{
			foreach(var item in _myTabs.Values)
			{
				item.Item2.ColorScheme = Schemes.TabLabel;
			}

			if(index != 0)
				_myTabs[index].Item2.ColorScheme = Schemes.TabHasTurn;
		}
	}

	class ScrollableTextView : TextView
	{
		public void ScrollToEnd()
		{
			MoveEnd();
			SetNeedsDraw();
		}
	}

	public static class BarColors
	{
		public static ColorScheme TryParse(GamePhase phase)
		{
			switch(phase)
			{
				case GamePhase.Insurance:
				case GamePhase.PlayerTurns:
				case GamePhase.Betting:
					return ProgressBarActive;

				case GamePhase.PreGame:
				case GamePhase.Drawing:
				case GamePhase.DealerTurn:
				case GamePhase.Resolve:
				case GamePhase.EndGame:
				default:
					return ProgressBarPassive;
			}
		}

		public static readonly ColorScheme ProgressBarActive = new ColorScheme {
			HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Yellow, Terminal.Gui.Color.Black),
		};

		public static readonly ColorScheme ProgressBarPassive = new ColorScheme {
			HotNormal = new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black),
		};
	}
}