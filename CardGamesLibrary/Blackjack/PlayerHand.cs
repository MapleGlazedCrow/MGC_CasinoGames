namespace CardGamesLibrary.Blackjack
{
	public class PlayerHand : Hand
	{
		private int offsetX = 0;
		private int offsetY = 0;

		private readonly List<Card> split = [];
		private int offsetSplitX = 0;
		private int offsetSplitY = 6;
		private int splitScore = 0;

		private bool hasSplit;

		public int SplitScore => splitScore;
		public int SplitCount => split.Count;
		public bool HasSplit => hasSplit;

		protected new string visualRep = CleanHand.ToString();
		public override string ToString() => visualRep;

		public override void Clear()
		{
			cards.Clear();
			handScore = 0;
			offsetX = 0;
			offsetY = 0;
			split.Clear();
			splitScore = 0;
			offsetSplitX = 0;
			offsetSplitY = 6;
			visualRep = CleanHand.ToString();
		}

		public override string PrintHandScore()
		{
			if(CardCount == 7 && HandScore <= 21)
				return "7-Card Charlie!";
			else if(!hasSplit && CardCount == 2 && HandScore == 21)
				return "Blackjack!";
			else if(HandScore > 21)
				return $"Bust! ({HandScore})";
			else return HandScore == -1 ? "Surrender!" : $"{HandScore}";
		}

		public string PrintSplitScore()
		{
			if(SplitCount == 7 && SplitScore <= 21)
				return "7-Card Charlie!";
			else if(HandScore > 21)
				return $"Bust! ({SplitScore})";
			else
				return $"{SplitScore}";
		}

		protected override void UpdateHandVisual(Card card)
		{
			string parts = card.ToString(true);
			string[] build = [.. visualRep.Split('\n', 16).Reverse()];
			for(int i = 0; i < 4; i++)
			{
				build[i + offsetY] = build[i + offsetY][0..(offsetX * 2)] + parts[((3 - i) * 5)..((4 - i) * 5)] + build[i + offsetY][(offsetX * 2 + 5)..];
			}

			offsetX++;
			offsetY++;

			visualRep = string.Join('\n', build.Reverse());
		}

		public string SplitToCompactString()
		{
			string[] cardGlyphs = [];
			foreach(var card in split)
			{
				cardGlyphs = [.. cardGlyphs, card.ToCompactString(true)];
			}
			return string.Join(", ", cardGlyphs);
		}

		public void AddCardToSplit(Card card)
		{
			split.Add(card);
			UpdateSplitVisual(card);
			splitScore = CumulateScore([.. split]);
		}

		private void UpdateSplitVisual(Card card)
		{
			string parts = card.ToString(true);
			string[] build = [.. visualRep.Split('\n', 16).Reverse()];
			for(int i = 0; i < 4; i++)
			{
				build[i + offsetSplitY] = build[i + offsetSplitY][0..(offsetSplitX * 2)] + parts[((3 - i) * 5)..((4 - i) * 5)] + build[i + offsetSplitY][(offsetSplitX * 2 + 5)..];
			}

			offsetSplitX++;
			offsetSplitY++;

			visualRep = string.Join('\n', build.Reverse());
		}

		public void SplitCards()
		{
			if(hasSplit)
				throw new Exception("This hand has already been split.");
			if(cards.Count > 2)
				throw new Exception("This hand can no longer be split.");
			if(cards[0].Symbol != cards[1].Symbol)
				throw new Exception("This hand is not a pair.");

			hasSplit = true;
			Card it = cards[1];
			cards.RemoveAt(1);
			AddCardToSplit(it);

			offsetX--;
			offsetY--;
		}

		public bool TrySurrender()
		{
			if(hasSplit || CardCount > 2)
			{
				return false;
			}
			else
			{
				handScore = -1;
				return true;
			}
		}

		protected override int CumulateScore(Card[] Cards)
		{
			int total = 0;
			int aces = 0;
			foreach(Card card in Cards)
			{
				switch(card.Symbol)
				{
					case 13:
					case 12:
					case 11:
					{
						total += 10;
						break;
					}
					case 1:
					{
						total += 11;
						++aces;
						break;
					}
					default:
					{
						total += card.Symbol;
						break;
					}
				}
			}

			while(total > 21 && aces-- > 0)
			{
				total -= 10;
			}

			return total;
		}

		protected static new string CleanHand =>
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n" +
			"                 \n";
	}
}
