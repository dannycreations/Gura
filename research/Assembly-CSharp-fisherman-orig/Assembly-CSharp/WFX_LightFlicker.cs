using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class WFX_LightFlicker : MonoBehaviour
{
	private void Start()
	{
		this.timer = this.time;
		base.StartCoroutine("Flicker");
	}

	private IEnumerator Flicker()
	{
		for (;;)
		{
			base.GetComponent<Light>().enabled = !base.GetComponent<Light>().enabled;
			do
			{
				this.timer -= Time.deltaTime;
				yield return null;
			}
			while (this.timer > 0f);
			this.timer = this.time;
		}
		yield break;
	}

	public float time = 0.05f;

	private float timer;
}
