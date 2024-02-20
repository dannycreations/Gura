using System;
using System.Collections.Generic;
using UnityEngine;

public class RenderersHelper
{
	public static SkinnedMeshRenderer GetRendererForObject(Transform root)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			SkinnedMeshRenderer component = root.GetChild(i).GetComponent<SkinnedMeshRenderer>();
			if (component != null && !component.Equals(null))
			{
				return component;
			}
		}
		return null;
	}

	public static Renderer GetRenderer(Transform root)
	{
		Renderer renderer = RenderersHelper.GetRendererForObject(root);
		if (renderer == null)
		{
			renderer = root.GetComponent<MeshRenderer>();
		}
		return renderer;
	}

	public static T GetRendererForObject<T>(Transform root) where T : class
	{
		for (int i = 0; i < root.childCount; i++)
		{
			T component = root.GetChild(i).GetComponent<T>();
			if (component != null && !component.Equals(null))
			{
				return component;
			}
		}
		return (T)((object)null);
	}

	public static List<T> GetAllRenderersForObject<T>(Transform root)
	{
		List<T> list = new List<T>();
		RenderersHelper.AddRenderers<T>(root, list);
		return list;
	}

	private static void AddRenderers<T>(Transform root, List<T> renderers)
	{
		T component = root.gameObject.GetComponent<T>();
		if (component != null && !component.Equals(null))
		{
			renderers.Add(component);
		}
		for (int i = 0; i < root.childCount; i++)
		{
			RenderersHelper.AddRenderers<T>(root.GetChild(i), renderers);
		}
	}
}
