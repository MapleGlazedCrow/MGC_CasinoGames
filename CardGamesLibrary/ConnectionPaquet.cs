namespace CardGamesLibrary
{
	public class ConnectionPaquet(Identifier id, string name, int chips)
	{
		public Identifier Id { get; private set; } = id;
		public string Name { get; private set; } = name;
		public int Chips { get; private set; } = chips;
	}
}
