using System;
using System.Collections.Generic;
using UnityEngine;

public class MB3_KMeansClustering
{
	public MB3_KMeansClustering(List<GameObject> gos, int numClusters)
	{
		for (int i = 0; i < gos.Count; i++)
		{
			if (gos[i] != null)
			{
				MB3_KMeansClustering.DataPoint dataPoint = new MB3_KMeansClustering.DataPoint(gos[i]);
				this._normalizedDataToCluster.Add(dataPoint);
			}
			else
			{
				Debug.LogWarning(string.Format("Object {0} in list of objects to cluster was null.", i));
			}
		}
		if (numClusters <= 0)
		{
			Debug.LogError("Number of clusters must be posititve.");
			numClusters = 1;
		}
		if (this._normalizedDataToCluster.Count <= numClusters)
		{
			Debug.LogError("There must be fewer clusters than objects to cluster");
			numClusters = this._normalizedDataToCluster.Count - 1;
		}
		this._numberOfClusters = numClusters;
		if (this._numberOfClusters <= 0)
		{
			this._numberOfClusters = 1;
		}
		this._clusters = new Vector3[this._numberOfClusters];
	}

	private void InitializeCentroids()
	{
		for (int i = 0; i < this._numberOfClusters; i++)
		{
			this._normalizedDataToCluster[i].Cluster = i;
		}
		for (int j = this._numberOfClusters; j < this._normalizedDataToCluster.Count; j++)
		{
			this._normalizedDataToCluster[j].Cluster = Random.Range(0, this._numberOfClusters);
		}
	}

	private bool UpdateDataPointMeans(bool force)
	{
		if (this.AnyAreEmpty(this._normalizedDataToCluster) && !force)
		{
			return false;
		}
		Vector3[] array = new Vector3[this._numberOfClusters];
		int[] array2 = new int[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			int cluster = this._normalizedDataToCluster[i].Cluster;
			array[cluster] += this._normalizedDataToCluster[i].center;
			array2[cluster]++;
		}
		for (int j = 0; j < this._numberOfClusters; j++)
		{
			this._clusters[j] = array[j] / (float)array2[j];
		}
		return true;
	}

	private bool AnyAreEmpty(List<MB3_KMeansClustering.DataPoint> data)
	{
		int[] array = new int[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			array[this._normalizedDataToCluster[i].Cluster]++;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] == 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool UpdateClusterMembership()
	{
		bool flag = false;
		float[] array = new float[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			for (int j = 0; j < this._numberOfClusters; j++)
			{
				array[j] = this.ElucidanDistance(this._normalizedDataToCluster[i], this._clusters[j]);
			}
			int num = this.MinIndex(array);
			if (num != this._normalizedDataToCluster[i].Cluster)
			{
				flag = true;
				this._normalizedDataToCluster[i].Cluster = num;
			}
		}
		return flag;
	}

	private float ElucidanDistance(MB3_KMeansClustering.DataPoint dataPoint, Vector3 mean)
	{
		return Vector3.Distance(dataPoint.center, mean);
	}

	private int MinIndex(float[] distances)
	{
		int num = 0;
		double num2 = (double)distances[0];
		for (int i = 0; i < distances.Length; i++)
		{
			if ((double)distances[i] < num2)
			{
				num2 = (double)distances[i];
				num = i;
			}
		}
		return num;
	}

	public List<Renderer> GetCluster(int idx, out Vector3 mean, out float size)
	{
		if (idx < 0 || idx >= this._numberOfClusters)
		{
			Debug.LogError("idx is out of bounds");
			mean = Vector3.zero;
			size = 1f;
			return new List<Renderer>();
		}
		this.UpdateDataPointMeans(true);
		List<Renderer> list = new List<Renderer>();
		mean = this._clusters[idx];
		float num = 0f;
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			if (this._normalizedDataToCluster[i].Cluster == idx)
			{
				float num2 = Vector3.Distance(mean, this._normalizedDataToCluster[i].center);
				if (num2 > num)
				{
					num = num2;
				}
				list.Add(this._normalizedDataToCluster[i].gameObject.GetComponent<Renderer>());
			}
		}
		mean = this._clusters[idx];
		size = num;
		return list;
	}

	public void Cluster()
	{
		bool flag = true;
		bool flag2 = true;
		this.InitializeCentroids();
		int num = this._normalizedDataToCluster.Count * 1000;
		int num2 = 0;
		while (flag2 && flag && num2 < num)
		{
			num2++;
			flag2 = this.UpdateDataPointMeans(false);
			flag = this.UpdateClusterMembership();
		}
	}

	private List<MB3_KMeansClustering.DataPoint> _normalizedDataToCluster = new List<MB3_KMeansClustering.DataPoint>();

	private Vector3[] _clusters = new Vector3[0];

	private int _numberOfClusters;

	private class DataPoint
	{
		public DataPoint(GameObject go)
		{
			this.gameObject = go;
			this.center = go.transform.position;
			if (go.GetComponent<Renderer>() == null)
			{
				Debug.LogError("Object does not have a renderer " + go);
			}
		}

		public Vector3 center;

		public GameObject gameObject;

		public int Cluster;
	}
}
