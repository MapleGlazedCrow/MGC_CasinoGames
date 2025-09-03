internal class Program
{
	static async Task Main()
	{

		Dictionary<string, string> arguments = [];

		try
		{
			Console.WriteLine("Specify the listening port :");
			arguments.Add("port", Console.ReadLine() ?? "");
			Console.WriteLine("Indicate the size of the lobby :");
			arguments.Add("seats", Console.ReadLine() ?? "");

			Console.WriteLine("Specify game type among these :" +
				"\n- blackjack" +
				"\n"
			);

			switch((Console.ReadLine() ?? "").ToUpperInvariant())
			{
				case "BLACKJACK":
					Console.Clear();
					await new BlackjackServer(
						port: int.Parse(arguments["port"]),
						seats: int.Parse(arguments["seats"])
					).StartAsync(new CancellationTokenSource().Token);
					break;

				default:
					throw new NotImplementedException("Game type not recognized");
			}
		}
		catch(Exception ex)
		{
			Console.Error.WriteLine(ex);
		}
	}
}
