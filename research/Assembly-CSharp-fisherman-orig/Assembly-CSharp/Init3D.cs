using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Init3D : MonoBehaviour
{
	public static SceneSettings SceneSettings
	{
		get
		{
			return Init3D._gSceneSettings;
		}
	}

	public static Transform DebugPlayer { get; set; }

	public static Terrain ActiveTerrain { get; private set; }

	private void buildMeshCollidersCache()
	{
		Init3D.meshCollidersCache = new Dictionary<int, ICollider>();
		foreach (MeshCollider meshCollider in Object.FindObjectsOfType<MeshCollider>())
		{
			if (GlobalConsts.FishMask == (GlobalConsts.FishMask | (1 << meshCollider.gameObject.layer)))
			{
				if (meshCollider.sharedMesh == null)
				{
					Debug.LogError(string.Format("MeshCollider {0} has no sharedMesh.", meshCollider.gameObject.name));
				}
				else if (meshCollider.sharedMesh.triangles.Length <= 128 && ExtrudedPolygon.CheckSourceCollider(meshCollider))
				{
					Init3D.meshCollidersCache[meshCollider.gameObject.GetInstanceID()] = new ExtrudedPolygon(meshCollider);
				}
				else
				{
					float num = Mathf.Max(meshCollider.bounds.size.x, meshCollider.bounds.size.z);
					float num2 = 2f * Mathf.Min(1f, 31f / num);
					Init3D.meshCollidersCache[meshCollider.gameObject.GetInstanceID()] = new HeightFieldChunk(meshCollider, num2);
				}
			}
		}
	}

	private void findActiveTerrain()
	{
		Init3D.ActiveTerrain = null;
		foreach (Terrain terrain in Terrain.activeTerrains)
		{
			if (terrain.gameObject.layer == GlobalConsts.TerrainLayer)
			{
				Init3D.ActiveTerrain = terrain;
				break;
			}
		}
		if (Init3D.ActiveTerrain == null)
		{
			Debug.LogError("Terrain with proper TerrainLayer has not been found in the scene.");
		}
	}

	private void Awake()
	{
		this.findActiveTerrain();
		this.buildMeshCollidersCache();
		GameFactory.Player = Init3D.FindObject("FakeHandsRoot").GetComponent<PlayerController>();
	}

	private void Start()
	{
		if (this._sceneSettings == null)
		{
			LogHelper.Error("Scene settings is not defined! Can't read height map and splat map data", new object[0]);
		}
		else
		{
			if (this._sceneSettings.HeightMap == null)
			{
				LogHelper.Error("Height map is not found. Please generate it in Bite editor", new object[0]);
			}
			if (this._sceneSettings.SplatMap == null)
			{
				LogHelper.Error("Splat map is not found. Please generate it in Bite editor", new object[0]);
			}
		}
		Init3D._gSceneSettings = this._sceneSettings;
		Shader.DisableKeyword("FISH_PROCEDURAL_BEND_BYPASS");
		if (!RenderSettings.fog || RenderSettings.fogMode != 2)
		{
			RenderSettings.fog = true;
			RenderSettings.fogMode = 2;
		}
		this.splashCache = DynWaterParticlesController.CreateSplash(GameFactory.Player.CameraController.transform, new Vector3(0f, -1000f, 0f), "2D/Splashes/pSplash_universal", 1f, 1f, false, false, 1);
	}

	private void OnDestroy()
	{
		try
		{
			Object.Destroy(this.splashCache);
			this.splashCache = null;
			GameFactory.Clear(false);
		}
		catch
		{
		}
		Init3D._gSceneSettings = null;
		this._sceneSettings = null;
	}

	private void OnApplicationQuit()
	{
		RodOnPodBehaviour.StopRodPodSim();
		foreach (GameFactory.RodSlot rodSlot in GameFactory.RodSlots)
		{
			if (rodSlot.Reel != null)
			{
				rodSlot.Reel.gameObject.SetActive(false);
			}
			if (rodSlot.Line != null)
			{
				rodSlot.Line.gameObject.SetActive(false);
			}
			if (rodSlot.Rod != null)
			{
				rodSlot.Rod.gameObject.SetActive(false);
			}
			if (rodSlot.Tackle != null)
			{
				rodSlot.Tackle.gameObject.SetActive(false);
			}
			if (rodSlot.Sim != null)
			{
				rodSlot.Sim.SaveReplayRecorder();
			}
			if (rodSlot.SimThread != null)
			{
				rodSlot.SimThread.InternalSim.SaveReplayRecorder();
			}
			rodSlot.Clear();
		}
		if (GameFactory.Player != null)
		{
			GameFactory.Player.gameObject.SetActive(false);
		}
		Debug.LogWarning("Init3D.OnApplicationQuit");
	}

	private static GameObject FindObject(string name)
	{
		foreach (Object @object in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
		{
			if (@object.name == name)
			{
				return (GameObject)@object;
			}
		}
		return null;
	}

	private void Update()
	{
		this._connectionTimer += Time.deltaTime;
		BoatShaderParametersController.ResetFrame();
		if (this._connectionTimer >= 5f)
		{
			this._connectionTimer = 0f;
			if (GameFactory.Player != null && GameFactory.Player.CameraController != null && GameFactory.Player.CameraController.Camera != null)
			{
				if (SettingsManager.Antialiasing == AntialiasingValue.Off)
				{
					if (GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().enabled = false;
					}
					if (GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().enabled = false;
					}
				}
				else if (SettingsManager.Antialiasing == AntialiasingValue.Low)
				{
					if (GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().enabled = false;
					}
					if (!GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().enabled = true;
					}
				}
				else if (SettingsManager.Antialiasing == AntialiasingValue.High)
				{
					if (!GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<SMAA>().enabled = true;
					}
					if (GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().isActiveAndEnabled)
					{
						GameFactory.Player.CameraController.Camera.GetComponent<Antialiasing>().enabled = false;
					}
				}
			}
		}
	}

	private static SceneSettings _gSceneSettings;

	[SerializeField]
	private SceneSettings _sceneSettings;

	private float _connectionTimer = 5f;

	private const float ConnectionTimerMax = 5f;

	private GameObject splashCache;

	public static Dictionary<int, ICollider> meshCollidersCache;
}
