using System;
using UnityEngine;

public class PondMarker : LookAtCamera
{
	public int PondId { get; private set; }

	public PondMarker.States State
	{
		get
		{
			return this._state;
		}
		set
		{
			this._state = value;
		}
	}

	public void Init(int pondId)
	{
		this.PondId = pondId;
	}

	public new void SetCamera(Camera cam)
	{
		base.SetCamera(cam);
	}

	private PondMarker.States _state;

	public enum States : byte
	{
		None,
		Show,
		Hide
	}
}
