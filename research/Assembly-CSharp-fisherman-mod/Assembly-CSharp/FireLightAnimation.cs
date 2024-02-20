using System;
using UnityEngine;

public class FireLightAnimation : StateMachineBehaviour
{
	public AnimatorStateInfo StateInfo { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.Lights == null || this.Lights.Length == 0)
		{
			this.Lights = animator.gameObject.transform.GetComponentsInChildren<Light>();
		}
		if (this.Intensity == null || this.Intensity.Length == 0)
		{
			this.Intensity = new float[this.Lights.Length];
			for (int i = 0; i < this.Lights.Length; i++)
			{
				this.Intensity[i] = this.Lights[i].intensity;
			}
		}
		this.phase = new float[this.Lights.Length];
		this.startIntensity = new float[this.Lights.Length];
		for (int j = 0; j < this.Lights.Length; j++)
		{
			this.phase[j] = Random.value * 100f;
			this.startIntensity[j] = this.Lights[j].intensity;
		}
		this.startTimestamp = Time.time;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < this.Lights.Length; i++)
		{
			float num = 1f + this.VariationFactor * Mathf.PerlinNoise(this.phase[i] + Time.time * this.VariationFrequency, this.phase[i]);
			this.Lights[i].intensity = num * Mathf.Lerp(this.startIntensity[i], this.Intensity[i], Mathf.SmoothStep(0f, 1f, (Time.time - this.startTimestamp) / this.TransitionTime));
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public Light[] Lights;

	public float[] Intensity;

	public float VariationFactor;

	public float VariationFrequency;

	public float TransitionTime;

	private float[] startIntensity;

	private float[] phase;

	private float startTimestamp;
}
