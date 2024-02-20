using System;
using UnityEngine;

public class RotateEarth : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			this.mouseXAmount = -Input.GetAxis("Mouse X") * 2f;
			this.mouseYAmount = Input.GetAxis("Mouse Y") * 2f;
			base.transform.Rotate(this.mouseYAmount, this.mouseXAmount, 0f, 0);
		}
	}

	private float mouseXAmount;

	private float mouseYAmount;
}
