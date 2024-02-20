using System;

namespace Unity.IO.Compression
{
	internal class OutputBuffer
	{
		internal void UpdateBuffer(byte[] output)
		{
			this.byteBuffer = output;
			this.pos = 0;
		}

		internal int BytesWritten
		{
			get
			{
				return this.pos;
			}
		}

		internal int FreeBytes
		{
			get
			{
				return this.byteBuffer.Length - this.pos;
			}
		}

		internal void WriteUInt16(ushort value)
		{
			this.byteBuffer[this.pos++] = (byte)value;
			this.byteBuffer[this.pos++] = (byte)(value >> 8);
		}

		internal void WriteBits(int n, uint bits)
		{
			this.bitBuf |= bits << this.bitCount;
			this.bitCount += n;
			if (this.bitCount >= 16)
			{
				this.byteBuffer[this.pos++] = (byte)this.bitBuf;
				this.byteBuffer[this.pos++] = (byte)(this.bitBuf >> 8);
				this.bitCount -= 16;
				this.bitBuf >>= 16;
			}
		}

		internal void FlushBits()
		{
			while (this.bitCount >= 8)
			{
				this.byteBuffer[this.pos++] = (byte)this.bitBuf;
				this.bitCount -= 8;
				this.bitBuf >>= 8;
			}
			if (this.bitCount > 0)
			{
				this.byteBuffer[this.pos++] = (byte)this.bitBuf;
				this.bitBuf = 0U;
				this.bitCount = 0;
			}
		}

		internal void WriteBytes(byte[] byteArray, int offset, int count)
		{
			if (this.bitCount == 0)
			{
				Array.Copy(byteArray, offset, this.byteBuffer, this.pos, count);
				this.pos += count;
			}
			else
			{
				this.WriteBytesUnaligned(byteArray, offset, count);
			}
		}

		private void WriteBytesUnaligned(byte[] byteArray, int offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				byte b = byteArray[offset + i];
				this.WriteByteUnaligned(b);
			}
		}

		private void WriteByteUnaligned(byte b)
		{
			this.WriteBits(8, (uint)b);
		}

		internal int BitsInBuffer
		{
			get
			{
				return this.bitCount / 8 + 1;
			}
		}

		internal OutputBuffer.BufferState DumpState()
		{
			OutputBuffer.BufferState bufferState;
			bufferState.pos = this.pos;
			bufferState.bitBuf = this.bitBuf;
			bufferState.bitCount = this.bitCount;
			return bufferState;
		}

		internal void RestoreState(OutputBuffer.BufferState state)
		{
			this.pos = state.pos;
			this.bitBuf = state.bitBuf;
			this.bitCount = state.bitCount;
		}

		private byte[] byteBuffer;

		private int pos;

		private uint bitBuf;

		private int bitCount;

		internal struct BufferState
		{
			internal int pos;

			internal uint bitBuf;

			internal int bitCount;
		}
	}
}
