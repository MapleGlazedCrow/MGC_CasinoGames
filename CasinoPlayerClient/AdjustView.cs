using Terminal.Gui;

namespace CasinoPlayerClient
{
	class AdjustView : FrameView
	{

		public AdjustView()
		{
			Width = 128;
			Height = 42 + 3;
			X = Pos.Center();
			Y = 0;
			BorderStyle = LineStyle.Dashed;
			ShadowStyle = ShadowStyle.None;
			ColorScheme = Schemes.FullRed;

			var centerBox = new FrameView() {
				Width = 50,
				Height = 16,
				X = Pos.Center(),
				Y = Pos.Center(),
				CanFocus = true,
				BorderStyle = LineStyle.None,
			};

			var directiveBox = new Label() {
				X = Pos.Center(),
				Y = Pos.Center(),
				Width = Dim.Fill(),
				Height = Dim.Percent(50),
				Text = "Please adjust the window\nto fit the frame.\nAdjust text size if needed.",
				TextAlignment = Alignment.Center,
			};

			var ok = new Button() {
				X = Pos.Center(),
				Y = Pos.Bottom(directiveBox),
				Text = "Confirm",
				IsDefault = true,
			};

			ok.Accepting += (_, _) =>
				SuperView?.Remove(this);


			centerBox.Add(directiveBox, ok);
			Add(centerBox);
		}
	}
}
