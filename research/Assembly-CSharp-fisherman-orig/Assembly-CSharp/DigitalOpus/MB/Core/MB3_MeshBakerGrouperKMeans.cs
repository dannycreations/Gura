using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public class MB3_MeshBakerGrouperKMeans : MB3_MeshBakerGrouperCore
	{
		public MB3_MeshBakerGrouperKMeans(GrouperData data)
		{
			this.d = data;
		}

		public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection)
		{
			Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
			List<GameObject> list = new List<GameObject>();
			int num = 20;
			foreach (GameObject gameObject in selection)
			{
				if (!(gameObject == null))
				{
					GameObject gameObject2 = gameObject;
					Renderer component = gameObject2.GetComponent<Renderer>();
					if (component is MeshRenderer || component is SkinnedMeshRenderer)
					{
						list.Add(gameObject2);
					}
				}
			}
			if (list.Count > 0 && num > 0 && num < list.Count)
			{
				MB3_KMeansClustering mb3_KMeansClustering = new MB3_KMeansClustering(list, num);
				mb3_KMeansClustering.Cluster();
				this.clusterCenters = new Vector3[num];
				this.clusterSizes = new float[num];
				for (int i = 0; i < num; i++)
				{
					List<Renderer> cluster = mb3_KMeansClustering.GetCluster(i, out this.clusterCenters[i], out this.clusterSizes[i]);
					if (cluster.Count > 0)
					{
						dictionary.Add("Cluster_" + i, cluster);
					}
				}
			}
			return dictionary;
		}

		public override void DrawGizmos(Bounds sceneObjectBounds)
		{
			if (this.clusterCenters != null && this.clusterSizes != null && this.clusterCenters.Length == this.clusterSizes.Length)
			{
				for (int i = 0; i < this.clusterSizes.Length; i++)
				{
					Gizmos.DrawWireSphere(this.clusterCenters[i], this.clusterSizes[i]);
				}
			}
		}

		public int numClusters = 4;

		public Vector3[] clusterCenters = new Vector3[0];

		public float[] clusterSizes = new float[0];
	}
}
