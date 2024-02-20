using System;
using UnityEngine;

public class HitchBase : MonoBehaviour, ISize
{
	public virtual float HitchProbability
	{
		get
		{
			return this._hitchProbability;
		}
	}

	public virtual bool CanBreak
	{
		get
		{
			return this._canBreak;
		}
	}

	public virtual float MaxLoad
	{
		get
		{
			return this._maxLoad;
		}
	}

	public virtual HitchBase.GeneratedItem[] GeneratedItems
	{
		get
		{
			return this._generatedItems;
		}
	}

	public virtual Vector3 Scale
	{
		get
		{
			return this._scale;
		}
	}

	public virtual void SetWidth(float width)
	{
		this._scale.x = width;
	}

	public virtual void SetDepth(float depth)
	{
		this._scale.z = depth;
	}

	[SerializeField]
	protected float _hitchProbability;

	[SerializeField]
	protected bool _canBreak;

	[SerializeField]
	protected float _maxLoad;

	[SerializeField]
	protected HitchBase.GeneratedItem[] _generatedItems;

	[SerializeField]
	protected Vector3 _scale = Vector3.one;

	[Serializable]
	public class GeneratedItem
	{
		public int ItemId;

		public float Probability;
	}
}
