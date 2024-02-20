using System;

namespace DeltaDNA
{
	public class PopupEventArgs : EventArgs
	{
		public PopupEventArgs(string id, string type, string value)
		{
			this.ID = id;
			this.ActionType = type;
			this.ActionValue = value;
		}

		public string ID { get; set; }

		public string ActionType { get; set; }

		public string ActionValue { get; set; }
	}
}
