using System;

namespace ObjectModel
{
	public class RoomPopulation
	{
		public string RoomId { get; set; }

		public int PlayerCount { get; set; }

		public int LanguageId { get; set; }

		public bool IsPrivate { get; set; }

		public string[] FriendList { get; set; }
	}
}
