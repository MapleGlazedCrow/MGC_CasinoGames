using System.Net.Sockets;
using System.Text;
using Terminal.Gui;

namespace CasinoPlayerClient
{
	abstract class GameClient(NetworkStream stream, FrameView view, CancellationToken ct)
	{

		protected NetworkStream Stream { get; } = stream;
		protected FrameView View { get; } = view;
		protected CancellationToken CT { get; } = ct;

		public abstract void Start();

		protected virtual async Task ReceiveLoop()
		{
			byte[] lengthBuffer = new byte[4];

			while(!CT.IsCancellationRequested)
			{
				int BytesRead = await Stream.ReadAsync(lengthBuffer);
				if(BytesRead == 0) continue;

				if(BitConverter.IsLittleEndian)
				{
					Array.Reverse(lengthBuffer);
				}

				int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

				var buffer = new byte[messageLength];
				int totalRead = 0;

				while(totalRead < messageLength)
				{
					int read = await Stream.ReadAsync(buffer, totalRead, messageLength - totalRead);
					if(read == 0) break;
					totalRead += read;
				}

				string json = Encoding.UTF8.GetString(buffer, 0, totalRead);

				MessageBox.Query("Crtl+Q to continue", json);
			}
		}
	}

	abstract class GameClient<TView>(NetworkStream stream, TView view, CancellationToken ct) : GameClient(stream, view, ct) where TView : FrameView
	{
		protected new TView View { get; } = view;
	}
}
