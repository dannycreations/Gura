using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public class LODCombinedMesh
	{
		public LODCombinedMesh(MB3_MeshBaker meshBaker, LODCluster cell)
		{
			this.cluster = cell;
			this.combinedMesh = new MB3_MultiMeshCombiner();
			this.combinedMesh.maxVertsInMesh = cell.GetClusterManager().GetBakerPrototype().maxVerticesPerCombinedMesh;
			this.SetMBValues(meshBaker);
			this.lodTransactions = new Dictionary<int, LODCombinedMesh.Transaction>();
			this.gosInCombiner = new HashSet<MB2_LOD>();
			this.gosAssignedToMe = new HashSet<MB2_LOD>();
			this.numVertsInMesh = 0;
			this.numApproxVertsInQ = 0;
		}

		public virtual LODClusterManager GetClusterManager()
		{
			return this.cluster.GetClusterManager();
		}

		public void UpdateSkinnedMeshApproximateBounds()
		{
			if (this.combinedMesh.renderType != MB_RenderType.skinnedMeshRenderer)
			{
				Debug.LogWarning("Should not call UpdateSkinnedMeshApproximateBounds on a non skinned combined mesh");
				return;
			}
			for (int i = 0; i < this.combinedMesh.meshCombiners.Count; i++)
			{
				MB3_MeshCombiner mb3_MeshCombiner = this.combinedMesh.meshCombiners[i].combinedMesh;
				if (mb3_MeshCombiner != null)
				{
					this.combinedMesh.meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBounds();
				}
			}
		}

		public virtual void ForceBakeImmediately()
		{
			if (this.numBakeImmediately == 0)
			{
				this.numBakeImmediately = 1;
			}
		}

		public virtual int GetNumVertsInMesh()
		{
			return this.numVertsInMesh;
		}

		public virtual int GetApproxNetVertsInQs()
		{
			return this.numApproxVertsInQ;
		}

		public virtual void SetLODCluster(LODCluster c)
		{
			this.cluster = c;
		}

		public virtual LODCluster GetLODCluster()
		{
			return this.cluster;
		}

		public virtual bool IsVisible()
		{
			return this.cluster.IsVisible();
		}

		public virtual int NumDirty()
		{
			return this.lodTransactions.Count;
		}

		public virtual int NumBakeImmediately()
		{
			return this.numBakeImmediately;
		}

		public virtual float DistSquaredToPlayer()
		{
			return this.cluster.DistSquaredToPlayer();
		}

		public void AssignToCombiner(MB2_LOD lod)
		{
			if (lod.GetCombiner() != this)
			{
				Debug.LogError("LOD was assigned to a different cluster.");
				return;
			}
			this.gosAssignedToMe.Add(lod);
		}

		public void UnassignFromCombiner(MB2_LOD lod)
		{
			this.gosAssignedToMe.Remove(lod);
		}

		public bool IsAssignedToThis(MB2_LOD lod)
		{
			return this.gosAssignedToMe.Contains(lod);
		}

		private void _CancelTransaction(MB2_LOD lod)
		{
			if (!lod.isInQueue)
			{
				Debug.LogError("Cancel transaction should never be called for LODs not in Q");
			}
			LODCombinedMesh.Transaction transaction = default(LODCombinedMesh.Transaction);
			transaction.action = lod.action;
			transaction.toIdx = lod.nextLevelIdx;
			transaction.lod = lod;
			LODCombinedMesh.Transaction transaction2;
			if (this.lodTransactions.TryGetValue(lod.gameObject.GetInstanceID(), out transaction2))
			{
				if (transaction2.action == MB2_LODOperation.toAdd)
				{
					this.numApproxVertsInQ -= lod.GetNumVerts(transaction2.toIdx);
				}
				if (transaction2.action == MB2_LODOperation.update)
				{
					this.numApproxVertsInQ -= lod.GetNumVerts(transaction2.toIdx);
					this.numApproxVertsInQ += lod.GetNumVerts(lod.currentLevelIdx);
				}
				if (transaction2.action == MB2_LODOperation.delete)
				{
					this.numApproxVertsInQ += transaction2.inMeshNumVerts;
				}
				this.lodTransactions.Remove(lod.gameObject.GetInstanceID());
				lod.OnRemoveFromQueue();
			}
			else
			{
				Debug.LogError("An LOD thought it was in the Q but it wasn't");
			}
		}

		private LODCombinedMesh.Transaction _AddTransaction(MB2_LOD lod)
		{
			LODCombinedMesh.Transaction transaction = default(LODCombinedMesh.Transaction);
			transaction.action = lod.action;
			transaction.toIdx = lod.nextLevelIdx;
			transaction.lod = lod;
			if (lod.isInCombined && lod.action == MB2_LODOperation.delete)
			{
				transaction.inMeshGameObjectID = lod.GetGameObjectID(lod.currentLevelIdx);
				transaction.inMeshNumVerts = lod.GetNumVerts(lod.currentLevelIdx);
			}
			LODCombinedMesh.Transaction transaction2;
			if (this.lodTransactions.TryGetValue(lod.gameObject.GetInstanceID(), out transaction2))
			{
				if (transaction2.action == MB2_LODOperation.toAdd)
				{
					this.numApproxVertsInQ -= lod.GetNumVerts(transaction2.toIdx);
				}
				if (transaction2.action == MB2_LODOperation.update)
				{
					this.numApproxVertsInQ -= lod.GetNumVerts(transaction2.toIdx);
					this.numApproxVertsInQ += lod.GetNumVerts(lod.currentLevelIdx);
				}
				if (transaction2.action == MB2_LODOperation.delete)
				{
					this.numApproxVertsInQ += transaction2.inMeshNumVerts;
				}
			}
			if (lod.action == MB2_LODOperation.toAdd)
			{
				this.numApproxVertsInQ += lod.GetNumVerts(lod.nextLevelIdx);
			}
			if (lod.action == MB2_LODOperation.update)
			{
				this.numApproxVertsInQ -= lod.GetNumVerts(lod.currentLevelIdx);
				this.numApproxVertsInQ += lod.GetNumVerts(lod.nextLevelIdx);
			}
			if (lod.action == MB2_LODOperation.delete)
			{
				this.numApproxVertsInQ -= lod.GetNumVerts(lod.currentLevelIdx);
			}
			if (lod.action == MB2_LODOperation.delete && !lod.isInCombined)
			{
				this.lodTransactions.Remove(lod.gameObject.GetInstanceID());
				lod.OnRemoveFromQueue();
			}
			else
			{
				this.lodTransactions[lod.gameObject.GetInstanceID()] = transaction;
				lod.OnAddToQueue();
			}
			return transaction;
		}

		public void LODCancelTransaction(MB2_LOD lod)
		{
			if (lod.GetCombiner() != this)
			{
				Debug.LogError("Wrong combiner");
			}
			if (this.cluster.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
			{
				MB2_Log.Log(MB2_LogLevel.trace, string.Concat(new object[] { "LODManager.LODCancelTransaction ", lod, " action ", lod.action }), MB2_LogLevel.trace);
			}
			this._CancelTransaction(lod);
		}

		public void LODChanged(MB2_LOD lod, bool immediate)
		{
			if (lod.GetCombiner() != this)
			{
				Debug.LogError("Wrong combiner");
			}
			if (this.cluster.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
			{
				MB2_Log.Log(MB2_LogLevel.trace, string.Concat(new object[] { "LODManager.LODChanged ", lod, " action ", lod.action }), MB2_LogLevel.trace);
			}
			this._AddTransaction(lod);
			MB2_LODManager.Manager().AddDirtyCombinedMesh(this);
		}

		public void RemoveLOD(MB2_LOD lod, bool immediate = true)
		{
			if (this.cluster.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
			{
				MB2_Log.Log(MB2_LogLevel.trace, "LODManager.RemoveLOD " + lod, MB2_LogLevel.trace);
			}
			if (!lod.isInQueue && !lod.isInCombined)
			{
				Debug.LogError(string.Concat(new object[] { "RemoveLOD: lod is not in combined or is in queue ", lod.isInQueue, " ", lod.isInCombined }));
				return;
			}
			if (lod.action != MB2_LODOperation.delete)
			{
				Debug.LogError("Action must be delete");
				return;
			}
			if (lod.isInQueue || lod.isInCombined)
			{
				this._AddTransaction(lod);
			}
			MB2_LODManager.Manager().AddDirtyCombinedMesh(this);
		}

		public void _TranslateLODs(Vector3 translation)
		{
			foreach (MB2_LOD mb2_LOD in this.gosAssignedToMe)
			{
				mb2_LOD._ResetPositionMarker();
			}
			if (this.combinedMesh.resultSceneObject != null)
			{
				Vector3 position = this.combinedMesh.resultSceneObject.transform.position;
				this.combinedMesh.resultSceneObject.transform.position = position + translation;
			}
			this.wasTranslated = true;
		}

		private float GetFullRatio()
		{
			int num = this.GetNumVertsInMesh() + this.GetApproxNetVertsInQs();
			return (float)num / (float)this.combinedMesh.maxVertsInMesh;
		}

		public virtual void Bake()
		{
			this._BakeWithSplitAndMerge();
		}

		private void _BakeWithoutSplitAndMerge()
		{
			HashSet<LODCombinedMesh> hashSet = this.cluster.AdjustForMaxAllowedPerLevel();
			this.BakeClusterCombiner();
			if (hashSet != null)
			{
				foreach (LODCombinedMesh lodcombinedMesh in hashSet)
				{
					if (lodcombinedMesh.cluster == this)
					{
						lodcombinedMesh.BakeClusterCombiner();
					}
				}
			}
		}

		private void _BakeWithSplitAndMerge()
		{
			HashSet<LODCombinedMesh> hashSet = this.cluster.AdjustForMaxAllowedPerLevel();
			bool flag = false;
			float num = this.GetFullRatio();
			if (num > LODCombinedMesh.splitCombinerThreshold)
			{
				LODCombinedMesh.LODCombinerSplitterMerger.SplitCombiner(this);
				flag = true;
				MB2_LODManager.Manager().statNumSplit++;
			}
			else if (num < LODCombinedMesh.mergeCombinerThreshold)
			{
				List<LODCombinedMesh> combiners = this.cluster.GetCombiners();
				for (int i = 0; i < combiners.Count; i++)
				{
					if (combiners[i] != this && combiners[i].GetFullRatio() < LODCombinedMesh.mergeCombinerThreshold)
					{
						LODCombinedMesh.LODCombinerSplitterMerger.MergeCombiner(this.cluster);
						flag = true;
						MB2_LODManager.Manager().statNumMerge++;
						break;
					}
					num = this.GetFullRatio();
					if (num > LODCombinedMesh.mergeCombinerThreshold)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				this.BakeClusterCombiner();
			}
			if (hashSet != null)
			{
				foreach (LODCombinedMesh lodcombinedMesh in hashSet)
				{
					if (lodcombinedMesh.cluster == this)
					{
						lodcombinedMesh.BakeClusterCombiner();
					}
				}
			}
		}

		public virtual void SetMBValues(MB3_MeshBaker mb)
		{
			MB3_MeshCombiner meshCombiner = mb.meshCombiner;
			this.combinedMesh.renderType = meshCombiner.renderType;
			this.combinedMesh.outputOption = MB2_OutputOptions.bakeIntoSceneObject;
			this.combinedMesh.lightmapOption = meshCombiner.lightmapOption;
			this.combinedMesh.textureBakeResults = meshCombiner.textureBakeResults;
			this.combinedMesh.doNorm = meshCombiner.doNorm;
			this.combinedMesh.doTan = meshCombiner.doTan;
			this.combinedMesh.doCol = meshCombiner.doCol;
			this.combinedMesh.doUV = meshCombiner.doUV;
			this.combinedMesh.doUV1 = meshCombiner.doUV1;
		}

		public virtual bool IsDirty()
		{
			return this.lodTransactions.Count > 0;
		}

		public virtual void PrePrioritize(Plane[][] fustrum, Vector3[] cameraPositions)
		{
			if (this.cluster == null)
			{
				Debug.LogError("cluster is null");
			}
			if (fustrum == null)
			{
				Debug.LogError("fustrum is null");
			}
			if (cameraPositions == null)
			{
				Debug.LogError("camPositions null");
			}
			this.cluster.PrePrioritize(fustrum, cameraPositions);
		}

		public virtual Bounds CalcBounds()
		{
			Bounds bounds;
			bounds..ctor(Vector3.zero, Vector3.one);
			if (this.gosAssignedToMe.Count > 0)
			{
				bool flag = false;
				foreach (MB2_LOD mb2_LOD in this.gosAssignedToMe)
				{
					if (mb2_LOD != null && MB2_Version.GetActive(mb2_LOD.gameObject))
					{
						if (flag)
						{
							bounds.Encapsulate(mb2_LOD.transform.position);
						}
						else
						{
							flag = true;
							float dimension = mb2_LOD.levels[0].dimension;
							bounds..ctor(mb2_LOD.transform.position, new Vector3(dimension, dimension, dimension));
						}
					}
				}
				if (!flag && this.cluster.GetClusterManager()._LOG_LEVEL >= MB2_LogLevel.info)
				{
					Debug.Log("CalcBounds called on a CombinedMesh that contained no valid LODs");
				}
			}
			else if (this.cluster.GetClusterManager()._LOG_LEVEL >= MB2_LogLevel.info)
			{
				Debug.Log("CalcBounds called on a CombinedMesh that contained no valid LODs");
			}
			return bounds;
		}

		private void ClearBake()
		{
			this.lodTransactions.Clear();
			this.gosInCombiner.Clear();
			this.combinedMesh.AddDeleteGameObjects(null, this.combinedMesh.GetObjectsInCombined().ToArray(), true);
			this.combinedMesh.Apply();
			this.numApproxVertsInQ = 0;
			this.numVertsInMesh = 0;
			this.numBakeImmediately = 0;
		}

		private void BakeClusterCombiner()
		{
			MB2_Log.Log(MB2_LogLevel.debug, string.Format("Bake called on cluster numTransactions={0}", this.lodTransactions.Count), this.GetClusterManager().LOG_LEVEL);
			if (this.lodTransactions.Count > 0)
			{
				List<int> list = new List<int>();
				List<GameObject> list2 = new List<GameObject>();
				List<MB2_LOD> list3 = new List<MB2_LOD>();
				List<MB2_LOD> list4 = new List<MB2_LOD>();
				List<MB2_LOD> list5 = new List<MB2_LOD>();
				foreach (int num in this.lodTransactions.Keys)
				{
					LODCombinedMesh.Transaction transaction = this.lodTransactions[num];
					if (transaction.action == MB2_LODOperation.toAdd)
					{
						list2.Add(transaction.lod.GetRendererGameObject(transaction.toIdx));
						list3.Add(transaction.lod);
					}
					else if (transaction.action == MB2_LODOperation.update)
					{
						list.Add(transaction.lod.GetGameObjectID(transaction.lod.currentLevelIdx));
						list2.Add(transaction.lod.GetRendererGameObject(transaction.toIdx));
						list5.Add(transaction.lod);
					}
					else if (transaction.action == MB2_LODOperation.delete)
					{
						list.Add(transaction.inMeshGameObjectID);
						if (transaction.lod != null)
						{
							list4.Add(transaction.lod);
						}
					}
				}
				if (this.wasTranslated)
				{
					if (this.combinedMesh.resultSceneObject != null)
					{
						this.combinedMesh.resultSceneObject.transform.position = Vector3.zero;
					}
					foreach (MB2_LOD mb2_LOD in this.gosInCombiner)
					{
						if (mb2_LOD.isInCombined && !list3.Contains(mb2_LOD) && !list4.Contains(mb2_LOD) && !list5.Contains(mb2_LOD))
						{
							list2.Add(mb2_LOD.GetRendererGameObject(mb2_LOD.currentLevelIdx));
							list.Add(mb2_LOD.GetRendererGameObject(mb2_LOD.currentLevelIdx).GetInstanceID());
						}
					}
					this.wasTranslated = false;
				}
				this.combinedMesh.AddDeleteGameObjectsByID(list2.ToArray(), list.ToArray(), true);
				this.combinedMesh.Apply();
				this.numApproxVertsInQ = 0;
				this.numVertsInMesh = 0;
				for (int i = 0; i < this.combinedMesh.meshCombiners.Count; i++)
				{
					this.numVertsInMesh += this.combinedMesh.meshCombiners[i].combinedMesh.GetMesh().vertexCount;
				}
				if (this.combinedMesh.resultSceneObject != null)
				{
					MB2_LODManager.BakerPrototype bakerPrototype = this.GetClusterManager().GetBakerPrototype();
					Transform transform = this.combinedMesh.resultSceneObject.transform;
					for (int j = 0; j < transform.childCount; j++)
					{
						GameObject gameObject = transform.GetChild(j).gameObject;
						gameObject.layer = bakerPrototype.layer;
						Renderer component = gameObject.GetComponent<Renderer>();
						component.castShadows = bakerPrototype.castShadow;
						component.receiveShadows = bakerPrototype.receiveShadow;
					}
				}
				MB2_LODManager mb2_LODManager = MB2_LODManager.Manager();
				mb2_LODManager.statTotalNumBakes++;
				mb2_LODManager.statLastNumBakes++;
				mb2_LODManager.statLastBakeFrame = Time.frameCount;
				if (this.combinedMesh.renderType == MB_RenderType.skinnedMeshRenderer)
				{
					for (int k = 0; k < this.combinedMesh.meshCombiners.Count; k++)
					{
						MB3_MeshCombiner mb3_MeshCombiner = this.combinedMesh.meshCombiners[k].combinedMesh;
						if (mb3_MeshCombiner != null)
						{
							SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)mb3_MeshCombiner.targetRenderer;
							bool updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
							skinnedMeshRenderer.updateWhenOffscreen = true;
							skinnedMeshRenderer.updateWhenOffscreen = updateWhenOffscreen;
						}
					}
				}
				this.lodTransactions.Clear();
				this.numBakeImmediately = 0;
				for (int l = 0; l < list4.Count; l++)
				{
					list4[l].OnBakeRemoved();
					this.gosInCombiner.Remove(list4[l]);
				}
				for (int m = 0; m < list3.Count; m++)
				{
					list3[m].OnBakeAdded();
					this.gosInCombiner.Add(list3[m]);
				}
				for (int n = 0; n < list5.Count; n++)
				{
					list5[n].OnBakeUpdated();
					this.gosInCombiner.Add(list5[n]);
				}
				if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
				{
					MB2_Log.Log(MB2_LogLevel.trace, string.Concat(new object[]
					{
						"Bake complete, num in combined ",
						this.combinedMesh.GetNumObjectsInCombined(),
						" fullRatio:",
						this.GetFullRatio().ToString("F5")
					}), this.GetClusterManager().LOG_LEVEL);
				}
			}
		}

		public virtual void Destroy()
		{
			this.combinedMesh.DestroyMesh();
			if (this.combinedMesh.resultSceneObject != null)
			{
				Object.Destroy(this.combinedMesh.resultSceneObject);
				this.combinedMesh.resultSceneObject = null;
			}
			this.cluster = null;
		}

		public virtual void GetObjectsThatWillBeInMesh(List<MB2_LOD> objsThatWillBeInMesh)
		{
			foreach (MB2_LOD mb2_LOD in this.gosInCombiner)
			{
				objsThatWillBeInMesh.Add(mb2_LOD);
			}
			foreach (int num in this.lodTransactions.Keys)
			{
				LODCombinedMesh.Transaction transaction = this.lodTransactions[num];
				if (transaction.action == MB2_LODOperation.toAdd)
				{
					objsThatWillBeInMesh.Add(transaction.lod);
				}
				if (transaction.action == MB2_LODOperation.delete)
				{
					objsThatWillBeInMesh.Remove(transaction.lod);
				}
			}
		}

		public virtual void Clear()
		{
			List<GameObject> objectsInCombined = this.combinedMesh.GetObjectsInCombined();
			if (this.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.Log(MB2_LogLevel.debug, string.Concat(new object[]
				{
					"Clear called on grid cluster num in combined ",
					objectsInCombined.Count,
					" numTrans=",
					this.lodTransactions.Count,
					" numAssignedToMe ",
					this.gosAssignedToMe.Count
				}), this.GetClusterManager().LOG_LEVEL);
			}
			this.combinedMesh.AddDeleteGameObjects(null, objectsInCombined.ToArray(), true);
			this.combinedMesh.Apply();
			foreach (MB2_LOD mb2_LOD in this.gosInCombiner)
			{
				if (mb2_LOD != null)
				{
					mb2_LOD.Clear();
				}
			}
			foreach (MB2_LOD mb2_LOD2 in this.gosAssignedToMe)
			{
				if (mb2_LOD2 != null)
				{
					mb2_LOD2.Clear();
				}
			}
			this.lodTransactions.Clear();
			this.gosInCombiner.Clear();
			this.gosAssignedToMe.Clear();
			this.numVertsInMesh = 0;
			this.numBakeImmediately = 0;
		}

		public virtual bool Contains(MB2_LOD lod)
		{
			return this.lodTransactions.ContainsKey(lod.GetInstanceID()) || this.gosInCombiner.Contains(lod);
		}

		public virtual void CheckIntegrity()
		{
			foreach (MB2_LOD mb2_LOD in this.gosAssignedToMe)
			{
				if (mb2_LOD.GetCombiner() != this)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"LOD ",
						mb2_LOD,
						" thinks it is in a different ",
						mb2_LOD.GetCombiner(),
						" than it is \n log dump",
						mb2_LOD.myLog.Dump()
					}));
				}
			}
			foreach (MB2_LOD mb2_LOD2 in this.gosInCombiner)
			{
				if (!mb2_LOD2.isInCombined)
				{
					Debug.LogError(mb2_LOD2 + "LOD thought it was in combined but wasn't\n log dump" + mb2_LOD2.myLog.Dump());
				}
				if (mb2_LOD2.action == MB2_LODOperation.toAdd)
				{
					Debug.LogError("bad lod action\n log dump" + mb2_LOD2.myLog.Dump());
				}
				if (!this.gosAssignedToMe.Contains(mb2_LOD2))
				{
					Debug.LogError(string.Concat(new object[]
					{
						"in combiner was not in assigned ",
						mb2_LOD2.GetCombiner(),
						" an it is \n log dump",
						mb2_LOD2.myLog.Dump()
					}));
				}
			}
			List<GameObject> objectsInCombined = this.combinedMesh.GetObjectsInCombined();
			for (int i = 0; i < objectsInCombined.Count; i++)
			{
				if (objectsInCombined[i] != null)
				{
					MB2_LOD componentInAncestor = MB2_LODManager.GetComponentInAncestor<MB2_LOD>(objectsInCombined[i].transform, false);
					if (componentInAncestor == null)
					{
						Debug.LogError("Couldn't find LOD for obj in combined mesh");
					}
					else if (!this.gosInCombiner.Contains(componentInAncestor))
					{
						Debug.LogError("lod was in combined mesh that is not in list of lods in cluster.");
					}
				}
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (int num4 in this.lodTransactions.Keys)
			{
				LODCombinedMesh.Transaction transaction = this.lodTransactions[num4];
				if (transaction.action == MB2_LODOperation.toAdd)
				{
					if (transaction.lod.isInCombined)
					{
						Debug.LogError("Bad action");
					}
					num += transaction.lod.GetNumVerts(transaction.toIdx);
				}
				if (transaction.action == MB2_LODOperation.update)
				{
					if (!transaction.lod.isInCombined)
					{
						Debug.LogError("Bad action");
					}
					num += transaction.lod.GetNumVerts(transaction.toIdx);
					num2 += transaction.lod.GetNumVerts(transaction.lod.currentLevelIdx);
				}
				if (transaction.action == MB2_LODOperation.delete)
				{
					if (!transaction.lod.isInCombined)
					{
						Debug.LogError("Bad action");
					}
					num2 += transaction.lod.GetNumVerts(transaction.lod.currentLevelIdx);
				}
			}
			for (int j = 0; j < this.combinedMesh.meshCombiners.Count; j++)
			{
				num3 += this.combinedMesh.meshCombiners[j].combinedMesh.GetMesh().vertexCount;
			}
			if (num3 != this.numVertsInMesh)
			{
				Debug.LogError(string.Concat(new object[] { "Num verts in mesh don't match measured ", num3, " thought ", this.numVertsInMesh }));
			}
			if (num - num2 != this.numApproxVertsInQ)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Num verts in Q don't match measured ",
					num - num2,
					" thought ",
					this.numApproxVertsInQ
				}));
			}
		}

		public void Update()
		{
			List<MB2_LOD> list = null;
			foreach (MB2_LOD mb2_LOD in this.gosAssignedToMe)
			{
				if (mb2_LOD == null)
				{
					if (list == null)
					{
						list = new List<MB2_LOD>();
					}
					list.Add(mb2_LOD);
				}
				else if (mb2_LOD.enabled && MB2_Version.GetActive(mb2_LOD.gameObject))
				{
					mb2_LOD.CheckIfLODsNeedToChange();
				}
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.gosAssignedToMe.Remove(list[i]);
				}
			}
		}

		public static string PrintFullRatios(List<LODCombinedMesh> cls)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < cls.Count; i++)
			{
				stringBuilder.AppendFormat("{0} full ratio {1} numObjs {2} numMeshes {3}\n", new object[]
				{
					i,
					cls[i].GetFullRatio().ToString("F5"),
					cls[i].gosInCombiner.Count,
					cls[i].combinedMesh.meshCombiners.Count
				});
			}
			return stringBuilder.ToString();
		}

		protected static float splitCombinerThreshold = 2f;

		protected static float mergeCombinerThreshold = 0.3f;

		public MB3_MultiMeshCombiner combinedMesh;

		protected Dictionary<int, LODCombinedMesh.Transaction> lodTransactions;

		protected HashSet<MB2_LOD> gosInCombiner;

		protected HashSet<MB2_LOD> gosAssignedToMe;

		public int numFramesBetweenChecks;

		public int numFramesBetweenChecksOffset;

		protected int numBakeImmediately;

		public LODCluster cluster;

		public int numVertsInMesh;

		public int numApproxVertsInQ;

		protected bool wasTranslated;

		public struct Transaction
		{
			public MB2_LODOperation action;

			public MB2_LOD lod;

			public int toIdx;

			public int inMeshGameObjectID;

			public int inMeshNumVerts;
		}

		public class LODCombinerSplitterMerger
		{
			public static void MergeCombiner(LODCluster cell)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (cell.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug || LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("=========== Merging Combiner " + cell);
				}
				List<LODCombinedMesh> combiners = cell.GetCombiners();
				if (combiners.Count < 2)
				{
					return;
				}
				combiners.Sort(new LODCombinedMesh.LODCombinerFullComparer());
				if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL == MB2_LogLevel.trace)
				{
					Debug.Log("ratios before" + LODCombinedMesh.PrintFullRatios(combiners));
				}
				LODCombinedMesh lodcombinedMesh = combiners[0];
				int num = 1;
				int num2 = lodcombinedMesh.GetNumVertsInMesh() + lodcombinedMesh.GetApproxNetVertsInQs();
				if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL == MB2_LogLevel.trace)
				{
					Debug.Log("============ numCombiners before " + lodcombinedMesh.cluster.GetCombiners().Count);
				}
				while (num < combiners.Count && num2 < lodcombinedMesh.combinedMesh.maxVertsInMesh)
				{
					LODCombinedMesh lodcombinedMesh2 = combiners[num];
					if (num2 + lodcombinedMesh2.GetNumVertsInMesh() + lodcombinedMesh2.GetApproxNetVertsInQs() > lodcombinedMesh.combinedMesh.maxVertsInMesh)
					{
						break;
					}
					num2 += lodcombinedMesh2.GetNumVertsInMesh() + lodcombinedMesh2.GetApproxNetVertsInQs();
					List<MB2_LOD> list = new List<MB2_LOD>(lodcombinedMesh2.gosAssignedToMe);
					for (int i = 0; i < list.Count; i++)
					{
						list[i].ForceRemove();
					}
					lodcombinedMesh2.ClearBake();
					for (int j = 0; j < list.Count; j++)
					{
						MB2_LOD mb2_LOD = list[j];
						lodcombinedMesh2.UnassignFromCombiner(mb2_LOD);
						mb2_LOD.SetCombiner(lodcombinedMesh);
						lodcombinedMesh.AssignToCombiner(mb2_LOD);
						mb2_LOD.ForceAdd();
					}
					cell.RemoveAndRecycleCombiner(lodcombinedMesh2);
					num++;
				}
				if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL == MB2_LogLevel.trace)
				{
					Debug.Log("========= numCombiners after " + lodcombinedMesh.cluster.GetCombiners().Count);
				}
				if (num > 1)
				{
					lodcombinedMesh.BakeClusterCombiner();
				}
				float num3 = Time.realtimeSinceStartup - realtimeSinceStartup;
				MB2_LODManager.Manager().statLastMergeTime = num3;
				if (lodcombinedMesh.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug || LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL == MB2_LogLevel.trace)
					{
						Debug.Log("ratios after " + LODCombinedMesh.PrintFullRatios(cell.GetCombiners()));
					}
					MB2_Log.LogDebug("=========== Done Merging Cluster merged {0} clusters in {1} sec", new object[]
					{
						num - 1,
						num3
					});
				}
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					lodcombinedMesh.cluster.CheckIntegrity();
				}
			}

			public static void SplitCombiner(LODCombinedMesh src)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					src.GetLODCluster().CheckIntegrity();
				}
				if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug || LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("=============== Splitting Combiner " + src.GetLODCluster(), new object[0]);
					if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.trace)
					{
						Debug.Log("ratios before " + LODCombinedMesh.PrintFullRatios(src.GetLODCluster().GetCombiners()));
					}
				}
				Dictionary<MB2_LOD, LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy> dictionary = new Dictionary<MB2_LOD, LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy>();
				foreach (MB2_LOD mb2_LOD in src.gosAssignedToMe)
				{
					MB2_LOD hierarchyRoot = mb2_LOD.GetHierarchyRoot();
					LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy lodhierarchy;
					if (!dictionary.TryGetValue(hierarchyRoot, out lodhierarchy))
					{
						lodhierarchy = new LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy(hierarchyRoot);
						dictionary.Add(hierarchyRoot, lodhierarchy);
					}
					lodhierarchy.lods.Add(mb2_LOD);
				}
				if (dictionary.Count == 1)
				{
					return;
				}
				if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
				{
					MB2_Log.LogDebug("Splitting Combiner found " + dictionary.Count + " hierarchies", new object[0]);
				}
				int num = 0;
				foreach (LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy lodhierarchy2 in dictionary.Values)
				{
					lodhierarchy2.ComputeNumberOfVertices();
					num += lodhierarchy2.numVerts;
				}
				int num2 = num / src.combinedMesh.maxVertsInMesh;
				if (num2 < 2)
				{
					num2 = 2;
				}
				LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner[] array = new LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner[num2];
				array[0] = new LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner(src);
				array[0].numVerts = 0;
				List<LODCombinedMesh> combiners = src.GetLODCluster().GetCombiners();
				int i = 1;
				int num3 = 0;
				while (num3 < combiners.Count && i < array.Length)
				{
					if (combiners[num3].GetFullRatio() < LODCombinedMesh.mergeCombinerThreshold)
					{
						array[i] = new LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner(combiners[num3]);
						i++;
					}
					num3++;
				}
				while (i < array.Length)
				{
					LODCombinedMesh freshCombiner = src.GetLODCluster().GetClusterManager().GetFreshCombiner(src.GetLODCluster());
					array[i] = new LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner(freshCombiner);
					i++;
				}
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					src.GetLODCluster().CheckIntegrity();
				}
				foreach (LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy lodhierarchy3 in dictionary.Values)
				{
					LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner newCombiner = array[0];
					for (int j = 1; j < array.Length; j++)
					{
						if (array[j].numVerts < newCombiner.numVerts)
						{
							newCombiner = array[j];
						}
					}
					newCombiner.lods.Add(lodhierarchy3);
					newCombiner.numVerts += lodhierarchy3.numVerts;
				}
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					src.GetLODCluster().CheckIntegrity();
				}
				if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
				{
					for (int k = 0; k < array.Length; k++)
					{
						Debug.Log(string.Concat(new object[]
						{
							"distributed ",
							array[k].lods.Count,
							" to combiner ",
							k
						}));
					}
				}
				foreach (LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner newCombiner2 in array)
				{
					for (int m = 0; m < newCombiner2.lods.Count; m++)
					{
						LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy lodhierarchy4 = newCombiner2.lods[m];
						for (int n = 0; n < lodhierarchy4.lods.Count; n++)
						{
							MB2_LOD mb2_LOD2 = lodhierarchy4.lods[n];
							mb2_LOD2.ForceRemove();
						}
					}
				}
				if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
				{
					MB2_Log.LogDebug("Clearing primary combiner", new object[0]);
				}
				array[0].combiner.ClearBake();
				for (int num4 = 0; num4 < array.Length; num4++)
				{
					LODCombinedMesh.LODCombinerSplitterMerger.NewCombiner newCombiner3 = array[num4];
					for (int num5 = 0; num5 < newCombiner3.lods.Count; num5++)
					{
						LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy lodhierarchy5 = newCombiner3.lods[num5];
						for (int num6 = 0; num6 < lodhierarchy5.lods.Count; num6++)
						{
							MB2_LOD mb2_LOD3 = lodhierarchy5.lods[num6];
							if (num4 >= 1)
							{
								mb2_LOD3.GetCombiner().UnassignFromCombiner(mb2_LOD3);
								mb2_LOD3.SetCombiner(newCombiner3.combiner);
								newCombiner3.combiner.AssignToCombiner(mb2_LOD3);
							}
						}
						lodhierarchy5.rootLod.ForceAdd();
					}
				}
				if (MB2_LODManager.CHECK_INTEGRITY)
				{
					src.GetLODCluster().CheckIntegrity();
				}
				for (int num7 = 0; num7 < array.Length; num7++)
				{
					if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
					{
						MB2_Log.LogDebug("Baking new combiner " + num7, new object[0]);
					}
					array[num7].combiner.BakeClusterCombiner();
					if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.trace)
					{
						Debug.Log(string.Concat(new object[]
						{
							"baked ",
							num7,
							" full ratio ",
							array[num7].combiner.GetFullRatio().ToString("F5")
						}));
					}
				}
				float num8 = Time.realtimeSinceStartup - realtimeSinceStartup;
				MB2_LODManager.Manager().statLastSplitTime = num8;
				if (src.GetClusterManager().LOG_LEVEL >= MB2_LogLevel.debug || LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.debug)
				{
					if (LODCombinedMesh.LODCombinerSplitterMerger.LOG_LEVEL >= MB2_LogLevel.trace)
					{
						Debug.Log("ratios after " + LODCombinedMesh.PrintFullRatios(src.GetLODCluster().GetCombiners()));
					}
					MB2_Log.LogDebug("=================Done split combiners " + num8 + " ============", new object[0]);
				}
			}

			public static MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

			private class LODHierarchy
			{
				public LODHierarchy(MB2_LOD root)
				{
					this.rootLod = root;
				}

				public void ComputeNumberOfVertices()
				{
					this.numVerts = 0;
					for (int i = 0; i < this.lods.Count; i++)
					{
						MB2_LOD mb2_LOD = this.lods[i];
						if (mb2_LOD != null)
						{
							if ((mb2_LOD.isInQueue && mb2_LOD.action == MB2_LODOperation.toAdd) || mb2_LOD.action == MB2_LODOperation.update)
							{
								this.numVerts += mb2_LOD.GetNumVerts(mb2_LOD.nextLevelIdx);
							}
							if (mb2_LOD.isInCombined)
							{
								if (!mb2_LOD.isInQueue || mb2_LOD.action != MB2_LODOperation.delete)
								{
									this.numVerts += mb2_LOD.GetNumVerts(mb2_LOD.currentLevelIdx);
								}
							}
						}
					}
				}

				public MB2_LOD rootLod;

				public List<MB2_LOD> lods = new List<MB2_LOD>();

				public int numVerts;
			}

			private class NewCombiner
			{
				public NewCombiner(LODCombinedMesh c)
				{
					this.lods = new List<LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy>();
					this.combiner = c;
					this.numVerts = c.GetNumVertsInMesh() + c.GetApproxNetVertsInQs();
				}

				public List<LODCombinedMesh.LODCombinerSplitterMerger.LODHierarchy> lods;

				public LODCombinedMesh combiner;

				public int numVerts;
			}
		}

		private class LODCombinerFullComparer : IComparer<LODCombinedMesh>
		{
			int IComparer<LODCombinedMesh>.Compare(LODCombinedMesh a, LODCombinedMesh b)
			{
				return a.GetNumVertsInMesh() + a.GetApproxNetVertsInQs() - b.GetNumVertsInMesh() - b.GetApproxNetVertsInQs();
			}
		}
	}
}
