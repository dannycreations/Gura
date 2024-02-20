using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(AnimationCurves))]
public class LimbIkTest : MonoBehaviour
{
	private LimbIkTest.State CurState
	{
		get
		{
			return this._curState;
		}
		set
		{
			LogHelper.Log("Enter state {0}", new object[] { value });
			this._curState = value;
		}
	}

	private void Awake()
	{
		this._curves = base.GetComponent<AnimationCurves>();
		for (int i = 0; i < this._objectsToAnimate.Length; i++)
		{
			if (this._objectsToAnimate != null)
			{
				Animation component = this._objectsToAnimate[i].GetComponent<Animation>();
				if (component != null)
				{
					LogHelper.Log("Register animable part {0}", new object[] { this._objectsToAnimate[i].name });
					this._parts.Add(new LimbIkTest.PartControllers(component, this._objectsToAnimate[i]));
				}
				else
				{
					LogHelper.Error("Object {0} has no attached Animation component", new object[] { this._objectsToAnimate[i].name });
				}
			}
		}
		this.CurState = LimbIkTest.State.IDLE;
		this.StartAnimation("Idle");
	}

	private void Update()
	{
		float curveValueForLocalTime = this._curves.GetCurveValueForLocalTime("ikWeightCurve", this._curAnimation, Mathf.Repeat(this._parts[0].Animator[this._curAnimation].time, this._parts[0].Animator[this._curAnimation].length));
		for (int i = 0; i < this._parts.Count; i++)
		{
			this._parts[i].Ik.solver.IKPositionWeight = curveValueForLocalTime;
			this._parts[i].Ik.solver.IKRotationWeight = curveValueForLocalTime;
		}
		if (this._currentSequence.Count > 0)
		{
			AnimationState animationState = this._parts[0].Animator[this._currentSequence.Peek()];
			if ((animationState.speed > 0f && animationState.time >= animationState.length) || (animationState.speed < 0f && animationState.time <= 0f))
			{
				this._currentSequence.Dequeue();
				if (this._currentSequence.Count > 0)
				{
					this.StartAnimation(this._currentSequence.Peek());
					if (this._currentSequence.Count == 1)
					{
						this.CurState = this._sequenceTargetState;
					}
				}
			}
		}
		if (this.CurState == LimbIkTest.State.IDLE)
		{
			if (Input.GetKeyUp(282))
			{
				this.StartAnimationSequence(LimbIkTest.State.THROWN, new string[] { "CloseRollStart", "IdleThrown" });
			}
		}
		else if (this.CurState == LimbIkTest.State.THROWN)
		{
			if (Input.GetKeyDown(114))
			{
				this.CurState = LimbIkTest.State.ROLL;
				this.StartAnimation("Roll");
			}
			if (Input.GetKeyUp(282))
			{
				this.CurState = LimbIkTest.State.IDLE;
				this.StartAnimation("Idle");
			}
		}
		else if (this.CurState == LimbIkTest.State.ROLL && Input.GetKeyUp(114))
		{
			this.CurState = LimbIkTest.State.THROWN;
			this.StartAnimation("IdleThrown");
		}
	}

	private void StartAnimation(string clipName)
	{
		LogHelper.Log("StartAnimation({0})", new object[] { clipName });
		this._curAnimation = clipName;
		for (int i = 0; i < this._parts.Count; i++)
		{
			this._parts[i].Animator.CrossFade(clipName, 0.1f);
		}
		this._reelAnimator.CrossFade(clipName, 0.1f);
	}

	private void StartAnimationSequence(LimbIkTest.State targetState, params string[] names)
	{
		if (this._currentSequence.Count == 0)
		{
			this._sequenceTargetState = targetState;
			this.StartAnimation(names[0]);
			for (int i = 0; i < names.Length; i++)
			{
				this._currentSequence.Enqueue(names[i]);
			}
		}
	}

	[SerializeField]
	private LimbIK[] _objectsToAnimate;

	[SerializeField]
	private Animation _reelAnimator;

	private List<LimbIkTest.PartControllers> _parts = new List<LimbIkTest.PartControllers>();

	private Queue<string> _currentSequence = new Queue<string>();

	private LimbIkTest.State _sequenceTargetState;

	private string _curAnimation;

	private AnimationCurves _curves;

	private LimbIkTest.State _curState;

	private class PartControllers
	{
		public PartControllers(Animation animator, LimbIK ik)
		{
			this._animator = animator;
			this._ik = ik;
		}

		public Animation Animator
		{
			get
			{
				return this._animator;
			}
		}

		public LimbIK Ik
		{
			get
			{
				return this._ik;
			}
		}

		private Animation _animator;

		private LimbIK _ik;
	}

	private enum State
	{
		IDLE,
		THROWN,
		ROLL
	}
}
