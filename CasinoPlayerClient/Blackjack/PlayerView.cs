using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace CasinoPlayerClient.Blackjack
{
	internal class PlayerView : FrameView
	{
		public FrameView MainBet;
		public FrameView MainScore;
		public FrameView SplitBet;
		public FrameView SplitScore;
		public FrameView BottomPanel;

		private Label Name { get; set; }

		public PlayerView(string name = "empty", string color = "#45abee")
		{
			Width = Dim.Fill();
			Height = Dim.Fill();
			X = 0;
			Y = 0;
			Width = 46;
			Height = 24;
			TextAlignment = Alignment.Center;
			CanFocus = false;
			BorderStyle = LineStyle.None;

			Add(new Line() { X = 0, Y = 0, Width = 2, });
			Name = new Label() {
				Text = $" {name} ",
				X = 2,
				Y = 0,
				ColorScheme = new ColorScheme {
					Normal = new Terminal.Gui.Attribute(new Color(color), Color.Black),
				}
			};
			Add(new Line() { X = Pos.Right(Name), Y = 0, Width = Dim.Fill(), });

			Add(new Label {
				X = 9,
				Y = 1,
				Width = 9,
				Height = 1,
				Text = "Main Bet:",
			});
			MainBet = new FrameView {
				X = 18,
				Y = 1,
				Width = 19,
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				Text = "............[ n/a ]",
			};

			Add(new Label {
				X = 9,
				Y = Pos.Bottom(MainBet),
				Width = 6,
				Height = 1,
				Text = "Score:",
			});
			MainScore = new FrameView {
				X = 15,
				Y = Pos.Bottom(MainBet),
				Width = 22,
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				Text = "...............[ n/a ]",
			};

			Add(new Label {
				X = 9,
				Y = Pos.Bottom(MainScore),
				Width = 10,
				Height = 1,
				Text = "Split Bet:",
			});
			SplitBet = new FrameView {
				X = 19,
				Y = Pos.Bottom(MainScore),
				Width = 18,
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				Text = "...........[ n/a ]",
			};

			Add(new Label {
				X = 9,
				Y = Pos.Bottom(SplitBet),
				Width = 6,
				Height = 1,
				Text = "Score:",
			});
			SplitScore = new FrameView {
				X = 15,
				Y = Pos.Bottom(SplitBet),
				Width = 22,
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				Text = "...............[ n/a ]",
			};

			Add(new Line() { X = Pos.Percent(0), Y = Pos.Bottom(SplitScore), Width = Dim.Percent(100) });

			BottomPanel = new FrameView {
				X = Pos.Center(),
				Y = Pos.Bottom(SplitScore) + 1,
				Width = 17,
				Height = 16,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				Text = ""
			};

			Add(Name, MainBet, MainScore, SplitBet, SplitScore, BottomPanel);
		}

		public void SetName(string name) => Name.Text = $" {name} ";
	}

	internal class CompactPlayerView : FrameView
	{
		FrameView MainBet;
		FrameView MainScore;
		FrameView SplitBet;
		FrameView SplitScore;
		FrameView MainHand;
		FrameView SplitHand;
		public CompactPlayerView(string _id = "0", string name = "un-occupied")
		{
			Id = _id;
			//Title = name;
			Width = Dim.Fill();
			Height = Dim.Fill();
			X = 0;
			Y = 0;
			Width = 40;
			Height = 8;
			TextAlignment = Alignment.Center;
			CanFocus = false;

			MainBet = new FrameView {
				X = 0,
				Y = 0,
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "Main Bet : [ 99999 ]"
			};
			MainScore = new FrameView {
				X = 0,
				Y = Pos.Bottom(MainBet),
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "Score : [ Blackjack! ]"
			};
			MainHand = new FrameView {
				X = 0,
				Y = Pos.Bottom(MainScore),
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "10x10x10x10x10x10x10x"
			};
			SplitBet = new FrameView {
				X = 0,
				Y = Pos.Bottom(MainHand),
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "Split Bet : [ 99999 ]"
			};
			SplitScore = new FrameView {
				X = 0,
				Y = Pos.Bottom(SplitBet),
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "Score : [ 7-Card Charlie! ]"
			};
			SplitHand = new FrameView {
				X = 0,
				Y = Pos.Bottom(SplitScore),
				Width = Dim.Percent(100),
				Height = 1,
				BorderStyle = LineStyle.None,
				CanFocus = false,
				TextAlignment = Alignment.Center,
				Text = "10x, 10x, 10x, 10x, 10x, 10x, 10x"
			};

			Add(MainBet, MainScore, MainHand, SplitBet, SplitScore, SplitHand);
		}
	}
}