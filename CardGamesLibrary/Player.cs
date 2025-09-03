using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	public abstract class Player<THand>
	where THand : Hand, new()
	{
		public required Identifier Id { get; init; }
		public required string Name { get; init; }
		public THand Hand { get; protected set; } = new();
		public int MainBet { get; set; }
		public required int Chips { get; set; }

		public int ColorCode { get; set; } = 37;
		public string FancyName => $"\u001b[{ColorCode}m" + Name + "\u001b[0m";

		public virtual void CleanHand()
		{
			Hand.Clear();
			MainBet = 0;
		}

		public bool TryTakeChips(int amount)
		{
			if(Chips >= amount)
			{
				Chips -= amount;
				return true;
			}
			return false;
		}

		public void AddChips(int amount) => Chips += amount;
	}

	public abstract record DisplayPlayer<THand, TPlayer>
		where THand : Hand, new()
		where TPlayer : Player<THand>
	{
		public string Id { get; }
		public string Name { get; }
		public string Chips { get; }
		public string MainBet { get; }
		public string MainScore { get; }
		public string CompleteVisual { get; }

		[JsonConstructor]
		public DisplayPlayer(string id, string name, string chips, string mainBet, string mainScore, string completeVisual)
		{
			Id = id;
			Name = name;
			Chips = chips;
			MainBet = mainBet;
			MainScore = mainScore;
			CompleteVisual = completeVisual;
		}

		public DisplayPlayer(Player<THand>? p = null) : this(
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
				"n/a" : p.Hand.ToString())
		{ }
	}
}
