using System;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public class LODClusterGrid : LODClusterBase
	{
		public LODClusterGrid(Bounds b, LODClusterManagerGrid m)
			: base(m)
		{
			this.b = b;
		}

		public override bool Contains(Vector3 v)
		{
			return this.b.Contains(v);
		}

		public override bool Intersects(Bounds b)
		{
			return b.Intersects(b);
		}

		public override bool Intersects(Plane[][] fustrum)
		{
			for (int i = 0; i < fustrum.Length; i++)
			{
				if (GeometryUtility.TestPlanesAABB(fustrum[i], this.b))
				{
					return true;
				}
			}
			return false;
		}

		public override Vector3 Center()
		{
			return this.b.center;
		}

		public override bool IsVisible()
		{
			return this.isVisible;
		}

		public override float DistSquaredToPlayer()
		{
			return this.distSquaredToPlayer;
		}

		public override void PrePrioritize(Plane[][] fustrum, Vector3[] cameraPositions)
		{
			if (this.lastPrePrioritizeFrame == Time.frameCount)
			{
				return;
			}
			this.isVisible = false;
			this.distSquaredToPlayer = float.PositiveInfinity;
			for (int i = 0; i < cameraPositions.Length; i++)
			{
				float sqrMagnitude = (cameraPositions[i] - this.b.center).sqrMagnitude;
				if (this.distSquaredToPlayer > sqrMagnitude)
				{
					this.distSquaredToPlayer = sqrMagnitude;
				}
			}
			this.lastPrePrioritizeFrame = Time.frameCount;
		}

		public override void DrawGizmos()
		{
			Gizmos.DrawWireCube(this.b.center, this.b.size);
		}

		public override string ToString()
		{
			return "LODClusterGrid " + this.b.ToString();
		}

		public override void UpdateSkinnedMeshApproximateBounds()
		{
			Debug.LogError("Grid clusters cannot be used for skinned meshes");
		}

		public void _TranslateCluster(Vector3 translation)
		{
			this.b.center = this.b.center + translation;
			for (int i = 0; i < this.combinedMeshes.Count; i++)
			{
				LODCombinedMesh lodcombinedMesh = this.combinedMeshes[i];
				lodcombinedMesh._TranslateLODs(translation);
			}
		}

		public Bounds b;

		public bool isVisible;

		public float distSquaredToPlayer = float.PositiveInfinity;

		public int lastPrePrioritizeFrame = -1;
	}
}
