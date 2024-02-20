using System;
using UnityEngine;

public class RotationController : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetAxis("Horizontal") != 0f)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles.y += Input.GetAxis("Horizontal") * this.speed;
			base.transform.eulerAngles = eulerAngles;
		}
	}

	public float speed = 1f;
}
