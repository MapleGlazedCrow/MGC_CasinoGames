namespace CardGamesLibrary.Blackjack
{
	public class DealerHand() : Hand
	{
		private bool hasHole = false;
		private Card? holeCard = null;

		public void AddCardToHole(Card card)
		{
			holeCard = card;
			hasHole = true;
			UpdateHandVisual(card);
		}

		public void RevealHole(out string str)
		{
			if(hasHole && holeCard is not null)
			{
				RemoveLastFromVisual();
				hasHole = false;
				AddCardToHand(holeCard);
				str = holeCard.ToCompactString(true);
				holeCard = null;
			}
			else
			{
				str = "";
			}
		}

		public override string PrintHandScore()
		{
			if(CardCount == 7 && HandScore <= 21)
			{
				return "7-Card Charlie!";
			}
			else if(CardCount == 2 && HandScore == 21)
			{
				return "Blackjack!";
			}
			else if(HandScore > 21)
			{
				return $"BUST! ({HandScore})";
			}
			else
			{
				return $"{HandScore}";
			}
		}

		protected override void UpdateHandVisual(Card card)
		{
			string parts = card.ToString(!hasHole);
			string[] build = [.. visualRep.Split('\n', 4).Reverse()];
			for(int i = 0; i < 4; i++)
			{
				build[i] += parts[((3 - i) * 5)..((4 - i) * 5)];
			}

			visualRep = string.Join('\n', build.Reverse());
		}

		private void RemoveLastFromVisual()
		{
			string[] build = [.. visualRep.Split('\n', 4).Reverse()];
			for(int i = 0; i < 4; i++)
			{
				build[i] = build[i][..(build[i].Length - 5)];
			}
			visualRep = string.Join('\n', build.Reverse());
		}

		public bool HasBlackjack()
		{
			if(hasHole && holeCard is not null)
				switch(holeCard.Symbol)
				{
					case 13:
					case 12:
					case 11:
					case 10:
						return handScore == 11;
					case 1:
						return handScore == 10;
					default:
						break;
				}
			return false;
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
	}
}
