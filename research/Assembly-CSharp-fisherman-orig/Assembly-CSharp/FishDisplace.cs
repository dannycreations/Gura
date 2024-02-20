using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(FishWaterBase))]
public class FishDisplace : MonoBehaviour
{
	public void Awake()
	{
		if (base.enabled)
		{
			this.OnEnable();
		}
		else
		{
			this.OnDisable();
		}
	}

	public void OnEnable()
	{
		Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
		Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
	}

	public void OnDisable()
	{
		Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
		Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
	}
}
