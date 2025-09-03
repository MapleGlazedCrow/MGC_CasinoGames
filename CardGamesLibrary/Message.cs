using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	public class Message
	{
		public MessageType Type { get; set; }
		public JsonElement Content { get; set; }
	}

	public enum MessageType
	{
		/// <summary>
		/// default
		/// </summary>
		UNKNOWN,
		/// <summary>
		/// accept connection
		/// </summary>
		ACCEPT,
		/// <summary>
		/// reject connection
		/// </summary>
		REJECT,
		/// <summary>
		/// chat messages
		/// </summary>
		MESSAGE,
		/// <summary>
		/// game event (card draw, player join)
		/// </summary>
		EVENT,
		/// <summary>
		/// info message (not your turn, incorrect command)
		/// </summary>
		INFO,
		/// <summary>
		/// info message but with sound and flash
		/// </summary>
		BELL,
		/// <summary>
		/// like info, but red
		/// </summary>
		ERROR,
		/// <summary>
		/// sends a fraction between 0 and 1 of the time remaining in a timeout
		/// </summary>
		TIME,
		/// <summary>
		/// full gamestate
		/// </summary>
		STATE,
		TURN,
		PHASE,
	}
}

//add new message type, separate dealer update and player update information