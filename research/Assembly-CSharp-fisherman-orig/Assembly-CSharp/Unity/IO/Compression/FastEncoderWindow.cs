using System;
using System.Diagnostics;

namespace Unity.IO.Compression
{
	internal class FastEncoderWindow
	{
		public FastEncoderWindow()
		{
			this.ResetWindow();
		}

		public int BytesAvailable
		{
			get
			{
				return this.bufEnd - this.bufPos;
			}
		}

		public DeflateInput UnprocessedInput
		{
			get
			{
				return new DeflateInput
				{
					Buffer = this.window,
					StartIndex = this.bufPos,
					Count = this.bufEnd - this.bufPos
				};
			}
		}

		public void FlushWindow()
		{
			this.ResetWindow();
		}

		private void ResetWindow()
		{
			this.window = new byte[16646];
			this.prev = new ushort[8450];
			this.lookup = new ushort[2048];
			this.bufPos = 8192;
			this.bufEnd = this.bufPos;
		}

		public int FreeWindowSpace
		{
			get
			{
				return 16384 - this.bufEnd;
			}
		}

		public void CopyBytes(byte[] inputBuffer, int startIndex, int count)
		{
			Array.Copy(inputBuffer, startIndex, this.window, this.bufEnd, count);
			this.bufEnd += count;
		}

		public void MoveWindows()
		{
			Array.Copy(this.window, this.bufPos - 8192, this.window, 0, 8192);
			for (int i = 0; i < 2048; i++)
			{
				int num = (int)(this.lookup[i] - 8192);
				if (num <= 0)
				{
					this.lookup[i] = 0;
				}
				else
				{
					this.lookup[i] = (ushort)num;
				}
			}
			for (int i = 0; i < 8192; i++)
			{
				long num2 = (long)this.prev[i] - 8192L;
				if (num2 <= 0L)
				{
					this.prev[i] = 0;
				}
				else
				{
					this.prev[i] = (ushort)num2;
				}
			}
			this.bufPos = 8192;
			this.bufEnd = this.bufPos;
		}

		private uint HashValue(uint hash, byte b)
		{
			return (hash << 4) ^ (uint)b;
		}

		private uint InsertString(ref uint hash)
		{
			hash = this.HashValue(hash, this.window[this.bufPos + 2]);
			uint num = (uint)this.lookup[(int)((UIntPtr)(hash & 2047U))];
			this.lookup[(int)((UIntPtr)(hash & 2047U))] = (ushort)this.bufPos;
			this.prev[this.bufPos & 8191] = (ushort)num;
			return num;
		}

		private void InsertStrings(ref uint hash, int matchLen)
		{
			if (this.bufEnd - this.bufPos <= matchLen)
			{
				this.bufPos += matchLen - 1;
			}
			else
			{
				while (--matchLen > 0)
				{
					this.InsertString(ref hash);
					this.bufPos++;
				}
			}
		}

		internal bool GetNextSymbolOrMatch(Match match)
		{
			uint num = this.HashValue(0U, this.window[this.bufPos]);
			num = this.HashValue(num, this.window[this.bufPos + 1]);
			int num2 = 0;
			int num3;
			if (this.bufEnd - this.bufPos <= 3)
			{
				num3 = 0;
			}
			else
			{
				int num4 = (int)this.InsertString(ref num);
				if (num4 != 0)
				{
					num3 = this.FindMatch(num4, out num2, 32, 32);
					if (this.bufPos + num3 > this.bufEnd)
					{
						num3 = this.bufEnd - this.bufPos;
					}
				}
				else
				{
					num3 = 0;
				}
			}
			if (num3 < 3)
			{
				match.State = MatchState.HasSymbol;
				match.Symbol = this.window[this.bufPos];
				this.bufPos++;
			}
			else
			{
				this.bufPos++;
				if (num3 <= 6)
				{
					int num5 = 0;
					int num6 = (int)this.InsertString(ref num);
					int num7;
					if (num6 != 0)
					{
						num7 = this.FindMatch(num6, out num5, (num3 >= 4) ? 8 : 32, 32);
						if (this.bufPos + num7 > this.bufEnd)
						{
							num7 = this.bufEnd - this.bufPos;
						}
					}
					else
					{
						num7 = 0;
					}
					if (num7 > num3)
					{
						match.State = MatchState.HasSymbolAndMatch;
						match.Symbol = this.window[this.bufPos - 1];
						match.Position = num5;
						match.Length = num7;
						this.bufPos++;
						num3 = num7;
						this.InsertStrings(ref num, num3);
					}
					else
					{
						match.State = MatchState.HasMatch;
						match.Position = num2;
						match.Length = num3;
						num3--;
						this.bufPos++;
						this.InsertStrings(ref num, num3);
					}
				}
				else
				{
					match.State = MatchState.HasMatch;
					match.Position = num2;
					match.Length = num3;
					this.InsertStrings(ref num, num3);
				}
			}
			if (this.bufPos == 16384)
			{
				this.MoveWindows();
			}
			return true;
		}

		private int FindMatch(int search, out int matchPos, int searchDepth, int niceLength)
		{
			int num = 0;
			int num2 = 0;
			int num3 = this.bufPos - 8192;
			byte b = this.window[this.bufPos];
			while (search > num3)
			{
				if (this.window[search + num] == b)
				{
					int i;
					for (i = 0; i < 258; i++)
					{
						if (this.window[this.bufPos + i] != this.window[search + i])
						{
							break;
						}
					}
					if (i > num)
					{
						num = i;
						num2 = search;
						if (i > 32)
						{
							break;
						}
						b = this.window[this.bufPos + i];
					}
				}
				if (--searchDepth == 0)
				{
					break;
				}
				search = (int)this.prev[search & 8191];
			}
			matchPos = this.bufPos - num2 - 1;
			if (num == 3 && matchPos >= 16384)
			{
				return 0;
			}
			return num;
		}

		[Conditional("DEBUG")]
		private void VerifyHashes()
		{
			for (int i = 0; i < 2048; i++)
			{
				ushort num = this.lookup[i];
				while (num != 0 && this.bufPos - (int)num < 8192)
				{
					ushort num2 = this.prev[(int)(num & 8191)];
					if (this.bufPos - (int)num2 >= 8192)
					{
						break;
					}
					num = num2;
				}
			}
		}

		private uint RecalculateHash(int position)
		{
			return (uint)((((int)this.window[position] << 8) ^ ((int)this.window[position + 1] << 4) ^ (int)this.window[position + 2]) & 2047);
		}

		private byte[] window;

		private int bufPos;

		private int bufEnd;

		private const int FastEncoderHashShift = 4;

		private const int FastEncoderHashtableSize = 2048;

		private const int FastEncoderHashMask = 2047;

		private const int FastEncoderWindowSize = 8192;

		private const int FastEncoderWindowMask = 8191;

		private const int FastEncoderMatch3DistThreshold = 16384;

		internal const int MaxMatch = 258;

		internal const int MinMatch = 3;

		private const int SearchDepth = 32;

		private const int GoodLength = 4;

		private const int NiceLength = 32;

		private const int LazyMatchThreshold = 6;

		private ushort[] prev;

		private ushort[] lookup;
	}
}
