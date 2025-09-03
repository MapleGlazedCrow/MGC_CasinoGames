using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary.Blackjack {
	public static class BlackjackPayouts {
		public static int Blackjack(float bet) => (int)(bet + 3 * (bet / 2));
		public static int Charlie(int bet) => bet + 4 * (bet / 1);
		public static int Win(int bet) => bet + 1 * (bet / 1);
		public static int Push(int bet) => bet + 0 * (bet / 1);
	}
}
