using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SWS
{
	public class WaypointManager : MonoBehaviour
	{
		private void Awake()
		{
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					WaypointManager.AddPath(transform.gameObject);
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
			DOTween.Init(null, null, null);
		}

		public static void AddPath(GameObject path)
		{
			if (path.name.Contains("(Clone)"))
			{
				path.name = path.name.Replace("(Clone)", string.Empty);
			}
			if (WaypointManager.Paths.ContainsKey(path.name))
			{
				Debug.LogWarning("Called AddPath() but Scene already contains Path " + path.name + ".");
				return;
			}
			PathManager componentInChildren = path.GetComponentInChildren<PathManager>();
			if (componentInChildren)
			{
				WaypointManager.Paths.Add(path.name, componentInChildren);
				return;
			}
			Debug.LogWarning("Called AddPath() but Transform " + path.name + " has no Path Component attached.");
		}

		private void OnDestroy()
		{
			WaypointManager.Paths.Clear();
		}

		public static void DrawStraight(Vector3[] waypoints)
		{
			for (int i = 0; i < waypoints.Length - 1; i++)
			{
				Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
			}
		}

		public static void DrawCurved(Vector3[] waypoints)
		{
			Vector3[] array = WaypointManager.BuildPathPoints(waypoints);
			Vector3 vector = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				Gizmos.DrawLine(array[i], vector);
				vector = array[i];
			}
		}

		public static Vector3[] BuildPathPoints(Vector3[] waypoints)
		{
			Vector3[] array = new Vector3[waypoints.Length + 2];
			waypoints.CopyTo(array, 1);
			array[0] = waypoints[1];
			array[array.Length - 1] = array[array.Length - 2];
			int num = array.Length * 10;
			Vector3[] array2 = new Vector3[num + 1];
			for (int i = 0; i <= num; i++)
			{
				float num2 = (float)i / (float)num;
				Vector3 point = WaypointManager.GetPoint(array, num2);
				array2[i] = point;
			}
			return array2;
		}

		public static Vector3 GetPoint(Vector3[] gizmoPoints, float t)
		{
			int num = gizmoPoints.Length - 3;
			int num2 = (int)Mathf.Floor(t * (float)num);
			int num3 = num - 1;
			if (num3 > num2)
			{
				num3 = num2;
			}
			float num4 = t * (float)num - (float)num3;
			Vector3 vector = gizmoPoints[num3];
			Vector3 vector2 = gizmoPoints[num3 + 1];
			Vector3 vector3 = gizmoPoints[num3 + 2];
			Vector3 vector4 = gizmoPoints[num3 + 3];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num4 * num4 * num4) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num4 * num4) + (-vector + vector3) * num4 + 2f * vector2);
		}

		public static float GetPathLength(Vector3[] waypoints)
		{
			float num = 0f;
			for (int i = 0; i < waypoints.Length - 1; i++)
			{
				num += Vector3.Distance(waypoints[i], waypoints[i + 1]);
			}
			return num;
		}

		public static List<Vector3> SmoothCurve(List<Vector3> pathToCurve, int interpolations)
		{
			if (interpolations < 1)
			{
				interpolations = 1;
			}
			int count = pathToCurve.Count;
			int num = count * Mathf.RoundToInt((float)interpolations) - 1;
			List<Vector3> list = new List<Vector3>(num);
			for (int i = 0; i < num + 1; i++)
			{
				float num2 = Mathf.InverseLerp(0f, (float)num, (float)i);
				List<Vector3> list2 = new List<Vector3>(pathToCurve);
				for (int j = count - 1; j > 0; j--)
				{
					for (int k = 0; k < j; k++)
					{
						list2[k] = (1f - num2) * list2[k] + num2 * list2[k + 1];
					}
				}
				list.Add(list2[0]);
			}
			return list;
		}

		public static readonly Dictionary<string, PathManager> Paths = new Dictionary<string, PathManager>();
	}
}
