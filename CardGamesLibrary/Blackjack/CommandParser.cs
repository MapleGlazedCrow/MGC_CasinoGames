using CardGamesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary.Blackjack
{
	public static class CommandParser
	{
		private static readonly Dictionary<string, GameCommandType> map = new(StringComparer.OrdinalIgnoreCase) {
			{"BET", GameCommandType.Bet},
			{"HIT", GameCommandType.Hit},
			{"STAND", GameCommandType.Stand},
			{"DOUBLE", GameCommandType.Double},
			{"SPLIT", GameCommandType.Split},
			{"SURRENDER", GameCommandType.Surrender},
			{"REFILL", GameCommandType.Refill},
			{"MSG", GameCommandType.Chat},
			{"SKIP", GameCommandType.Skip},
		};

		public static bool TryParse(string input, out GameCommand command, out string? error)
		{
			command = null!;
			error = null!;

			var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
			if(parts.Length == 0)
			{
				error = "Empty command.";
				return false;
			}

			string keyword = parts[0].ToUpperInvariant();
			string arg = parts.Length > 1 ? parts[1] : "";
			if(map.TryGetValue(keyword, out GameCommandType type))
			{
				switch(type)
				{
					case GameCommandType.Refill:
					case GameCommandType.Bet:
						if(int.TryParse(arg, out _))
						{
							command = new GameCommand(type, arg);
							return true;
						}
						error = "Command requires a numeric amount.";
						return false;

					case GameCommandType.Hit:
					case GameCommandType.Stand:
					case GameCommandType.Double:
					case GameCommandType.Split:
					case GameCommandType.Surrender:
					case GameCommandType.Skip:
						command = new GameCommand(type);
						return true;

					case GameCommandType.Chat:
						command = new GameCommand(type, arg);
						return true;

					default:
					case GameCommandType.None:
						error = $"Unknown command: {keyword}";
						return false;
				}
			}
			return false;
		}
	}

	public record GameCommand(GameCommandType Type, string? Arg = null);

	public enum GameCommandType : byte
	{
		None = 0x01,
		Bet = 0x02,
		Hit = 0x03,
		Stand = 0x04,
		Double = 0x05,
		Split = 0x06,
		Surrender = 0x07,
		Refill = 0x08,
		Chat = 0x09,
		Skip = 0x10,
	}
}
