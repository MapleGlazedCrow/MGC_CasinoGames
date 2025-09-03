using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CardGamesLibrary
{
	public static class MessageFactory
	{
		public static Message Wrap<T>(MessageType type, T content) =>
			new() { Type = type, Content = JsonSerializer.SerializeToElement(content) };

		public static void ConstructFrame<T>(T content, out byte[] frame)
		{
			string json = JsonSerializer.Serialize(content);

			byte[] data = Encoding.UTF8.GetBytes(json);
			byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

			if(BitConverter.IsLittleEndian)
				Array.Reverse(lengthPrefix);

			frame = new byte[lengthPrefix.Length + data.Length];
			Array.Copy(lengthPrefix, frame, lengthPrefix.Length);
			Array.Copy(data, 0, frame, lengthPrefix.Length, data.Length);
		}

		/// <summary>
		/// Should be in a try-catch
		/// </summary>
		/// <exception cref="IOException"/>
		public static async Task<T?> ReadFrameAsync<T>(NetworkStream stream, int timeout = Timeout.Infinite)
		{
			stream.ReadTimeout = timeout;
			byte[] lengthBuffer = new byte[4];

			int lenghtRead = stream.Read(lengthBuffer);
			if(lenghtRead == 0) return default;

			if(BitConverter.IsLittleEndian)
				Array.Reverse(lengthBuffer);

			int length = BitConverter.ToInt32(lengthBuffer, 0);

			var buffer = new byte[length];
			int totalRead = 0;

			while(totalRead < length)
			{
				int read = await stream.ReadAsync(buffer.AsMemory(totalRead, length - totalRead));
				if(read == 0) break;
				totalRead += read;
			}

			string json = Encoding.UTF8.GetString(buffer, 0, totalRead);
			return JsonSerializer.Deserialize<T>(json);
		}
	}
}
