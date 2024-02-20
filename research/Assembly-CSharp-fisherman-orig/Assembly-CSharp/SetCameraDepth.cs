using System;
using UnityEngine;

[ExecuteInEditMode]
public class SetCameraDepth : MonoBehaviour
{
	private void Update()
	{
		base.GetComponent<Camera>().depthTextureMode = this.depthMode;
	}

	[SerializeField]
	private DepthTextureMode depthMode;
}
