using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using DigitalOpus.MB.Lod;
using UnityEngine;

[AddComponentMenu("Mesh Baker/LOD")]
public class MB2_LOD : MonoBehaviour
{
	public float distanceSquaredToClosestCamera
	{
		get
		{
			return this._distSqrToClosestCamera;
		}
	}

	public int currentLevelIdx
	{
		get
		{
			return this.currentLODidx;
		}
	}

	public int nextLevelIdx
	{
		get
		{
			return this.nextLODidx;
		}
	}

	public void SetWasDestroyedFlag()
	{
		this._wasLODDestroyed = true;
	}

	public bool isInCombined
	{
		get
		{
			return this._isInCombined;
		}
	}

	public bool isInQueue
	{
		get
		{
			return this._isInQueue;
		}
	}

	public void Start()
	{
		if (this.setupStatus == MB2_LOD.SwitchDistanceSetup.notSetup && this.Init())
		{
			MB2_LOD componentInAncestor = MB2_LODManager.GetComponentInAncestor<MB2_LOD>(base.transform, true);
			this.manager.SetupHierarchy(componentInAncestor);
		}
		if (this.combinedMesh != null && MB2_LODManager.CHECK_INTEGRITY)
		{
			this.combinedMesh.GetLODCluster().CheckIntegrity();
		}
	}

	public string GetStatusMessage()
	{
		float num = Mathf.Sqrt(this._distSqrToClosestCamera);
		string text = string.Empty;
		string text2 = text;
		text = string.Concat(new object[] { text2, "isInCombined= ", this.isInCombined, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "isInQueue= ", this.isInQueue, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "currentLODidx= ", this.currentLODidx, "\n" });
		if (this.nextLODidx != this.currentLODidx)
		{
			text2 = text;
			text = string.Concat(new object[] { text2, " switchingTo= ", this.nextLODidx, "\n" });
		}
		text2 = text;
		text = string.Concat(new object[] { text2, "bestOrthographicIdx=", this.orthographicIdx, "\n" });
		text2 = text;
		text = string.Concat(new object[] { text2, "dist to camera= ", num, "\n" });
		if (this.combinedMesh != null)
		{
			text = text + "meshbaker=" + this.combinedMesh.GetClusterManager().GetBakerPrototype().baker;
		}
		return text;
	}

	public void _ResetPositionMarker()
	{
		this._position = base.transform.position;
	}

	public LODCombinedMesh GetCombiner()
	{
		return this.combinedMesh;
	}

	public void SetCombiner(LODCombinedMesh c)
	{
		this.combinedMesh = c;
	}

	public Vector3 GetHierarchyPosition()
	{
		return this.hierarchyRoot._position;
	}

