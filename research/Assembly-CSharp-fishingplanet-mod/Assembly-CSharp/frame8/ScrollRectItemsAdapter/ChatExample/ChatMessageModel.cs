using System;

namespace frame8.ScrollRectItemsAdapter.ChatExample
{
	public class ChatMessageModel
	{
		public DateTime TimestampAsDateTime
		{
			get
			{
				return ChatMessageModel.EPOCH_START_TIME.AddSeconds((double)this.timestampSec).ToLocalTime();
			}
		}

		public string Text
		{
			get
			{
				return this._Text;
			}
			set
			{
				if (this._Text == value)
				{
					return;
				}
				this._Text = value;
				this.HasPendingVisualSizeChange = true;
			}
		}

		public int ImageIndex
		{
			get
			{
				return this._ImageIndex;
			}
			set
			{
				if (this._ImageIndex == value)
				{
					return;
				}
				this._ImageIndex = value;
				this.HasPendingVisualSizeChange = true;
			}
		}

		public bool IsMine { get; set; }

		public bool HasPendingVisualSizeChange { get; set; }

		public static readonly DateTime EPOCH_START_TIME = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public int timestampSec;

		private string _Text;

		private int _ImageIndex;
	}
}
