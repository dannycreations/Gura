using System;
using UnityEngine;

public class MB_ExampleMover : MonoBehaviour
{
	private void Update()
	{
		Vector3 vector;
		vector..ctor(5f, 5f, 5f);
		ref Vector3 ptr = ref vector;
		int num;
		vector[num = this.axis] = ptr[num] * Mathf.Sin(Time.time);
		base.transform.position = vector;
	}

	public int axis;
}
