namespace CardGamesLibrary {
	public class Card(int _num, int _color) {
		protected readonly int symbol = _num;
		protected readonly int suit = _color;

		public int Symbol { get { return symbol; } }
		public int Suit { get { return suit; } }

		public string ToString(bool value) => value ? GetFace() : GetBack();

		private string GetFace() =>
			$" ___ " +
			$"|{FaceSymbols[symbol]} |" +
			$"|{FaceSuits[suit]}  |" +
			$"|___|";

		private static string GetBack() =>
			$" ___ " +
			$"|   |" +
			$"|♦♦♦|" +
			$"|___|";

		public string ToCompactString(bool value) => value ?
				$"{FaceSymbols[symbol].Trim()}{FaceSuits[suit]}" :
				$"???";

		private static readonly Dictionary<int, string> FaceSymbols = new() {
			{ 0, "  " },
			{ 1, "A " },
			{ 2, "2 " },
			{ 3, "3 " },
			{ 4, "4 " },
			{ 5, "5 " },
			{ 6, "6 " },
			{ 7, "7 " },
			{ 8, "8 " },
			{ 9, "9 " },
			{ 10, "10" },
			{ 11, "J " },
			{ 12, "Q " },
			{ 13, "K " },
			{ 14, "𝕁 " },
			{ 15, "𝐉 " }
		};
		private static readonly Dictionary<int, string> FaceSuits = new() {
			{ 0, " " },
			{ 1, "♠" },
			{ 2, "♥" },
			{ 3, "♦" },
			{ 4, "♣" },
			{ 5, "𝕂" },
			{ 6, "𝐊" }
		};
	}

	public enum Symbols {
		BLANK = 0,
		ACE = 1,
		TWO = 2,
		THREE = 3,
		FOUR = 4,
		FIVE = 5,
		SIX = 6,
		SEVEN = 7,
		HEIGHT = 8,
		NINE = 9,
		TEN = 10,
		JACK = 11,
		QUEEN = 12,
		KING = 13,
		JOKER_W = 14,
		JOKER_B = 15
	}
}
