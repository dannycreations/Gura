using System;
using UnityEngine;

namespace FSMAnimator
{
	public class AnimAxisSequence
	{
		public AnimAxisSequence(string fsmName, AnimAxisSequence.PlayAnimationF playAnimationF, string idle, string forward, string forwardIdle, string backward, string backwardIdle)
		{
			this._playAnimationF = playAnimationF;
			this._idle = idle;
			this._forward = forward;
			this._forwardIdle = forwardIdle;
			this._backward = backward;
			this._backwardIdle = backwardIdle;
			this._fsm = new TFSM<AnimAxisSequence.State, AnimAxisSequence.Signal>(fsmName, AnimAxisSequence.State.NONE);
			this._fsm.AddState(AnimAxisSequence.State.NONE, null, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.IDLE, delegate
			{
				this.PlayAnimation(this._idle);
			}, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.FORWARD, null, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.FORWARD_TO_IDLE, null, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.FORWARD_IDLE, delegate
			{
				this.PlayAnimation(this._forwardIdle);
			}, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.BACKWARD, null, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.BACKWARD_TO_IDLE, null, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddState(AnimAxisSequence.State.BACKWARD_IDLE, delegate
			{
				this.PlayAnimation(this._backwardIdle);
			}, null, null, 0f, AnimAxisSequence.Signal.START, null, null);
			this._fsm.AddTransition(AnimAxisSequence.State.NONE, AnimAxisSequence.State.IDLE, AnimAxisSequence.Signal.START, null);
			this._fsm.AddTransition(AnimAxisSequence.State.IDLE, AnimAxisSequence.State.FORWARD, AnimAxisSequence.Signal.FOWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._forward, AnimAxisSequence.Signal.MAX_FORCE, 1f, 0f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD, AnimAxisSequence.State.FORWARD_IDLE, AnimAxisSequence.Signal.MAX_FORCE, null);
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD, AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.Signal.NO_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.IDLE_RESTORED);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD, AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.Signal.BACKWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.IDLE_RESTORED);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.IDLE, AnimAxisSequence.State.BACKWARD, AnimAxisSequence.Signal.BACKWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._backward, AnimAxisSequence.Signal.MAX_FORCE, 1f, 0f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD, AnimAxisSequence.State.BACKWARD_IDLE, AnimAxisSequence.Signal.MAX_FORCE, null);
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD, AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.Signal.NO_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.IDLE_RESTORED);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD, AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.Signal.FOWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.IDLE_RESTORED);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD_IDLE, AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.Signal.NO_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._forward, AnimAxisSequence.Signal.IDLE_RESTORED, -1f, -1f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD_IDLE, AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.Signal.BACKWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._forward, AnimAxisSequence.Signal.IDLE_RESTORED, -1f, -1f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD_IDLE, AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.Signal.NO_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._backward, AnimAxisSequence.Signal.IDLE_RESTORED, -1f, -1f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD_IDLE, AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.Signal.FOWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.PlayTransitAnimation(this._backward, AnimAxisSequence.Signal.IDLE_RESTORED, -1f, -1f, 0.1f);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.State.IDLE, AnimAxisSequence.Signal.IDLE_RESTORED, null);
			this._fsm.AddTransition(AnimAxisSequence.State.FORWARD_TO_IDLE, AnimAxisSequence.State.FORWARD, AnimAxisSequence.Signal.FOWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.MAX_FORCE);
			});
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.State.IDLE, AnimAxisSequence.Signal.IDLE_RESTORED, null);
			this._fsm.AddTransition(AnimAxisSequence.State.BACKWARD_TO_IDLE, AnimAxisSequence.State.BACKWARD, AnimAxisSequence.Signal.BACKWARD_FORCE, delegate(AnimAxisSequence.State id, AnimAxisSequence.State toId)
			{
				this.ReverseTransitAnimation(AnimAxisSequence.Signal.MAX_FORCE);
			});
			this._fsm.AddAnyStateTransition(AnimAxisSequence.State.NONE, AnimAxisSequence.Signal.STOP, true);
		}

		public void RefreshAnimation()
		{
			if (this._fsm.CurStateID == AnimAxisSequence.State.IDLE)
			{
				this.PlayAnimation(this._idle);
			}
		}

		public void Start()
		{
			if (this._fsm.CurStateID == AnimAxisSequence.State.IDLE)
			{
				this.PlayAnimation(this._idle);
			}
			else
			{
				this._fsm.SendSignal(AnimAxisSequence.Signal.START);
			}
		}

		public void Stop()
		{
			this._fsm.SendSignal(AnimAxisSequence.Signal.STOP);
		}

		public void Update(float axis)
		{
			if (this._transitionState != null && ((this._transitionState.speed > 0f && this._transitionState.time > this._transitionState.length) || (this._transitionState.speed < 0f && this._transitionState.time <= 0f)))
			{
				this._transitionState = null;
				this._fsm.SendSignal(this._transitionSignal);
			}
			if (Mathf.Approximately(axis, 0f))
			{
				this._fsm.SendSignal(AnimAxisSequence.Signal.NO_FORCE);
			}
			else
			{
				this._fsm.SendSignal((axis >= 0f) ? AnimAxisSequence.Signal.FOWARD_FORCE : AnimAxisSequence.Signal.BACKWARD_FORCE);
			}
			this._fsm.Update();
		}

		private void ReverseTransitAnimation(AnimAxisSequence.Signal signal)
		{
			this.PlayTransitAnimation(this._transitionState.name, signal, -this._transitionState.speed, this._transitionState.time, 0f);
		}

		private void PlayTransitAnimation(string clipName, AnimAxisSequence.Signal transitionSignal, float animSpeed = 1f, float time = 0f, float blendTime = 0.1f)
		{
			this._transitionState = this._playAnimationF(clipName, animSpeed, time, blendTime);
			this._transitionSignal = transitionSignal;
		}

		private void PlayAnimation(string clipName)
		{
			this._transitionState = null;
			this._playAnimationF(clipName, 1f, 0f, 0f);
		}

		private AnimAxisSequence.PlayAnimationF _playAnimationF;

		private string _idle;

		private string _forward;

		private string _forwardIdle;

		private string _backward;

		private string _backwardIdle;

		private TFSM<AnimAxisSequence.State, AnimAxisSequence.Signal> _fsm;

		private AnimationState _transitionState;

		private AnimAxisSequence.Signal _transitionSignal;

		public delegate AnimationState PlayAnimationF(string clipName, float animSpeed = 1f, float time = 0f, float blendTime = 0f);

		private enum State
		{
			NONE,
			IDLE,
			FORWARD,
			BACKWARD,
			FORWARD_TO_IDLE,
			BACKWARD_TO_IDLE,
			FORWARD_IDLE,
			BACKWARD_IDLE
		}

		private enum Signal
		{
			START,
			FOWARD_FORCE,
			BACKWARD_FORCE,
			MAX_FORCE,
			NO_FORCE,
			IDLE_RESTORED,
			STOP
		}
	}
}
