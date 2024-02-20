using System;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKey(97))
		{
			base.transform.Rotate(0f, 0f, 0.1f);
		}
		if (Input.GetKey(101))
		{
			base.transform.Rotate(0f, 0f, -0.1f);
		}
		if (Input.GetKey(276))
		{
			base.transform.Rotate(0f, 0.07f, 0f);
		}
		if (Input.GetKey(275))
		{
			base.transform.Rotate(0f, -0.07f, 0f);
		}
		if (Input.GetKey(273))
		{
			base.transform.Rotate(0.07f, 0f, 0f);
		}
		if (Input.GetKey(274))
		{
			base.transform.Rotate(-0.07f, 0f, 0f);
		}
		if (Input.GetKey(122))
		{
			this.camera.Rotate(-0.07f, 0f, 0f);
		}
		if (Input.GetKey(115))
		{
			this.camera.Rotate(0.07f, 0f, 0f);
		}
		if (Input.GetKey(113))
		{
			this.camera.Rotate(0f, -0.07f, 0f);
		}
		if (Input.GetKey(100))
		{
			this.camera.Rotate(0f, 0.07f, 0f);
		}
	}

	private float mouseXAmount;

	private float mouseYAmount;

	public Transform camera;
}
