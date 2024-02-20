using System;
using UnityEngine;

public abstract class UnderwaterItemBehaviour
{
	protected UnderwaterItemBehaviour(UnderwaterItemController controller)
	{
		this._owner = controller;
		this.phyObject = null;
	}

	protected Transform transform
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.transform;
		}
	}

	public UnderwaterItemController Owner
	{
		get
		{
			return this._owner;
		}
	}

	public UnderwaterItemObject phyObject { get; protected set; }

	public virtual void Start()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnDestroy()
	{
		this._owner = null;
	}

	protected UnderwaterItemController _owner;
}
