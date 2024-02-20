using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using mset;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoadPreviewScene : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action<bool> OnLoad;

	public void LoadScene(string assetName, ModelInfo mInfo, Viewer3DArguments.ViewArgs viewArgs = null)
	{
		Pond currentPond = StaticUserData.CurrentPond;
		if (!this._isLoading)
		{
			this._isLoading = true;
			this._isFirstTime = false;
			LoadPreviewScene.ActiveInstance = this;
			UIHelper.Waiting(true, null);
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			base.StartCoroutine(AssetBundleManager.LoadAssetBundleFromDisk(this._bundleName, new Action<AssetBundle>(this.OnLoadedPreviewBundle)));
			base.StartCoroutine(this.LoadPreview(assetName));
			base.StartCoroutine(this.SetPreview(viewArgs, mInfo));
		}
	}

	public void ShowFirstTime()
	{
		this._isFirstTime = true;
		base.StartCoroutine(AssetBundleManager.LoadAssetBundleFromDisk(this._bundleName, new Action<AssetBundle>(this.OnLoadedPreviewBundle)));
	}

	public void LoadPreview(GameObject newPrefab, Viewer3DArguments.ViewArgs viewArgs = null)
	{
		this.prefab = newPrefab;
		base.StartCoroutine(this.SetPreview(viewArgs, null));
	}

	private void CantShowPreviewMessage()
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
		this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantShowPreview"), false, false, false, null);
		this.messageBox.GetComponent<EventAction>().ActionCalled += delegate(object o, EventArgs e)
		{
			if (this.messageBox != null)
			{
				this.messageBox.Close();
			}
		};
	}

	private void OnLoadedPreviewBundle(AssetBundle assetBundle)
	{
		this._sceneBundle = assetBundle;
		base.StartCoroutine(AssetBundleManager.LoadPrefabFromExisting(this._sceneBundle, this._bundleName, new Action<Object>(this.OnLoadedPreviewAsset)));
	}

	private void VerifyLastItemReselection()
	{
		if (this._lastSelected != null && BlockableRegion.Current != null)
		{
			BlockableRegion.Current.OverrideLastSelected(this._lastSelected);
		}
	}

	private void OnLoadedPreviewAsset(Object loaded)
	{
		HelpLinePanel.DisablePanel();
		this._instantiatedPreviewRoot = Object.Instantiate(loaded) as GameObject;
		this._instantiatedPreviewRoot.transform.position = Vector3.zero;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this._lastSelected = ((!(BlockableRegion.Current != null)) ? null : BlockableRegion.Current.GetSavedLastSelected());
		UIHelper.Waiting(false, null);
		this._isLoading = false;
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockInput(null);
		this.groupsConfigs.Clear();
		ManagerScenes.Instance.Background.SetVisibility(false);
		foreach (CanvasGroup canvasGroup in SetupGUICamera.ActiveCanvasGroups)
		{
			this.groupsConfigs.Add(new LoadPreviewScene.cgStats
			{
				Alpha = canvasGroup.alpha,
				BlocksRaycasts = canvasGroup.blocksRaycasts,
				Interactable = canvasGroup.interactable,
				IgnoreParentGroups = canvasGroup.ignoreParentGroups,
				group = canvasGroup
			});
			canvasGroup.interactable = false;
			canvasGroup.alpha = 0f;
			canvasGroup.ignoreParentGroups = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	private IEnumerator LoadPreview(string assetName)
	{
		this.prefab = null;
		ResourceRequest prefabRequest = Resources.LoadAsync(assetName, typeof(GameObject));
		while (!prefabRequest.isDone)
		{
			yield return null;
		}
		if (prefabRequest.asset == null)
		{
			throw new ArgumentException("Cannot load prefab '" + assetName + "' for preview.");
		}
		this.prefab = prefabRequest.asset as GameObject;
		yield break;
	}

	public void UnloadScene()
	{
		if (PondControllers.Instance == null || PondControllers.Instance.IsInMenu)
		{
			ManagerScenes.Instance.Background.SetVisibility(true);
		}
		LoadPreviewScene.ActiveInstance = null;
		foreach (LoadPreviewScene.cgStats cgStats in this.groupsConfigs)
		{
			if (SetupGUICamera.ActiveCanvasGroups.Contains(cgStats.group))
			{
				cgStats.group.alpha = cgStats.Alpha;
				cgStats.group.blocksRaycasts = cgStats.BlocksRaycasts;
				cgStats.group.ignoreParentGroups = cgStats.IgnoreParentGroups;
				cgStats.group.interactable = cgStats.Interactable;
			}
		}
		Object.Destroy(this._instantiatedPreviewRoot);
		Object.Destroy(this._viewModel);
		this._sceneBundle.Unload(true);
		Resources.UnloadUnusedAssets();
		if (this._light != null)
		{
			this._light.enabled = true;
		}
		if (this._skyManager != null)
		{
			this._skyManager.enabled = true;
		}
		if (PondControllers.Instance == null || PondControllers.Instance.IsInMenu)
		{
			MenuHelpers.Instance.SetEnabledGUICamera(true);
		}
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
		if (InfoMessageController.Instance != null)
		{
			InfoMessageController.Instance.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
		}
		if (ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
		}
		HelpLinePanel.EnablePanel();
		LoadPreviewScene.OnLoad(false);
	}

	private IEnumerator SetPreview(Viewer3DArguments.ViewArgs viewArgs, ModelInfo mInfo)
	{
		while (this.prefab == null || this._isLoading)
		{
			yield return null;
		}
		LoadPreviewScene.OnLoad(true);
		Viewer3D viewer = Object.FindObjectOfType<Viewer3D>();
		Viewer3D viewer3D = viewer;
		viewer3D.CloseAction = (Action)Delegate.Combine(viewer3D.CloseAction, new Action(this.UnloadScene));
		if (this.prefab.GetComponent<FishAnimationController>() != null)
		{
			this.LoadFish(viewer, mInfo);
		}
		else
		{
			this.LoadItem(viewer, viewArgs, mInfo);
		}
		MenuHelpers.Instance.SetEnabledGUICamera(false);
		Init3D init3D = Object.FindObjectOfType<Init3D>();
		if (init3D != null)
		{
			this._light = TransformHelper.FindObjectsByPath("SKY/LIGHTS")[0].GetComponentInChildren<Light>();
			this._light.enabled = false;
			this._skyManager = TransformHelper.FindObjectsByPath("SKY/Sky Manager")[0].GetComponent<SkyManager>();
			this._skyManager.enabled = false;
		}
		if (InfoMessageController.Instance != null)
		{
			InfoMessageController.Instance.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
		}
		if (ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
		}
		yield break;
	}

	private void LoadFish(Viewer3D viewer, ModelInfo mInfo)
	{
		this._viewModel = Object.Instantiate<GameObject>(this.prefab);
		Object.Destroy(this._viewModel.GetComponent<FishAnimationController>());
		Object.Destroy(this._viewModel.GetComponent<FishController>());
		Animation component = this._viewModel.GetComponent<Animation>();
		component.playAutomatically = false;
		AnimationState animationState = component["idle"];
		animationState.blendMode = 0;
		animationState.weight = 0f;
		animationState.layer = 5;
		animationState.enabled = true;
		animationState.wrapMode = 2;
		component.Play("idle");
		SkinnedMeshRenderer componentInChildren = this._viewModel.GetComponentInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentInChildren.materials.Length; i++)
		{
			if (!this._ignoredFishReplaceShaders.Contains(componentInChildren.materials[i].shader.name))
			{
				componentInChildren.materials[i].shader = Shader.Find("Marmoset/Beta/Skin Fish Debug IBL");
			}
		}
		Shader.EnableKeyword("FISH_FUR_PROCEDURAL_BEND_BYPASS");
		viewer.SetModel(this._viewModel, componentInChildren.sharedMesh.bounds, 0f, new Vector3(0f, 65f, 0f), mInfo, true, default(Vector3));
		this.VerifyLastItemReselection();
	}

	private void LoadItem(Viewer3D viewer, Viewer3DArguments.ViewArgs viewArgs, ModelInfo mInfo)
	{
		this._viewModel = Object.Instantiate<GameObject>(this.prefab);
		if (mInfo.ItemSubType == ItemSubTypes.RodStand)
		{
			RodPodController component = this._viewModel.GetComponent<RodPodController>();
			if (component != null)
			{
				component.ActivateOutline(false);
			}
		}
		MeshRenderer[] componentsInChildren = this._viewModel.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] componentsInChildren2 = this._viewModel.GetComponentsInChildren<SkinnedMeshRenderer>();
		Mesh mesh = null;
		MeshFilter meshFilter = null;
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			meshFilter = this._viewModel.GetComponentInChildren<MeshFilter>();
			mesh = meshFilter.sharedMesh;
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				for (int j = 0; j < meshRenderer.materials.Length; j++)
				{
					if (meshRenderer.materials[j].shader.name == "M_Special/Rod IBL")
					{
						meshRenderer.materials[j].shader = Shader.Find("Marmoset/Bumped Specular IBL");
					}
				}
				if (mInfo.ItemSubType == ItemSubTypes.Rod || mInfo.ItemSubType == ItemSubTypes.CastingRod || mInfo.ItemSubType == ItemSubTypes.MatchRod || mInfo.ItemSubType == ItemSubTypes.TelescopicRod || mInfo.ItemSubType == ItemSubTypes.SpinningRod || mInfo.ItemSubType == ItemSubTypes.Reel || mInfo.ItemSubType == ItemSubTypes.SpinReel || mInfo.ItemSubType == ItemSubTypes.CastReel)
				{
					base.StartCoroutine(RodInitialize.LoadAndReplaceHQTextures(new Renderer[] { meshRenderer }));
				}
			}
		}
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in componentsInChildren2)
			{
				skinnedMeshRenderer = skinnedMeshRenderer2;
				mesh = skinnedMeshRenderer2.sharedMesh ?? mesh;
				for (int l = 0; l < skinnedMeshRenderer2.materials.Length; l++)
				{
					if (skinnedMeshRenderer2.materials[l].shader.name == "M_Special/Rod IBL")
					{
						skinnedMeshRenderer2.materials[l].shader = Shader.Find("Marmoset/Bumped Specular IBL");
					}
				}
				if (mInfo.ItemSubType == ItemSubTypes.Rod || mInfo.ItemSubType == ItemSubTypes.CastingRod || mInfo.ItemSubType == ItemSubTypes.MatchRod || mInfo.ItemSubType == ItemSubTypes.TelescopicRod || mInfo.ItemSubType == ItemSubTypes.SpinningRod || mInfo.ItemSubType == ItemSubTypes.Reel || mInfo.ItemSubType == ItemSubTypes.SpinReel || mInfo.ItemSubType == ItemSubTypes.CastReel)
				{
					base.StartCoroutine(RodInitialize.LoadAndReplaceHQTextures(new Renderer[] { skinnedMeshRenderer2 }));
				}
			}
		}
		if (mesh != null)
		{
			Vector3 vector = this._viewModel.transform.InverseTransformPoint(((!(skinnedMeshRenderer != null)) ? meshFilter.transform : skinnedMeshRenderer.transform).position);
			viewer.SetModel(this._viewModel, mesh.bounds, (viewArgs == null) ? new Viewer3DArguments.ViewArgs() : viewArgs, mInfo, vector);
			this.VerifyLastItemReselection();
		}
	}

	// Note: this type is marked as 'beforefieldinit'.
	static LoadPreviewScene()
	{
		LoadPreviewScene.OnLoad = delegate(bool b)
		{
		};
	}

	public static LoadPreviewScene ActiveInstance;

	private const bool _onlyOnGlobalMap = false;

	private string _bundleName = "3dpreviewasset";

	private bool _isLoading;

	private GameObject prefab;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private GameObject _instantiatedPreviewRoot;

	private GameObject _viewModel;

	private Light _light;

	private SkyManager _skyManager;

	private const string _fishShaderName = "Marmoset/Beta/Skin Fish Debug IBL";

	private const string _rodShaderName = "M_Special/Rod IBL";

	private const string _rodReplaceShaderName = "Marmoset/Bumped Specular IBL";

	private List<string> _ignoredFishReplaceShaders = new List<string> { "M_Special/Fur_40Shells_noskin", "M_Special/Ghost Fish" };

	private bool _isFirstTime;

	private AssetBundle _sceneBundle;

	private MessageBox messageBox;

	private MenuHelpers helpers = new MenuHelpers();

	private List<LoadPreviewScene.cgStats> groupsConfigs = new List<LoadPreviewScene.cgStats>();

	private GameObject _lastSelected;

	private struct cgStats
	{
		public CanvasGroup group;

		public float Alpha;

		public bool BlocksRaycasts;

		public bool Interactable;

		public bool IgnoreParentGroups;
	}
}
