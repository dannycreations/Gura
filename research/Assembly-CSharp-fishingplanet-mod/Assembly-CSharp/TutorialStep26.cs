using System;

public class TutorialStep26 : TutorialStep
{
	public override void DoEndAction()
	{
		base.IsActive = false;
		PhotonConnectionFactory.Instance.FinishTutorial();
	}
}
