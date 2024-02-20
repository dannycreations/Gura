using System;
using System.Net.Sockets;

namespace Photon.Realtime
{
	public class PingMono : PhotonPing
	{
		public override bool StartPing(string ip)
		{
			base.Init();
			try
			{
				if (ip.Contains("."))
				{
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				}
				else
				{
					this.sock = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				}
				this.sock.ReceiveTimeout = 5000;
				this.sock.Connect(ip, 5055);
				this.PingBytes[this.PingBytes.Length - 1] = this.PingId;
				this.sock.Send(this.PingBytes);
				this.PingBytes[this.PingBytes.Length - 1] = this.PingId - 1;
			}
			catch (Exception ex)
			{
				this.sock = null;
				Console.WriteLine(ex);
			}
			return false;
		}

		public override bool Done()
		{
			if (this.GotResult || this.sock == null)
			{
				return true;
			}
			if (this.sock.Available <= 0)
			{
				return false;
			}
			int num = this.sock.Receive(this.PingBytes, SocketFlags.None);
			if (this.PingBytes[this.PingBytes.Length - 1] != this.PingId || num != this.PingLength)
			{
				this.DebugString += " ReplyMatch is false! ";
			}
			this.Successful = num == this.PingBytes.Length && this.PingBytes[this.PingBytes.Length - 1] == this.PingId;
			this.GotResult = true;
			return true;
		}

		public override void Dispose()
		{
			try
			{
				this.sock.Close();
			}
			catch
			{
			}
			this.sock = null;
		}

		private Socket sock;
	}
}
