using System;
using UnityEngine;

public class DestroyFirework : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		this.DestroyTimer -= Time.deltaTime;
		if (this.DestroyTimer <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
		this.StopLoopTimer -= Time.deltaTime;
		if (this.StopLoopTimer <= 0f)
		{
			this.ParticleInstance.loop = false;
		}
	}

	public float DestroyTimer = 0.5f;

	public float StopLoopTimer = 0.5f;

	public ParticleSystem ParticleInstance;
}