	public void AdjustNextLevelIndex(int newIdx)
	{
		if (newIdx < 0)
		{
			Debug.LogError("Bad argument " + newIdx);
			return;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, string.Concat(new object[] { "AdjustNextLevelIndex ", this, " newIdx=", newIdx }), this.LOG_LEVEL);
		}
		if (newIdx > this.levels.Length)
		{
			newIdx = this.levels.Length;
		}
		this.DoStateTransition(newIdx);
	}

	public int GetGameObjectID(int idx)
	{
		if (idx >= this.levels.Length)
		{
			Debug.LogError("Called GetGameObjectID when level was too high. " + idx);
			return -1;
		}
		if (this.levels[idx].swapMeshWithLOD0)
		{
			return this.levels[0].instanceID;
		}
		return this.levels[idx].instanceID;
	}

	public GameObject GetRendererGameObject(int idx)
	{
		if (idx >= this.levels.Length)
		{
			Debug.LogError("Called GetRendererGameObject when level was too high. " + idx);
			return null;
		}
		if (this.levels[idx].swapMeshWithLOD0)
		{
			return this.levels[0].lodObject.gameObject;
		}
		return this.levels[idx].lodObject.gameObject;
	}

	public int GetNumVerts(int idx)
	{
		if (idx >= this.levels.Length)
		{
			return 0;
		}
		return this.levels[idx].numVerts;
	}

	private bool Init()
	{
		if (this.setupStatus == MB2_LOD.SwitchDistanceSetup.setup)
		{
			return true;
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.myLog = new LODLog(100);
		}
		else
		{
			this.myLog = new LODLog(0);
		}
		this.manager = MB2_LODManager.Manager();
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, this + "Init called", this.LOG_LEVEL);
		}
		if (!MB2_LODManager.ENABLED)
		{
			return false;
		}
		if (this.isInQueue || this.isInCombined)
		{
			Debug.LogError("Should not call Init on LOD that is in queue or in combined.");
			return false;
		}
		this._isInCombined = false;
		this.setupStatus = MB2_LOD.SwitchDistanceSetup.notSetup;
		if (this.manager == null)
		{
			Debug.LogError("LOD coruld not find LODManager");
			return false;
		}
		MB2_LODCamera[] cameras = this.manager.GetCameras();
		if (cameras.Length == 0)
		{
			Debug.LogError("There is no camera with an MB2_LODCamera script on it.");
			return false;
		}
		float num = 60f;
		bool flag = false;
		for (int i = 0; i < cameras.Length; i++)
		{
			Camera component = cameras[i].GetComponent<Camera>();
			if (component == null)
			{
				Debug.LogError("MB2_LODCamera script is not not attached to an object with a Camera component.");
				this.setupStatus = MB2_LOD.SwitchDistanceSetup.error;
				return false;
			}
			if (component == Camera.main && !component.orthographic)
			{
				num = component.fieldOfView;
				flag = true;
			}
			if (!component.orthographic && !flag)
			{
				num = component.fieldOfView;
			}
		}
		if (this.levels == null)
		{
			Debug.LogError("LOD " + this + " had no levels.");
			this.setupStatus = MB2_LOD.SwitchDistanceSetup.error;
			return false;
		}
		for (int j = 0; j < this.levels.Length; j++)
		{
			MB2_LOD.LOD lod = this.levels[j];
			if (lod.lodObject == null)
			{
				Debug.LogError(string.Concat(new object[] { this, " LOD Level ", j, " does not have a renderer." }));
				return false;
			}
			if (lod.lodObject is SkinnedMeshRenderer && this.renderType == MB_RenderType.meshRenderer)
			{
				Debug.LogError(string.Concat(new object[] { this, " LOD Level ", j, " is a skinned mesh but Baker Render Type was MeshRenderer. Baker Render Type must be set to SkinnedMesh on this LOD component." }));
				return false;
			}
			if (!lod.lodObject.transform.IsChildOf(base.transform))
			{
				Debug.LogError(string.Concat(new object[] { this, " LOD Level ", j, " is not a child of the LOD object." }));
				return false;
			}
			if (lod.lodObject.gameObject == base.gameObject)
			{
				Debug.LogError(this + " MB2_LOD component must be a parent ancestor of the level of detail renderers. It cannot be attached to the same game object as the level of detail renderers.");
			}
			if (j == 0 && lod.swapMeshWithLOD0)
			{
				Debug.LogWarning(this + " The first level of an LOD cannot have swap Mesh With LOD set.");
				lod.swapMeshWithLOD0 = false;
			}
			Animation animation = lod.lodObject.GetComponent<Animation>();
			Transform transform = lod.lodObject.transform;
			while (transform.parent != base.transform && transform.parent != transform)
			{
				transform = transform.parent;
				if (animation == null)
				{
					animation = transform.GetComponent<Animation>();
				}
			}
			lod.anim = animation;
			lod.root = transform.gameObject;
			if (lod.swapMeshWithLOD0)
			{
				MB2_Version.SetActiveRecursively(lod.root, false);
			}
			if (lod.bakeIntoCombined)
			{
				lod.numVerts = MB_Utility.GetMesh(lod.lodObject.gameObject).vertexCount;
			}
			lod.instanceID = lod.lodObject.gameObject.GetInstanceID();
			if (this.renderType == MB_RenderType.skinnedMeshRenderer && this.combinedMesh != null && this.combinedMesh.GetClusterManager().GetBakerPrototype().meshBaker != null && this.combinedMesh.GetClusterManager().GetBakerPrototype().meshBaker.meshCombiner.renderType == MB_RenderType.meshRenderer)
			{
				Debug.LogError(string.Concat(new object[] { " LOD ", this, " RenderType is SkinnedMeshRenderer but baker ", j, " is a MeshRenderer. won't be able to add this to the combined mesh." }));
				return false;
			}
		}
		this.lodZeroMesh = MB_Utility.GetMesh(this.levels[0].lodObject.gameObject);
		if (this.CalculateSwitchDistances(num, true))
		{
			for (int k = 0; k < this.levels.Length; k++)
			{
				if (this.levels[k].anim != null)
				{
					this.levels[k].anim.Sample();
				}
				this.MySetActiveRecursively(k, false);
			}
			this.setupStatus = MB2_LOD.SwitchDistanceSetup.setup;
			this.currentLODidx = this.levels.Length;
			this.nextLODidx = this.levels.Length;
			this._position = base.transform.position;
			return true;
		}
		this.setupStatus = MB2_LOD.SwitchDistanceSetup.error;
		return false;
	}

	private Vector3 AbsVector3(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	public bool CalculateSwitchDistances(float fieldOfView, bool showWarnings)
	{
		bool flag = true;
		if (this.levels.Length == 0)
		{
			Debug.LogError(this + " does not have any LOD levels set up.");
			flag = false;
		}
		for (int i = 1; i < this.levels.Length; i++)
		{
			if (this.levels[i].screenPercentage >= this.levels[i - 1].screenPercentage)
			{
				if (showWarnings)
				{
					Debug.LogError("LOD object " + this + " screenPercentage must be in decending order");
				}
				flag = false;
			}
		}
		float[] array = new float[this.levels.Length];
		for (int j = 0; j < this.levels.Length; j++)
		{
			Renderer lodObject = this.levels[j].lodObject;
			if (lodObject != null)
			{
				Bounds bounds = lodObject.bounds;
				if (lodObject is SkinnedMeshRenderer)
				{
					bool activeSelf = MB2_LOD.GetActiveSelf(lodObject.gameObject);
					bool enabled = lodObject.enabled;
					MB2_LOD.SetActiveSelf(lodObject.gameObject, true);
					lodObject.enabled = true;
					Matrix4x4 localToWorldMatrix = lodObject.transform.localToWorldMatrix;
					SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)lodObject;
					bounds..ctor(localToWorldMatrix * skinnedMeshRenderer.localBounds.center, this.AbsVector3(localToWorldMatrix * skinnedMeshRenderer.localBounds.size));
					MB2_LOD.SetActiveSelf(lodObject.gameObject, activeSelf);
					lodObject.enabled = enabled;
				}
				this.levels[j].switchDistances = 0f;
				this.levels[j].sqrtDist = 0f;
				if (this.levels[j].screenPercentage <= 0f)
				{
					if (showWarnings)
					{
						Debug.LogError("LOD object " + this + " screenPercentage must be greater than zero.");
					}
					flag = false;
				}
				else
				{
					float num = (bounds.size.x + bounds.size.y + bounds.size.z) / 3f;
					if (num == 0f)
					{
						if (showWarnings)
						{
							Debug.LogError("LOD " + this + " the object has no size");
						}
						flag = false;
					}
					else
					{
						array[j] = num;
						this.levels[j].dimension = num;
						for (int k = 0; k < j; k++)
						{
							if (array[k] > 1.5f * num || array[k] < num / 1.5f)
							{
								if (showWarnings)
								{
									Debug.LogError(string.Concat(new object[] { "LOD ", this, " the render bounds of lod levels ", j, " and ", k, " are very differnt sizes.They should be very close to the same size. LOD uses these to determine when to switch from one LOD to another." }));
								}
								flag = false;
							}
						}
						float num2 = 50f / Mathf.Tan(0.017453292f * fieldOfView / 2f);
						this.levels[j].switchDistances = num * num2 / (50f * this.levels[j].screenPercentage);
						this.levels[j].sqrtDist = this.levels[j].switchDistances;
						this.levels[j].switchDistances = this.levels[j].switchDistances * this.levels[j].switchDistances;
					}
				}
			}
			else
			{
				flag = false;
			}
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace && this.myLog != null)
		{
			this.myLog.Log(MB2_LogLevel.trace, string.Concat(new object[] { this, "CalculateSwitchDistances called fov=", fieldOfView, " success=", flag }), this.LOG_LEVEL);
		}
		return flag;
	}

	public void Clear()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, this + "Clear called", this.LOG_LEVEL);
		}
		this.currentLODidx = this.levels.Length;
		this.nextLODidx = this.currentLODidx;
		this.setupStatus = MB2_LOD.SwitchDistanceSetup.notSetup;
		this._isInQueue = false;
		this._isInCombined = false;
		this.action = MB2_LODOperation.none;
		this.combinedMesh = null;
		this.clustersAreSetup = false;
	}

	public void CheckIfLODsNeedToChange()
	{
		if (!MB2_LODManager.ENABLED)
		{
			return;
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
		if (this.setupStatus == MB2_LOD.SwitchDistanceSetup.error || this.setupStatus == MB2_LOD.SwitchDistanceSetup.notSetup)
		{
			return;
		}
		MB2_LODCamera[] cameras = this.manager.GetCameras();
		if (cameras.Length == 0)
		{
			return;
		}
		int num = this.levels.Length;
		if (this.forceToLevel != -1)
		{
			if (this.forceToLevel < 0 || this.forceToLevel > this.levels.Length)
			{
				Debug.LogWarning("Force To Level was not a valid level index value for LOD " + this);
			}
			else
			{
				num = this.forceToLevel;
				this._distSqrToClosestCamera = this.levels[num].sqrtDist;
			}
		}
		else
		{
			this._distSqrToClosestCamera = this.manager.GetDistanceSqrToClosestPerspectiveCamera(base.transform.position);
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (this._distSqrToClosestCamera < this.levels[i].switchDistances)
				{
					num = i;
					break;
				}
			}
			this.orthographicIdx = this.GetHighestLODIndexOfOrothographicCameras(cameras);
			if (this.orthographicIdx < num)
			{
				num = this.orthographicIdx;
			}
		}
		if (num != this.nextLODidx)
		{
			if (this.combinedMesh.GetClusterManager().GetBakerPrototype().clusterType == MB2_LODManager.BakerPrototype.CombinerType.grid && this.bakeIntoCombined && this._position != base.transform.position)
			{
				Debug.LogError("Can't move LOD " + this + " after it has been added to a combined mesh unless baker type is 'Simple'");
				return;
			}
			this.DoStateTransition(num);
		}
	}

	private void DoStateTransition(int lodIdx)
	{
		if (lodIdx < 0 || lodIdx > this.levels.Length)
		{
			Debug.LogError("lodIdx out of range " + lodIdx);
		}
		if (this._isInQueue)
		{
			if (this.action == MB2_LODOperation.toAdd)
			{
				this.SwapBetweenLevels(this.nextLODidx, this.currentLODidx);
			}
			else if (this.action == MB2_LODOperation.delete)
			{
				this.SwapBetweenLevels(this.nextLODidx, this.currentLODidx);
			}
			this._CallLODCancelTransaction();
			this.action = MB2_LODOperation.none;
			this.nextLODidx = this.currentLevelIdx;
		}
		MB2_LODOperation mb2_LODOperation;
		if (this._isInCombined)
		{
			if (lodIdx == this.levels.Length || !this.levels[lodIdx].bakeIntoCombined)
			{
				mb2_LODOperation = MB2_LODOperation.delete;
			}
			else
			{
				mb2_LODOperation = MB2_LODOperation.update;
			}
		}
		else if (lodIdx < this.levels.Length && this.levels[lodIdx].bakeIntoCombined)
		{
			mb2_LODOperation = MB2_LODOperation.toAdd;
		}
		else
		{
			mb2_LODOperation = MB2_LODOperation.none;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, string.Concat(new object[] { this, " DoStateTransition newA=", mb2_LODOperation, " newNextLevel=", lodIdx }), this.LOG_LEVEL);
		}
		if (mb2_LODOperation == MB2_LODOperation.toAdd && this.currentLevelIdx == this.levels.Length)
		{
			this.SwapBetweenLevels(this.nextLODidx, lodIdx);
		}
		else if (mb2_LODOperation == MB2_LODOperation.delete && lodIdx == this.levels.Length)
		{
			this.SwapBetweenLevels(this.nextLODidx, lodIdx);
		}
		this.nextLODidx = lodIdx;
		if (this.bakeIntoCombined)
		{
			this.action = mb2_LODOperation;
			this._CallLODChanged();
		}
		else
		{
			this.action = MB2_LODOperation.none;
			this.currentLODidx = this.nextLODidx;
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				this.myLog.Log(MB2_LogLevel.debug, this + " LODChanged but not baking next level=" + this.nextLODidx, this.LOG_LEVEL);
			}
		}
	}

	public void ForceRemove()
	{
		this.action = MB2_LODOperation.none;
		this._isInQueue = false;
		this._isInCombined = false;
		if (this.currentLODidx != this.nextLODidx && this.currentLODidx < this.levels.Length)
		{
			this.MySetActiveRecursively(this.currentLODidx, false);
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "ForceRemove called " + this, this.LOG_LEVEL);
		}
		if (this.nextLODidx < this.levels.Length)
		{
			this.MySetActiveRecursively(this.nextLODidx, true);
		}
		this.currentLODidx = this.nextLODidx;
	}

	public void ForceAdd()
	{
		if (this.isInCombined || this.isInQueue || this.nextLODidx >= this.levels.Length || !this.levels[this.nextLODidx].bakeIntoCombined)
		{
			return;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "ForceAdd called " + this, this.LOG_LEVEL);
		}
		if (this.nextLODidx < this.levels.Length)
		{
			this.action = MB2_LODOperation.toAdd;
		}
		this.combinedMesh.LODChanged(this, false);
	}

	private void _CallLODCancelTransaction()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			this.myLog.Log(MB2_LogLevel.debug, string.Concat(new object[] { this, " calling _CallLODCancelTransaction action=", this.action, " next level=", this.nextLODidx }), this.LOG_LEVEL);
		}
		if (this.currentLevelIdx < this.levels.Length && this.currentLevelIdx > 0 && this.levels[this.currentLevelIdx].swapMeshWithLOD0)
		{
			Mesh mesh = MB_Utility.GetMesh(this.levels[this.currentLevelIdx].lodObject.gameObject);
			this.SetMesh(this.levels[0].lodObject.gameObject, mesh);
		}
		else if (this.currentLevelIdx == 0)
		{
			this.SetMesh(this.levels[0].lodObject.gameObject, this.lodZeroMesh);
		}
		this.combinedMesh.LODCancelTransaction(this);
	}

	private void _CallLODChanged()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			this.myLog.Log(MB2_LogLevel.debug, string.Concat(new object[] { this, " calling LODChanged action=", this.action, " next level=", this.nextLODidx }), this.LOG_LEVEL);
		}
		if (this.action != MB2_LODOperation.none)
		{
			if (this.nextLODidx < this.levels.Length && this.nextLODidx > 0 && this.levels[this.nextLODidx].swapMeshWithLOD0)
			{
				Mesh mesh = MB_Utility.GetMesh(this.levels[this.nextLODidx].lodObject.gameObject);
				this.SetMesh(this.levels[0].lodObject.gameObject, mesh);
			}
			else if (this.nextLODidx == 0)
			{
				this.SetMesh(this.levels[0].lodObject.gameObject, this.lodZeroMesh);
			}
			this.combinedMesh.LODChanged(this, false);
		}
		else if (this.nextLODidx == this.levels.Length || (this.nextLODidx < this.levels.Length && !this.levels[this.nextLODidx].bakeIntoCombined))
		{
			this.OnBakeAdded();
		}
	}

	public void OnBakeRemoved()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "OnBakeRemoved " + this, this.LOG_LEVEL);
		}
		if (!this._isInQueue || this.action != MB2_LODOperation.delete || !this.isInCombined)
		{
			Debug.LogError("OnBakeRemoved called on an LOD in an invalid state: " + this.ToString());
		}
		this._isInCombined = false;
		this.action = MB2_LODOperation.none;
		this._isInQueue = false;
		if (this.currentLODidx < this.levels.Length)
		{
			this.MySetActiveRecursively(this.currentLODidx, false);
		}
		if (this.nextLODidx < this.levels.Length)
		{
			this.MySetActiveRecursively(this.nextLODidx, true);
		}
		this.currentLODidx = this.nextLODidx;
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	public void OnBakeAdded()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "OnBakeAdded " + this, this.LOG_LEVEL);
		}
		if (this.nextLODidx < this.levels.Length && this.levels[this.nextLODidx].bakeIntoCombined)
		{
			if (!this._isInQueue || this.action != MB2_LODOperation.toAdd || this.isInCombined)
			{
				Debug.LogError("OnBakeAdded called on an LOD in an invalid state: " + this.ToString() + " log " + this.myLog.Dump());
			}
			this._isInCombined = true;
		}
		else
		{
			if (this._isInQueue || this.action != MB2_LODOperation.none || this.isInCombined)
			{
				Debug.LogError("OnBakeAdded called on an LOD in an invalid state: " + this.ToString() + " log " + this.myLog.Dump());
			}
			this._isInCombined = false;
		}
		this._isInQueue = false;
		this.action = MB2_LODOperation.none;
		this.SwapBetweenLevels(this.currentLODidx, this.nextLODidx);
		this.currentLODidx = this.nextLODidx;
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	public void OnBakeUpdated()
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "OnBakeUpdated " + this, this.LOG_LEVEL);
		}
		if (!this._isInQueue || this.action != MB2_LODOperation.update || !this.isInCombined)
		{
			Debug.LogError("OnBakeUpdated called on an LOD in an invalid state: " + this.ToString());
		}
		if (this.nextLODidx >= this.levels.Length)
		{
			Debug.LogError("Update will remove all meshes from combined. This should never happen.");
		}
		this._isInQueue = false;
		this._isInCombined = true;
		this.action = MB2_LODOperation.none;
		this.SwapBetweenLevels(this.currentLODidx, this.nextLODidx);
		this.currentLODidx = this.nextLODidx;
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	public void OnRemoveFromQueue()
	{
		this._isInQueue = false;
		this.action = MB2_LODOperation.none;
		this.nextLODidx = this.currentLODidx;
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "OnRemoveFromQueue complete " + this, this.LOG_LEVEL);
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	public void OnAddToQueue()
	{
		this._isInQueue = true;
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, "OnAddToAddQueue complete " + this, this.LOG_LEVEL);
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	private void OnDestroy()
	{
		if (this.setupStatus != MB2_LOD.SwitchDistanceSetup.setup)
		{
			return;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, this + "OnDestroy called", this.LOG_LEVEL);
		}
		if (!this._wasLODDestroyed)
		{
			this.myLog.Log(MB2_LogLevel.debug, "An MB2_LOD object " + this + " was destroyed using Unity's Destroy method. This can leave destroyed meshes in the combined mesh. Try using MB2_LODManager.Manager().LODDestroy() instead.", this.LOG_LEVEL);
		}
		this._removeIfInCombined();
		if (this.combinedMesh != null)
		{
			this.combinedMesh.UnassignFromCombiner(this);
		}
		this.combinedMesh = null;
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	private void OnEnable()
	{
		if (this.setupStatus != MB2_LOD.SwitchDistanceSetup.setup)
		{
			return;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, this + "OnEnable called", this.LOG_LEVEL);
		}
		if (this.levels != null)
		{
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (!this._isInCombined && this.currentLODidx == i)
				{
					this.MySetActiveRecursively(i, true);
				}
				else
				{
					this.MySetActiveRecursively(i, false);
				}
			}
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	private void OnDisable()
	{
		if (this.myLog != null && this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			this.myLog.Log(MB2_LogLevel.trace, this + "OnDisable called", this.LOG_LEVEL);
		}
		this._removeIfInCombined();
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	private void _removeIfInCombined()
	{
		if (this._isInCombined || this._isInQueue)
		{
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (this.levels[i].lodObject != null && !this.levels[i].swapMeshWithLOD0)
				{
					this.MySetActiveRecursively(i, false);
				}
			}
			this.nextLODidx = this.levels.Length;
			if ((this.isInCombined || this.isInQueue) && MB2_LODManager.Manager() != null)
			{
				this.action = MB2_LODOperation.delete;
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					this.myLog.Log(MB2_LogLevel.trace, this + " Calling  LODManager.RemoveLOD", this.LOG_LEVEL);
				}
				this.combinedMesh.RemoveLOD(this, true);
			}
		}
	}

	public bool ArePrototypesSetup()
	{
		return this.clustersAreSetup;
	}

	public MB2_LODManager.BakerPrototype GetBaker(MB2_LODManager.BakerPrototype[] allPrototypes, bool ignoreLightmapping)
	{
		if (this.baker != null)
		{
			return this.baker;
		}
		if (this.setupStatus != MB2_LOD.SwitchDistanceSetup.setup && !this.Init())
		{
			return null;
		}
		this.myLog.Log(MB2_LogLevel.debug, this + " GetBaker called setting up baker", this.LOG_LEVEL);
		MB2_LODManager.BakerPrototype bakerPrototype = null;
		for (int i = 0; i < this.levels.Length; i++)
		{
			if (this.levels[i].bakeIntoCombined)
			{
				Renderer lodObject = this.levels[i].lodObject;
				Mesh mesh = MB_Utility.GetMesh(this.levels[i].lodObject.gameObject);
				Rect rect;
				rect..ctor(0f, 0f, 1f, 1f);
				bool flag = MB_Utility.hasOutOfBoundsUVs(mesh, ref rect);
				int lightmapIndex = lodObject.lightmapIndex;
				HashSet<Material> hashSet = new HashSet<Material>();
				for (int j = 0; j < lodObject.sharedMaterials.Length; j++)
				{
					if (lodObject.sharedMaterials[j] != null && lodObject.sharedMaterials[j].shader != null)
					{
						hashSet.Add(lodObject.sharedMaterials[j]);
					}
				}
				if (this.bakerLabel != null && this.bakerLabel.Length > 0)
				{
					for (int k = 0; k < allPrototypes.Length; k++)
					{
						if (allPrototypes[k].label.Equals(this.bakerLabel))
						{
							if (!ignoreLightmapping && lightmapIndex != allPrototypes[k].lightMapIndex)
							{
								Debug.LogError("LOD " + this + " had a bakerLabel, but had a different lightmap index than that baker");
							}
							if (!hashSet.IsSubsetOf(allPrototypes[k].materials))
							{
								Debug.LogError("LOD " + this + " had a bakerLabel, but had materials are not in that baker");
							}
							bakerPrototype = allPrototypes[k];
							break;
						}
					}
					if (bakerPrototype != null)
					{
						goto IL_49D;
					}
					Debug.LogError(string.Concat(new object[] { "LOD ", this, " had a bakerLabel '", this.bakerLabel, "' that was not matched by any baker" }));
				}
				MB2_LODManager.BakerPrototype bakerPrototype2 = null;
				MB2_LODManager.BakerPrototype bakerPrototype3 = null;
				string text = string.Empty;
				string text2 = string.Empty;
				for (int l = 0; l < allPrototypes.Length; l++)
				{
					text2 = string.Empty;
					MB2_LODManager.BakerPrototype bakerPrototype4 = null;
					MB2_LODManager.BakerPrototype bakerPrototype5 = null;
					if (flag && allPrototypes[l].materials.SetEquals(hashSet))
					{
						bakerPrototype5 = allPrototypes[l];
					}
					if (hashSet.IsSubsetOf(allPrototypes[l].materials))
					{
						bakerPrototype4 = allPrototypes[l];
					}
					if (!ignoreLightmapping && lightmapIndex != allPrototypes[l].lightMapIndex && bakerPrototype4 != null)
					{
						text2 += "\n  lightmapping check failed";
					}
					if (allPrototypes[l].meshBaker.meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer && this.renderType != MB_RenderType.skinnedMeshRenderer && bakerPrototype4 != null)
					{
						text2 += "\n  rendertype did not match";
					}
					if (allPrototypes[l].meshBaker.meshCombiner.renderType == MB_RenderType.meshRenderer && this.renderType != MB_RenderType.meshRenderer && bakerPrototype4 != null)
					{
						text2 += "\n  rendertype did not match";
					}
					if (text2.Length == 0)
					{
						if (bakerPrototype5 != null)
						{
							if (bakerPrototype3 != null)
							{
								Debug.LogWarning("The set of materials on LOD " + this + " matched multiple bakers. Try use labels to resolve the conflict.");
							}
							bakerPrototype3 = bakerPrototype5;
						}
						if (bakerPrototype4 != null)
						{
							if (bakerPrototype2 != null)
							{
								Debug.LogWarning("The set of materials on LOD " + this + " matched multiple bakers. Try use labels to resolve the conflict.");
							}
							bakerPrototype2 = bakerPrototype4;
						}
					}
					else
					{
						string text3 = text;
						text = string.Concat(new object[] { text3, "LOD ", i, " Baker ", l, " matched the materials but could not match because: ", text2 });
					}
				}
				if (bakerPrototype3 != null)
				{
					bakerPrototype = bakerPrototype3;
				}
				else
				{
					if (bakerPrototype2 == null)
					{
						string text4 = string.Empty;
						foreach (Material material in hashSet)
						{
							text4 = text4 + material + ",";
						}
						Debug.LogError(string.Concat(new object[]
						{
							"Could not find a baker that can accept the materials on LOD ", this, "\nmaterials [", text4, "]\nlightmapIndex = ", lightmapIndex, " (ignore lightmapping = ", ignoreLightmapping, ")\nout of bounds uvs ", flag,
							" (if true then set of prototype materials must match exactly.)\n", text
						}));
						return null;
					}
					bakerPrototype = bakerPrototype2;
				}
			}
			IL_49D:;
		}
		this.baker = bakerPrototype;
		return this.baker;
	}

	public void SetupHierarchy(MB2_LODManager.BakerPrototype[] allPrototypes, bool ignoreLightmapping)
	{
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			MB2_Log.LogDebug("Setting up hierarchy for " + this, new object[0]);
		}
		MB2_LOD componentInAncestor = MB2_LODManager.GetComponentInAncestor<MB2_LOD>(base.transform, false);
		if (componentInAncestor != this)
		{
			Debug.LogError("Should only be called on the root LOD.");
		}
		this.hierarchyRoot = this;
		if (this.combinedMesh != null)
		{
			return;
		}
		this.GetBaker(allPrototypes, ignoreLightmapping);
		if (this.baker != null)
		{
			LODCluster clusterFor = this.baker.baker.GetClusterFor(this.GetHierarchyPosition());
			this.combinedMesh = clusterFor.SuggestCombiner();
			clusterFor.AssignLODToCombiner(this);
		}
		MB2_LOD._RecurseSetup(base.transform, allPrototypes, ignoreLightmapping);
	}

	private static void _RecurseSetup(Transform t, MB2_LODManager.BakerPrototype[] allPrototypes, bool ignoreLightmapping)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			MB2_LOD component = child.GetComponent<MB2_LOD>();
			if (component != null)
			{
				if (component.Init())
				{
					component.GetBaker(allPrototypes, ignoreLightmapping);
				}
				if (component.baker != null)
				{
					component.FindHierarchyRoot();
					LODCluster clusterFor = component.baker.baker.GetClusterFor(component.GetHierarchyPosition());
					component.combinedMesh = clusterFor.SuggestCombiner();
					clusterFor.AssignLODToCombiner(component);
					component.clustersAreSetup = true;
				}
			}
			MB2_LOD._RecurseSetup(child, allPrototypes, ignoreLightmapping);
		}
	}

	public MB2_LOD GetHierarchyRoot()
	{
		return this.hierarchyRoot;
	}

	private MB2_LOD FindHierarchyRoot()
	{
		Transform transform = base.transform.parent;
		MB2_LOD mb2_LOD = this;
		while (transform != null)
		{
			MB2_LOD component = transform.GetComponent<MB2_LOD>();
			if (component != null)
			{
				MB2_LODManager.BakerPrototype bakerPrototype = component.baker;
				if (bakerPrototype != null && bakerPrototype == this.baker)
				{
					mb2_LOD = component;
				}
			}
			if (transform == transform.root)
			{
				break;
			}
			transform = transform.parent;
		}
		this.hierarchyRoot = mb2_LOD;
		return mb2_LOD;
	}

	public override string ToString()
	{
		string text = string.Empty;
		if (this.nextLODidx < this.levels.Length)
		{
			text += this.levels[this.nextLODidx].instanceID;
		}
		return string.Format("[MB2_LOD {0} id={1}: inComb={2} inQ={3} act={4} nxt={5} curr={6} nxtRendInstId={7}]", new object[]
		{
			base.name,
			base.GetInstanceID(),
			this.isInCombined,
			this.isInQueue,
			this.action,
			this.nextLODidx,
			this.currentLODidx,
			text
		});
	}

	public void CheckState(bool exInCombined, bool exInQueue, MB2_LODOperation exAction, int exNextIdx, int exCurrentIdx)
	{
		if (this.isInCombined != exInCombined)
		{
			Debug.LogError(string.Concat(new object[] { "inCombined Test fail. was ", this.isInCombined, " expects=", exInCombined }));
		}
		if (this.isInQueue != exInQueue)
		{
			Debug.LogError(string.Concat(new object[]
			{
				base.GetInstanceID(),
				" inQueue Test fail. was ",
				this.isInQueue,
				" expects=",
				exInQueue
			}));
		}
		if (this.action != exAction)
		{
			Debug.LogError(string.Concat(new object[] { "action Test fail. was ", this.action, " expects=", exAction }));
		}
		if (this.nextLODidx != exNextIdx)
		{
			Debug.LogError(string.Concat(new object[] { "next idx Test fail. was ", this.nextLODidx, " expects=", exNextIdx }));
		}
		if (this.currentLODidx != exCurrentIdx)
		{
			Debug.LogError(string.Concat(new object[] { "current idx Test fail. was ", this.currentLODidx, " expects=", exCurrentIdx }));
		}
		if (MB2_LODManager.CHECK_INTEGRITY)
		{
			this.CheckIntegrity();
		}
	}

	private void CheckIntegrity()
	{
		if (this == null)
		{
			return;
		}
		if (this._isInCombined && this.currentLODidx >= this.levels.Length)
		{
			Debug.LogError(this + " IntegrityCheckFailed invalid currentLODidx" + this);
		}
		if (this.action != MB2_LODOperation.none && !this.isInQueue)
		{
			Debug.LogError(this + " Invalid action if not in queue " + this);
		}
		if (this.action == MB2_LODOperation.none && this.isInQueue)
		{
			Debug.LogError(this + " Invalid action if in queue " + this);
		}
		if (this.action == MB2_LODOperation.toAdd && this.isInCombined)
		{
			Debug.LogError(this + " Invalid action if in combined " + this);
		}
		if (this.action == MB2_LODOperation.delete && !this.isInCombined)
		{
			Debug.LogError(this + " Invalid action if not in combined " + this);
		}
		if (this.action == MB2_LODOperation.delete && this.currentLODidx >= this.levels.Length)
		{
			Debug.LogError(this + " Invalid currentLODidx " + this.currentLODidx);
		}
		if (this.setupStatus == MB2_LOD.SwitchDistanceSetup.setup)
		{
			for (int i = 0; i < this.levels.Length; i++)
			{
				if (this.levels[i].lodObject != null && MB2_LOD.GetActiveSelf(this.levels[i].lodObject.gameObject))
				{
					if ((i != 0 || this.currentLODidx >= this.levels.Length || !this.levels[this.currentLODidx].swapMeshWithLOD0) && !this.levels[i].swapMeshWithLOD0)
					{
						if (!this.isInQueue && i != this.currentLODidx)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"f=",
								Time.frameCount,
								" ",
								this,
								" lodObject of wrong level was active was:",
								i,
								" should be:",
								this.currentLODidx
							}));
							Debug.Log("LogDump " + this.myLog.Dump());
						}
						Renderer component = this.levels[i].lodObject.GetComponent<Renderer>();
						if (this._isInCombined && component.enabled)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"f=",
								Time.frameCount,
								" ",
								this,
								" lodObject object in combined and its renderer was enabled id ",
								this.levels[i].instanceID,
								" when inCombined. should all be inactive ",
								this.currentLODidx
							}));
							Debug.Log("LogDump " + this.myLog.Dump());
						}
					}
				}
			}
		}
		if (this.combinedMesh != null)
		{
			if (!this.combinedMesh.IsAssignedToThis(this))
			{
				Debug.LogError("LOD was assigned to combinedMesh but combinedMesh didn't contain " + this);
			}
			LODCluster lodcluster = this.combinedMesh.GetLODCluster();
			if (lodcluster != null && !lodcluster.GetCombiners().Contains(this.combinedMesh))
			{
				Debug.LogError("Cluster was assigned to cell but it wasn't in its list of clusters");
			}
		}
		if (MB2_LOD.GetActiveSelf(base.gameObject) && !this._isInCombined)
		{
			bool flag = false;
			for (int j = 0; j < this.levels.Length; j++)
			{
				if (MB2_LOD.GetActiveSelf(this.levels[j].lodObject.gameObject) && this.levels[j].lodObject.enabled)
				{
					flag = true;
				}
			}
			if (!flag && this.nextLODidx < this.levels.Length)
			{
				Debug.LogError("All levels were invisible " + this);
			}
		}
	}

	private int GetHighestLODIndexOfOrothographicCameras(MB2_LODCamera[] cameras)
	{
		if (cameras.Length == 0)
		{
			return 0;
		}
		int num = this.levels.Length;
		foreach (MB2_LODCamera mb2_LODCamera in cameras)
		{
			if (mb2_LODCamera.enabled && MB2_LOD.GetActiveSelf(mb2_LODCamera.gameObject) && mb2_LODCamera.GetComponent<Camera>().orthographic)
			{
				float num2 = mb2_LODCamera.GetComponent<Camera>().orthographicSize * 2f;
				float num3 = this.levels[0].dimension / num2;
				for (int j = 0; j < num; j++)
				{
					if (num3 > this.levels[j].screenPercentage)
					{
						if (j < num)
						{
							num = j;
						}
						break;
					}
				}
			}
		}
		return num;
	}

	private void SwapBetweenLevels(int oldIdx, int newIdx)
	{
		if (oldIdx < this.levels.Length && newIdx < this.levels.Length && (oldIdx == 0 || this.levels[oldIdx].swapMeshWithLOD0) && (newIdx == 0 || this.levels[newIdx].swapMeshWithLOD0))
		{
			base.gameObject.SendMessage("LOD_OnSetLODActive", this.levels[0].root, 1);
			return;
		}
		if (oldIdx < this.levels.Length)
		{
			this.MySetActiveRecursively(oldIdx, false);
		}
		if (newIdx < this.levels.Length)
		{
			this.MySetActiveRecursively(newIdx, true);
		}
	}

	private void MySetActiveRecursively(int idx, bool a)
	{
		if (idx >= this.levels.Length)
		{
			return;
		}
		if (idx > 0 && this.levels[idx].swapMeshWithLOD0)
		{
			if (a)
			{
				Mesh mesh = MB_Utility.GetMesh(this.levels[idx].lodObject.gameObject);
				this.SetMesh(this.levels[0].lodObject.gameObject, mesh);
				if (this.levels[0].anim != null)
				{
					this.levels[0].anim.Sample();
				}
			}
			else
			{
				this.SetMesh(this.levels[0].lodObject.gameObject, this.lodZeroMesh);
				if (this.levels[0].anim != null)
				{
					this.levels[0].anim.Sample();
				}
			}
			if (MB2_LOD.GetActiveSelf(this.levels[0].root) != a)
			{
				MB2_Version.SetActiveRecursively(this.levels[0].root, a);
				base.gameObject.SendMessage("LOD_OnSetLODActive", this.levels[0].root, 1);
			}
			if (a && !this._isInCombined)
			{
				this.levels[0].lodObject.enabled = true;
			}
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				this.myLog.Log(MB2_LogLevel.trace, string.Concat(new object[] { "SettingActive (swaps mesh on level zero) to ", a, " for level ", idx, " on ", this }), this.LOG_LEVEL);
			}
		}
		else
		{
			if (MB2_LOD.GetActiveSelf(this.levels[idx].root) != a)
			{
				MB2_Version.SetActiveRecursively(this.levels[idx].root, a);
				base.gameObject.SendMessage("LOD_OnSetLODActive", this.levels[idx].root, 1);
			}
			if (a && !this._isInCombined)
			{
				this.levels[idx].lodObject.enabled = true;
			}
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				this.myLog.Log(MB2_LogLevel.trace, string.Concat(new object[] { "SettingActive to ", a, " for level ", idx, " on ", this }), this.LOG_LEVEL);
			}
		}
	}

	private void SetMesh(GameObject go, Mesh m)
	{
		if (go == null)
		{
			return;
		}
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			component.sharedMesh = m;
			return;
		}
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			component2.sharedMesh = m;
			return;
		}
		Debug.LogError("Object " + go.name + " does not have a MeshFilter or a SkinnedMeshRenderer component");
	}

	public static bool GetActiveSelf(GameObject go)
	{
		return go.activeSelf;
	}

	public static void SetActiveSelf(GameObject go, bool isActive)
	{
		go.SetActive(isActive);
	}

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	public LODLog myLog;

	public string bakerLabel = string.Empty;

	public MB_RenderType renderType;

	public int forceToLevel = -1;

	public bool bakeIntoCombined = true;

	public MB2_LOD.LOD[] levels;

	private Mesh lodZeroMesh;

	private float _distSqrToClosestCamera;

	private LODCombinedMesh combinedMesh;

	private MB2_LODManager.BakerPrototype baker;

	private MB2_LOD hierarchyRoot;

	private int currentLODidx;

	private int nextLODidx;

	private int orthographicIdx;

	private MB2_LODManager manager;

	private MB2_LOD.SwitchDistanceSetup setupStatus;

	private bool clustersAreSetup;

	private bool _wasLODDestroyed;

	private Vector3 _position;

	private bool _isInCombined;

	private bool _isInQueue;

	[NonSerialized]
	public MB2_LODOperation action = MB2_LODOperation.none;

	public enum SwitchDistanceSetup
	{
		notSetup,
		error,
		setup
	}

	[Serializable]
	public class LOD
	{
		public bool swapMeshWithLOD0;

		public bool bakeIntoCombined = true;

		public Animation anim;

		public Renderer lodObject;

		public int instanceID;

		public GameObject root;

		public float screenPercentage;

		public float dimension;

		public float sqrtDist;

		public float switchDistances;

		public int numVerts;
	}

	public class MB2_LODDistToCamComparer : IComparer<MB2_LOD>
	{
		int IComparer<MB2_LOD>.Compare(MB2_LOD aObj, MB2_LOD bObj)
		{
			return (int)(aObj._distSqrToClosestCamera - bObj._distSqrToClosestCamera);
		}
	}
}
