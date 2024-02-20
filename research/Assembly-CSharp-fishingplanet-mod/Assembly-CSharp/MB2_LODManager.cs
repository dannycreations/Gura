using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using DigitalOpus.MB.Lod;
using UnityEngine;

public class MB2_LODManager : MonoBehaviour
{
	public static MB2_LODManager Manager()
	{
		if (MB2_LODManager.destroyedFrameCount == Time.frameCount)
		{
			return null;
		}
		if (MB2_LODManager._manager == null)
		{
			MB2_LODManager[] array = (MB2_LODManager[])Object.FindObjectsOfType(typeof(MB2_LODManager));
			if (array == null || array.Length == 0)
			{
				Debug.LogError("There were MB2_LOD scripts in the scene that couldn't find an MB2_LODManager in the scene. Try dragging the LODManager prefab into the scene and configuring some bakers.");
			}
			else if (array.Length > 1)
			{
				Debug.LogError("There was more than one LODManager object found in the scene.");
				MB2_LODManager._manager = null;
			}
			else
			{
				MB2_LODManager._manager = array[0];
			}
		}
		return MB2_LODManager._manager;
	}

	private void Awake()
	{
		MB2_LODManager.destroyedFrameCount = -1;
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			this.frameInfo = new MB2_LODManager.BakeDiagnostic[30];
		}
		this._Setup();
	}

	private void _Setup()
	{
		if (this.isSetup)
		{
			return;
		}
		this.checkScheduler = new LODCheckScheduler();
		this.checkScheduler.Init(this);
		if (this.bakers.Length == 0)
		{
			Debug.LogWarning("LOD Manager has no bakers. LOD objects will not be added to any combined meshes.");
		}
		if (this.numBakersPerGC <= 0)
		{
			MB2_Log.Log(MB2_LogLevel.info, "LOD Manager Number of Bakes before gargage collection is less than one. Garbage collector will never be run by the LOD Manager.", this.LOG_LEVEL);
		}
		if (this.maxCombineTimePerFrame <= 0f)
		{
			Debug.LogError("Combine Time Per Frame must be greater than zero.");
		}
		this.dirtyCombinedMeshes.Clear();
		for (int i = 0; i < this.bakers.Length; i++)
		{
			if (!this.bakers[i].Initialize(this.bakers))
			{
				MB2_LODManager.ENABLED = false;
				return;
			}
		}
		MB2_Log.Log(MB2_LogLevel.info, "LODManager.Start called initialized " + this.bakers.Length + " bakers", this.LOG_LEVEL);
		this.UpdateMeshesThatNeedToChange();
		this.isSetup = true;
	}

	public void SetupHierarchy(MB2_LOD lod)
	{
		if (!this.isSetup)
		{
			this._Setup();
		}
		if (!this.isSetup)
		{
			return;
		}
		lod.SetupHierarchy(this.bakers, this.ignoreLightmapping);
	}

	public void RemoveClustersIntersecting(Bounds bnds)
	{
		if (!MB2_LODManager.ENABLED)
		{
			return;
		}
		MB2_Log.Log(MB2_LogLevel.debug, "MB2_LODManager.RemoveClustersIntersecting " + bnds, this.LOG_LEVEL);
		for (int i = 0; i < this.bakers.Length; i++)
		{
			this.bakers[i].baker.RemoveCluster(bnds);
		}
	}

	public void AddDirtyCombinedMesh(LODCombinedMesh c)
	{
		if (!this.dirtyCombinedMeshes.ContainsKey(c))
		{
			this.dirtyCombinedMeshes.Add(c, c);
		}
	}

	private void Update()
	{
		this.checkScheduler.CheckIfLODsNeedToChange();
	}

	private void LateUpdate()
	{
		if (GC.CollectionCount(0) > this.GCcollectionCount)
		{
			this.GCcollectionCount = GC.CollectionCount(0);
		}
		this.UpdateMeshesThatNeedToChange();
		this.DestroyObjectsInLimbo();
		this.UpdateSkinnedMeshApproximateBoundsIfNecessary();
		if (this.LOG_LEVEL == MB2_LogLevel.debug)
		{
			if (this.frameInfo == null)
			{
				this.frameInfo = new MB2_LODManager.BakeDiagnostic[30];
			}
			int num = Time.frameCount % 30;
			this.frameInfo[num] = new MB2_LODManager.BakeDiagnostic(this);
			if (num == 0)
			{
				this.frameInfo[29].deltaTime = (int)(Time.deltaTime * 1000f);
			}
			else
			{
				this.frameInfo[num - 1].deltaTime = (int)(Time.deltaTime * 1000f);
			}
			if (num == 29)
			{
				Debug.Log(MB2_LODManager.BakeDiagnostic.PrettyPrint(this.frameInfo));
			}
		}
	}

	private void UpdateMeshesThatNeedToChange()
	{
		if (MB2_LODManager.destroyedFrameCount == Time.frameCount)
		{
			return;
		}
		if (!MB2_LODManager.ENABLED)
		{
			return;
		}
		if (!this.baking_enabled)
		{
			return;
		}
		if (this.lodCameras == null || this.lodCameras.Length == 0)
		{
			return;
		}
		this.statNumDirty = this.dirtyCombinedMeshes.Count;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (this.bakesSinceLastGC > this.numBakersPerGC)
		{
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			GC.Collect();
			this.statLastGarbageCollectionTime = Time.realtimeSinceStartup - realtimeSinceStartup2;
			this.statTotalGarbageCollectionTime += this.statLastGarbageCollectionTime;
			this.statLastGCFrame = Time.frameCount;
			this.bakesSinceLastGC = 0;
		}
		float num = 0f;
		if (this.statNumDirty > 0)
		{
			List<LODCombinedMesh> list = this.PrioritizeCombinedMeshs();
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				MB2_Log.Log(MB2_LogLevel.trace, string.Format("LODManager.UpdateMeshesThatNeedToChange called. dirty clusters= {0}", list.Count), this.LOG_LEVEL);
			}
			this.statLastNumBakes = 0;
			int num2 = list.Count - 1;
			while (num2 >= 0 && list[num2].NumBakeImmediately() > 0)
			{
				float realtimeSinceStartup3 = Time.realtimeSinceStartup;
				LODCombinedMesh lodcombinedMesh = list[num2];
				if (lodcombinedMesh.cluster == null)
				{
					list.RemoveAt(num2);
				}
				else
				{
					lodcombinedMesh.Bake();
					if (!lodcombinedMesh.IsDirty())
					{
						this.dirtyCombinedMeshes.Remove(lodcombinedMesh);
					}
					float num3 = Time.realtimeSinceStartup - realtimeSinceStartup3;
					if (num3 > this.statMaxCombinedMeshBakeTime)
					{
						this.statMaxCombinedMeshBakeTime = num3;
					}
					if (num3 < this.statMinCombinedMeshBakeTime)
					{
						this.statMinCombinedMeshBakeTime = num3;
					}
					this.statAveCombinedMeshBakeTime = this.statAveCombinedMeshBakeTime * ((float)this.statTotalNumBakes - 1f) / (float)this.statTotalNumBakes + num3 / (float)this.statTotalNumBakes;
					num += num3;
				}
				num2--;
			}
			while (Time.realtimeSinceStartup - realtimeSinceStartup < this.maxCombineTimePerFrame && num2 >= 0)
			{
				float realtimeSinceStartup4 = Time.realtimeSinceStartup;
				if (list[num2].cluster == null)
				{
					list.RemoveAt(num2);
				}
				else
				{
					list[num2].Bake();
					if (!list[num2].IsDirty())
					{
						this.dirtyCombinedMeshes.Remove(list[num2]);
					}
					float num4 = Time.realtimeSinceStartup - realtimeSinceStartup4;
					if (num4 > this.statMaxCombinedMeshBakeTime)
					{
						this.statMaxCombinedMeshBakeTime = num4;
					}
					if (num4 < this.statMinCombinedMeshBakeTime)
					{
						this.statMinCombinedMeshBakeTime = num4;
					}
					this.statAveCombinedMeshBakeTime = this.statAveCombinedMeshBakeTime * ((float)this.statTotalNumBakes - 1f) / (float)this.statTotalNumBakes + num4 / (float)this.statTotalNumBakes;
					num += num4;
				}
				num2--;
			}
			this.bakesSinceLastGC += this.statLastNumBakes;
			this.statLastCombinedMeshBakeTime = num;
			this.statTotalCombinedMeshBakeTime += this.statLastCombinedMeshBakeTime;
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.checkIntegrity();
		}
	}

	private List<LODCombinedMesh> PrioritizeCombinedMeshs()
	{
		Plane[][] array = new Plane[this.lodCameras.Length][];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GeometryUtility.CalculateFrustumPlanes(this.lodCameras[i].GetComponent<Camera>());
		}
		Vector3[] array2 = new Vector3[this.lodCameras.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = this.lodCameras[j].transform.position;
		}
		List<LODCombinedMesh> list = new List<LODCombinedMesh>(this.dirtyCombinedMeshes.Keys);
		for (int k = list.Count - 1; k >= 0; k--)
		{
			if (list[k].cluster == null)
			{
				list.RemoveAt(k);
			}
			else
			{
				list[k].PrePrioritize(array, array2);
			}
		}
		list.Sort(this.combinedMeshPriorityComparer);
		return list;
	}

	private void checkIntegrity()
	{
		for (int i = 0; i < this.bakers.Length; i++)
		{
			this.bakers[i].baker.CheckIntegrity();
		}
	}

	private void printSet(HashSet<Material> s)
	{
		IEnumerator enumerator = s.GetEnumerator();
		Debug.Log("== Set =====");
		while (enumerator.MoveNext())
		{
			object obj = enumerator.Current;
			Debug.Log(obj);
		}
	}

	private void OnDestroy()
	{
		MB2_LODManager.destroyedFrameCount = Time.frameCount;
		MB2_Log.Log(MB2_LogLevel.debug, "Destroying LODManager", this.LOG_LEVEL);
		for (int i = 0; i < this.bakers.Length; i++)
		{
			if (this.bakers[i].baker != null)
			{
				this.bakers[i].baker.Destroy();
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!MB2_LODManager.ENABLED)
		{
			return;
		}
		if (this.bakers != null)
		{
			for (int i = 0; i < this.bakers.Length; i++)
			{
				if (this.bakers[i].baker != null)
				{
					this.bakers[i].baker.DrawGizmos();
				}
			}
		}
	}

	public string GetStats()
	{
		string text = string.Empty;
		string text2 = text;
		text = string.Concat(new object[] { text2, "statTotalNumBakes=", this.statTotalNumBakes, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statTotalNumSplit=", this.statNumSplit, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statTotalNumMerge=", this.statNumMerge, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statAveCombinedMeshBakeTime=", this.statAveCombinedMeshBakeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statMaxCombinedMeshBakeTime=", this.statMaxCombinedMeshBakeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statMinCombinedMeshBakeTime=", this.statMinCombinedMeshBakeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statTotalGarbageCollectionTime=", this.statTotalGarbageCollectionTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statTotalCombinedMeshBakeTime=", this.statTotalCombinedMeshBakeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statTotalCheckLODNeedToChangeTime=", this.statTotalCheckLODNeedToChangeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statLastSplitTime=", this.statLastSplitTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statLastMergeTime=", this.statLastMergeTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statLastGarbageCollectionTime=", this.statLastGarbageCollectionTime, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "statLastBakeFrame=", this.statLastBakeFrame, "\n" });
		text2 = text;
		return string.Concat(new object[] { text2, "statNumDirty=", this.statNumDirty, "\n" });
	}

	public static T GetComponentInAncestor<T>(Transform tt, bool highest = false) where T : Component
	{
		Transform transform = tt;
		if (highest)
		{
			T t = (T)((object)null);
			while (transform != null)
			{
				T component = transform.GetComponent<T>();
				if (component != null)
				{
					t = component;
				}
				if (transform == transform.root)
				{
					break;
				}
				transform = transform.parent;
			}
			return t;
		}
		while (transform != null && transform.parent != transform)
		{
			T component2 = transform.GetComponent<T>();
			if (component2 != null)
			{
				return component2;
			}
			transform = transform.parent;
		}
		return (T)((object)null);
	}

	public MB2_LODCamera[] GetCameras()
	{
		if (this.lodCameras == null)
		{
			MB2_LODCamera[] array = (MB2_LODCamera[])MB2_Version.FindSceneObjectsOfType(typeof(MB2_LODCamera));
			if (array.Length == 0)
			{
				MB2_Log.Log(MB2_LogLevel.error, "There was no cameras in the scene with an MB2_LOD camera script attached", this.LOG_LEVEL);
			}
			else
			{
				this.lodCameras = array;
			}
		}
		return this.lodCameras;
	}

	public void AddBaker(MB2_LODManager.BakerPrototype bp)
	{
		if (!bp.Initialize(this.bakers))
		{
			return;
		}
		MB2_LODManager.BakerPrototype[] array = new MB2_LODManager.BakerPrototype[this.bakers.Length + 1];
		Array.Copy(this.bakers, array, this.bakers.Length);
		array[array.Length - 1] = bp;
		this.bakers = array;
		Debug.Log((this.bakers[0] == bp) + " a " + this.bakers[0].Equals(bp));
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.Log(MB2_LogLevel.debug, "Adding Baker to LODManager.", this.LOG_LEVEL);
		}
	}

	public void RemoveBaker(MB2_LODManager.BakerPrototype bp)
	{
		List<MB2_LODManager.BakerPrototype> list = new List<MB2_LODManager.BakerPrototype>();
		list.AddRange(this.bakers);
		Debug.Log((this.bakers[0] == bp) + " " + this.bakers[0].Equals(bp));
		if (list.Contains(bp))
		{
			bp.Clear();
			list.Remove(bp);
			this.bakers = list.ToArray();
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.Log(MB2_LogLevel.debug, "Found BP and removed", this.LOG_LEVEL);
			}
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.Log(MB2_LogLevel.debug, "Remving Baker from LODManager.", this.LOG_LEVEL);
		}
	}

	public void AddCamera(MB2_LODCamera cam)
	{
		MB2_LODCamera[] cameras = this.GetCameras();
		for (int i = 0; i < cameras.Length; i++)
		{
			if (cam == cameras[i])
			{
				return;
			}
		}
		MB2_LODCamera[] array = new MB2_LODCamera[cameras.Length + 1];
		Array.Copy(cameras, array, cameras.Length);
		array[array.Length - 1] = cam;
		this.lodCameras = array;
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.Log(MB2_LogLevel.debug, "MB2_LODManager.AddCamera added a camera length is now " + this.lodCameras.Length, this.LOG_LEVEL);
		}
	}

	public void RemoveCamera(MB2_LODCamera cam)
	{
		MB2_LODCamera[] cameras = this.GetCameras();
		List<MB2_LODCamera> list = new List<MB2_LODCamera>();
		list.AddRange(cameras);
		list.Remove(cam);
		this.lodCameras = list.ToArray();
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.Log(MB2_LogLevel.debug, "MB2_LODManager.RemovedCamera removed a camera length is now " + this.lodCameras.Length, this.LOG_LEVEL);
		}
	}

	public void LODDestroy(MB2_LOD lodComponent)
	{
		lodComponent.SetWasDestroyedFlag();
		MB2_LOD[] components = lodComponent.GetComponents<MB2_LOD>();
		for (int i = 0; i < components.Length; i++)
		{
			if (lodComponent != components[i])
			{
				this.LODDestroy(components[i]);
			}
		}
		bool flag = false;
		if (!lodComponent.isInCombined)
		{
			Object.Destroy(lodComponent.gameObject);
		}
		else
		{
			MB2_Version.SetActiveRecursively(lodComponent.gameObject, false);
			this.limbo.Add(lodComponent);
			flag = true;
		}
		if (this.LOG_LEVEL == MB2_LogLevel.trace)
		{
			MB2_Log.Log(MB2_LogLevel.trace, string.Concat(new object[] { "MB2_LODManager.LODDestroy ", lodComponent, " inLimbo=", flag }), this.LOG_LEVEL);
		}
	}

	private void DestroyObjectsInLimbo()
	{
		if (this.limbo.Count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = this.limbo.Count - 1; i >= 0; i--)
		{
			if (this.limbo[i] == null)
			{
				Debug.LogWarning("An object that was destroyed using LODManager.Manager().Destroy was also destroyed using unity Destroy. This object cannot be cleaned up by the LODManager.");
			}
			else if (!this.limbo[i].isInCombined)
			{
				Object.Destroy(this.limbo[i].gameObject);
				this.limbo.RemoveAt(i);
				num++;
			}
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.Log(MB2_LogLevel.debug, "MB2_LODManager DestroyObjectsInLimbo destroyed " + num, this.LOG_LEVEL);
		}
	}

	private void UpdateSkinnedMeshApproximateBoundsIfNecessary()
	{
		for (int i = 0; i < this.bakers.Length; i++)
		{
			if (this.bakers[i].updateSkinnedMeshApproximateBounds && this.bakers[i].meshBaker.meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				this.bakers[i].baker.UpdateSkinnedMeshApproximateBounds();
			}
		}
	}

	public int GetNextFrameCheckOffset()
	{
		return this.checkScheduler.GetNextFrameCheckOffset();
	}

	public float GetDistanceSqrToClosestPerspectiveCamera(Vector3 pos)
	{
		if (this.lodCameras.Length == 0)
		{
			return 0f;
		}
		float num = float.PositiveInfinity;
		for (int i = 0; i < this.lodCameras.Length; i++)
		{
			MB2_LODCamera mb2_LODCamera = this.lodCameras[i];
			if (mb2_LODCamera.enabled && MB2_Version.GetActive(mb2_LODCamera.gameObject) && !mb2_LODCamera.GetComponent<Camera>().orthographic)
			{
				Vector3 vector = mb2_LODCamera.transform.position - pos;
				float num2 = Vector3.Dot(vector, vector);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void ForceBakeAllDirty()
	{
		foreach (LODCombinedMesh lodcombinedMesh in this.dirtyCombinedMeshes.Keys)
		{
			lodcombinedMesh.ForceBakeImmediately();
		}
	}

	public void TranslateWorld(Vector3 translation)
	{
		for (int i = 0; i < this.bakers.Length; i++)
		{
			if (this.bakers[i].clusterType == MB2_LODManager.BakerPrototype.CombinerType.grid)
			{
				((LODClusterManagerGrid)this.bakers[i].baker).TranslateAllClusters(translation);
			}
		}
	}

	private static MB2_LODManager _manager;

	private static int destroyedFrameCount = -1;

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	public static bool ENABLED = true;

	public static bool CHECK_INTEGRITY;

	public bool baking_enabled = true;

	public float maxCombineTimePerFrame = 0.03f;

	public bool ignoreLightmapping = true;

	public MB2_LODManager.BakerPrototype[] bakers;

	public LODCheckScheduler checkScheduler;

	private Dictionary<LODCombinedMesh, LODCombinedMesh> dirtyCombinedMeshes = new Dictionary<LODCombinedMesh, LODCombinedMesh>();

	private MB2_LODCamera[] lodCameras;

	public IComparer<LODCombinedMesh> combinedMeshPriorityComparer = new MB2_LODClusterComparer();

	public List<MB2_LOD> limbo = new List<MB2_LOD>();

	public int numBakersPerGC = 2;

	private int bakesSinceLastGC;

	private int GCcollectionCount;

	private bool isSetup;

	public MB2_LODManager.BakeDiagnostic[] frameInfo;

	[HideInInspector]
	public int statTotalNumBakes;

	[HideInInspector]
	public float statAveCombinedMeshBakeTime = 0.03f;

	[HideInInspector]
	public float statMaxCombinedMeshBakeTime;

	[HideInInspector]
	public float statMinCombinedMeshBakeTime = 100f;

	[HideInInspector]
	public float statTotalCombinedMeshBakeTime = 0.03f;

	[HideInInspector]
	public float statLastCombinedMeshBakeTime;

	[HideInInspector]
	public int statLastNumBakes;

	[HideInInspector]
	public int statLastGCFrame;

	[HideInInspector]
	public float statLastGarbageCollectionTime;

	[HideInInspector]
	public float statTotalGarbageCollectionTime;

	[HideInInspector]
	public int statLastBakeFrame;

	[HideInInspector]
	public int statNumDirty;

	[HideInInspector]
	public int statNumSplit;

	[HideInInspector]
	public int statNumMerge;

	[HideInInspector]
	public float statLastMergeTime;

	[HideInInspector]
	public float statLastSplitTime;

	[HideInInspector]
	public float statLastCheckLODNeedToChangeTime;

	[HideInInspector]
	public float statTotalCheckLODNeedToChangeTime;

	public enum ChangeType
	{
		changeAdd,
		changeRemove,
		changeUpdate
	}

	public struct BakeDiagnostic
	{
		public BakeDiagnostic(MB2_LODManager manager)
		{
			this.frame = Time.frameCount;
			this.numCombinedMeshsInQ = manager.dirtyCombinedMeshes.Count;
			this.deltaTime = 0;
			if (manager.statLastBakeFrame == this.frame)
			{
				this.bakeTime = (int)(manager.statLastCombinedMeshBakeTime * 1000f);
				this.numCombinedMeshsBaked = manager.statLastNumBakes;
			}
			else
			{
				this.bakeTime = 0;
				this.numCombinedMeshsBaked = 0;
			}
			this.checkLODNeedToChangeTime = (int)(manager.statLastCheckLODNeedToChangeTime * 1000f);
			if (manager.statLastGCFrame == this.frame)
			{
				this.gcTime = (int)(manager.statLastGarbageCollectionTime * 1000f);
			}
			else
			{
				this.gcTime = 0;
			}
		}

		public static string PrettyPrint(MB2_LODManager.BakeDiagnostic[] data)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("--------------------------------------------");
			stringBuilder.AppendLine("Frame  deltaTime  numBakes  bakeTime  gcTime  checkTime  numInQ");
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].numCombinedMeshsBaked > 0)
				{
					stringBuilder.AppendFormat("{0}:     {1}       {2}       {3}       {4}       {5}       {6}\n", new object[]
					{
						data[i].frame.ToString().PadLeft(4),
						data[i].deltaTime.ToString().PadLeft(4),
						data[i].numCombinedMeshsBaked.ToString().PadLeft(4),
						data[i].bakeTime.ToString().PadLeft(4),
						data[i].gcTime.ToString().PadLeft(4),
						data[i].checkLODNeedToChangeTime.ToString().PadLeft(4),
						data[i].numCombinedMeshsInQ.ToString().PadLeft(4)
					});
				}
				else
				{
					stringBuilder.AppendFormat("{0}:     {1}       {2}        -        {3}       {4}       {5}\n", new object[]
					{
						data[i].frame.ToString().PadLeft(4),
						data[i].deltaTime.ToString().PadLeft(4),
						data[i].bakeTime.ToString().PadLeft(4),
						data[i].gcTime.ToString().PadLeft(4),
						data[i].checkLODNeedToChangeTime.ToString().PadLeft(4),
						data[i].numCombinedMeshsInQ.ToString().PadLeft(4)
					});
				}
			}
			stringBuilder.AppendLine("--------------------------------------------");
			return stringBuilder.ToString();
		}

		public int frame;

		public int deltaTime;

		public int bakeTime;

		public int checkLODNeedToChangeTime;

		public int gcTime;

		public int numCombinedMeshsBaked;

		public int numCombinedMeshsInQ;
	}

	[Serializable]
	public class BakerPrototype
	{
		public bool Initialize(MB2_LODManager.BakerPrototype[] bakers)
		{
			if (this.meshBaker == null)
			{
				Debug.LogError("Baker does not have a MeshBaker assigned. Create a 'Mesh and Material Baker'.and assign it to this baker.");
				return false;
			}
			if (this.maxVerticesPerCombinedMesh < 3)
			{
				Debug.LogError("Baker maxVerticesPerCombinedMesh must be greater than 3.");
				return false;
			}
			if (this.gridSize <= 0f && this.clusterType == MB2_LODManager.BakerPrototype.CombinerType.grid)
			{
				Debug.LogError("Baker gridSize must be greater than zero.");
				return false;
			}
			if (this.meshBaker.textureBakeResults == null || this.meshBaker.textureBakeResults.materials == null)
			{
				Debug.LogError("Baker does not have a texture bake result. Assign a texture bake result.");
				return false;
			}
			if (this.meshBaker.meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer && this.clusterType == MB2_LODManager.BakerPrototype.CombinerType.simple && !this.updateSkinnedMeshApproximateBounds)
			{
				Debug.Log("You are combining skinned meshes but Update Skinned Mesh Approximate bounds is not checked. You should check this setting if your meshes can move outside the fixed bounds or your meshes may vanish unexpectedly.");
			}
			if (this.numFramesBetweenLODChecks < 1)
			{
				Debug.LogError("'Num Frames Between LOD Checks' must be greater than zero.");
				return false;
			}
			if (this.materials == null)
			{
				this.materials = new HashSet<Material>();
			}
			MB2_TextureBakeResults textureBakeResults = this.meshBaker.textureBakeResults;
			for (int i = 0; i < textureBakeResults.materials.Length; i++)
			{
				if (!(textureBakeResults.materials[i] != null) || !(textureBakeResults.materials[i].shader != null))
				{
					Debug.LogError("One of the materials or shaders is null in prototype ");
					return false;
				}
				this.materials.Add(textureBakeResults.materials[i]);
			}
			int[] array = this.maxNumberPerLevel;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] < 0)
				{
					Debug.LogError("Max Number Per Level values must be positive");
					array[j] = 1000;
				}
			}
			for (int k = 0; k < bakers.Length; k++)
			{
				if (bakers[k] != this && bakers[k].label.Length > 0 && this.label.Equals(bakers[k].label))
				{
					Debug.LogError("Bakers have duplicate label" + bakers[k].label);
					return false;
				}
				if (bakers[k] != this && (this.materials.Overlaps(bakers[k].materials) & (bakers[k].label.Length == 0) & (this.label.Length == 0)) && bakers[k].materials.Count != 1)
				{
					Debug.LogWarning("Bakers " + k + " share materials with another baker. Assigning LOD objects to bakers may be ambiguous. Try setting labels to resolve conflicts.");
				}
			}
			this.CreateClusterManager();
			return true;
		}

		public void CreateClusterManager()
		{
			LODClusterManager lodclusterManager = null;
			if (this.clusterType == MB2_LODManager.BakerPrototype.CombinerType.grid)
			{
				lodclusterManager = new LODClusterManagerGrid(this);
				((LODClusterManagerGrid)lodclusterManager).gridSize = (int)this.gridSize;
			}
			else if (this.clusterType == MB2_LODManager.BakerPrototype.CombinerType.simple)
			{
				lodclusterManager = new LODClusterManagerSimple(this);
			}
			else if (this.clusterType == MB2_LODManager.BakerPrototype.CombinerType.moving)
			{
				lodclusterManager = new LODClusterManagerMoving(this);
			}
			this.baker = lodclusterManager;
		}

		public void Clear()
		{
			if (this.baker != null)
			{
				this.baker.Clear();
			}
		}

		public MB3_MeshBaker meshBaker;

		public int lightMapIndex = -1;

		public int layer = 1;

		public bool castShadow = true;

		public bool receiveShadow = true;

		public string label = string.Empty;

		public MB2_LODManager.BakerPrototype.CombinerType clusterType;

		public int maxVerticesPerCombinedMesh = 32000;

		public float gridSize = 250f;

		public HashSet<Material> materials = new HashSet<Material>();

		public int[] maxNumberPerLevel = new int[0];

		public bool updateSkinnedMeshApproximateBounds;

		public int numFramesBetweenLODChecks = 20;

		[NonSerialized]
		public LODClusterManager baker;

		public enum CombinerType
		{
			grid,
			simple,
			moving
		}
	}
}
