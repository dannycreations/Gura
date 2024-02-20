using System;
using UnityEngine;

public class RotateSun : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			this.mouseXAmount = -Input.GetAxis("Mouse X") * 3f;
			base.transform.Rotate(0f, this.mouseXAmount, 0f);
		}
		if (Input.GetKey(127))
		{
			base.transform.Rotate(0f, 0.2f, 0f);
		}
		if (Input.GetKey(281))
		{
			base.transform.Rotate(0f, -0.2f, 0f);
		}
		if (Input.GetKey(278))
		{
			base.transform.Rotate(0.2f, 0f, 0f);
		}
		if (Input.GetKey(279))
		{
			base.transform.Rotate(-0.2f, 0f, 0f);
		}
	}

	private float mouseXAmount;
}
