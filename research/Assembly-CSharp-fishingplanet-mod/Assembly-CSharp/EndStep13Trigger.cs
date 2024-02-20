using System;

public class EndStep13Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		this._isTriggering = TutorialStep.Shop != null && TutorialStep.Shop.ShopContentHasChild;
	}
}
