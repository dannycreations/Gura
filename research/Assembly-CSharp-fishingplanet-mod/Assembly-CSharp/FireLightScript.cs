using System;
using UnityEngine;

public class FireLightScript : MonoBehaviour
{
	private void Update()
	{
		this.random = Random.Range(0f, 150f);
		float num = Mathf.PerlinNoise(this.random, Time.time);
		this.fireLight.GetComponent<Light>().intensity = Mathf.Lerp(this.minIntensity, this.maxIntensity, num);
	}

	public float minIntensity = 0.25f;

	public float maxIntensity = 0.5f;

	public Light fireLight;

	private float random;
}
