using System;
using UnityEngine;

public class GlowBlinkingScript : MonoBehaviour
{
	private void Start()
	{
		this.skinnedMeshRenderer = base.gameObject.GetComponent<Renderer>();
		this.instancedMaterial = this.skinnedMeshRenderer.material;
	}

	private void Update()
	{
		this.instancedMaterial.SetFloat("_GlowStrength", this.curve.Evaluate(Time.time));
	}

	public Renderer skinnedMeshRenderer;

	public Material instancedMaterial;

	public AnimationCurve curve;
}
