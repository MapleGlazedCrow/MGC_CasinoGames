using CardGamesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackDealer
{
	public class PhaseTimer
	{
		private readonly int _durationSeconds;
		private readonly int _tickIntervalMs;
		private readonly Func<int, Task> _onTick;
		private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

		private DateTime start;

		public PhaseTimer(int durationSeconds, int tickIntervalMs, Func<int, Task> onTick)
		{
			_durationSeconds = durationSeconds;
			_tickIntervalMs = tickIntervalMs;
			_onTick = onTick;
		}

		public async Task<bool> RunAsync(CancellationToken _ct)
		{
			ResetTime();

			try
			{
				while(true)
				{
					var elapsed = (int)(DateTime.UtcNow - start).TotalSeconds;
					var remaining = _durationSeconds - elapsed;
					if(remaining <= 0) break;

					await _onTick(remaining);

					// wait either for tick interval, or until early completion/cancel
					var delay = Task.Delay(_tickIntervalMs, _ct);
					var finished = await Task.WhenAny(delay, _tcs.Task);

					if(finished == _tcs.Task) break;
				}
			}
			catch(TaskCanceledException) { }

			return !_ct.IsCancellationRequested;
		}

		/// <summary>Stops the timer early because all clients acted.</summary>
		public void CompleteEarly() => _tcs.TrySetResult();

		public void ResetTime() => start = DateTime.UtcNow;
	}
}
