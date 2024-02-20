using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DeltaDNA
{
	public class EventStore : IDisposable
	{
		public EventStore(string dir)
		{
			Logger.LogInfo("Creating Event Store");
			if (this.InitialiseFileStreams(dir))
			{
				this._initialised = true;
			}
			else
			{
				Logger.LogError("Failed to initialise event store in " + dir);
			}
		}

		public bool Push(string obj)
		{
			object @lock = EventStore._lock;
			bool flag;
			lock (@lock)
			{
				if (!this._initialised)
				{
					Logger.LogError("Event Store not initialised");
					flag = false;
				}
				else
				{
					flag = EventStore.PushEvent(obj, this._infs);
				}
			}
			return flag;
		}

		public bool Swap()
		{
			object @lock = EventStore._lock;
			bool flag;
			lock (@lock)
			{
				if (this._initialised && this._outfs.Length == 0L)
				{
					EventStore.SwapStreams(ref this._infs, ref this._outfs);
					string @string = PlayerPrefs.GetString(EventStore.PF_KEY_IN_FILE);
					string string2 = PlayerPrefs.GetString(EventStore.PF_KEY_OUT_FILE);
					if (string.IsNullOrEmpty(@string))
					{
						Logger.LogError("Event Store corruption, PlayerPrefs in file key is missing");
					}
					else if (string.IsNullOrEmpty(string2))
					{
						Logger.LogError("Event Store corruption, PlayerPrefs out file key is missing");
					}
					else
					{
						PlayerPrefs.SetString(EventStore.PF_KEY_IN_FILE, string2);
						PlayerPrefs.SetString(EventStore.PF_KEY_OUT_FILE, @string);
					}
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		public List<string> Read()
		{
			object @lock = EventStore._lock;
			List<string> list2;
			lock (@lock)
			{
				List<string> list = new List<string>();
				try
				{
					if (this._initialised)
					{
						EventStore.ReadEvents(this._outfs, list);
					}
				}
				catch (Exception ex)
				{
					Logger.LogError("Problem reading events: " + ex.Message);
					EventStore.ClearStream(this._outfs);
					return null;
				}
				list2 = list;
			}
			return list2;
		}

		public void ClearOut()
		{
			object @lock = EventStore._lock;
			lock (@lock)
			{
				if (this._initialised)
				{
					EventStore.ClearStream(this._outfs);
				}
			}
		}

		public void ClearAll()
		{
			object @lock = EventStore._lock;
			lock (@lock)
			{
				if (this._initialised)
				{
					EventStore.ClearStream(this._infs);
					EventStore.ClearStream(this._outfs);
				}
			}
		}

		public void FlushBuffers()
		{
			object @lock = EventStore._lock;
			lock (@lock)
			{
				if (this._initialised)
				{
					this._infs.Flush();
					this._outfs.Flush();
				}
			}
		}

		~EventStore()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			object @lock = EventStore._lock;
			lock (@lock)
			{
				try
				{
					if (!this._disposed && disposing)
					{
						if (this._infs != null)
						{
							this._infs.Dispose();
						}
						if (this._outfs != null)
						{
							this._outfs.Dispose();
						}
					}
				}
				catch (Exception ex)
				{
					Logger.LogError("Failed to dispose EventStore cleanly. " + ex.Message);
				}
				finally
				{
					this._disposed = true;
				}
			}
		}

		private bool InitialiseFileStreams(string dir)
		{
			try
			{
				string text = null;
				string text2 = null;
				string @string = PlayerPrefs.GetString(EventStore.PF_KEY_IN_FILE, EventStore.FILE_A);
				string string2 = PlayerPrefs.GetString(EventStore.PF_KEY_OUT_FILE, EventStore.FILE_B);
				if (!string.IsNullOrEmpty(dir))
				{
					if (!Utils.DirectoryExists(dir))
					{
						Logger.LogDebug("Directory not found, creating " + dir);
						Utils.CreateDirectory(dir);
					}
					text = Path.Combine(dir, @string);
					text2 = Path.Combine(dir, string2);
				}
				this._infs = Utils.CreateStream(text);
				this._infs.Seek(0L, SeekOrigin.End);
				this._outfs = Utils.CreateStream(text2);
				this._outfs.Seek(0L, SeekOrigin.Begin);
				PlayerPrefs.SetString(EventStore.PF_KEY_IN_FILE, @string);
				PlayerPrefs.SetString(EventStore.PF_KEY_OUT_FILE, string2);
				return true;
			}
			catch (Exception ex)
			{
				Logger.LogError("Failed to initialise file stream: " + ex.Message);
			}
			return false;
		}

		public static bool PushEvent(string obj, Stream stream)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(obj);
			byte[] bytes2 = BitConverter.GetBytes(bytes.Length);
			if (stream.Length + (long)bytes.Length < EventStore.MAX_FILE_SIZE_BYTES)
			{
				List<byte> list = new List<byte>();
				list.AddRange(bytes2);
				list.AddRange(bytes);
				byte[] array = list.ToArray();
				stream.Write(array, 0, array.Length);
				return true;
			}
			return false;
		}

		public static void ReadEvents(Stream stream, IList<string> events)
		{
			byte[] array = new byte[4];
			while (stream.Read(array, 0, array.Length) > 0)
			{
				int num = BitConverter.ToInt32(array, 0);
				byte[] array2 = new byte[num];
				stream.Read(array2, 0, array2.Length);
				string @string = Encoding.UTF8.GetString(array2, 0, array2.Length);
				events.Add(@string);
			}
			stream.Seek(0L, SeekOrigin.Begin);
		}

		public static void SwapStreams(ref Stream sin, ref Stream sout)
		{
			sin.Flush();
			Stream stream = sin;
			sin = sout;
			sout = stream;
			sin.Seek(0L, SeekOrigin.Begin);
			sin.SetLength(0L);
			sout.Seek(0L, SeekOrigin.Begin);
		}

		public static void ClearStream(Stream stream)
		{
			stream.Seek(0L, SeekOrigin.Begin);
			stream.SetLength(0L);
		}

		private static readonly string PF_KEY_IN_FILE = "DDSDK_EVENT_IN_FILE";

		private static readonly string PF_KEY_OUT_FILE = "DDSDK_EVENT_OUT_FILE";

		private static readonly string FILE_A = "A";

		private static readonly string FILE_B = "B";

		private static readonly long MAX_FILE_SIZE_BYTES = 1048576L;

		private bool _initialised;

		private bool _disposed;

		private Stream _infs;

		private Stream _outfs;

		private static object _lock = new object();
	}
}
