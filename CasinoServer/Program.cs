using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Terminal.Gui;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;

internal class Program
{
	static async Task Main(string[] args)
	{
		ArgumentNullException.ThrowIfNull(nameof(args));

		Dictionary<string, string> arguments = [];

#if DEBUG
		arguments.Add("port", "9000");
		arguments.Add("seats", "5");
#endif

		try
		{
			foreach(var arg in args)
			{
				if(arg.Contains('='))
				{
					arguments.Add(arg.Split('=')[0].TrimStart('-'), arg.Split('=')[1]);
				}
			}
			await new BlackjackServer(
				port: int.Parse(arguments["port"]),
				seats: int.Parse(arguments["seats"])
			).StartAsync(new CancellationTokenSource().Token);
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
}
