using System;

public class PlayerProfileInit : UserShowProfile
{
	public override void ShowPlayerProfile()
	{
		if (this._clicked == null)
		{
			base.RequestById(this.parentFishTopItem.UserId);
		}
	}

	public ConcreteTop parentFishTopItem;
}
