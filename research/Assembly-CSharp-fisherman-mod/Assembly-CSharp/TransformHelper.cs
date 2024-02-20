using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransformHelper
{
	public static void ChangeLayersRecursively(Transform trans, string name)
	{
		trans.gameObject.layer = LayerMask.NameToLayer(name);
		IEnumerator enumerator = trans.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				TransformHelper.ChangeLayersRecursively(transform, name);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public static void ChangeLayersRecursively(Transform trans, int layer)
	{
		trans.gameObject.layer = layer;
		IEnumerator enumerator = trans.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				TransformHelper.ChangeLayersRecursively(transform, layer);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public static List<GameObject> FindObjectsByPath(string path)
	{
		string[] nodes = path.Split(new char[] { '/' });
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			if (nodes.Length == 1)
			{
				List<GameObject> list = rootGameObjects.Where((GameObject o) => o.name == nodes[0]).ToList<GameObject>();
				if (list.Count > 0)
				{
					return list;
				}
			}
			else
			{
				GameObject gameObject = rootGameObjects.FirstOrDefault((GameObject o) => o.name == nodes[0]);
				if (gameObject != null)
				{
					List<GameObject> list2 = TransformHelper.FindObjectByNodes(gameObject, nodes, 1);
					if (list2 != null)
					{
						return list2;
					}
				}
			}
		}
		return null;
	}

	private static List<GameObject> FindObjectByNodes(GameObject curObject, string[] nodes, int curIndex)
	{
		string text = nodes[curIndex];
		if (curIndex == nodes.Length - 1)
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < curObject.transform.childCount; i++)
			{
				Transform child = curObject.transform.GetChild(i);
				if (child.name == text)
				{
					list.Add(child.gameObject);
				}
			}
			return (list.Count <= 0) ? null : list;
		}
		for (int j = 0; j < curObject.transform.childCount; j++)
		{
			Transform child2 = curObject.transform.GetChild(j);
			if (child2.name == text)
			{
				return TransformHelper.FindObjectByNodes(child2.gameObject, nodes, curIndex + 1);
			}
		}
		return null;
	}

	public static Transform FindDeepChild(string childName)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				if (rootGameObjects[j].name == childName)
				{
					return rootGameObjects[j].transform;
				}
				Transform transform = TransformHelper.FindDeepChild(rootGameObjects[j].transform, childName);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}

	public static Transform FindDeepChild(Transform root, string childName)
	{
		int childCount = root.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (child.name == childName)
			{
				return child;
			}
			if (child.childCount > 0)
			{
				Transform transform = TransformHelper.FindDeepChild(child, childName);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}

	public static Dictionary<string, Transform> BuildSrcBonesMap(SkinnedMeshRenderer renderer)
	{
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		for (int i = 0; i < renderer.bones.Length; i++)
		{
			dictionary[renderer.bones[i].gameObject.name] = renderer.bones[i];
		}
		return dictionary;
	}

	public static void CopyBoneTransforms(SkinnedMeshRenderer renderer, Dictionary<string, Transform> scrBonesMap, Transform[] bonesCache)
	{
		Transform[] bones = renderer.bones;
		for (int i = 0; i < bones.Length; i++)
		{
			if (!(bones[i] == null))
			{
				GameObject gameObject = bones[i].gameObject;
				if (!scrBonesMap.TryGetValue(gameObject.name, out bonesCache[i]))
				{
					LogHelper.Error("Unable to map bone \"{0}\" to target skeleton in {1} object", new object[] { gameObject.name, renderer.name });
					break;
				}
			}
		}
		renderer.bones = bonesCache;
	}
}
