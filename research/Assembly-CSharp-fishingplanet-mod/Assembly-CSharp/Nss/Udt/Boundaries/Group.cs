using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nss.Udt.Boundaries
{
	public class Group : MonoBehaviour
	{
		public SplineBend SplineBend
		{
			get
			{
				return this._splineBend;
			}
		}

		public Vector3 Centroid
		{
			get
			{
				if (this.segments.Count > 0)
				{
					Vector3 vector = Vector3.zero;
					for (int i = 0; i < this.segments.Count; i++)
					{
						vector += this.segments[i].Midpoint;
					}
					return vector / (float)this.segments.Count;
				}
				return Vector3.zero;
			}
		}

		public void Connect()
		{
			for (int i = 0; i < this.segments.Count - 1; i++)
			{
				this.segments[i].end = this.segments[i + 1].start;
			}
			if (this.isClosed && this.segments.Count > 2)
			{
				this.segments[this.segments.Count - 1].end = this.segments[0].start;
			}
		}

		public void Translate(Vector3 delta)
		{
			for (int i = 0; i < this.segments.Count; i++)
			{
				this.segments[i].start += delta;
				this.segments[i].end += delta;
			}
		}

		private void Awake()
		{
			if (this._splineBend != null)
			{
				Object.Destroy(this._splineBend.gameObject);
			}
			this.GenerateColliders();
		}

		private void FixedUpdate()
		{
			if (Application.isEditor && this.forceUpdate)
			{
				this.GenerateColliders();
			}
		}

		public Material GenerateMaterial()
		{
			Material material = new Material(Shader.Find("Custom/TranspUnlit"))
			{
				hideFlags = 61
			};
			material.color = this.color;
			return material;
		}

		public Mesh GenerateMesh()
		{
			List<Mesh> list = new List<Mesh>();
			for (int i = 0; i < this.segments.Count; i++)
			{
				list.Add(this.segments[i].GetMesh(this.height, this.depth, this.depthAnchor));
			}
			Mesh mesh;
			if (this.segments.Count == 1)
			{
				mesh = list[0];
			}
			else
			{
				CombineInstance[] array = new CombineInstance[list.Count];
				for (int j = 0; j < list.Count; j++)
				{
					array[j].mesh = list[j];
				}
				mesh = new Mesh
				{
					hideFlags = 61
				};
				mesh.CombineMeshes(array, true, false);
				list.ForEach(delegate(Mesh sm)
				{
					Object.DestroyImmediate(sm);
				});
			}
			return mesh;
		}

		public void GenerateColliders()
		{
			if (this.forceUpdate)
			{
				BoxCollider[] componentsInChildren = base.GetComponentsInChildren<BoxCollider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Object.DestroyImmediate(componentsInChildren[i].gameObject);
				}
			}
			for (int j = 0; j < this.segments.Count; j++)
			{
				GameObject gameObject = new GameObject(string.Format("{0}-{1}", base.name, this.segments[j].name));
				gameObject.transform.parent = base.gameObject.transform;
				gameObject.layer = base.gameObject.layer;
				gameObject.isStatic = true;
				BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
				if (!boxCollider.enabled)
				{
					LogHelper.Error("Found wall segment {0} with disabled collider!", new object[] { gameObject.name });
				}
				boxCollider.size = this.segments[j].GetSize(this.height, this.depth);
				boxCollider.center = this.segments[j].GetCenter(this.height, this.depth, this.depthAnchor);
				Quaternion yaxisRotation = this.segments[j].GetYAxisRotation();
				if (yaxisRotation != Quaternion.identity)
				{
					gameObject.transform.rotation = yaxisRotation;
				}
				gameObject.transform.Rotate(0f, 90f, 0f);
				gameObject.transform.position = this.segments[j].Midpoint;
			}
		}

		[SerializeField]
		private SplineBend _splineBend;

		public List<Segment> segments = new List<Segment>();

		public float height;

		public DepthAnchorTypes depthAnchor;

		public float depth;

		public bool isClosed;

		public Color color;

		[SerializeField]
		private bool forceUpdate;
	}
}
