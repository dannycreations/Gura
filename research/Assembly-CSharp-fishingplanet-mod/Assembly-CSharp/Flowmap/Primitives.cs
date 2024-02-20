using System;
using UnityEngine;

namespace Flowmap
{
	public static class Primitives
	{
		public static Mesh PlaneMesh
		{
			get
			{
				if (!Primitives.planeMesh)
				{
					Primitives.planeMesh = new Mesh();
					Primitives.planeMesh.name = "Plane";
					Primitives.planeMesh.vertices = new Vector3[]
					{
						new Vector3(-0.5f, 0f, -0.5f),
						new Vector3(0.5f, 0f, -0.5f),
						new Vector3(-0.5f, 0f, 0.5f),
						new Vector3(0.5f, 0f, 0.5f)
					};
					Primitives.planeMesh.uv = new Vector2[]
					{
						new Vector2(0f, 0f),
						new Vector2(1f, 0f),
						new Vector2(0f, 1f),
						new Vector2(1f, 1f)
					};
					Primitives.planeMesh.normals = new Vector3[]
					{
						Vector3.up,
						Vector3.up,
						Vector3.up,
						Vector3.up
					};
					Primitives.planeMesh.triangles = new int[] { 2, 1, 0, 3, 1, 2 };
					Primitives.planeMesh.tangents = new Vector4[]
					{
						new Vector4(1f, 0f, 0f, 1f),
						new Vector4(1f, 0f, 0f, 1f),
						new Vector4(1f, 0f, 0f, 1f),
						new Vector4(1f, 0f, 0f, 1f)
					};
					Primitives.planeMesh.hideFlags = 61;
				}
				return Primitives.planeMesh;
			}
		}

		private static Mesh planeMesh;
	}
}
