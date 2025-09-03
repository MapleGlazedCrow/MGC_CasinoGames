using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace CasinoPlayerClient
{
	public static class Schemes
	{
		public static readonly ColorScheme Default = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Focus = new Terminal.Gui.Attribute(Color.White, Color.Black),
			HotNormal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			HotFocus = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
		};

		public static readonly ColorScheme TextField = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Focus = new Terminal.Gui.Attribute(Color.White, Color.Black),
			HotNormal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			HotFocus = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.DarkGray)
		};

		public static readonly ColorScheme TextBox = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Focus = new Terminal.Gui.Attribute(Color.White, Color.Black),
			HotNormal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			HotFocus = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.White, Color.Black)
		};

		public static readonly ColorScheme TopLevel = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Focus = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
			HotNormal = new Terminal.Gui.Attribute(Color.Yellow, Color.DarkGray),
			HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Yellow),
			Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray),
		};

		public static readonly ColorScheme MenuBar = new() {
			Normal = new Terminal.Gui.Attribute(Color.Black, Color.White),
			Focus = new Terminal.Gui.Attribute(Color.Black, Color.DarkGray),
			HotNormal = new Terminal.Gui.Attribute(Color.Black, Color.White),
			HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.DarkGray),
			Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray),
		};

		public static readonly ColorScheme Button = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Focus = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			HotNormal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			HotFocus = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
		};

		public static readonly ColorScheme Test = new() {
			Normal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Blue),
			Focus = new Terminal.Gui.Attribute(Color.BrightRed, Color.Red),
			HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Cyan),
			HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Yellow),
			Disabled = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Magenta),
		};

		public static readonly ColorScheme FullRed = new() {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Red),
			Focus = new Terminal.Gui.Attribute(Color.White, Color.BrightRed),
			HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Cyan),
			HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Yellow),
			Disabled = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Magenta),
		};

		public static readonly ColorScheme Tabs = new ColorScheme {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.White, Color.Black),
		};

		public static readonly ColorScheme TabLabel = new ColorScheme {
			Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
			Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
		};

		public static readonly ColorScheme TabHasTurn = new ColorScheme {
			Normal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
		};

		/**
		 * Normal : regular everything
		 * Focus : hover or focus (obv)
		 * HotFocus : hotkey indicator when hover (ie. first letter for menus and buttons (ie _Focus ; the F is color))
		 * HotNormal : hotkey ndocator when not hover
		 */
	}
	/*
	public static class CustomBorderStyles
	{
		public static readonly Border roundedTopSharpBottom = new BorderStyle {
			TopLeft = '╭',    // rounded top-left
			TopRight = '╮',   // rounded top-right
			BottomLeft = '└', // sharp bottom-left
			BottomRight = '┘',// sharp bottom-right
			Horizontal = '─',
			Vertical = '│'
		};
	}*/
}
