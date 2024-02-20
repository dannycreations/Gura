using System;
using System.Diagnostics;
using UnityEngine;

public class GUITools : MonoBehaviour
{
	private static void Deactivate(Transform t)
	{
		GUITools.SetActiveSelf(t.gameObject, false);
	}

	public static void SetActive(GameObject go, bool state)
	{
		GUITools.SetActive(go, state, true);
	}

	public static void SetActive(GameObject go, bool state, bool compatibilityMode)
	{
		if (go)
		{
			if (state)
			{
				GUITools.Activate(go.transform, compatibilityMode);
			}
			else
			{
				GUITools.Deactivate(go.transform);
			}
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static void SetActiveSelf(GameObject go, bool state)
	{
		go.SetActive(state);
	}

	private static void Activate(Transform t)
	{
		GUITools.Activate(t, false);
	}

	private static void Activate(Transform t, bool compatibilityMode)
	{
		GUITools.SetActiveSelf(t.gameObject, true);
		if (compatibilityMode)
		{
			int i = 0;
			int childCount = t.childCount;
			while (i < childCount)
			{
				Transform child = t.GetChild(i);
				if (child.gameObject.activeSelf)
				{
					return;
				}
				i++;
			}
			int j = 0;
			int childCount2 = t.childCount;
			while (j < childCount2)
			{
				Transform child2 = t.GetChild(j);
				GUITools.Activate(child2, true);
				j++;
			}
		}
	}

	public static GameObject AddChild(GameObject parent, GameObject prefab)
	{
		GameObject gameObject;
		if (parent != null)
		{
			gameObject = Object.Instantiate<GameObject>(prefab, parent.transform, true);
		}
		else
		{
			gameObject = Object.Instantiate<GameObject>(prefab);
		}
		if (gameObject != null && parent != null)
		{
			Transform transform = gameObject.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			gameObject.layer = parent.layer;
		}
		return gameObject;
	}
}
