using System;
using UnityEngine;

[Serializable]
public class AnimatedObject
{
	public GameObject obj;

	public int layersCount;

	public bool isAllTimeActiveObject;

	public int syncLayerIndex;

	public bool isLogEnabled;

	public CAModelSettings debugSettings;
}
