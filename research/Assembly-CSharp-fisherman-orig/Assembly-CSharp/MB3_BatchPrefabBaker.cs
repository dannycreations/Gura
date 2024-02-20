﻿using System;
using UnityEngine;

public class MB3_BatchPrefabBaker : MonoBehaviour
{
	public MB3_BatchPrefabBaker.MB3_PrefabBakerRow[] prefabRows;

	public string outputPrefabFolder;

	[Serializable]
	public class MB3_PrefabBakerRow
	{
		public GameObject sourcePrefab;

		public GameObject resultPrefab;
	}
}
