using System;
using UnityEngine;

public class ChaseCam : MonoBehaviour
{
	private void Start()
	{
		if (!this.Target)
		{
			this.Target = GameObject.FindGameObjectWithTag("Player").transform;
		}
	}

	private void LateUpdate()
	{
		if (!this.Target)
		{
			return;
		}
		base.transform.position = this.Target.position;
		base.transform.rotation = this.Target.rotation;
		Vector3 position = base.transform.position;
		position.z -= this.Distance;
		position.y += this.Height;
		base.transform.position = position;
		base.transform.LookAt(this.Target);
	}

	public Transform Target;

	public float Distance = 10f;

	public float Height = 10f;
}
