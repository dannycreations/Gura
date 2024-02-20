using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public class MB3_MeshBakerGrouperPie : MB3_MeshBakerGrouperCore
	{
		public MB3_MeshBakerGrouperPie(GrouperData data)
		{
			this.d = data;
		}

		public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection)
		{
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			if (this.d.pieNumSegments == 0)
			{
				Debug.LogError("pieNumSegments must be greater than zero.");
				return dictionary;
			}
			if (this.d.pieAxis.magnitude <= 1E-06f)
			{
				Debug.LogError("Pie axis must have length greater than zero.");
				return dictionary;
			}
			this.d.pieAxis.Normalize();
			Quaternion quaternion = Quaternion.FromToRotation(this.d.pieAxis, Vector3.up);
			Debug.Log("Collecting renderers in each cell");
			foreach (GameObject gameObject in selection)
			{
				if (!(gameObject == null))
				{
					GameObject gameObject2 = gameObject;
					Renderer component = gameObject2.GetComponent<Renderer>();
					if (component is MeshRenderer || component is SkinnedMeshRenderer)
					{
						Vector3 vector = component.bounds.center - this.d.origin;
						vector.Normalize();
						vector = quaternion * vector;
						float num;
						if (Mathf.Abs(vector.x) < 0.0001f && Mathf.Abs(vector.z) < 0.0001f)
						{
							num = 0f;
						}
						else
						{
							num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
							if (num < 0f)
							{
								num = 360f + num;
							}
						}
						int num2 = Mathf.FloorToInt(num / 360f * (float)this.d.pieNumSegments);
						string text = "seg_" + num2;
						List<Renderer> list;
						if (dictionary.ContainsKey(text))
						{
							list = dictionary[text];
						}
						else
						{
							list = new List<Renderer>();
							dictionary.Add(text, list);
						}
						if (!list.Contains(component))
						{
							list.Add(component);
						}
					}
				}
			}
			return dictionary;
		}

		public override void DrawGizmos(Bounds sourceObjectBounds)
		{
			if (this.d.pieAxis.magnitude < 0.1f)
			{
				return;
			}
			if (this.d.pieNumSegments < 1)
			{
				return;
			}
			float magnitude = sourceObjectBounds.extents.magnitude;
			MB3_MeshBakerGrouperPie.DrawCircle(this.d.pieAxis, this.d.origin, magnitude, 24);
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, this.d.pieAxis);
			Quaternion quaternion2 = Quaternion.AngleAxis(180f / (float)this.d.pieNumSegments, Vector3.up);
			Vector3 vector = Vector3.forward;
			for (int i = 0; i < this.d.pieNumSegments; i++)
			{
				Vector3 vector2 = quaternion * vector;
				Gizmos.DrawLine(this.d.origin, this.d.origin + vector2 * magnitude);
				vector = quaternion2 * vector;
				vector = quaternion2 * vector;
			}
		}

		public static void DrawCircle(Vector3 axis, Vector3 center, float radius, int subdiv)
		{
			Quaternion quaternion = Quaternion.AngleAxis((float)(360 / subdiv), axis);
			Vector3 vector;
			vector..ctor(axis.y, -axis.x, axis.z);
			vector.Normalize();
			vector *= radius;
			for (int i = 0; i < subdiv + 1; i++)
			{
				Vector3 vector2 = quaternion * vector;
				Gizmos.DrawLine(center + vector, center + vector2);
				vector = vector2;
			}
		}
	}
}
