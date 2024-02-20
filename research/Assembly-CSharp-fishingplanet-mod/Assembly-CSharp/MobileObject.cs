using System;
using UnityEngine;

public abstract class MobileObject : MonoBehaviour
{
	public float DistToLeaderToStop
	{
		get
		{
			return this._distToLeaderToStop;
		}
	}

	public float DistToLeaderToStart
	{
		get
		{
			return this._distToLeaderToStart;
		}
	}

	public Transform DebugCameraBone
	{
		get
		{
			return this._debugCameraBone;
		}
	}

	public bool Flag { get; set; }

	public string Group { get; protected set; }

	public MobileObject LeadingObj { get; set; }

	public abstract void Freeze();

	public abstract void UnFreeze();

	public abstract void Launch(float prc);

	public abstract float MaxSpeed { get; }

	public void Deactivate()
	{
		base.gameObject.SetActive(false);
	}

	[SerializeField]
	private Transform _debugCameraBone;

	[SerializeField]
	private float _distToLeaderToStop = 15f;

	[SerializeField]
	private float _distToLeaderToStart = 25f;

	public Action<MobileObject> EDisabled = delegate
	{
	};
}
