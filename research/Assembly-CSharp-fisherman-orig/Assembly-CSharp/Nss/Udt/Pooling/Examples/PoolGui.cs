using System;
using UnityEngine;

namespace Nss.Udt.Pooling.Examples
{
	public class PoolGui : MonoBehaviour
	{
		private void OnGUI()
		{
			if (GUILayout.Button("Spawn super cube", new GUILayoutOption[]
			{
				GUILayout.Width(200f),
				GUILayout.Height(100f)
			}))
			{
				GameObject next = PoolController.Instance.GetNext("super-cube", true);
				next.transform.position = Vector3.zero;
			}
		}
	}
}
