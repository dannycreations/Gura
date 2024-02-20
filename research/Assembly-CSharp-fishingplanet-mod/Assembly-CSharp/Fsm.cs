using System;
using System.Collections.Generic;

public class Fsm<TOwnerClass>
{
	public Fsm(string description, TOwnerClass owner, bool debugOutput = false)
	{
		this._description = description;
		this._owner = owner;
		this._doDebugOutput = debugOutput;
	}

	public Type CurrentStateType
	{
		get
		{
			return this._currentState.GetType();
		}
	}

	public FsmBaseState<TOwnerClass> CurrentState
	{
		get
		{
			return this._currentState;
		}
	}

	public void RegisterState<T>() where T : FsmBaseState<TOwnerClass>, new()
	{
		Type typeFromHandle = typeof(T);
		if (this._states.ContainsKey(typeFromHandle))
		{
			this.LogError("State {0} already registered", new object[] { typeFromHandle });
			return;
		}
		this._states[typeFromHandle] = delegate
		{
			T t = new T();
			t.Init(this._owner);
			return t;
		};
	}

	public void Start<T>() where T : FsmBaseState<TOwnerClass>, new()
	{
		this.Log("start");
		this.EnterState(typeof(T), null);
	}

	public void EnterState(Type stateType, FsmBaseState<TOwnerClass> sourceState = null)
	{
		Type type = ((this._currentState != null) ? this._currentState.GetType() : null);
		this._currentState = this._states[stateType]();
		this._currentState.PrevState = type;
		if (sourceState != null)
		{
			this._currentState.Transition(sourceState);
		}
		else
		{
			this._currentState.Enter();
		}
	}

	public void Update()
	{
		if (this._currentState != null)
		{
			this._currentState.PreUpdate();
			Type type = this._currentState.Update();
			if (type != null)
			{
				this._currentState.NextState = type;
				this._currentState.Exit();
				this.EnterState(type, null);
			}
		}
	}

	public void LateUpdate()
	{
		if (this._currentState != null)
		{
			Type type = this._currentState.LateUpdate();
			if (type != null)
			{
				this._currentState.NextState = type;
				this._currentState.Exit();
				this.EnterState(type, null);
			}
		}
	}

	public void LogError(string textPattern, params object[] args)
	{
	}

	public void Log(string text)
	{
		if (this._doDebugOutput)
		{
		}
	}

	private FsmBaseState<TOwnerClass> _currentState;

	private Dictionary<Type, Func<FsmBaseState<TOwnerClass>>> _states = new Dictionary<Type, Func<FsmBaseState<TOwnerClass>>>();

	private bool _doDebugOutput;

	private TOwnerClass _owner;

	private string _description;
}
