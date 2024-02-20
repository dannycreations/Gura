using System;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			base.transform.Translate(0f, 0f, 100f);
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			base.transform.Translate(0f, 0f, -100f);
		}
	}
}
