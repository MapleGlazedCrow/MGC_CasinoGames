using CardGamesLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	public abstract class Hand()
	{
		protected readonly List<Card> cards = [];
		protected int handScore = 0;

		protected string visualRep = CleanHand.ToString();

		public int HandScore => handScore;
		public int CardCount => cards.Count;

		/// <inheritdoc/>
		public override string ToString() => visualRep;

		/// <summary>
		/// Returns a string of symbols and suits that represents the current object.
		/// </summary>
		/// <returns>A string of symbols and suits that represents the current object.</returns>
		public string HandToCompactString()
		{
			string[] cardGlyphs = [];
			foreach(var card in cards)
			{
				cardGlyphs = [.. cardGlyphs, card.ToCompactString(true)];
			}
			return string.Join(", ", cardGlyphs);
		}

		/// <summary>
		/// Adds the provided <paramref name="card"/> to the current objet.
		/// </summary>
		/// <param name="card">The <see cref="Card"/> to be added to the current object.</param>
		public void AddCardToHand(Card card)
		{
			cards.Add(card);
			UpdateHandVisual(card);
			handScore = CumulateScore([.. cards]);
		}

		/// <summary>
		/// Modifies the visual representation of the current object to add the provided <paramref name="card"/> to the display.
		/// </summary>
		/// <param name="card">The <see cref="Card"/> to be displayed.</param>
		protected abstract void UpdateHandVisual(Card card);

		public virtual void Clear()
		{
			cards.Clear();
			handScore = 0;
			visualRep = CleanHand.ToString();
		}

		/// <summary>
		/// Calculates the total score of the provided <paramref name="Cards"/>.
		/// </summary>
		/// <param name="Cards"></param>
		/// <returns></returns>
		protected abstract int CumulateScore(Card[] Cards);

		public abstract string PrintHandScore();

		protected static string CleanHand =>
			"\n" +
			"\n" +
			"\n";
	}
}
