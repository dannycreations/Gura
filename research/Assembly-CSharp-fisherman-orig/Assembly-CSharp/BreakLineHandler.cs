using System;
using I2.Loc;
using UnityEngine;

public class BreakLineHandler : MonoBehaviour
{
	private void Update()
	{
		if (ControlsController.ControlsActions.BreakLine.WasClicked && GameFactory.Player != null && GameFactory.Player.IsBreakLineAvailable)
		{
			GameFactory.Message.HideMessage();
			this.ShowBreakLinePanel();
		}
	}

	private void ShowBreakLinePanel()
	{
		ControlsController.ControlsActions.BlockMouseButtons(true, true, true, true);
		ControlsController.ControlsActions.BlockAxis();
		UIHelper.ShowYesNo(ScriptLocalization.Get("BreakLineCaption"), delegate
		{
			GameActionAdapter.Instance.BreakLine();
			UIAudioSourceListener.Instance.LineCut();
		}, null, "YesCaption", null, "NoCaption", null, null, null);
	}
}
