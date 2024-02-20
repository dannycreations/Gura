using System;
using System.Collections.Generic;
using UnityEngine;

public class GBSettings : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		if (this.gizmoActive)
		{
			if (this.raycastMode == GBSettings.RaycastMode.coneCast)
			{
				if (!this.delete)
				{
					Gizmos.color = Color.green;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawWireSphere(this.gizmoPos, this.brushSize);
			}
			else if (this.raycastMode == GBSettings.RaycastMode.sphereCast)
			{
				if (!this.delete)
				{
					Gizmos.color = Color.cyan;
				}
				else
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawWireSphere(this.gizmoPos, 0.3f);
			}
		}
	}

	public float brushSize = 10f;

	public bool fireFromCamera;

	public float minScale = 1f;

	public float maxScale = 1f;

	public float spacing = 5f;

	public bool preventOverlap;

	public float yOffset;

	public bool alignToNormal = true;

	public bool randomRotX;

	public bool randomRotY;

	public bool randomRotZ;

	public bool brushActive = true;

	public Vector3 randomRotation = new Vector3(0f, 360f, 0f);

	public bool delete;

	public GBSettings.RaycastMode raycastMode;

	public GameObject parentObject;

	public float minBrushSize = 0.1f;

	public float maxBrushSize = 8f;

	public float minMinScale = 0.1f;

	public float maxMinScale = 5f;

	public float minMaxScale = 0.1f;

	public float maxMaxScale = 5f;

	public float minYOffset = -2.5f;

	public float maxYOffset = 2.5f;

	public float minSpacing = 0.1f;

	public float maxSpacing = 50f;

	public Vector3 gizmoPos;

	public bool gizmoActive;

	public List<GameObject> activeGeometry;

	public List<string> activeGeometryPaths;

	public enum RaycastMode
	{
		coneCast,
		sphereCast
	}
}
