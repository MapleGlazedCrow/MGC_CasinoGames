using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	public class LobbyInformation
	{
		public int LobbySize { get; init; }
		public LobbyGame Game { get; init; }

	}

	public enum LobbyGame
	{
		BLACKJACK,
	}
}
