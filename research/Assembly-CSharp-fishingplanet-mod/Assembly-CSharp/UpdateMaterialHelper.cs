using System;
using UnityEngine;

public class UpdateMaterialHelper : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (this.mat)
		{
			this.mat.SetMatrix("_Camera2World", GlobalConsts.Camera2World);
		}
	}

	public Material mat;
}
