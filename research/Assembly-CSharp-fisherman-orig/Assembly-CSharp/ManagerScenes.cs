using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Cayman;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerScenes : MonoBehaviour
{
	public static float ProgressOfLoad { get; set; }

	public static bool InTransition { get; private set; }

	public SetupGUICamera FirstScreenForm
	{
		get
		{
			return this._firstScreenForm;
		}
	}

	public ActivityState StartForm
	{
		get
		{
			return this._startForm;
		}
	}

	public ActivityState TravelingForm
	{
		get
		{
			return this._travelingForm;
		}
	}

	public ActivityState LoadingForm
	{
		get
		{
			return this._loadingForm;
		}
	}

	public Background Background
	{
		get
		{
			return this._background;
		}
	}

	public static ManagerScenes Instance
	{
		get
		{
			return ManagerScenes._instance;
		}
	}

	public bool CanLoadGlobalMap
	{
		get
		{
			return this._lastLoadedScene != "GlobalMapScene";
		}
	}

	private void Awake()
	{
		if (ManagerScenes._instance != null)
		{
			Object.Destroy(ManagerScenes._instance.gameObject);
			MenuHelpers.Instance.KillCamera();
		}
		ManagerScenes._instance = this;
		Object.DontDestroyOnLoad(this);
		this._forms.Add(this._startForm);
		this._forms.Add(this._travelingForm);
		this._forms.Add(this._loadingForm);
		MenuHelpers.Instance.GUICamera.transform.SetParent(base.transform);
	}

	public void LoadGlobalMap()
	{
		if (CaymanGenerator.Instance != null)
		{
			CaymanGenerator.Instance.OnGotoGlobalMap();
		}
		base.StartCoroutine(this.LoadLevel("GlobalMapScene"));
	}

	public void LoadStartScene()
	{
		base.StartCoroutine(this.LoadLevel("StartScene"));
	}

	public void LoadScene(string sceneName)
	{
		if (sceneName == string.Empty)
		{
			this.LoadGlobalMap();
			return;
		}
		GC.Collect();
		base.StartCoroutine(this.LoadLevel(sceneName));
	}

	private IEnumerator LoadLevel(string sceneName)
	{
		PhotonConnectionFactory.Instance.StopPhotonMessageQueue();
		bool isGeometryPresent;
		try
		{
			LogHelper.Log("LoadLevel({0})", new object[] { sceneName });
			this._lastLoadedScene = sceneName;
			this._firstScreenForm.gameObject.SetActive(false);
			ManagerScenes.InTransition = true;
			float initialProgress = ManagerScenes.ProgressOfLoad;
			string geometryScene = sceneName + "_geometry";
			isGeometryPresent = Application.CanStreamedLevelBeLoaded(geometryScene);
			AsyncOperation operation;
			if (isGeometryPresent)
			{
				operation = SceneManager.LoadSceneAsync(geometryScene);
				while (!operation.isDone)
				{
					ManagerScenes.ProgressOfLoad = initialProgress + operation.progress * (1f - initialProgress - 0.1f);
					yield return null;
				}
				LogHelper.Log("{0} loading complete", new object[] { geometryScene });
				this.SetupCameras();
			}
			initialProgress = ManagerScenes.ProgressOfLoad;
			operation = SceneManager.LoadSceneAsync(sceneName, (!isGeometryPresent) ? 0 : 1);
			while (!operation.isDone)
			{
				ManagerScenes.ProgressOfLoad = initialProgress + operation.progress * (1f - initialProgress);
				yield return null;
			}
			LogHelper.Log("{0} loading complete", new object[] { sceneName });
		}
		finally
		{
			PhotonConnectionFactory.Instance.StartPhotonMessageQueue();
		}
		ManagerScenes.ProgressOfLoad = 0f;
		ManagerScenes.InTransition = false;
		if (!isGeometryPresent)
		{
			this.SetupCameras();
		}
		for (int i = 0; i < this._forms.Count; i++)
		{
			this.DeactivateForm(this._forms[i]);
		}
		if (sceneName == "StartScene")
		{
			this._firstScreenForm.gameObject.SetActive(true);
		}
		yield break;
	}

	private void CleanupCameras()
	{
		IEnumerable<Camera> enumerable = Camera.allCameras.Where((Camera x) => x.CompareTag("GUICamera"));
		foreach (Camera camera in enumerable)
		{
			if (camera != MenuHelpers.Instance.GUICamera)
			{
				Object.Destroy(camera.gameObject);
			}
		}
	}

	private void SetupCameras()
	{
		this.CleanupCameras();
		for (int i = 0; i < this._forms.Count; i++)
		{
			this._forms[i].SetupCamera();
		}
		this._background.GetComponent<SetupGUICamera>().Setup();
		this._firstScreenForm.Setup();
	}

	private void DeactivateForm(ActivityState form)
	{
		form.Hide(true);
		LoadPond component = form.GetComponent<LoadPond>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		LoadLocation component2 = form.GetComponent<LoadLocation>();
		if (component2 != null)
		{
			Object.Destroy(component2);
		}
		BackFromPond component3 = form.GetComponent<BackFromPond>();
		if (component3 != null)
		{
			Object.Destroy(component3);
		}
		ConnectToLobbyAndGotoGame component4 = form.GetComponent<ConnectToLobbyAndGotoGame>();
		if (component4 != null)
		{
			Object.Destroy(component4);
		}
	}

	public const string StartSceneName = "StartScene";

	public const string GlobalMapSceneName = "GlobalMapScene";

	[SerializeField]
	private SetupGUICamera _firstScreenForm;

	[SerializeField]
	private ActivityState _startForm;

	[SerializeField]
	private ActivityState _travelingForm;

	[SerializeField]
	private ActivityState _loadingForm;

	[SerializeField]
	private Background _background;

	private static ManagerScenes _instance;

	private List<ActivityState> _forms = new List<ActivityState>();

	private string _lastLoadedScene;
}
