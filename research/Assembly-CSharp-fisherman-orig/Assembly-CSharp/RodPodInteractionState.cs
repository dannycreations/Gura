using System;
using UnityEngine;

public class RodPodInteractionState : PlayerStateBase
{
	protected void PrepareDownMovement(float downHeight)
	{
		base.Player.SavedPosition = new Vector3?(base.Player.Collider.position);
		if (base.Player.IsSailing)
		{
			RodPodInteractionState._downPlayerPos = base.Player.Collider.position;
		}
		else
		{
			RodPodInteractionState._downPlayerPos = base.Player.Collider.position + Vector3.down * (base.Player.Collider.position.y - downHeight);
		}
	}

	protected void FinishDownMovement()
	{
		base.Player.SavedPosition = null;
	}

	protected void PlayRodInteractionAnimation(string clipName, Vector3 from, Vector3 to)
	{
		this.AnimationState = base.Player.PlayAnimation(clipName, 3f, 1f, 0f);
		this.FromToMovement(this.AnimationState.length / 3f, from, to);
	}

	protected void FromToMovement(float duration, Vector3 from, Vector3 to)
	{
		this._actionDuration = duration;
		this._actionTill = Time.time + duration;
		this._movementFrom = from;
		this._movementTo = to;
	}

	protected void StartRodInteraction(float rTime, Transform to, RodBehaviour rod)
	{
		Transform transform = rod.transform;
		transform.parent = to;
		float num = transform.localPosition.magnitude / 1f;
		this._movementFrom = transform.localPosition;
		this._movementTo = Vector3.zero;
		this._actionDuration = Mathf.Max(num, rTime);
		this._actionTill = Time.time + this._actionDuration;
	}

	protected void CalcSmothPrc()
	{
		this._smothPrc = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - (this._actionTill - Time.time) / this._actionDuration));
	}

	protected static Vector3 _downPlayerPos;

	protected const float _rodMovementSpeed = 1f;

	protected const float _rodRotationSpeed = 180f;

	protected const float PlayerDyMovement = 0.7f;

	protected const float WaitingPopupDelay = 2f;

	protected Vector3 _movementFrom;

	protected Vector3 _movementTo;

	protected Quaternion _rotationFrom;

	protected Quaternion _rotationTo;

	protected float _actionDuration;

	protected float _actionTill = -1f;

	protected float _smothPrc;

	protected float _animationFinishedTimeStamp;
}
