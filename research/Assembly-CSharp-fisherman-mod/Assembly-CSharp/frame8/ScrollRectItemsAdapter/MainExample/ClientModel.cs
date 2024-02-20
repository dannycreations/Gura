using System;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	public class ClientModel
	{
		public float AverageScore01
		{
			get
			{
				return (this.availability01 + this.contractChance01 + this.longTermClient01) / 3f;
			}
		}

		public int avatarImageId;

		public int[] friendsAvatarIds;

		public string clientName;

		public string location;

		public float availability01;

		public float contractChance01;

		public float longTermClient01;

		public bool isOnline;

		public bool expanded;

		public float nonExpandedSize;
	}
}
