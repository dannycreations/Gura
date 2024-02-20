using System;
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
	public float MovementSpeed
	{
		get
		{
			return this._movementSpeed;
		}
	}

	public float RotationSpeed
	{
		get
		{
			return this._rotationSpeed;
		}
	}

	public bool KeepSpeedTillTheEnd
	{
		get
		{
			return this._keepSpeedTillTheEnd;
		}
	}

	public float Duration
	{
		get
		{
			return this._duration;
		}
	}

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

	[SerializeField]
	private float _movementSpeed = -1f;

	[SerializeField]
	private float _rotationSpeed = -1f;

	[SerializeField]
	private bool _keepSpeedTillTheEnd;

	[SerializeField]
	private float _duration = -1f;
}
