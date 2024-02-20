using System;

public class PhotoModeScreenCamera : PhotoModeScreen
{
	protected override void Awake()
	{
		base.Awake();
		this._actions = new CustomPlayerAction[]
		{
			ControlsController.ControlsActions.PHMDollyZoomInUI,
			ControlsController.ControlsActions.PHMDollyZoomOutUI,
			ControlsController.ControlsActions.PHMDollyZoomInByScrollUI,
			ControlsController.ControlsActions.PHMDollyZoomOutByScrollUI
		};
		this._axisActions = new CustomPlayerTwoAxisAction[]
		{
			ControlsController.ControlsActions.PHMOffsetUI,
			ControlsController.ControlsActions.PHMOrbitUI,
			ControlsController.ControlsActions.PhotoModeLooks
		};
	}

	public override string ToString()
	{
		return "PhotoModeScreenCamera";
	}
}
