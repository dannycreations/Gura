using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	private void Update()
	{
		if (GameFactory.Player != null)
		{
			Camera camera = GameFactory.Player.CameraController.Camera;
			if (camera != null)
			{
				Vector3 vector = camera.transform.position - base.transform.position;
				float magnitude = vector.magnitude;
				vector.x = (vector.z = 0f);
				base.transform.LookAt(camera.transform.position - vector);
				base.transform.Rotate(0f, this._addYaw, 0f);
			}
		}
	}

	[SerializeField]
	private float _addYaw;
}
