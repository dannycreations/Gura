using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class TFSM<TStateEnum, TSignalEnum> where TStateEnum : struct, IConvertible, IComparable where TSignalEnum : struct, IConvertible, IComparable
{
	public TFSM(string name, TStateEnum initialStateID)
	{
		this._fsmName = name;
		this.CurStateID = initialStateID;
	}

	public TStateEnum CurStateID { get; private set; }

	public void AddState(TStateEnum stateID, StateSwitchingDelegate enterF = null, StateSwitchingDelegate leaveF = null, StateSwitchingDelegate updateF = null, float stateMinimalTime = 0f, TSignalEnum timedSignal = default(TSignalEnum), Func<TSignalEnum> timedSignalF = null, StateSwitchingDelegate anyStateLeaveF = null)
	{
		if (this._states.ContainsKey(stateID))
		{
			return;
		}
		this._states[stateID] = new TFSM<TStateEnum, TSignalEnum>.State(enterF, leaveF, updateF, stateMinimalTime, timedSignal, timedSignalF, anyStateLeaveF);
	}

	public void AddAnyStateTransition(TStateEnum toID, TSignalEnum signalID, bool callEnterF = true)
	{
		if (!this._anyStateTransitions.ContainsKey(signalID))
		{
			this._anyStateTransitions[signalID] = new TFSM<TStateEnum, TSignalEnum>.AnyStateTransitionData
			{
				State = toID,
				WillCallEnterFunction = callEnterF
			};
		}
	}

	public void AddTransition(TStateEnum fromID, TStateEnum toID, TSignalEnum signalID, TransitionDelegate<TStateEnum> transitionF = null)
	{
		if (!this._states.ContainsKey(fromID) || !this._states.ContainsKey(toID))
		{
			if (!this._states.ContainsKey(fromID))
			{
			}
			if (!this._states.ContainsKey(toID))
			{
			}
			return;
		}
		if (!this._transitions.ContainsKey(signalID))
		{
			this._transitions[signalID] = new Dictionary<TStateEnum, TFSM<TStateEnum, TSignalEnum>.Transition>();
		}
		else if (this._transitions[signalID].ContainsKey(fromID))
		{
			return;
		}
		this._transitions[signalID][fromID] = new TFSM<TStateEnum, TSignalEnum>.Transition(fromID, toID, transitionF);
	}

	public bool SendSignal(TSignalEnum signalID)
	{
		if (this._anyStateTransitions.ContainsKey(signalID))
		{
			if (this._states[this.CurStateID].AnyStateLeaveF != null)
			{
				this._states[this.CurStateID].AnyStateLeaveF();
			}
			TFSM<TStateEnum, TSignalEnum>.AnyStateTransitionData anyStateTransitionData = this._anyStateTransitions[signalID];
			this.CurStateID = anyStateTransitionData.State;
			TFSM<TStateEnum, TSignalEnum>.State state = this._states[this.CurStateID];
			if (anyStateTransitionData.WillCallEnterFunction && state.EnterF != null)
			{
				state.EnterF();
			}
			this._curStateFreeAt = ((state.StateMinimalTime <= 0f) ? (-1f) : (Time.time + state.StateMinimalTime));
			return true;
		}
		if (this._curStateFreeAt > Time.time)
		{
			return false;
		}
		if (!this._transitions.ContainsKey(signalID))
		{
			return false;
		}
		if (!this._transitions[signalID].ContainsKey(this.CurStateID))
		{
			return false;
		}
		TFSM<TStateEnum, TSignalEnum>.Transition transition = this._transitions[signalID][this.CurStateID];
		if (this._states[this.CurStateID].LeaveF != null)
		{
			this._states[this.CurStateID].LeaveF();
		}
		if (transition.TransitionF != null)
		{
			transition.TransitionF(this.CurStateID, transition.ToID);
		}
		this.CurStateID = transition.ToID;
		TFSM<TStateEnum, TSignalEnum>.State state2 = this._states[this.CurStateID];
		if (state2.EnterF != null)
		{
			state2.EnterF();
		}
		this._curStateFreeAt = ((state2.StateMinimalTime <= 0f) ? (-1f) : (Time.time + state2.StateMinimalTime));
		return true;
	}

	public void Update()
	{
		TFSM<TStateEnum, TSignalEnum>.State state = this._states[this.CurStateID];
		if (state.UpdateF != null)
		{
			state.UpdateF();
		}
		if (this._curStateFreeAt <= Time.time)
		{
			if (state.TimedSignalF != null)
			{
				this.SendSignal(state.TimedSignalF());
			}
			else
			{
				TSignalEnum timedSignal = state.TimedSignal;
				if (!timedSignal.Equals(default(TSignalEnum)))
				{
					this.SendSignal(state.TimedSignal);
				}
			}
		}
	}

	[Conditional("TFSM")]
	private void Log(string msg, params object[] args)
	{
		LogHelper.Log("{0}:TFSM.{1}", new object[]
		{
			this._fsmName,
			string.Format(msg, args)
		});
	}

	[Conditional("TFSM")]
	private void LogError(string msg, params object[] args)
	{
		LogHelper.Log("{0}:TFSM.Error: {1}", new object[]
		{
			this._fsmName,
			string.Format(msg, args)
		});
	}

	private Dictionary<TStateEnum, TFSM<TStateEnum, TSignalEnum>.State> _states = new Dictionary<TStateEnum, TFSM<TStateEnum, TSignalEnum>.State>();

	private Dictionary<TSignalEnum, Dictionary<TStateEnum, TFSM<TStateEnum, TSignalEnum>.Transition>> _transitions = new Dictionary<TSignalEnum, Dictionary<TStateEnum, TFSM<TStateEnum, TSignalEnum>.Transition>>();

	private Dictionary<TSignalEnum, TFSM<TStateEnum, TSignalEnum>.AnyStateTransitionData> _anyStateTransitions = new Dictionary<TSignalEnum, TFSM<TStateEnum, TSignalEnum>.AnyStateTransitionData>();

	private float _curStateFreeAt;

	private string _fsmName;

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct AnyStateTransitionData
	{
		public TStateEnum State { get; set; }

		public bool WillCallEnterFunction { get; set; }
	}

	private class State
	{
		public State(StateSwitchingDelegate enterF, StateSwitchingDelegate leaveF, StateSwitchingDelegate updateF, float stateMinimalTime, TSignalEnum timedSignal, Func<TSignalEnum> timedSignalF, StateSwitchingDelegate anyStateLeaveF)
		{
			this.EnterF = enterF;
			this.LeaveF = leaveF;
			this.UpdateF = updateF;
			this.StateMinimalTime = stateMinimalTime;
			this.TimedSignal = timedSignal;
			this.TimedSignalF = timedSignalF;
			this.AnyStateLeaveF = anyStateLeaveF;
		}

		public StateSwitchingDelegate EnterF { get; private set; }

		public StateSwitchingDelegate LeaveF { get; private set; }

		public StateSwitchingDelegate AnyStateLeaveF { get; private set; }

		public StateSwitchingDelegate UpdateF { get; private set; }

		public float StateMinimalTime { get; private set; }

		public TSignalEnum TimedSignal { get; private set; }

		public Func<TSignalEnum> TimedSignalF { get; private set; }
	}

	private class Transition
	{
		public Transition(TStateEnum fromID, TStateEnum toID, TransitionDelegate<TStateEnum> transitionF)
		{
			this.FromID = fromID;
			this.ToID = toID;
			this.TransitionF = transitionF;
		}

		public TStateEnum FromID { get; private set; }

		public TStateEnum ToID { get; private set; }

		public TransitionDelegate<TStateEnum> TransitionF { get; private set; }
	}
}
