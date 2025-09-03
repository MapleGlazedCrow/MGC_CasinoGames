using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackDealer
{
	internal static class Logger
	{
		public static void LogError(string m) => Console.WriteLine($"\x1b[31m{DateTime.Now} | {m}\x1b[0m");

		public static void LogWarning(string m) => Console.WriteLine($"\x1b[33m{DateTime.Now} | {m}\x1b[0m");

		public static void LogInfo(string m) => Console.WriteLine($"\x1b[34m{DateTime.Now} | {m}\x1b[0m");

		public static void LogNormal(string m) => Console.WriteLine($"\x1b[37m{DateTime.Now} | {m}\x1b[0m");
	}
}
