using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public Camera Camera2LookAt { get; private set; }

	protected virtual void Update()
	{
		if (this.Camera2LookAt != null)
		{
			Vector3 vector = this.Camera2LookAt.transform.position - base.transform.position;
			vector.x = (vector.z = 0f);
			base.transform.LookAt(this.Camera2LookAt.transform.position - vector);
			base.transform.Rotate(0f, 180f, 0f);
		}
	}

	public void SetCamera(Camera cam)
	{
		this.Camera2LookAt = cam;
	}
}
