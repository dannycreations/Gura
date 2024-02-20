using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class DebugPlayer : MonoBehaviour
{
	private DebugPlayer.HandData Hand
	{
		get
		{
			return this._handsData[(int)this._isLeftHand];
		}
	}

	private Transform IKTarget
	{
		get
		{
			return this.Hand.IK.solver.target;
		}
	}

	private void Awake()
	{
		this._handsData[0] = new DebugPlayer.HandData(TransformHelper.FindDeepChild(base.transform, "loc_rod_right"), "TakeRodFromPod", "StandUpWithRod", "PutRodOnPod", "StandUpWithoutRod", "IdleThrown", this._rightIK);
		this._handsData[1] = new DebugPlayer.HandData(TransformHelper.FindDeepChild(base.transform, "loc_rod_left"), "TakeRodFromPodCasting", "StandUpWithRodCasting", "PutRodOnPodCasting", "StandUpWithoutRodCasting", "BaitIdleThrown", this._leftIK);
		this._closer.EAdjusted += this.OnAdjusted;
		this._animator = base.GetComponent<Animation>();
		this._rightIK.enabled = false;
		this._leftIK.enabled = false;
		Transform[] array = new Transform[this._handsRenderer.bones.Length];
		Dictionary<string, Transform> dictionary = TransformHelper.BuildSrcBonesMap(this._handsRenderer);
		TransformHelper.CopyBoneTransforms(this._sleeveRenderer, dictionary, array);
		Object.Destroy(this._sleeveRenderer.rootBone.gameObject);
	}

	private void Start()
	{
		this._pod = this._closer.gameObject.GetComponent<RodPodController>();
	}

	private void OnAdjusted()
	{
		FABRIK ik = this.Hand.IK;
		if (this._curState == DebugPlayer.State.WithRod)
		{
			ik.solver.target = this._pod.CreatePutRodTarget(1, 0f);
			this.PlayAnimation(this.Hand.PutRodInAnimation, 0f);
			this.StartAction(DebugPlayer.State.PutRodIn, this._curAnimation.length, base.transform.position, base.transform.position - new Vector3(0f, this._movementDY, 0f));
		}
		else
		{
			Transform transform = new GameObject("TakeRodTarget").transform;
			transform.rotation = this._rod.rotation;
			transform.position = this._rod.position;
			ik.solver.target = transform;
			this.PlayAnimation(this.Hand.TakeRodInAnimation, 0.2f);
			this.StartAction(DebugPlayer.State.TakeRodIn, this._curAnimation.length, base.transform.position, base.transform.position - new Vector3(0f, this._movementDY, 0f));
		}
		ik.solver.IKPositionWeight = 0f;
		ik.enabled = true;
	}

	private void StartAction(DebugPlayer.State state, float duration, Vector3 from, Vector3 to)
	{
		this._curState = state;
		this._actionDuration = duration;
		this._actionTill = Time.time + duration;
		this._movementFrom = from;
		this._movementTo = to;
	}

	private bool IsAnimationFinished
	{
		get
		{
			return this._curAnimation == null || (this._curAnimation.speed > 0f && this._curAnimation.time >= this._curAnimation.length) || (this._curAnimation.speed < 0f && this._curAnimation.time < 0f);
		}
	}

	private void PlayAnimation(string animName, float blendTime = 0f)
	{
		AnimationState animationState = this._animator[animName];
		if (animationState == null)
		{
			LogHelper.Error("invalid animation name {0}", new object[] { animName });
			return;
		}
		this._curAnimation = animationState;
		animationState.time = 0f;
		animationState.speed = 1f;
		animationState.weight = 1f;
		this._animator.CrossFade(animName, blendTime);
	}

	private void TryToTakeRod(byte isLeftHand)
	{
		if (this._actionTill <= 0f && this._curState == DebugPlayer.State.None)
		{
			this._isLeftHand = isLeftHand;
			RodPodSlot slotObj = this._pod.GetSlotObj(this._targetSlot);
			if (slotObj == null)
			{
				LogHelper.Error("Invalid slot index {0} to interact!", new object[] { this._targetSlot });
				return;
			}
			this._rod = slotObj.transform;
			this._rodPivot = this._rod.parent;
			slotObj.SetActive(true);
			this._closer.AdjustPlayer(this._rodPivot);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(108))
		{
			this.TryToTakeRod(1);
		}
		else if (Input.GetKeyUp(114))
		{
			this.TryToTakeRod(0);
		}
		else if (Input.GetKeyUp(97) && this._curState == DebugPlayer.State.WithRod)
		{
			this._closer.AdjustPlayer(this._rodPivot);
		}
		if (this._swingTransform != null)
		{
			this._swingTransform.localRotation = Quaternion.Euler(this._swingRotation, this._swingInitialYaw, 0f);
		}
		if (this._actionTill > 0f)
		{
			float num = Mathf.Clamp01(1f - (this._actionTill - Time.time) / this._actionDuration);
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			if (this._curState == DebugPlayer.State.TakeRodIn)
			{
				base.transform.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				this.Hand.IK.solver.IKPositionWeight = num2;
				if (this._actionTill < Time.time)
				{
					Vector3 position = this.IKTarget.position;
					Vector3 position2 = this.Hand.RodBone.position;
					float num3 = (position - position2).magnitude / this._rodMovementSpeed;
					this._fromRotation = this.IKTarget.rotation;
					this._toRotation = this.Hand.RodBone.rotation;
					float num4 = Mathf.Abs(Math3d.ClampAngleTo180(Vector3.Angle(this._rod.forward, this.Hand.RodBone.forward)));
					float num5 = num4 / this._rodRotationSpeed;
					float num6 = Mathf.Max(num3, num5);
					this.StartAction(DebugPlayer.State.RodAdjusting, num6, position, position2);
				}
			}
			else if (this._curState == DebugPlayer.State.PutRodIn)
			{
				base.transform.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				this.Hand.IK.solver.IKPositionWeight = num2;
				if (this._actionTill < Time.time)
				{
					Vector3 position3 = this.Hand.RodBone.position;
					Vector3 position4 = this.IKTarget.position;
					float num7 = (position3 - position4).magnitude / this._rodMovementSpeed;
					this._fromRotation = this.Hand.RodBone.rotation;
					this._toRotation = this.IKTarget.rotation;
					float num8 = Mathf.Abs(Math3d.ClampAngleTo180(Vector3.Angle(this.Hand.RodBone.forward, this._rod.forward)));
					float num9 = num8 / this._rodRotationSpeed;
					float num10 = Mathf.Max(num7, num9);
					this.StartAction(DebugPlayer.State.RodBackAdjusting, num10, position3, position4);
				}
			}
			else if (this._curState == DebugPlayer.State.RodAdjusting)
			{
				this._rod.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				this._rod.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, num2);
				if (this._actionTill < Time.time)
				{
					this.PlayAnimation(this.Hand.TakeRodOutAnimation, 0f);
					this.StartAction(DebugPlayer.State.TakeRodOut, this._curAnimation.length, base.transform.position, base.transform.position + new Vector3(0f, this._movementDY, 0f));
				}
			}
			else if (this._curState == DebugPlayer.State.RodBackAdjusting)
			{
				this._rod.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				this._rod.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, num2);
				if (this._actionTill < Time.time)
				{
					this._rod.position = this.IKTarget.position;
					this._rod.rotation = this.IKTarget.rotation;
					this._rod.SetParent(this._rodPivot, true);
					this.PlayAnimation(this.Hand.PutRodOutAnimation, 0f);
					this.StartAction(DebugPlayer.State.PutRodOut, this._curAnimation.length, base.transform.position, base.transform.position + new Vector3(0f, this._movementDY, 0f));
				}
			}
			else if (this._curState == DebugPlayer.State.TakeRodOut)
			{
				this.Hand.IK.solver.IKPositionWeight = 1f - num2;
				base.transform.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				this._rod.position = this.Hand.RodBone.position;
				this._rod.rotation = this.Hand.RodBone.rotation;
				if (this._actionTill < Time.time)
				{
					this.Hand.IK.enabled = false;
					this._rod.SetParent(this.Hand.RodBone, true);
					this._actionTill = -1f;
					this._curState = DebugPlayer.State.WithRod;
					this.PlayAnimation(this.Hand.RodIdleAnimation, 0.25f);
					Object.Destroy(this.Hand.IK.solver.target.gameObject);
				}
			}
			else if (this._curState == DebugPlayer.State.PutRodOut)
			{
				this.Hand.IK.solver.IKPositionWeight = 1f - num2;
				base.transform.position = Vector3.Lerp(this._movementFrom, this._movementTo, num2);
				if (this._actionTill < Time.time)
				{
					this.Hand.IK.enabled = false;
					this._actionTill = -1f;
					this._curState = DebugPlayer.State.None;
					this.PlayAnimation("empty", 0.25f);
					Object.Destroy(this.Hand.IK.solver.target.gameObject);
				}
			}
		}
	}

	private void OnDestroy()
	{
		this._closer.EAdjusted -= this.OnAdjusted;
	}

	[SerializeField]
	private float _movementDY = 1f;

	[SerializeField]
	private float _rodRotationSpeed = 180f;

	[SerializeField]
	private float _rodMovementSpeed = 1f;

	[SerializeField]
	private SkinnedMeshRenderer _handsRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _sleeveRenderer;

	[SerializeField]
	private PlayerTargetCloser _closer;

	[SerializeField]
	private FABRIK _leftIK;

	[SerializeField]
	private FABRIK _rightIK;

	[SerializeField]
	private float _swingRotation = -20f;

	[SerializeField]
	private float _swingInitialYaw = 180f;

	[SerializeField]
	private Transform _swingTransform;

	[SerializeField]
	private int _targetSlot = 1;

	private Animation _animator;

	private AnimationState _curAnimation;

	private DebugPlayer.State _curState;

	private float _actionTill = -1f;

	private float _actionDuration;

	private Vector3 _movementFrom;

	private Vector3 _movementTo;

	private Quaternion _fromRotation;

	private Quaternion _toRotation;

	private Transform _rod;

	private Transform _rodPivot;

	private RodPodController _pod;

	private byte _isLeftHand;

	private DebugPlayer.HandData[] _handsData = new DebugPlayer.HandData[2];

	private struct HandData
	{
		public HandData(Transform rodBone, string takeRodInAnimation, string takeRodOutAnimation, string putRodInAnimation, string putRodOutAnimation, string rodIdleAnimation, FABRIK ik)
		{
			this.RodBone = rodBone;
			this.TakeRodInAnimation = takeRodInAnimation;
			this.TakeRodOutAnimation = takeRodOutAnimation;
			this.PutRodInAnimation = putRodInAnimation;
			this.PutRodOutAnimation = putRodOutAnimation;
			this.RodIdleAnimation = rodIdleAnimation;
			this.IK = ik;
		}

		public readonly Transform RodBone;

		public readonly string TakeRodInAnimation;

		public readonly string TakeRodOutAnimation;

		public readonly string PutRodInAnimation;

		public readonly string PutRodOutAnimation;

		public readonly string RodIdleAnimation;

		public readonly FABRIK IK;
	}

	private enum State
	{
		None,
		TakeRodIn,
		RodAdjusting,
		TakeRodOut,
		PutRodIn,
		PutRodOut,
		RodBackAdjusting,
		WithRod
	}
}
