using System;

[Serializable]
public abstract class EndTutorialTriggerContainer : TutorialTriggerContainer
{
	protected bool IsHudGameObjectActive
	{
		get
		{
			return !(ShowHudElements.Instance == null) && ShowHudElements.Instance.IsActive();
		}
	}
}
