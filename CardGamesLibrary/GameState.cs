using CardGamesLibrary.Blackjack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	public abstract class GameState<TDealerHand, TPlayerHand, TPlayer>
		where TDealerHand : Hand, new()
		where TPlayerHand : Hand, new()
		where TPlayer : Player<TPlayerHand>
	{
		protected GameState()
		{
			Players = [];
			Dealer = new();
		}

		public Dictionary<int, TPlayer?> Players { get; protected set; }
		public TDealerHand Dealer { get; protected set; }
		public int CurrentTurn { get; set; } = 0;
		public GamePhase CurrentPhase { get; set; } = GamePhase.PreGame;

		public void ResetDealer() => Dealer = new();
	}

	public record DisplayGameState<THand, TPlayer, TDisplayPlayer>(
		List<TDisplayPlayer> Players,
		DisplayDealer Dealer,
		int CurrentTurn
	)
		where THand : Hand, new()
		where TPlayer : Player<THand>
		where TDisplayPlayer : DisplayPlayer<THand, TPlayer>;

	public record DisplayDealer(string Visual, string Score);
}