using System;
using Assets.Scripts.Common.Managers.Helpers;

public class StartStep26Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (MessageBoxList.Instance.currentMessage == null)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	private MenuHelpers _helper = new MenuHelpers();
}
