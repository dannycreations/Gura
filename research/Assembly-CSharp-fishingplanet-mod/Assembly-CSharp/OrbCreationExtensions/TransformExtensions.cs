using System;
using System.Collections;
using UnityEngine;

namespace OrbCreationExtensions
{
	public static class TransformExtensions
	{
		public static bool IsPartOf(this Transform trans, Transform aTransform)
		{
			return trans == aTransform || (trans.parent != null && trans.parent != trans && trans.parent.IsPartOf(aTransform));
		}

		public static Transform FindFirstChildWithName(this Transform trans, string childName)
		{
			if (trans.gameObject.name == childName)
			{
				return trans;
			}
			IEnumerator enumerator = trans.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					Transform transform2 = transform.FindFirstChildWithName(childName);
					if (transform2 != null)
					{
						return transform2;
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
			return null;
		}

		public static Transform FindFirstChildWhereNameContains(this Transform trans, string childName)
		{
			if (trans.gameObject.name.IndexOf(childName) >= 0)
			{
				return trans;
			}
			IEnumerator enumerator = trans.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					Transform transform2 = transform.FindFirstChildWhereNameContains(childName);
					if (transform2 != null)
					{
						return transform2;
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
			return null;
		}

		public static T GetFirstComponentInParents<T>(this Transform trans) where T : MonoBehaviour
		{
			T component = trans.gameObject.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			if (trans.parent != null && trans.parent != trans)
			{
				return trans.parent.GetFirstComponentInParents<T>();
			}
			return (T)((object)null);
		}

		public static Vector3 PointToWorldSpace(this Transform trans, Vector3 p)
		{
			return trans.TransformPoint(p);
		}

		public static Vector3 PointToLocalSpace(this Transform trans, Vector3 p)
		{
			return trans.InverseTransformPoint(p);
		}
	}
}
