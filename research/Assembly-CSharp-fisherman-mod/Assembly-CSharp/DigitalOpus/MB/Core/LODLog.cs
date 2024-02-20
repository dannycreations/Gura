using System;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class LODLog
	{
		public LODLog(short bufferSize)
		{
			this.logMessages = new string[(int)bufferSize];
		}

		public void Log(MB2_LogLevel l, string msg, MB2_LogLevel currentThreshold)
		{
			MB2_Log.Log(l, msg, currentThreshold);
			if (this.logMessages.Length == 0)
			{
				return;
			}
			if (l <= currentThreshold)
			{
				this.logMessages[this.pos] = string.Format("frm={0} {1} {2}", Time.frameCount, l, msg);
				this.pos++;
				if (this.pos >= this.logMessages.Length)
				{
					this.pos = 0;
				}
			}
		}

		public string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			if (this.logMessages == null || this.logMessages.Length < 1)
			{
				return string.Empty;
			}
			if (this.logMessages[this.logMessages.Length - 1] != null)
			{
				num = this.pos;
			}
			for (int i = 0; i < this.logMessages.Length; i++)
			{
				int num2 = (num + i) % this.logMessages.Length;
				if (this.logMessages[num2] == null)
				{
					break;
				}
				stringBuilder.AppendLine(this.logMessages[num2]);
			}
			return stringBuilder.ToString();
		}

		private int pos;

		private string[] logMessages;
	}
}
