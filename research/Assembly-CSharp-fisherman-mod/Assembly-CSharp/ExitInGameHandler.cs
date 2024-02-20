using System;
using UnityEngine;

public class ExitInGameHandler : MonoBehaviour
{
	public static ExitInGameHandler Instance { get; private set; }

	public bool IsActive { get; private set; }

	private void Awake()
	{
		ExitInGameHandler.Instance = this;
	}

	public void OnEnable()
	{
		this.IsActive = true;
		this.blockedAxisElsewhere = ControlsController.ControlsActions.IsBlockedAxis;
		if (!this.blockedAxisElsewhere)
		{
			ControlsController.ControlsActions.BlockAxis();
		}
		CursorManager.ShowCursor();
	}

	private void FixedUpdate()
	{
		if (!ControlsController.ControlsActions.IsBlockedAxis)
		{
			ControlsController.ControlsActions.BlockAxis();
			this.blockedAxisElsewhere = false;
		}
	}

	public void OnDisable()
	{
		if (ControlsController.ControlsActions.IsBlockedAxis && !this.blockedAxisElsewhere)
		{
			ControlsController.ControlsActions.UnBlockAxis();
		}
		this.blockedAxisElsewhere = false;
		CursorManager.HideCursor();
		this.IsActive = false;
	}

	private bool blockedAxisElsewhere;
}
