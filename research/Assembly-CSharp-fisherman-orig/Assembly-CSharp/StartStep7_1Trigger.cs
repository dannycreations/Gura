using System;

public class StartStep7_1Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		this._isTriggering = this.ShowMessage;
		this.ShowMessage = false;
	}

	public bool ShowMessage;
}
