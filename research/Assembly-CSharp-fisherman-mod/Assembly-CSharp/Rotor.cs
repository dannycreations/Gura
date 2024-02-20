using System;
using UnityEngine;

public class Rotor : MonoBehaviour
{
	private void Awake()
	{
		this._angle = Random.Range(0f, this._maxInitialAngle);
	}

	private void Update()
	{
		Vector3 vector = Vector3.right;
		if (this._axis == Rotor.Axis.Y)
		{
			vector = Vector3.up;
		}
		else if (this._axis == Rotor.Axis.Z)
		{
			vector = Vector3.forward;
		}
		this._angle += this._angleSpeed * Time.deltaTime;
		this._bone.localRotation = Quaternion.AngleAxis(this._angle, vector);
	}

	[SerializeField]
	private float _maxInitialAngle = 90f;

	[SerializeField]
	private float _angleSpeed = 15f;

	[SerializeField]
	private Transform _bone;

	[SerializeField]
	private Rotor.Axis _axis;

	private float _angle;

	public enum Axis
	{
		X,
		Y,
		Z
	}
}
