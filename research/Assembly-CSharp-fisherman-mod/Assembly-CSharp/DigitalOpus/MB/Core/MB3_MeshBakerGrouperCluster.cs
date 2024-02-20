using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public class MB3_MeshBakerGrouperCluster : MB3_MeshBakerGrouperCore
	{
		public MB3_MeshBakerGrouperCluster(GrouperData data, List<GameObject> gos)
		{
			this.d = data;
		}

		public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection)
		{
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			for (int i = 0; i < this._clustersToDraw.Count; i++)
			{
				MB3_AgglomerativeClustering.ClusterNode clusterNode = this._clustersToDraw[i];
				List<Renderer> list = new List<Renderer>();
				for (int j = 0; j < clusterNode.leafs.Length; j++)
				{
					Renderer component = this.cluster.clusters[clusterNode.leafs[j]].leaf.go.GetComponent<Renderer>();
					if (component is MeshRenderer || component is SkinnedMeshRenderer)
					{
						list.Add(component);
					}
				}
				dictionary.Add("Cluster_" + i, list);
			}
			return dictionary;
		}

		public void BuildClusters(List<GameObject> gos, ProgressUpdateCancelableDelegate progFunc)
		{
			MB3_MeshBakerGrouperCluster.<BuildClusters>c__AnonStorey0 <BuildClusters>c__AnonStorey = new MB3_MeshBakerGrouperCluster.<BuildClusters>c__AnonStorey0();
			<BuildClusters>c__AnonStorey.gos = gos;
			if (<BuildClusters>c__AnonStorey.gos.Count == 0)
			{
				Debug.LogWarning("No objects to cluster. Add some objects to the list of Objects To Combine.");
				return;
			}
			if (this.cluster == null)
			{
				this.cluster = new MB3_AgglomerativeClustering();
			}
			List<MB3_AgglomerativeClustering.item_s> list = new List<MB3_AgglomerativeClustering.item_s>();
			int i;
			for (i = 0; i < <BuildClusters>c__AnonStorey.gos.Count; i++)
			{
				if (<BuildClusters>c__AnonStorey.gos[i] != null && list.Find((MB3_AgglomerativeClustering.item_s x) => x.go == <BuildClusters>c__AnonStorey.gos[i]) == null)
				{
					Renderer component = <BuildClusters>c__AnonStorey.gos[i].GetComponent<Renderer>();
					if (component != null && (component is MeshRenderer || component is SkinnedMeshRenderer))
					{
						list.Add(new MB3_AgglomerativeClustering.item_s
						{
							go = <BuildClusters>c__AnonStorey.gos[i],
							coord = component.bounds.center
						});
					}
				}
			}
			this.cluster.items = list;
			this.cluster.agglomerate(progFunc);
			if (!this.cluster.wasCanceled)
			{
				float num;
				float num2;
				this._BuildListOfClustersToDraw(progFunc, out num, out num2);
				this.d.maxDistBetweenClusters = Mathf.Lerp(num, num2, 0.9f);
			}
		}

		private void _BuildListOfClustersToDraw(ProgressUpdateCancelableDelegate progFunc, out float smallest, out float largest)
		{
			this._clustersToDraw.Clear();
			if (this.cluster.clusters == null)
			{
				smallest = 1f;
				largest = 10f;
				return;
			}
			if (progFunc != null)
			{
				progFunc("Building Clusters To Draw A:", 0f);
			}
			List<MB3_AgglomerativeClustering.ClusterNode> list = new List<MB3_AgglomerativeClustering.ClusterNode>();
			largest = 1f;
			smallest = 10000000f;
			for (int i = 0; i < this.cluster.clusters.Length; i++)
			{
				MB3_AgglomerativeClustering.ClusterNode clusterNode = this.cluster.clusters[i];
				if (clusterNode.distToMergedCentroid <= this.d.maxDistBetweenClusters)
				{
					if (this.d.includeCellsWithOnlyOneRenderer)
					{
						this._clustersToDraw.Add(clusterNode);
					}
					else if (clusterNode.leaf == null)
					{
						this._clustersToDraw.Add(clusterNode);
					}
				}
				if (clusterNode.distToMergedCentroid > largest)
				{
					largest = clusterNode.distToMergedCentroid;
				}
				if (clusterNode.height > 0 && clusterNode.distToMergedCentroid < smallest)
				{
					smallest = clusterNode.distToMergedCentroid;
				}
			}
			if (progFunc != null)
			{
				progFunc("Building Clusters To Draw B:", 0f);
			}
			for (int j = 0; j < this._clustersToDraw.Count; j++)
			{
				list.Add(this._clustersToDraw[j].cha);
				list.Add(this._clustersToDraw[j].chb);
			}
			for (int k = 0; k < list.Count; k++)
			{
				this._clustersToDraw.Remove(list[k]);
			}
			this._radii = new float[this._clustersToDraw.Count];
			if (progFunc != null)
			{
				progFunc("Building Clusters To Draw C:", 0f);
			}
			for (int l = 0; l < this._radii.Length; l++)
			{
				MB3_AgglomerativeClustering.ClusterNode clusterNode2 = this._clustersToDraw[l];
				Bounds bounds;
				bounds..ctor(clusterNode2.centroid, Vector3.one);
				for (int m = 0; m < clusterNode2.leafs.Length; m++)
				{
					Renderer component = this.cluster.clusters[clusterNode2.leafs[m]].leaf.go.GetComponent<Renderer>();
					if (component != null)
					{
						bounds.Encapsulate(component.bounds);
					}
				}
				this._radii[l] = bounds.extents.magnitude;
			}
			if (progFunc != null)
			{
				progFunc("Building Clusters To Draw D:", 0f);
			}
			this._ObjsExtents = largest + 1f;
			this._minDistBetweenClusters = Mathf.Lerp(smallest, 0f, 0.9f);
			if (this._ObjsExtents < 2f)
			{
				this._ObjsExtents = 2f;
			}
		}

		public override void DrawGizmos(Bounds sceneObjectBounds)
		{
			if (this.cluster == null || this.cluster.clusters == null)
			{
				return;
			}
			if (this._lastMaxDistBetweenClusters != this.d.maxDistBetweenClusters)
			{
				float num;
				float num2;
				this._BuildListOfClustersToDraw(null, out num, out num2);
				this._lastMaxDistBetweenClusters = this.d.maxDistBetweenClusters;
			}
			for (int i = 0; i < this._clustersToDraw.Count; i++)
			{
				Gizmos.color = Color.white;
				MB3_AgglomerativeClustering.ClusterNode clusterNode = this._clustersToDraw[i];
				Gizmos.DrawWireSphere(clusterNode.centroid, this._radii[i]);
			}
		}

		public MB3_AgglomerativeClustering cluster;

		private float _lastMaxDistBetweenClusters;

		public float _ObjsExtents = 10f;

		public float _minDistBetweenClusters = 0.001f;

		private List<MB3_AgglomerativeClustering.ClusterNode> _clustersToDraw = new List<MB3_AgglomerativeClustering.ClusterNode>();

		private float[] _radii;
	}
}
