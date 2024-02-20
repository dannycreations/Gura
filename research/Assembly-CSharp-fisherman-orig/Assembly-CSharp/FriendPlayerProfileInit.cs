using System;

public class FriendPlayerProfileInit : UserShowProfile
{
	public override void ShowPlayerProfile()
	{
		if (this._clicked == null)
		{
			base.RequestById(this.friendListItem.UserId);
		}
	}

	public FriendListItemBase friendListItem;
}
