using System;
using System.Collections.Generic;
using UnityEngine;

public class CFX_SpawnSystem : MonoBehaviour
{
	public static GameObject GetNextObject(GameObject sourceObj, bool activateObject = true)
	{
		int instanceID = sourceObj.GetInstanceID();
		if (!CFX_SpawnSystem.instance.poolCursors.ContainsKey(instanceID))
		{
			Debug.LogError(string.Concat(new object[] { "[CFX_SpawnSystem.GetNextObject()] Object hasn't been preloaded: ", sourceObj.name, " (ID:", instanceID, ")\n" }), CFX_SpawnSystem.instance);
			return null;
		}
		int num = CFX_SpawnSystem.instance.poolCursors[instanceID];
		GameObject gameObject;
		if (CFX_SpawnSystem.instance.onlyGetInactiveObjects)
		{
			int num2 = num;
			for (;;)
			{
				gameObject = CFX_SpawnSystem.instance.instantiatedObjects[instanceID][num];
				CFX_SpawnSystem.instance.increasePoolCursor(instanceID);
				num = CFX_SpawnSystem.instance.poolCursors[instanceID];
				if (gameObject != null && !gameObject.activeSelf)
				{
					break;
				}
				if (num == num2)
				{
					goto Block_5;
				}
			}
			goto IL_15A;
			Block_5:
			if (!CFX_SpawnSystem.instance.instantiateIfNeeded)
			{
				Debug.LogWarning("[CFX_SpawnSystem.GetNextObject()] There are no active instances available in the pool for \"" + sourceObj.name + "\"\nYou may need to increase the preloaded object count for this prefab?", CFX_SpawnSystem.instance);
				return null;
			}
			Debug.Log("[CFX_SpawnSystem.GetNextObject()] A new instance has been created for \"" + sourceObj.name + "\" because no active instance were found in the pool.\n", CFX_SpawnSystem.instance);
			CFX_SpawnSystem.PreloadObject(sourceObj, 1);
			List<GameObject> list = CFX_SpawnSystem.instance.instantiatedObjects[instanceID];
			gameObject = list[list.Count - 1];
			IL_15A:;
		}
		else
		{
			gameObject = CFX_SpawnSystem.instance.instantiatedObjects[instanceID][num];
			CFX_SpawnSystem.instance.increasePoolCursor(instanceID);
		}
		if (activateObject && gameObject != null)
		{
			gameObject.SetActive(true);
		}
		return gameObject;
	}

	public static void PreloadObject(GameObject sourceObj, int poolSize = 1)
	{
		CFX_SpawnSystem.instance.addObjectToPool(sourceObj, poolSize);
	}

	public static void UnloadObjects(GameObject sourceObj)
	{
		CFX_SpawnSystem.instance.removeObjectsFromPool(sourceObj);
	}

	public static bool AllObjectsLoaded
	{
		get
		{
			return CFX_SpawnSystem.instance.allObjectsLoaded;
		}
	}

	private void addObjectToPool(GameObject sourceObject, int number)
	{
		int instanceID = sourceObject.GetInstanceID();
		if (!this.instantiatedObjects.ContainsKey(instanceID))
		{
			this.instantiatedObjects.Add(instanceID, new List<GameObject>());
			this.poolCursors.Add(instanceID, 0);
		}
		for (int i = 0; i < number; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(sourceObject);
			gameObject.SetActive(false);
			CFX_AutoDestructShuriken[] componentsInChildren = gameObject.GetComponentsInChildren<CFX_AutoDestructShuriken>(true);
			foreach (CFX_AutoDestructShuriken cfx_AutoDestructShuriken in componentsInChildren)
			{
				cfx_AutoDestructShuriken.OnlyDeactivate = true;
			}
			CFX_LightIntensityFade[] componentsInChildren2 = gameObject.GetComponentsInChildren<CFX_LightIntensityFade>(true);
			foreach (CFX_LightIntensityFade cfx_LightIntensityFade in componentsInChildren2)
			{
				cfx_LightIntensityFade.autodestruct = false;
			}
			this.instantiatedObjects[instanceID].Add(gameObject);
			if (this.hideObjectsInHierarchy)
			{
				gameObject.hideFlags = 1;
			}
			if (this.spawnAsChildren)
			{
				gameObject.transform.parent = base.transform;
			}
		}
	}

	private void removeObjectsFromPool(GameObject sourceObject)
	{
		int instanceID = sourceObject.GetInstanceID();
		if (!this.instantiatedObjects.ContainsKey(instanceID))
		{
			Debug.LogWarning(string.Concat(new object[] { "[CFX_SpawnSystem.removeObjectsFromPool()] There aren't any preloaded object for: ", sourceObject.name, " (ID:", instanceID, ")\n" }), base.gameObject);
			return;
		}
		for (int i = this.instantiatedObjects[instanceID].Count - 1; i >= 0; i--)
		{
			GameObject gameObject = this.instantiatedObjects[instanceID][i];
			this.instantiatedObjects[instanceID].RemoveAt(i);
			Object.Destroy(gameObject);
		}
		this.instantiatedObjects.Remove(instanceID);
		this.poolCursors.Remove(instanceID);
	}

	private void increasePoolCursor(int uniqueId)
	{
		Dictionary<int, int> dictionary;
		(dictionary = CFX_SpawnSystem.instance.poolCursors)[uniqueId] = dictionary[uniqueId] + 1;
		if (CFX_SpawnSystem.instance.poolCursors[uniqueId] >= CFX_SpawnSystem.instance.instantiatedObjects[uniqueId].Count)
		{
			CFX_SpawnSystem.instance.poolCursors[uniqueId] = 0;
		}
	}

	private void Awake()
	{
		if (CFX_SpawnSystem.instance != null)
		{
			Debug.LogWarning("CFX_SpawnSystem: There should only be one instance of CFX_SpawnSystem per Scene!\n", base.gameObject);
		}
		CFX_SpawnSystem.instance = this;
	}

	private void Start()
	{
		this.allObjectsLoaded = false;
		for (int i = 0; i < this.objectsToPreload.Length; i++)
		{
			CFX_SpawnSystem.PreloadObject(this.objectsToPreload[i], this.objectsToPreloadTimes[i]);
		}
		this.allObjectsLoaded = true;
	}

	private static CFX_SpawnSystem instance;

	public GameObject[] objectsToPreload = new GameObject[0];

	public int[] objectsToPreloadTimes = new int[0];

	public bool hideObjectsInHierarchy;

	public bool spawnAsChildren = true;

	public bool onlyGetInactiveObjects;

	public bool instantiateIfNeeded;

	private bool allObjectsLoaded;

	private Dictionary<int, List<GameObject>> instantiatedObjects = new Dictionary<int, List<GameObject>>();

	private Dictionary<int, int> poolCursors = new Dictionary<int, int>();
}
