using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardGamesLibrary.Blackjack
{
	public class Player : CardGamesLibrary.Player<PlayerHand>
	{
		public int SplitBet { get; set; }
		public int InsurranceBet { get; set; }

		public override void CleanHand()
		{
			SplitBet = 0;
			InsurranceBet = 0;
			base.CleanHand();
		}
	}

	public record DisplayPlayer : DisplayPlayer<PlayerHand, Blackjack.Player>
	{
		public bool IsNull { get; }
		public string SplitBet { get; }
		public string SplitScore { get; }
		public string InsurranceBet { get; }

		[JsonConstructor]
		public DisplayPlayer(bool isNull, string id, string name, string chips, string mainBet, string mainScore, string completeVisual, string splitBet, string splitScore, string insurranceBet)
			: base(id, name, chips, mainBet, mainScore, completeVisual)
		{
			IsNull = isNull;
			SplitBet = splitBet;
			SplitScore = splitScore;
			InsurranceBet = insurranceBet;
		}

		public DisplayPlayer(Blackjack.Player? p = null) : this(
			isNull: p is null,
			id: p is null ?
				"n/a" : p.Id.ToString(),
			name: p is null ?
				"Empty" : p.Name,
			chips: p is null ?
				"n/a" : p.Chips.ToString(),
			mainBet: p is null ?
				"n/a" : p.MainBet.ToString(),
			mainScore: p is null ?
				"n/a" : p.Hand.PrintHandScore(),
			completeVisual: p is null ?
				"n/a" : p.Hand.ToString(),
			splitBet: p is null ?
				"n/a" : p.SplitBet.ToString(),
			splitScore: p is null ?
				"n/a" : p.Hand.PrintSplitScore(),
			insurranceBet: p is null ?
				"n/a" : p.InsurranceBet.ToString()
		)
		{ }
	}
}
