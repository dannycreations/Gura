using System;
using UnityEngine;

public class BaitController : TackleControllerBase
{
	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return base.transform.rotation;
		}
	}

	public Transform hookAnchor;
}
