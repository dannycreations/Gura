using System;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (this.target != null)
		{
			base.transform.position = this.target.position;
		}
	}

	public Transform target;
}
