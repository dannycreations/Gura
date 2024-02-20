using System;

public class RoomsSubMenu : SubMenuFoldoutBase
{
	public override void SetOpened(bool opened, Action callback = null)
	{
		if (opened)
		{
			ShowLocationInfo.Instance.OpenRoomsSubMenu();
		}
		base.SetOpened(opened, callback);
		if (!opened)
		{
			ShowLocationInfo.Instance.CloseWindow();
		}
	}
}
