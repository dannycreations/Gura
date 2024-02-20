using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceSettings : MonoBehaviour
{
	public static Terrain Terrain
	{
		get
		{
			return SurfaceSettings._terrain;
		}
	}

	public static float LowestBottomY { get; private set; }

	public static SurfaceSettings Instance { get; private set; }

	private void Awake()
	{
		SurfaceSettings.Instance = this;
		SurfaceSettings._terrain = base.GetComponent<Terrain>();
		Terrain[] array = Object.FindObjectsOfType<Terrain>();
		SurfaceSettings.LowestBottomY = array[0].terrainData.bounds.max.y + array[0].transform.position.y;
		for (int i = 0; i < array.Length; i++)
		{
			float num = array[i].terrainData.bounds.min.y + array[i].transform.position.y;
			if (num < SurfaceSettings.LowestBottomY)
			{
				SurfaceSettings.LowestBottomY = num;
			}
		}
	}

	private void OnDestroy()
	{
		SurfaceSettings._terrain = null;
	}

	public static SurfaceMaterial GetMaterialByName(string name)
	{
		SurfaceMaterial surfaceMaterial;
		return (!SurfaceSettings._materialNameToMaterialType.TryGetValue(name, out surfaceMaterial)) ? SurfaceMaterial.NONE : surfaceMaterial;
	}

	public SurfaceMaterial GetMaterialForLayer(int layerIndex)
	{
		return (layerIndex >= this._layersOrder.Length) ? SurfaceMaterial.NONE : this._layersOrder[layerIndex];
	}

	public SurfaceMaterial GetMaterial(RaycastHit hitInfo, int stepPixelsSize)
	{
		if (hitInfo.transform != null)
		{
			Terrain component = hitInfo.transform.gameObject.GetComponent<Terrain>();
			if (component != null)
			{
				TerrainData terrainData = component.terrainData;
				float num = hitInfo.textureCoord.x * (float)terrainData.alphamapWidth;
				float num2 = hitInfo.textureCoord.y * (float)terrainData.alphamapHeight;
				float[,,] alphamaps = terrainData.GetAlphamaps(Math.Max(0, (int)((double)num - (double)stepPixelsSize * 0.5)), Math.Max(0, (int)((double)num2 - (double)stepPixelsSize * 0.5)), stepPixelsSize, stepPixelsSize);
				float[] array = new float[alphamaps.GetLength(2)];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = 0f;
					for (int j = 0; j < stepPixelsSize; j++)
					{
						for (int k = 0; k < stepPixelsSize; k++)
						{
							array[i] += alphamaps[j, k, i];
						}
					}
				}
				int num3 = 0;
				float num4 = array[0];
				for (int l = 0; l < array.Length; l++)
				{
					if (num4 < array[l])
					{
						num3 = l;
						num4 = array[l];
					}
				}
				if (num3 < this._layersOrder.Length)
				{
					return this._layersOrder[num3];
				}
			}
			else if (hitInfo.collider != null)
			{
				return SurfaceSettings.GetMaterialByName(hitInfo.collider.material.name);
			}
		}
		return SurfaceMaterial.NONE;
	}

	[SerializeField]
	private SurfaceMaterial[] _layersOrder;

	private static Terrain _terrain;

	private static Dictionary<string, SurfaceMaterial> _materialNameToMaterialType = new Dictionary<string, SurfaceMaterial>
	{
		{
			"concrete (Instance)",
			SurfaceMaterial.CONCRETE
		},
		{
			"grass (Instance)",
			SurfaceMaterial.GRASS
		},
		{
			"sand (Instance)",
			SurfaceMaterial.SAND
		},
		{
			"stone (Instance)",
			SurfaceMaterial.STONE
		},
		{
			"wood (Instance)",
			SurfaceMaterial.WOOD
		},
		{
			"ground (Instance)",
			SurfaceMaterial.GROUND
		},
		{
			"snow (Instance)",
			SurfaceMaterial.SNOW
		}
	};
}
