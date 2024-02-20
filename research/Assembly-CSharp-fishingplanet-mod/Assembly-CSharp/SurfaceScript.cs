using System;
using UnityEngine;

public class SurfaceScript : MonoBehaviour
{
	private void Start()
	{
		Material material;
		if (base.transform.parent.GetComponent<MarkerScript>().objectScript.materialType == 0)
		{
			material = (Material)Object.Instantiate(Resources.Load("surfaceMaterial", typeof(Material)));
		}
		else
		{
			material = (Material)Object.Instantiate(Resources.Load("surfaceAlphaMaterial", typeof(Material)));
		}
		material.color.a = base.transform.parent.GetComponent<MarkerScript>().objectScript.surfaceOpacity;
		base.gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
	}
}
