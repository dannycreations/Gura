using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public abstract class LODClusterManager
	{
		public LODClusterManager(MB2_LODManager.BakerPrototype bp)
		{
			this._bakerPrototype = bp;
		}

		public MB2_LogLevel LOG_LEVEL
		{
			get
			{
				return this._LOG_LEVEL;
			}
			set
			{
				this._LOG_LEVEL = value;
			}
		}

		public virtual MB2_LODManager.BakerPrototype GetBakerPrototype()
		{
			return this._bakerPrototype;
		}

		public virtual void Destroy()
		{
			for (int i = this.clusters.Count - 1; i >= 0; i--)
			{
				this.clusters[i].Destroy();
			}
			this.clusters.Clear();
			this.recycledClusters.Clear();
		}

		public virtual LODCluster GetClusterContaining(Vector3 v)
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				if (this.clusters[i].Contains(v))
				{
					return this.clusters[i];
				}
			}
			return null;
		}

		public virtual void RemoveCluster(Bounds b)
		{
			LODCluster clusterIntersecting = this.GetClusterIntersecting(b);
			if (clusterIntersecting != null)
			{
				clusterIntersecting.Clear();
				this.clusters.Remove(clusterIntersecting);
			}
		}

		public virtual void Clear()
		{
			for (int i = this.clusters.Count - 1; i >= 0; i--)
			{
				this.clusters[i].Clear();
				this.clusters.RemoveAt(i);
			}
		}

		public virtual void RecycleCluster(LODCombinedMesh c)
		{
			if (c == null)
			{
				return;
			}
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.Log(MB2_LogLevel.debug, "LODClusterManagerGrid.RecycleCluster", this.LOG_LEVEL);
			}
			c.Clear();
			c.cluster = null;
			if (!this.recycledClusters.Contains(c))
			{
				this.recycledClusters.Add(c);
			}
			if (c.combinedMesh.resultSceneObject != null)
			{
				MB2_Version.SetActiveRecursively(c.combinedMesh.resultSceneObject, false);
			}
		}

		public virtual void DrawGizmos()
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				this.clusters[i].DrawGizmos();
			}
		}

		public virtual void CheckIntegrity()
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				this.clusters[i].CheckIntegrity();
			}
			for (int j = 0; j < this.recycledClusters.Count; j++)
			{
				this.recycledClusters[j].CheckIntegrity();
			}
		}

		public virtual LODCombinedMesh GetFreshCombiner(LODCluster cell)
		{
			LODCombinedMesh lodcombinedMesh;
			if (this.recycledClusters.Count > 0)
			{
				lodcombinedMesh = this.recycledClusters[this.recycledClusters.Count - 1];
				this.recycledClusters.RemoveAt(this.recycledClusters.Count - 1);
				lodcombinedMesh.SetLODCluster(cell);
			}
			else
			{
				lodcombinedMesh = new LODCombinedMesh(this._bakerPrototype.meshBaker, cell);
			}
			if (lodcombinedMesh.combinedMesh.resultSceneObject != null)
			{
				MB2_Version.SetActiveRecursively(lodcombinedMesh.combinedMesh.resultSceneObject, true);
			}
			cell.AddCombiner(lodcombinedMesh);
			lodcombinedMesh.numFramesBetweenChecks = -1;
			lodcombinedMesh.numFramesBetweenChecksOffset = -1;
			if (lodcombinedMesh.combinedMesh != null && lodcombinedMesh.combinedMesh.resultSceneObject != null)
			{
				lodcombinedMesh.combinedMesh.resultSceneObject.name = lodcombinedMesh.combinedMesh.resultSceneObject.name.Replace("-recycled", string.Empty);
			}
			cell.nextCheckFrame = Time.frameCount + 1;
			return lodcombinedMesh;
		}

		public virtual void UpdateSkinnedMeshApproximateBounds()
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				this.clusters[i].UpdateSkinnedMeshApproximateBounds();
			}
		}

		public abstract LODCluster GetClusterFor(Vector3 p);

		public virtual void ForceCheckIfLODsChanged()
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				this.clusters[i].ForceCheckIfLODsChanged();
			}
		}

		private LODCluster GetClusterIntersecting(Bounds b)
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				if (this.clusters[i].Intersects(b))
				{
					return this.clusters[i];
				}
			}
			return null;
		}

		public MB2_LogLevel _LOG_LEVEL = MB2_LogLevel.info;

		public MB2_LODManager.BakerPrototype _bakerPrototype;

		public List<LODCluster> clusters = new List<LODCluster>();

		protected List<LODCombinedMesh> recycledClusters = new List<LODCombinedMesh>();
	}
}
