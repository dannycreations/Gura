using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.GlobalMap;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class Globe : MonoBehaviour
{
	private PondsRotations PondsRotations
	{
		get
		{
			return UIHelper.IsMouse ? this._pondsRotations : this._pondsRotationsCtrl;
		}
	}

	private float MaxZoom
	{
		get
		{
			return UIHelper.IsMouse ? this._maxZoomMouse : this._maxZoom;
		}
	}

	private void Awake()
	{
		this._ic.Init0(this._camera.transform);
		for (int i = 0; i < this._rangesHintArrows.Length; i++)
		{
			this._rangesHintArrowsDict[this._rangesHintArrows[i].Binding] = this._rangesHintArrows[i];
		}
		PhotonConnectionFactory.Instance.Profile.PersistentData = null;
		this._pondsRotations = Resources.Load<PondsRotations>("2D/PondsRotations");
		this._pondsRotationsCtrl = Resources.Load<PondsRotations>("2D/PondsRotationsController");
		Ponds lastPondId = SetPondsOnGlobalMap.LastPondId;
		this._planet.DisplayedPlanetParent.rotation = this.PondsRotations.FindRot(lastPondId, this._camera.transform.position.z);
		this.RotateCameraToSouthAmerica((int)lastPondId);
		this._state |= Globe.States.GlobalInited;
	}

	private void Start()
	{
		ShowPondInfo.Instance.TravelInit.OnTravelToPond += this.TravelInit_OnTravelToPond;
		ScreenManager.Instance.OnTransfer += this.Instance_OnTransfer;
		ScreenManager.Instance.OnScreenChanged += this.Instance_OnScreenChanged;
		this.UpdateCameraRect();
		CacheLibrary.MapCache.OnGetPond += this.MapCache_OnGetPond;
		SubMenuControllerNew component = ShowPondInfo.Instance.GetComponent<SubMenuControllerNew>();
		component.OnActive += this.SubMenu_OnActive;
		component.FishMenu.OnScene3D += this.FishMenu_OnScene3D;
		InputModuleManager.OnInputTypeChanged += this.InputModuleManager_OnInputTypeChanged;
	}

	private void Update()
	{
		if ((double)Math.Abs(this._screenW - (float)Screen.width) > 0.001 || (double)Math.Abs(this._screenH - (float)Screen.height) > 0.001)
		{
			this._screenW = (float)Screen.width;
			this._screenH = (float)Screen.height;
			this.UpdateCameraRect();
		}
		if (!this.InState(Globe.States.GlobalInited) || this.IsModalWindowActive || this.InState(Globe.States.TravelToPond))
		{
			return;
		}
		if (!this.InState(Globe.States.SpaceInited) && !this.InState(Globe.States.CameraInited))
		{
			this._planet.DisplayedPlanet.gameObject.SetActive(true);
			this._state |= Globe.States.CameraInited;
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOFieldOfView(this._camera, this._fov, this._fovAnimTime), 18), delegate
			{
				this._state |= Globe.States.SpaceInited;
				this._ic.Init(this._planet.DisplayedPlanetParent, this._camera.transform, this._camera, this._tCameraRoot);
			});
		}
		float maxZoom = this.MaxZoom;
		if (!MessageBoxList.Instance.IsActive && !InfoMessageController.Instance.IsActive)
		{
			this._ic.Update(this._speedRotateByMouse, this._isLockEarthRotX, this._maxZoomFoV, this._minZoomFoV, (!UIHelper.IsMouse) ? this._zoomSpeedFoVGamepad : this._zoomSpeedFoVMouse, this._cameraAngleMax, this._zoomSpeedForStop);
		}
		if (this.InState(Globe.States.Inited))
		{
			float num = Mathf.Abs(Mathf.Abs(maxZoom) - Mathf.Abs(this._minZoom));
			float num2 = Mathf.Abs(Mathf.Abs(this._camera.transform.position.z) - Mathf.Abs(maxZoom));
			float num3 = 100f - num2 * 100f / num;
			num3 = Mathf.Min(Mathf.Max(num3, 0f), 100f);
			float magnitude = (this._camera.transform.position - this._planet.DisplayedPlanet.position).magnitude;
			float num4 = Mathf.Abs(magnitude) - this._planet.DisplayedPlanet.localScale.x;
			float num5 = Mathf.Sqrt(Mathf.Pow(magnitude, 2f) + Mathf.Pow(this._planet.DisplayedPlanet.localScale.x, 2f));
			float num6 = this._animTime - Math.Abs(this._ic.RotEarthSpeedAroundY);
			if (num6 < 0f)
			{
				num6 = 0f;
			}
			for (int i = 0; i < this._ponds.Count; i++)
			{
				PondMarker pondMarker = this._ponds[i];
				Vector3 position = pondMarker.transform.position;
				float num7 = Vector3.Angle(pondMarker.transform.forward, this._planet.DisplayedPlanet.position - position);
				this.CheckState(pondMarker, (Mathf.Abs(num7) <= this._markerInvisibleAngle) ? PondMarker.States.Show : PondMarker.States.Hide, num6);
				Vector3 vector = this._camera.WorldToViewportPoint(position);
				SetPondsOnGlobalMap.SetNormalizedPosition(pondMarker.PondId, vector.x, vector.y);
				if (pondMarker.State == PondMarker.States.Show)
				{
					float magnitude2 = (this._camera.transform.position - position).magnitude;
					num = num5 - num4;
					num2 = Mathf.Abs(Mathf.Abs(magnitude2) - Mathf.Abs(num5));
					float num8 = this.maxPrc;
					if (this._camera.fieldOfView > this.fieldOfView0)
					{
						float num9 = this.fieldOfView1 - this.fieldOfView0;
						float num10 = (this._camera.fieldOfView - num9) / num9;
						float num11 = this.minPrc - this.maxPrc;
						num8 = num11 * num10 + this.maxPrc;
					}
					float num12 = num2 * num8 / num;
					SetPondsOnGlobalMap.SetScalePrc(pondMarker.PondId, num12);
				}
			}
			SetPondsOnGlobalMap.SortPonds();
			if (this._curPondId == -1)
			{
				int pondId = SetPondsOnGlobalMap.PondId;
				if (pondId != -1)
				{
					this.MapCache_OnGetPond(null, new GlobalMapPondCacheEventArgs
					{
						Pond = new Pond
						{
							PondId = pondId
						}
					});
				}
			}
			if (this._rangesHintArrows != null && this._rangesHintArrowsDict.Count > 0)
			{
				Transform transform = this._camera.transform;
				Transform displayedPlanetParent = this._planet.DisplayedPlanetParent;
				float num13 = Vector3.Angle(displayedPlanetParent.position - this._ic.CamPos0, displayedPlanetParent.position - transform.position);
				bool flag = this._rangesHintArrowsDict[UINavigation.Bindings.Right].InRange(displayedPlanetParent.rotation.eulerAngles.y);
				bool flag2 = this._rangesHintArrowsDict[UINavigation.Bindings.Left].InRange(displayedPlanetParent.rotation.eulerAngles.y);
				bool flag3 = this._rangesHintArrowsDict[UINavigation.Bindings.Down].InRange(displayedPlanetParent.rotation.eulerAngles.y);
				bool flag4 = num13 < this._cameraAngleSouthAmericaHint || transform.position.y > this._ic.CamPos0.y;
				if (!UIHelper.IsMouse)
				{
					flag4 = flag4 && SetPondsOnGlobalMap.PondId != 220;
				}
				SetPondsOnGlobalMap.GlobeHelp.SetActiveRotateHintArrow(UINavigation.Bindings.Right, flag && SetPondsOnGlobalMap.ActiveEurope);
				SetPondsOnGlobalMap.GlobeHelp.SetActiveRotateHintArrow(UINavigation.Bindings.Left, flag2);
				SetPondsOnGlobalMap.GlobeHelp.SetActiveRotateHintArrow(UINavigation.Bindings.Down, flag3 && flag4 && SetPondsOnGlobalMap.ActiveSouthAmerica);
			}
		}
		else if (SetPondsOnGlobalMap.Ponds.Count > 0 && !this.InState(Globe.States.Inited))
		{
			this.Init();
		}
	}

	private void OnDestroy()
	{
		if (ShowPondInfo.Instance != null)
		{
			SubMenuControllerNew component = ShowPondInfo.Instance.GetComponent<SubMenuControllerNew>();
			if (component != null)
			{
				component.OnActive -= this.SubMenu_OnActive;
				component.FishMenu.OnScene3D -= this.FishMenu_OnScene3D;
			}
		}
		ScreenManager.Instance.OnTransfer -= this.Instance_OnTransfer;
		ScreenManager.Instance.OnScreenChanged -= this.Instance_OnScreenChanged;
		CacheLibrary.MapCache.OnGetPond -= this.MapCache_OnGetPond;
		ShowPondInfo.Instance.TravelInit.OnTravelToPond -= this.TravelInit_OnTravelToPond;
		InputModuleManager.OnInputTypeChanged -= this.InputModuleManager_OnInputTypeChanged;
		this._ponds.ForEach(delegate(PondMarker p)
		{
			Object.Destroy(p.gameObject);
		});
		this._ponds.Clear();
	}

	private void InputModuleManager_OnInputTypeChanged(InputModuleManager.InputType it)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (this._curPondId != -1)
		{
			PondMarker pondMarker = this._ponds.First((PondMarker p) => p.PondId == this._curPondId);
			this.Rotation2Pond(pondMarker);
		}
		this._ic.ResetCamera();
		this._camera.transform.RotateAround(this._planet.DisplayedPlanetParent.position, Vector3.right, -this._camera.transform.rotation.eulerAngles.x);
		this.RotateCameraToSouthAmerica(this._curPondId);
	}

	private void RotateCameraToSouthAmerica(int pondId)
	{
		if (UIHelper.IsMouse && pondId == 220)
		{
			this._camera.transform.RotateAround(this._planet.DisplayedPlanetParent.position, Vector3.right, -28f);
		}
	}

	private void Init()
	{
		List<Pond> ponds = SetPondsOnGlobalMap.Ponds;
		for (int i = 0; i < ponds.Count; i++)
		{
			PondMarker pondMarker;
			if (i < this._markersPool.Length)
			{
				pondMarker = this._markersPool[i];
				pondMarker.transform.SetParent(this._planet.DisplayedPlanet);
			}
			else
			{
				pondMarker = Object.Instantiate<GameObject>(this._pondMarkerPrefab, this._planet.DisplayedPlanet).GetComponent<PondMarker>();
			}
			pondMarker.gameObject.SetActive(true);
			pondMarker.transform.localScale = this._localScale;
			pondMarker.transform.localPosition = new Vector3((float)ponds[i].MapX, (float)ponds[i].MapY, (float)ponds[i].MapZ);
			pondMarker.name = string.Format("PondMarker_{0}", ponds[i].PondId);
			pondMarker.Init(ponds[i].PondId);
			pondMarker.SetCamera(this._camera);
			this._ponds.Add(pondMarker);
		}
		this._state |= Globe.States.Inited;
	}

	private void UpdateCameraRect()
	{
		if (this._canvasScaler == null)
		{
			this._canvasScaler = SetPondsOnGlobalMap.MapTextureImage.GetComponentInParent<CanvasScaler>();
		}
		this._rt = new RenderTexture((int)(SetPondsOnGlobalMap.MapTextureImage.rectTransform.rect.width * this._screenW / this._canvasScaler.referenceResolution.x), (int)(SetPondsOnGlobalMap.MapTextureImage.rectTransform.rect.height * this._screenH / this._canvasScaler.referenceResolution.y), 32);
		this._camera.targetTexture = this._rt;
		SetPondsOnGlobalMap.MapTextureImage.texture = this._rt;
	}

	private bool InState(Globe.States s)
	{
		return (this._state & s) == s;
	}

	private void CheckState(PondMarker pm, PondMarker.States state, float animTime)
	{
		if (pm.State != state)
		{
			pm.State = state;
			SetPondsOnGlobalMap.DoFade(pm.PondId, (state != PondMarker.States.Show) ? 0f : 1f, animTime);
		}
	}

	private void Instance_OnTransfer(bool isTransfer)
	{
		if (isTransfer)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Instance_OnScreenChanged(GameScreenType s)
	{
		if (this.InState(Globe.States.SpaceInited))
		{
			bool flag = s == GameScreenType.GlobalMap;
			if ((flag && base.gameObject.activeSelf) || (!flag && !base.gameObject.activeSelf))
			{
				return;
			}
			base.gameObject.SetActive(flag);
		}
	}

	private void MapCache_OnGetPond(object sender, GlobalMapPondCacheEventArgs e)
	{
		if (this.InState(Globe.States.TravelToPond))
		{
			return;
		}
		PondMarker pondMarker = this._ponds.FirstOrDefault((PondMarker p) => p.PondId == e.Pond.PondId);
		if (pondMarker == null || e.Pond.PondId == this._curPondId)
		{
			return;
		}
		this._curPondId = e.Pond.PondId;
		this.Rotation2Pond(pondMarker);
	}

	private void Rotation2Pond(PondMarker pm)
	{
		Quaternion rotation = this._planet.DisplayedPlanetParent.rotation;
		Quaternion quaternion = this.PondsRotations.FindRot((Ponds)pm.PondId, this._camera.transform.position.z);
		float num = Quaternion.Angle(rotation, quaternion);
		if (num < 1f)
		{
			return;
		}
		float num2 = this._speedRotationToPin;
		if (Mathf.Abs(num) >= 90f)
		{
			num2 = this._speedMore90RotationToPin;
		}
		this._ic.Rotation2Pond(rotation, quaternion, num2);
	}

	private void FishMenu_OnScene3D(bool isActive)
	{
		this._planet.DisplayedPlanet.gameObject.SetActive(!isActive);
		this._camera.gameObject.SetActive(!isActive);
	}

	private void SubMenu_OnActive(bool isActive)
	{
		if (isActive && ShowPondInfo.Instance.GetComponent<SubMenuControllerNew>().IsTravelSubMenuOpened && SettingsManager.InputType != InputModuleManager.InputType.GamePad)
		{
			return;
		}
		this._ic.SetEnable(!isActive);
	}

	private bool IsModalWindowActive
	{
		get
		{
			return false;
		}
	}

	private void TravelInit_OnTravelToPond(Action callback)
	{
		this._state |= Globe.States.TravelToPond;
		DashboardTabSetter.Instance.SetActive(false);
		ShowPondInfo.Instance.TravelInit.OnTravelToPond -= this.TravelInit_OnTravelToPond;
		this._ponds.ForEach(delegate(PondMarker p)
		{
			this.CheckState(p, PondMarker.States.None, this._fovAnimTime / 4f);
		});
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOFieldOfView(this._camera, this._fovTravel, this._fovAnimTime / 2f), 20), delegate
		{
			this._camera.enabled = false;
			callback();
		});
	}

	[SerializeField]
	private float _cameraAngleSouthAmericaHint = 27f;

	[SerializeField]
	private Transform _tCameraRoot;

	[SerializeField]
	private float maxPrc = 80f;

	[SerializeField]
	private float minPrc = 20f;

	[SerializeField]
	private float fieldOfView0 = 50f;

	[SerializeField]
	private float fieldOfView1 = 100f;

	[SerializeField]
	private float _maxZoomFoV = 100f;

	[SerializeField]
	private float _minZoomFoV = 35f;

	[SerializeField]
	private float _zoomSpeedFoVMouse = 200f;

	[SerializeField]
	private float _zoomSpeedFoVGamepad = 10f;

	[SerializeField]
	private float _cameraAngleMax = 34f;

	[SerializeField]
	private float _zoomSpeedForStop = 4f;

	[Space(10f)]
	[SerializeField]
	private CanvasGroup _space;

	[SerializeField]
	private GameObject _pondMarkerPrefab;

	[SerializeField]
	private GUIEarth _planet;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private Transform _sun;

	[Tooltip("Show/Hide marker behind Earth")]
	[SerializeField]
	private float _animTime = 0.4f;

	[Tooltip("Degrees / 1 sec")]
	[SerializeField]
	private float _speedRotationToPin = 0.025f;

	[Tooltip("Degrees / 1 sec")]
	[SerializeField]
	private float _speedMore90RotationToPin = 0.015625f;

	[SerializeField]
	private float _zoomCamTime = 1f;

	[SerializeField]
	private float _maxZoomMouse = -3092.189f;

	[SerializeField]
	private float _maxZoom = -2700f;

	[SerializeField]
	private float _minZoom = -10000f;

	[SerializeField]
	private float _speedZoom = 100f;

	[SerializeField]
	private float _initialSpeedZoom = 1f;

	[SerializeField]
	private float _zoom0 = -50000f;

	[SerializeField]
	private float _fov = 50f;

	[SerializeField]
	private float _fovTravel = 60f;

	[SerializeField]
	private float _fovAnimTime = 1.2f;

	[Space(5f)]
	[SerializeField]
	private float _camMoveLookSpeed = 1f;

	[SerializeField]
	private bool _isLockEarthRotX = true;

	[SerializeField]
	private float _markerInvisibleAngle = 90f;

	[Tooltip("Mouse controll")]
	[SerializeField]
	private Vector2 _speedRotateByMouse = new Vector2(0.00625f, 0.0015625f);

	[SerializeField]
	private Globe.Range[] _rangesHintArrows;

	[Space(10f)]
	[SerializeField]
	private Ponds _pond = Ponds.FirstStep;

	[SerializeField]
	private Globe.Zooms _zoom = Globe.Zooms.Z_4000;

	[Space(10f)]
	[SerializeField]
	private PondMarker[] _markersPool;

	private int _curPondId = -1;

	private readonly Vector3 _localScale = new Vector3(0.0125f, 0.0125f, 0.0125f);

	private List<PondMarker> _ponds = new List<PondMarker>();

	private GlobeInputController _ic = new GlobeInputController();

	private const float NativeWidth = 1920f;

	private const float NativeHeight = 1080f;

	private const string RotPrefabPath = "2D/PondsRotations";

	private const string RotCtrlPrefabPath = "2D/PondsRotationsController";

	private float _screenW = 1920f;

	private float _screenH = 1080f;

	private PondsRotations _pondsRotations;

	private PondsRotations _pondsRotationsCtrl;

	private Dictionary<UINavigation.Bindings, Globe.Range> _rangesHintArrowsDict = new Dictionary<UINavigation.Bindings, Globe.Range>();

	private Globe.States _state;

	private RenderTexture _rt;

	private CanvasScaler _canvasScaler;

	[Serializable]
	public class Range
	{
		public bool InRange(float v)
		{
			return v >= this.Min && v <= this.Max;
		}

		public float Min;

		public float Max;

		public UINavigation.Bindings Binding;
	}

	public enum Zooms
	{
		Z_2900 = -2900,
		Z_4000 = -4000,
		Z_5200 = -5200,
		Z_6400 = -6400,
		Z_7600 = -7600,
		Z_8800 = -8800,
		Z_10000 = -10000
	}

	[Flags]
	private enum States : byte
	{
		None = 0,
		Inited = 1,
		GlobalInited = 2,
		CameraInited = 4,
		SpaceInited = 8,
		TravelToPond = 64
	}
}
