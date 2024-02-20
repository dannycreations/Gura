using System;
using UnityEngine;

public class DeferredRotation : MonoBehaviour
{
	internal void Start()
	{
		if (base.transform.parent != null)
		{
			this.LastRotation = base.transform.parent.rotation;
		}
	}

	internal void LateUpdate()
	{
		if (base.transform.parent == null)
		{
			return;
		}
		Quaternion rotation = base.transform.parent.rotation;
		Quaternion quaternion = Quaternion.Inverse(rotation);
		Quaternion quaternion2 = Quaternion.Slerp(this.LastRotation, rotation, Time.deltaTime * this.speed);
		base.transform.localRotation = quaternion * quaternion2;
		this.LastRotation = quaternion2;
	}

	public float speed = 12f;

	internal Quaternion LastRotation;
}
