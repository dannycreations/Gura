using System;
using System.Collections.Generic;
using System.Linq;

public class TutorialStep24 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Map", "MapAdditional" });
		ControlsController.ControlsActions.BlockMouseButtons(true, true, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}

	public override bool IsWaitingHintMessage()
	{
		if (!(InfoMessageController.Instance != null) || !(InfoMessageController.Instance.currentMessage != null) || InfoMessageController.Instance.currentMessage.MessageType != InfoMessageTypes.LevelUp)
		{
			if (!MessageFactory.InfoMessagesQueue.Any((InfoMessage p) => p.MessageType == InfoMessageTypes.LevelUp))
			{
				return false;
			}
		}
		return true;
	}
}
