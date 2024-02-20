using System;

public class FsmBaseState<TOwnerClass>
{
	public Type PrevState { get; set; }

	public Type NextState { get; set; }

	public void Init(TOwnerClass owner)
	{
		this._owner = owner;
	}

	public virtual void Transition(FsmBaseState<TOwnerClass> source)
	{
		this.Enter();
	}

	public void Enter()
	{
		this.onEnter();
	}

	protected virtual void onEnter()
	{
	}

	public void Exit()
	{
		this.onExit();
	}

	protected virtual void onExit()
	{
	}

	public Type Update()
	{
		return this.onUpdate();
	}

	protected virtual Type onUpdate()
	{
		return null;
	}

	public void PreUpdate()
	{
		this.OnPreUpdate();
	}

	protected virtual void OnPreUpdate()
	{
	}

	public Type LateUpdate()
	{
		return this.OnLateUpdate();
	}

	protected virtual Type OnLateUpdate()
	{
		return null;
	}

	protected TOwnerClass _owner;
}
