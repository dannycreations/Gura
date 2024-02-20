using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Blur Behind Viewport")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurBehindViewport : MonoBehaviour
{
	private void OnPreRender()
	{
		BlurBehind.SetViewport();
	}

	private void OnPostRender()
	{
		BlurBehind.ResetViewport();
	}
}
