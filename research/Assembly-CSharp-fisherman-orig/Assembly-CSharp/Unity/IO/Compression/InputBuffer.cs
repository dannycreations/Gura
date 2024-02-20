using System;

namespace Unity.IO.Compression
{
	internal class InputBuffer
	{
		public int AvailableBits
		{
			get
			{
				return this.bitsInBuffer;
			}
		}

		public int AvailableBytes
		{
			get
			{
				return this.end - this.start + this.bitsInBuffer / 8;
			}
		}

		public bool EnsureBitsAvailable(int count)
		{
			if (this.bitsInBuffer < count)
			{
				if (this.NeedsInput())
				{
					return false;
				}
				this.bitBuffer |= (uint)((uint)this.buffer[this.start++] << (this.bitsInBuffer & 31));
				this.bitsInBuffer += 8;
				if (this.bitsInBuffer < count)
				{
					if (this.NeedsInput())
					{
						return false;
					}
					this.bitBuffer |= (uint)((uint)this.buffer[this.start++] << (this.bitsInBuffer & 31));
					this.bitsInBuffer += 8;
				}
			}
			return true;
		}

		public uint TryLoad16Bits()
		{
			if (this.bitsInBuffer < 8)
			{
				if (this.start < this.end)
				{
					this.bitBuffer |= (uint)((uint)this.buffer[this.start++] << (this.bitsInBuffer & 31));
					this.bitsInBuffer += 8;
				}
				if (this.start < this.end)
				{
					this.bitBuffer |= (uint)((uint)this.buffer[this.start++] << (this.bitsInBuffer & 31));
					this.bitsInBuffer += 8;
				}
			}
			else if (this.bitsInBuffer < 16 && this.start < this.end)
			{
				this.bitBuffer |= (uint)((uint)this.buffer[this.start++] << (this.bitsInBuffer & 31));
				this.bitsInBuffer += 8;
			}
			return this.bitBuffer;
		}

		private uint GetBitMask(int count)
		{
			return (1U << count) - 1U;
		}

		public int GetBits(int count)
		{
			if (!this.EnsureBitsAvailable(count))
			{
				return -1;
			}
			int num = (int)(this.bitBuffer & this.GetBitMask(count));
			this.bitBuffer >>= count;
			this.bitsInBuffer -= count;
			return num;
		}

		public int CopyTo(byte[] output, int offset, int length)
		{
			int num = 0;
			while (this.bitsInBuffer > 0 && length > 0)
			{
				output[offset++] = (byte)this.bitBuffer;
				this.bitBuffer >>= 8;
				this.bitsInBuffer -= 8;
				length--;
				num++;
			}
			if (length == 0)
			{
				return num;
			}
			int num2 = this.end - this.start;
			if (length > num2)
			{
				length = num2;
			}
			Array.Copy(this.buffer, this.start, output, offset, length);
			this.start += length;
			return num + length;
		}

		public bool NeedsInput()
		{
			return this.start == this.end;
		}

		public void SetInput(byte[] buffer, int offset, int length)
		{
			this.buffer = buffer;
			this.start = offset;
			this.end = offset + length;
		}

		public void SkipBits(int n)
		{
			this.bitBuffer >>= n;
			this.bitsInBuffer -= n;
		}

		public void SkipToByteBoundary()
		{
			this.bitBuffer >>= this.bitsInBuffer % 8;
			this.bitsInBuffer -= this.bitsInBuffer % 8;
		}

		private byte[] buffer;

		private int start;

		private int end;

		private uint bitBuffer;

		private int bitsInBuffer;
	}
}
