using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CardGamesLibrary.Security
{
	public static class RoomCodeGenerator
	{
		private const string sAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

		public static string Generate(int byteLength = 5)
		{
			var data = RandomNumberGenerator.GetBytes(byteLength);
			return Base32Encode(data);
		}

		private static string Base32Encode(byte[] data)
		{
			StringBuilder result = new();
			int buffer = data[0];
			int next = 1;
			int bitsLeft = 8;

			while(bitsLeft > 0 || next < data.Length)
			{
				if(bitsLeft < 5)
				{
					if(next < data.Length)
					{
						buffer <<= 8;
						buffer |= data[next++] & 0xFF;
						bitsLeft += 8;
					}
					else
					{
						int pad = 5 - bitsLeft;
						buffer <<= pad;
						bitsLeft += pad;
					}
				}

				int index = 0x1F & (buffer >> (bitsLeft - 5));
				bitsLeft -= 5;
				result.Append(sAlphabet[index]);
			}
			return result.ToString();
		}
	}
}
