using System;
using UnityEngine;

public class CameraParticleHelper : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnPreCull()
	{
		Camera component = base.gameObject.GetComponent<Camera>();
		Shader.SetGlobalMatrix("_Camera2World", component.cameraToWorldMatrix);
		GlobalConsts.Camera2World = component.cameraToWorldMatrix;
	}
}
