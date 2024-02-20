using System;
using UnityEngine;

public class EarthMovments : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(0f, 0f, this.rotationSpeed * Time.deltaTime);
	}

	public float rotationSpeed;
}
