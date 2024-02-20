using System;
using System.Runtime.InteropServices;

namespace Ovr
{
	public struct DistortionMesh
	{
		internal DistortionMesh(DistortionMesh_Raw raw)
		{
			this.VertexCount = raw.VertexCount;
			this.pVertexData = new DistortionVertex[this.VertexCount];
			this.IndexCount = raw.IndexCount;
			this.pIndexData = new short[this.IndexCount];
			Type typeFromHandle = typeof(DistortionVertex);
			int num = Marshal.SizeOf(typeFromHandle);
			int num2 = 2;
			long num3 = raw.pVertexData.ToInt64();
			long num4 = raw.pIndexData.ToInt64();
			int num5 = 0;
			while ((long)num5 < (long)((ulong)raw.VertexCount))
			{
				this.pVertexData[num5] = (DistortionVertex)Marshal.PtrToStructure(new IntPtr(num3), typeFromHandle);
				num3 += (long)num;
				num5++;
			}
			int num6 = 0;
			while ((long)num6 < (long)((ulong)raw.IndexCount))
			{
				this.pIndexData[num6] = Marshal.ReadInt16(new IntPtr(num4));
				num4 += (long)num2;
				num6++;
			}
		}

		public DistortionVertex[] pVertexData;

		public short[] pIndexData;

		public uint VertexCount;

		public uint IndexCount;
	}
}
