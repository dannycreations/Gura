using System;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public class LODClusterMoving : LODClusterBase
	{
		public LODClusterMoving(LODClusterManagerMoving m)
			: base(m)
		{
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

		public virtual void UpdateBounds()
		{
			this.b = this.combinedMeshes[0].CalcBounds();
			for (int i = 1; i < this.combinedMeshes.Count; i++)
			{
				this.b.Encapsulate(this.combinedMeshes[i].CalcBounds());
			}
		}

		public override void UpdateSkinnedMeshApproximateBounds()
		{
			for (int i = 0; i < this.combinedMeshes.Count; i++)
			{
				this.combinedMeshes[i].UpdateSkinnedMeshApproximateBounds();
			}
		}

		public Bounds b;

		public bool isVisible;

		public float distSquaredToPlayer = float.PositiveInfinity;

		public int lastPrePrioritizeFrame = -1;
	}
}
