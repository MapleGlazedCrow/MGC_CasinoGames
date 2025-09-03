using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary.Blackjack
{
	public class GameState : GameState<DealerHand, PlayerHand, Player>
	{
		public GameState(int playerCount) : base()
		{
			Players = Enumerable.Range(1, playerCount)
			.ToDictionary(i => i, _ => (Player?)null);
		}
	}

	public record DisplayGameState(List<Blackjack.DisplayPlayer> Players, DisplayDealer Dealer, int CurrentTurn)
		: CardGamesLibrary.DisplayGameState<PlayerHand, Player, Blackjack.DisplayPlayer>(Players, Dealer, CurrentTurn);
}