using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public abstract class LODClusterBase : LODCluster
	{
		public LODClusterBase(LODClusterManager m)
		{
			this.manager = m;
			this._lastAdjustForMaxAllowedFrame = -1;
			this.manager.GetFreshCombiner(this);
		}

		public int nextCheckFrame
		{
			get
			{
				return this._nextCheckFrame;
			}
			set
			{
				this._nextCheckFrame = value;
			}
		}

		public List<LODCombinedMesh> GetCombiners()
		{
			return new List<LODCombinedMesh>(this.combinedMeshes);
		}

		public abstract bool Contains(Vector3 v);

		public abstract bool Intersects(Bounds b);

		public abstract bool Intersects(Plane[][] fustrum);

		public abstract Vector3 Center();

		public abstract void DrawGizmos();

		public abstract bool IsVisible();

		public abstract float DistSquaredToPlayer();

		public virtual void Destroy()
		{
			for (int i = this.combinedMeshes.Count - 1; i >= 0; i--)
			{
				this.combinedMeshes[i].Destroy();
			}
		}

		public virtual void Clear()
		{
			for (int i = this.combinedMeshes.Count - 1; i >= 0; i--)
			{
				this.combinedMeshes[i].Clear();
				this.combinedMeshes[i].combinedMesh.resultSceneObject.name = this.combinedMeshes[i].combinedMesh.resultSceneObject.name + "-recycled";
				this.manager.RecycleCluster(this.combinedMeshes[i]);
			}
		}

		public virtual void CheckIntegrity()
		{
			for (int i = 0; i < this.combinedMeshes.Count; i++)
			{
				this.combinedMeshes[i].CheckIntegrity();
				if (this.combinedMeshes[i].GetLODCluster() != this)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"Cluster was a child of this cell ",
						i,
						" but its parent was another cell. num ",
						this.combinedMeshes.Count,
						" ",
						this.combinedMeshes[i].GetLODCluster()
					}));
				}
				for (int j = 0; j < this.combinedMeshes.Count; j++)
				{
					if (i != j && this.combinedMeshes[i] == this.combinedMeshes[j])
					{
						Debug.LogError("same cluster has been added twice.");
					}
				}
			}
		}

		public virtual LODClusterManager GetClusterManager()
		{
			return this.manager;
		}

		public virtual void RemoveAndRecycleCombiner(LODCombinedMesh cl)
		{
			this.combinedMeshes.Remove(cl);
			if (this.combinedMeshes.Contains(cl))
			{
				Debug.LogError("removed but still contains.");
			}
			this.manager.RecycleCluster(cl);
		}

		public virtual void AddCombiner(LODCombinedMesh cl)
		{
			if (!this.combinedMeshes.Contains(cl))
			{
				this.combinedMeshes.Add(cl);
			}
			else
			{
				Debug.LogError("error in AddCombiner");
			}
		}

		public virtual LODCombinedMesh SuggestCombiner()
		{
			LODCombinedMesh lodcombinedMesh = this.combinedMeshes[0];
			int num = lodcombinedMesh.GetNumVertsInMesh() + lodcombinedMesh.GetApproxNetVertsInQs();
			for (int i = 1; i < this.combinedMeshes.Count; i++)
			{
				int num2 = this.combinedMeshes[i].GetNumVertsInMesh() + this.combinedMeshes[i].GetApproxNetVertsInQs();
				if (num > num2)
				{
					num = num2;
					lodcombinedMesh = this.combinedMeshes[i];
				}
			}
			return lodcombinedMesh;
		}

		public virtual void AssignLODToCombiner(MB2_LOD l)
		{
			if (MB2_LODManager.CHECK_INTEGRITY && !this.combinedMeshes.Contains(l.GetCombiner()))
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Error in AssignLODToCombiner ",
					l,
					" combiner ",
					l.GetCombiner(),
					" is not in this LODCluster this=",
					this,
					" other=",
					l.GetCombiner().GetLODCluster()
				}));
			}
			l.GetCombiner().AssignToCombiner(l);
		}

		public virtual void UpdateSkinnedMeshApproximateBounds()
		{
			Debug.LogError("Grid combinedMeshes cannot be used for skinned meshes");
		}

		public virtual void PrePrioritize(Plane[][] fustrum, Vector3[] cameraPositions)
		{
		}

		public virtual HashSet<LODCombinedMesh> AdjustForMaxAllowedPerLevel()
		{
			if (this._lastAdjustForMaxAllowedFrame == Time.frameCount)
			{
				return null;
			}
			int[] maxNumberPerLevel = this.manager.GetBakerPrototype().maxNumberPerLevel;
			if (maxNumberPerLevel == null || maxNumberPerLevel.Length == 0)
			{
				return null;
			}
			HashSet<LODCombinedMesh> hashSet = new HashSet<LODCombinedMesh>();
			List<MB2_LOD> list = new List<MB2_LOD>();
			for (int i = 0; i < this.combinedMeshes.Count; i++)
			{
				this.combinedMeshes[i].GetObjectsThatWillBeInMesh(list);
			}
			list.Sort(new MB2_LOD.MB2_LODDistToCamComparer());
			HashSet<MB2_LOD>[] array = new HashSet<MB2_LOD>[maxNumberPerLevel.Length];
			int num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				num += maxNumberPerLevel[j];
				array[j] = new HashSet<MB2_LOD>();
			}
			HashSet<MB2_LOD> hashSet2 = new HashSet<MB2_LOD>();
			int num2 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				int nextLevelIdx = list[k].nextLevelIdx;
				bool flag = false;
				if (nextLevelIdx < array.Length && num2 < num)
				{
					for (int l = nextLevelIdx; l < array.Length; l++)
					{
						if (array[l].Count < maxNumberPerLevel[l])
						{
							array[l].Add(list[k]);
							num2++;
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					hashSet2.Add(list[k]);
				}
			}
			if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug)
			{
				string text = string.Format("AdjustForMaxAllowedPerLevel objsThatWillBeInMesh={0}\n", list.Count);
				for (int m = 0; m < array.Length; m++)
				{
					text += string.Format("b{0} capacity={1} contains={2}\n", m, maxNumberPerLevel[m], array[m].Count);
				}
				text += string.Format("b[leftovers] contains={0}\n", hashSet2.Count);
				MB2_Log.Log(MB2_LogLevel.info, text, this.GetClusterManager().LOG_LEVEL);
			}
			for (int n = 1; n < array.Length; n++)
			{
				foreach (MB2_LOD mb2_LOD in array[n])
				{
					if (mb2_LOD.nextLevelIdx != n)
					{
						if (n >= mb2_LOD.levels.Length)
						{
							if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
							{
								MB2_Log.Log(MB2_LogLevel.trace, string.Format("A Demoting obj in bucket={0} obj={1}", n, mb2_LOD), this.GetClusterManager().LOG_LEVEL);
							}
							mb2_LOD.AdjustNextLevelIndex(n);
							hashSet.Add(mb2_LOD.GetCombiner());
						}
						else
						{
							if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
							{
								MB2_Log.Log(MB2_LogLevel.trace, string.Format("B Demoting obj in bucket={0} obj={1}", n, mb2_LOD), this.GetClusterManager().LOG_LEVEL);
							}
							mb2_LOD.AdjustNextLevelIndex(n);
							hashSet.Add(mb2_LOD.GetCombiner());
						}
					}
				}
			}
			int num3 = array.Length - 1;
			foreach (MB2_LOD mb2_LOD2 in hashSet2)
			{
				if (mb2_LOD2.nextLevelIdx <= num3)
				{
					if (num3 >= mb2_LOD2.levels.Length - 1)
					{
						if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
						{
							MB2_Log.Log(MB2_LogLevel.trace, string.Format("C Demoting obj in bucket={0} obj={1}", num3 + 1, mb2_LOD2), this.GetClusterManager().LOG_LEVEL);
						}
						mb2_LOD2.AdjustNextLevelIndex(num3 + 1);
						hashSet.Add(mb2_LOD2.GetCombiner());
					}
					else
					{
						if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
						{
							MB2_Log.Log(MB2_LogLevel.trace, string.Format("D Demoting obj in bucket={0} obj={1}", num3 + 1, mb2_LOD2), this.GetClusterManager().LOG_LEVEL);
						}
						mb2_LOD2.AdjustNextLevelIndex(num3 + 1);
						hashSet.Add(mb2_LOD2.GetCombiner());
					}
				}
			}
			this._lastAdjustForMaxAllowedFrame = Time.frameCount;
			return hashSet;
		}

		public virtual void ForceCheckIfLODsChanged()
		{
			for (int i = 0; i < this.combinedMeshes.Count; i++)
			{
				this.combinedMeshes[i].Update();
			}
		}

		public LODClusterManager manager;

		private int _nextCheckFrame;

		private int _lastAdjustForMaxAllowedFrame;

		protected List<LODCombinedMesh> combinedMeshes = new List<LODCombinedMesh>();
	}
}
