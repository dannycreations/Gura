using System;

namespace ObjectModel
{
	public class ChatMessage
	{
		public ChatMessage()
		{
			this.MessageType = LocalEventType.Chat;
			this.AddedDateTime = DateTime.Now;
		}

		public Player Sender { get; set; }

		public Player Recepient { get; set; }

		public string Channel { get; set; }

		public string Group { get; set; }

		public string Message { get; set; }

		public string Data { get; set; }

		public string OneTimeData { get; set; }

		public DateTime? Timestamp { get; set; }

		public string Id { get; set; }

		public DateTime AddedDateTime { get; set; }

		public LocalEventType MessageType { get; set; }

		public bool IsAnswer
		{
			get
			{
				return !string.IsNullOrEmpty(this.Data) && this.Data.StartsWith("Answer");
			}
		}

		public char AnswerType
		{
			get
			{
				return this.Data["Answer".Length];
			}
		}

		public char Answer
		{
			get
			{
				return this.Data["Answer".Length + 1];
			}
		}

		public bool IsRequest
		{
			get
			{
				return string.IsNullOrEmpty(this.Message) && !string.IsNullOrEmpty(this.Data);
			}
		}

		public char RequestType
		{
			get
			{
				return this.Data[0];
			}
		}
	}
}
