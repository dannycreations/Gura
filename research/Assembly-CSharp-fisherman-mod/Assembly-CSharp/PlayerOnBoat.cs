using System;
using Boats;
using UnityEngine;

public class PlayerOnBoat : PlayerStateBase
{
	public override bool CantOpenInventory
	{
		get
		{
			return false;
		}
	}

	protected override void onEnter()
	{
		base.onEnter();
		if (!base.Player.IsSailing)
		{
			base.Player.BoardingFrom = new Vector3?(base.Player.Collider.position);
			IBoatController boatByCategory = base.Player.GetBoatByCategory((int)base.Player.BoardingCategory);
			this._isWaitingForBoarding = true;
			base.Player.CameraController.enabled = false;
			base.Player.TargetCloser.EAdjusted += this.OnBoarded;
			base.Player.BoardingFromLocalCameraPosition = base.Player.CameraController.Camera.transform.localPosition;
			base.Player.BoardingToLocalCameraPosition = boatByCategory.DriverCameraLocalPosition;
			if (base.Player.ImmediateBoarding)
			{
				base.Player.TargetCloser.AdjustPlayerImmediate(boatByCategory.DriverPivot);
			}
			else
			{
				base.Player.TargetCloser.AdjustPlayer(boatByCategory.DriverPivot);
			}
			base.Player.DisableMovement();
		}
		else if (base.Player.UnboardingObject != null)
		{
			if (!this._isWaitingForUnBoarding)
			{
				this.StartUnboarding();
			}
		}
		else
		{
			base.Player.StopAnimation("Idle");
			base.Player.StopAnimation("BaitIdle");
			base.Player.StopAnimation("IdlePitch");
			base.Player.StopAnimation("BaitIdlePitch");
			if (base.Player.IsBoatMapClosing)
			{
				base.Player.FinishBoatMapClosing();
			}
			else
			{
				base.Player.OnInitFishingToDrivingDone();
			}
		}
	}

	private void StartUnboarding()
	{
		base.Player.Collider.position = base.Player.BoardingFrom.Value;
		this._isWaitingForUnBoarding = true;
		base.Player.BoardingFromLocalCameraPosition = base.Player.CameraController.Camera.transform.localPosition;
		base.Player.BoardingToLocalCameraPosition = base.Player.CameraController.NormalCameraLocalPosition;
		base.Player.TargetCloser.AdjustPlayer(base.Player.UnboardingObject);
		base.Player.TargetCloser.EAdjusted += this.OnUnBoarded;
	}

	private void OnBoarded()
	{
		base.Player.ImmediateBoarding = false;
		IBoatController boatByCategory = base.Player.GetBoatByCategory((int)base.Player.BoardingCategory);
		base.Player.TargetCloser.EAdjusted -= this.OnBoarded;
		base.Player.MoveToBoat();
		this._isWaitingForBoarding = false;
		base.Player.BoardingFrom = null;
	}

	private void OnUnBoarded()
	{
		this._isWaitingForUnBoarding = false;
		base.Player.TargetCloser.EAdjusted -= this.OnUnBoarded;
		base.Player.OnMoveFromBoatFinished();
		base.Player.BoardingFrom = null;
	}

	protected override Type onUpdate()
	{
		if (base.Player.UnboardingObject != null)
		{
			if (!this._isWaitingForUnBoarding)
			{
				this.StartUnboarding();
			}
			return null;
		}
		if (this._isWaitingForBoarding)
		{
			return null;
		}
		if (!base.Player.IsSailing)
		{
			return typeof(PlayerEmpty);
		}
		if (base.Player.IsBoatFishing)
		{
			return typeof(PlayerEmpty);
		}
		if (base.Player.IsTransitionToMap)
		{
			return typeof(PlayerEmpty);
		}
		return base.onUpdate();
	}

	private bool _isWaitingForBoarding;

	private bool _isWaitingForUnBoarding;
}
