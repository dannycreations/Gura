using System;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
	private void Awake()
	{
		this._point = base.transform.GetChild(0);
	}

	private void Update()
	{
		if (this._actionTime > 0f)
		{
			float num = Time.time - this._actionTime;
			float num2 = num / this._rotationTime;
			base.transform.rotation = Quaternion.Slerp(this._from, this._to, num2);
		}
		if (Input.GetKeyUp(282))
		{
			base.transform.rotation *= Quaternion.Inverse(base.transform.rotation);
		}
		if (Input.GetKeyUp(283))
		{
			this._from = base.transform.rotation;
			base.transform.LookAt(this._target.position);
			Vector3 vector = this._point.position - base.transform.position;
			Vector3 vector2 = Vector3.Cross(base.transform.forward, vector);
			float num3 = Vector3.SignedAngle(base.transform.forward, vector, vector2);
			Vector3 vector3 = base.transform.InverseTransformVector(vector2);
			Quaternion quaternion = Quaternion.AngleAxis(-num3, vector3);
			base.transform.rotation *= quaternion;
			this._to = base.transform.rotation;
			base.transform.rotation = this._from;
			this._actionTime = Time.time;
		}
		if (Input.GetKeyUp(284))
		{
			base.transform.rotation *= Quaternion.AngleAxis(30f, Vector3.right);
		}
	}

	[SerializeField]
	private Transform _target;

	[SerializeField]
	private float _rotationTime = 2f;

	private Transform _point;

	private float _actionTime = -1f;

	private Quaternion _from;

	private Quaternion _to;
}
