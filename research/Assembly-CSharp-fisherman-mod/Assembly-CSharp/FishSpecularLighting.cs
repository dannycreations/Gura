using System;
using UnityEngine;

[RequireComponent(typeof(FishWaterBase))]
[ExecuteInEditMode]
public class FishSpecularLighting : MonoBehaviour
{
	public void Start()
	{
		this.waterBase = (FishWaterBase)base.gameObject.GetComponent(typeof(FishWaterBase));
	}

	public void Update()
	{
		if (!this.waterBase)
		{
			this.waterBase = (FishWaterBase)base.gameObject.GetComponent(typeof(FishWaterBase));
		}
		if (this.specularLight && this.waterBase.sharedMaterial)
		{
			this.waterBase.sharedMaterial.SetVector("_WorldLightDir", this.specularLight.transform.forward);
		}
	}

	public Transform specularLight;

	private FishWaterBase waterBase;
}
