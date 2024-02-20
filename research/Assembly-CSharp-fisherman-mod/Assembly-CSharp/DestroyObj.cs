using System;
using UnityEngine;

public class DestroyObj : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		this.timer -= Time.deltaTime;
		if (this.timer <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public float timer = 0.5f;
}
