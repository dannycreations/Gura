using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
	internal class SocketUdp : IPhotonSocket
	{
		public SocketUdp(PeerBase npeer)
			: base(npeer)
		{
			if (base.ReportDebugOfLevel(5))
			{
				base.Listener.DebugReturn(5, "CSharpSocket: UDP, Unity3d.");
			}
			this.PollReceive = false;
		}

		public override bool Connect()
		{
			object obj = this.syncer;
			bool flag;
			lock (obj)
			{
				if (!base.Connect())
				{
					flag = false;
				}
				else
				{
					base.State = 1;
					new Thread(new ThreadStart(this.DnsAndConnect))
					{
						Name = "photon dns thread",
						IsBackground = true
					}.Start();
					flag = true;
				}
			}
			return flag;
		}

		public override bool Disconnect()
		{
			if (base.ReportDebugOfLevel(3))
			{
				base.EnqueueDebugReturn(3, "CSharpSocket.Disconnect()");
			}
			base.State = 3;
			object obj = this.syncer;
			lock (obj)
			{
				if (this.sock != null)
				{
					try
					{
						this.sock.Close();
					}
					catch (Exception ex)
					{
						base.EnqueueDebugReturn(3, "Exception in Disconnect(): " + ex);
					}
					this.sock = null;
				}
			}
			base.State = 0;
			return true;
		}

		public override PhotonSocketError Send(byte[] data, int length)
		{
			object obj = this.syncer;
			lock (obj)
			{
				if (!this.sock.Connected)
				{
					return 1;
				}
				try
				{
					this.sock.Send(data, 0, length, SocketFlags.None);
				}
				catch
				{
					return 3;
				}
			}
			return 0;
		}

		public override PhotonSocketError Receive(out byte[] data)
		{
			data = null;
			return 2;
		}

		internal void DnsAndConnect()
		{
			try
			{
				object obj = this.syncer;
				lock (obj)
				{
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					IPAddress ipAddress = IPhotonSocket.GetIpAddress(base.ServerAddress);
					this.sock.Connect(ipAddress, base.ServerPort);
					base.State = 2;
				}
			}
			catch (SecurityException ex)
			{
				if (base.ReportDebugOfLevel(1))
				{
					base.Listener.DebugReturn(1, "Connect() failed: " + ex.ToString());
				}
				base.HandleException(1022);
				return;
			}
			catch (Exception ex2)
			{
				if (base.ReportDebugOfLevel(1))
				{
					base.Listener.DebugReturn(1, "Connect() failed: " + ex2.ToString());
				}
				base.HandleException(1023);
				return;
			}
			new Thread(new ThreadStart(this.ReceiveLoop))
			{
				Name = "photon receive thread",
				IsBackground = true
			}.Start();
		}

		public void ReceiveLoop()
		{
			byte[] array = new byte[base.MTU];
			while (base.State == 2)
			{
				try
				{
					int num = this.sock.Receive(array);
					base.HandleReceivedDatagram(array, num, true);
				}
				catch (Exception ex)
				{
					if (base.State != 3 && base.State != null)
					{
						if (base.ReportDebugOfLevel(1))
						{
							base.EnqueueDebugReturn(1, string.Concat(new object[] { "Receive issue. State: ", base.State, " Exception: ", ex }));
						}
						base.HandleException(1039);
					}
				}
			}
			this.Disconnect();
		}

		private Socket sock;

		private readonly object syncer = new object();
	}
}
