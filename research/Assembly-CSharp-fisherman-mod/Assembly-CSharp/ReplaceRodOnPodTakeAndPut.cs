using System;
using UnityEngine;

public class ReplaceRodOnPodTakeAndPut : RodPodInteractionState
{
	protected override void onEnter()
	{
		base.Player.CreatePutTargetForCurrentRod();
		base.Player.TakingRod.Behaviour.OnRodPodTransitionStart();
		this._dippingRod = base.Player.RodObject.transform;
		this._dippingRodBehaviour = base.Player.Rod;
		PlayerController.HandData takingHand = base.Player.TakingHand;
		Transform target = takingHand.IK.solver.target;
		Vector3 vector = Math3d.ProjectOXZ(target.forward);
		this._fromRotation = target.rotation;
		this._toRotation = Quaternion.FromToRotation(Vector3.forward, vector);
		if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
		{
			this._toRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
		}
		float num = Mathf.Abs(Math3d.ClampAngleTo180(Vector3.Angle(target.forward, vector)));
		float num2 = num / 180f;
		base.StartRodInteraction(num2, takingHand.RodBone, base.Player.TakingRod.Behaviour);
	}

	protected override Type OnLateUpdate()
	{
		this._dippingRodBehaviour.RodSlot.Sim.AutoTension();
		base.Player.TakingRod.Behaviour.RodSlot.Sim.AutoTension();
		base.CalcSmothPrc();
		Transform transform = base.Player.TakingRod.transform;
		if (!base.Player.IsSailing)
		{
			base.Player.Collider.position = RodPodInteractionState._downPlayerPos;
		}
		if (!this._isAnimationStarted)
		{
			transform.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, this._smothPrc);
			transform.localPosition = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			if (Mathf.Approximately(this._smothPrc, 1f))
			{
				this._isAnimationStarted = true;
				base.Player.TakenRodRotation = this._toRotation;
				transform.localPosition = Vector3.zero;
				this._fromRotation = this._dippingRod.rotation;
				this._toRotation = base.Player.CurHand.IK.solver.target.rotation;
				if (base.Player.ReelType == ReelTypes.Baitcasting)
				{
					this._toRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
				}
				base.PlayRodInteractionAnimation((base.Player.ReelType != ReelTypes.Baitcasting) ? "RPSPutSpinOnPod" : "RPSPutCastingOnPod", base.Player.Collider.position, base.Player.Collider.position);
			}
		}
		else
		{
			PlayerController.HandData takingHand = base.Player.TakingHand;
			takingHand.IK.solver.IKPositionWeight = 1f - this._smothPrc;
			transform.rotation = base.Player.TakenRodRotation;
			base.Player.CurHand.IK.solver.IKPositionWeight = this._smothPrc;
			this._dippingRod.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, this._smothPrc);
			this._dippingRod.localPosition = Vector3.zero;
			if (base.IsAnimationFinished)
			{
				transform.rotation = base.Player.TakenRodRotation;
				takingHand.IK.enabled = false;
				Object.Destroy(takingHand.IK.solver.target.gameObject);
				this._dippingRod.rotation = this._toRotation;
				return typeof(ReplaceRodOnPodOut);
			}
		}
		return null;
	}

	private bool _isAnimationStarted;

	private Quaternion _fromRotation;

	private Quaternion _toRotation;

	private Transform _dippingRod;

	private Rod1stBehaviour _dippingRodBehaviour;
}
