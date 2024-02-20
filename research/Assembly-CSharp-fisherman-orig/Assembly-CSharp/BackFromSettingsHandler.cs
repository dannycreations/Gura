using System;
using Assets.Scripts.Common.Managers.Helpers;

public class BackFromSettingsHandler : ChangeForm
{
	public void ClickHandler()
	{
		base.ChangeWithShowDashboard(BackFromSettingsHandler.LastMainWindows);
	}

	public static ActivityState LastMainWindows;

	private MenuHelpers helpers = new MenuHelpers();
}
