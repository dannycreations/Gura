using System;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public class LODClusterManagerGrid : LODClusterManager
	{
		public LODClusterManagerGrid(MB2_LODManager.BakerPrototype bp)
			: base(bp)
		{
		}

		public int gridSize
		{
			get
			{
				return this._gridSize;
			}
			set
			{
				if (this.clusters.Count > 0)
				{
					MB2_Log.Log(MB2_LogLevel.error, "Can't change the gridSize once clusters exist.", base.LOG_LEVEL);
				}
				else
				{
					this._gridSize = value;
				}
			}
		}

		public override LODCluster GetClusterFor(Vector3 p)
		{
			LODCluster lodcluster = this.GetClusterContaining(p);
			if (lodcluster == null)
			{
				lodcluster = new LODClusterGrid(new Bounds(new Vector3((float)this._gridSize * Mathf.Round(p.x / (float)this._gridSize), (float)this._gridSize * Mathf.Round(p.y / (float)this._gridSize), (float)this._gridSize * Mathf.Round(p.z / (float)this._gridSize)), new Vector3((float)this._gridSize, (float)this._gridSize, (float)this._gridSize)), this);
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					lodcluster.CheckIntegrity();
				}
				this.clusters.Add(lodcluster);
				if (base.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.Log(MB2_LogLevel.debug, string.Concat(new object[] { "Created new cell ", lodcluster, " to contain point ", p }), base.LOG_LEVEL);
				}
			}
			return lodcluster;
		}

		public void TranslateAllClusters(Vector3 translation)
		{
			for (int i = 0; i < this.clusters.Count; i++)
			{
				LODClusterGrid lodclusterGrid = (LODClusterGrid)this.clusters[i];
				lodclusterGrid._TranslateCluster(translation);
			}
		}

		public int _gridSize = 250;
	}
}
