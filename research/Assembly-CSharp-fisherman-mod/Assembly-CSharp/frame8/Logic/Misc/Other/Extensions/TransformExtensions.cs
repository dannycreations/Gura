using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class TransformExtensions
	{
		public static void GetComponentAtPath<T>(this Transform transform, string path, out T foundComponent) where T : Component
		{
			Transform transform2 = null;
			if (path == null)
			{
				IEnumerator enumerator = transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform3 = (Transform)obj;
						T component = transform3.GetComponent<T>();
						if (component != null)
						{
							foundComponent = component;
							return;
						}
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
			else
			{
				transform2 = transform.Find(path);
			}
			if (transform2 == null)
			{
				foundComponent = (T)((object)null);
			}
			else
			{
				foundComponent = transform2.GetComponent<T>();
			}
		}

		public static T GetComponentAtPath<T>(this Transform transform, string path) where T : Component
		{
			T t;
			transform.GetComponentAtPath(path, out t);
			return t;
		}

		public static Transform[] GetChildren(this Transform tr)
		{
			int childCount = tr.childCount;
			Transform[] array = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				array[i] = tr.GetChild(i);
			}
			return array;
		}

		public static void GetEnoughChildrenToFitInArray(this Transform tr, Transform[] array)
		{
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = tr.GetChild(i);
			}
		}

		public static List<Transform> GetDescendants(this Transform tr)
		{
			Transform[] children = tr.GetChildren();
			List<Transform> list = new List<Transform>();
			list.AddRange(children);
			int num = children.Length;
			for (int i = 0; i < num; i++)
			{
				list.AddRange(children[i].GetDescendants());
			}
			return list;
		}

		public static void GetDescendantsAndRelativePaths(this Transform tr, ref Dictionary<Transform, string> mapDescendantToPath)
		{
			tr.GetDescendantsAndRelativePaths(string.Empty, ref mapDescendantToPath);
		}

		private static void GetDescendantsAndRelativePaths(this Transform tr, string currentPath, ref Dictionary<Transform, string> mapDescendantToPath)
		{
			Transform[] children = tr.GetChildren();
			int num = children.Length;
			for (int i = 0; i < num; i++)
			{
				Transform transform = children[i];
				string text = currentPath + "/" + transform.name;
				mapDescendantToPath[transform] = text;
				transform.GetDescendantsAndRelativePaths(text, ref mapDescendantToPath);
			}
		}

		public static int GetNumberOfAncestors(this Transform tr)
		{
			int num = 0;
			while (tr = tr.parent)
			{
				num++;
			}
			return num;
		}
	}
}
