using System;
using System.IO;
using System.Text;
using Unity.IO.Compression;

namespace ObjectModel
{
	public static class CompressHelper
	{
		public static byte[] CompressString(string value)
		{
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
				{
					byte[] bytes = Encoding.Unicode.GetBytes(value);
					gzipStream.Write(bytes, 0, bytes.Length);
					gzipStream.Flush();
					gzipStream.Close();
					array = memoryStream.ToArray();
				}
			}
			return array;
		}

		public static string DecompressString(byte[] value)
		{
			string @string;
			using (MemoryStream memoryStream = new MemoryStream(value))
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					using (MemoryStream memoryStream2 = new MemoryStream())
					{
						CompressHelper.CopyTo(gzipStream, memoryStream2);
						byte[] array = memoryStream2.ToArray();
						@string = Encoding.Unicode.GetString(array);
					}
				}
			}
			return @string;
		}

		public static byte[] CompressArray(byte[] value)
		{
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
				{
					gzipStream.Write(value, 0, value.Length);
					gzipStream.Flush();
					gzipStream.Close();
					array = memoryStream.ToArray();
				}
			}
			return array;
		}

		public static byte[] DecompressArray(byte[] value)
		{
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream(value))
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					using (MemoryStream memoryStream2 = new MemoryStream())
					{
						CompressHelper.CopyTo(gzipStream, memoryStream2);
						array = memoryStream2.ToArray();
					}
				}
			}
			return array;
		}

		private static void CopyTo(GZipStream inflator, MemoryStream inflatedStream)
		{
			byte[] array = new byte[1024];
			int num;
			while ((num = inflator.Read(array, 0, 1024)) > 0)
			{
				inflatedStream.Write(array, 0, num);
			}
			inflatedStream.Flush();
		}

		private const int BufferLength = 1024;
	}
}
