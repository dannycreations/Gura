using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Mesh Baker/LOD Camera")]
public class MB2_LODCamera : MonoBehaviour
{
	private void Awake()
	{
		MB2_LODManager mb2_LODManager = MB2_LODManager.Manager();
		if (mb2_LODManager != null)
		{
			mb2_LODManager.AddCamera(this);
		}
	}

	private void OnDestroy()
	{
		MB2_LODManager mb2_LODManager = MB2_LODManager.Manager();
		if (mb2_LODManager != null)
		{
			mb2_LODManager.RemoveCamera(this);
		}
	}
}
