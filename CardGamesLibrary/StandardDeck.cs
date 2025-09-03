namespace CardGamesLibrary {
	public class StandardDeck {
		private readonly Queue<Card> cards;

		/// <summary>
		/// Creates a deck comprised of <paramref name="deckCount"/> combined decks.
		/// <para>These decks do not contain Jokers (<see cref="Card"/>(14,5) and <see cref="Card"/>(15,6))</para>
		/// </summary>
		/// <param name="deckCount">The number of 52 <see cref="Card"/> decks to include</param>
		public StandardDeck(int deckCount)
		{
			cards = [];
			for (int i = 0; i < deckCount; i++)
			{
				foreach (var item in NewDeck)
				{
					cards.Enqueue(item);
				}
			}
		}

		public void ShuffleRoutine()
		{
			FisherYatesShuffle();
			FisherYatesShuffle();
			RiffleSuffle();
		}

		//shuffling
		private void FisherYatesShuffle()
		{
			Random r = new();
			List<Card> temp = [.. cards];
			cards.Clear();
			while (temp.Count > 0)
			{
				int i = r.Next(0, temp.Count);
				cards.Enqueue(temp[i]);
				temp.RemoveAt(i);
			}
		}

		private void RiffleSuffle()
		{
			Random r = new Random();
			List<Card> A = cards.ToList()[0..^(cards.Count / 2)];
			List<Card> B = cards.ToList()[((cards.Count / 2) + 1)..cards.Count];
			cards.Clear();
			while (A.Count > 0 && B.Count > 0)
			{
				int i = r.Next(0, 2);
				if (r.Next(0, 2) == 0)
				{
					cards.Enqueue(A[0]);
					A.RemoveAt(0);
				}
				else
				{
					cards.Enqueue(B[0]);
					B.RemoveAt(0);
				}
			}

			while (A.Count > 0)
			{
				cards.Enqueue(A[0]);
				A.RemoveAt(0);
			}

			while (B.Count > 0)
			{
				cards.Enqueue(B[0]);
				B.RemoveAt(0);
			}
		}

		//TakeCard
		public Card DrawCard() => cards.Dequeue();

		// put card at back of the deck
		public void PutBack(Card card) => cards.Enqueue(card);

		//insert a cut card
		public void InsertAt(int index, Card card)
		{
			List<Card> temps = [.. cards];
			temps.Insert(index, card);
			cards.Clear();
			foreach (var item in temps)
			{
				cards.Enqueue(item);
			}
		}

		private static Card[] NewDeck => [
			new Card(1, 1),  new Card(1, 2),  new Card(1, 3),  new Card(1, 4),
			new Card(2, 1),  new Card(2, 2),  new Card(2, 3),  new Card(2, 4),
			new Card(3, 1),  new Card(3, 2),  new Card(3, 3),  new Card(3, 4),
			new Card(4, 1),  new Card(4, 2),  new Card(4, 3),  new Card(4, 4),
			new Card(5, 1),  new Card(5, 2),  new Card(5, 3),  new Card(5, 4),
			new Card(6, 1),  new Card(6, 2),  new Card(6, 3),  new Card(6, 4),
			new Card(7, 1),  new Card(7, 2),  new Card(7, 3),  new Card(7, 4),
			new Card(8, 1),  new Card(8, 2),  new Card(8, 3),  new Card(8, 4),
			new Card(9, 1),  new Card(9, 2),  new Card(9, 3),  new Card(9, 4),
			new Card(10, 1), new Card(10, 2), new Card(10, 3), new Card(10, 4),
			new Card(11, 1), new Card(11, 2), new Card(11, 3), new Card(11, 4),
			new Card(12, 1), new Card(12, 2), new Card(12, 3), new Card(12, 4),
			new Card(13, 1), new Card(13, 2), new Card(13, 3), new Card(13, 4)
		];
	}
}