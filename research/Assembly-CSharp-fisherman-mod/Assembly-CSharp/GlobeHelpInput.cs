using System;
using System.Linq;
using DG.Tweening;
using ObjectModel;
using UnityEngine;

public class GlobeHelpInput : MonoBehaviour
{
	private void Start()
	{
		ClientMissionsManager.Instance.TrackedMissionUpdated += this.Populate;
		ShowPondInfo.Instance.TravelInit.OnTravelToPond += this.TravelInit_OnTravelToPond;
		this.UpdateTexts(SettingsManager.InputType == InputModuleManager.InputType.GamePad);
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnDestroy()
	{
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.Populate;
		ShowPondInfo.Instance.TravelInit.OnTravelToPond -= this.TravelInit_OnTravelToPond;
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void Update()
	{
		if (this._pond != null && SetPondsOnGlobalMap.Ponds.Count > 0)
		{
			bool flag = !SetPondsOnGlobalMap.IsPondBtnVisible(this._pond.PondId) && !this._isTravelToPond && ClientMissionsManager.Instance.CurrentTrackedMission != null;
			if (this._isHintActive != flag)
			{
				this._isHintActive = flag;
				this.SetActiveHint(this._isHintActive);
			}
		}
		else if (this._isHintActive)
		{
			this._isHintActive = false;
			this.SetActiveHint(this._isHintActive);
		}
	}

	public void MoveDownArrow(float earthAngleY)
	{
	}

	public void MoveRightArrow(float cameraAngle, bool moveToNorthPole, float cameraAngleMax)
	{
		float num = ((!moveToNorthPole) ? this._southPoleMax : this._northPoleMax);
		this._rtArrowRootRight.anchoredPosition = new Vector2(this._rtArrowRootRight.anchoredPosition.x, num * cameraAngle / cameraAngleMax + this._rightY0);
	}

	public void SetActiveRotateHintArrow(UINavigation.Bindings binding, bool flag)
	{
		for (int i = 0; i < this._arrowsRotate.Length; i++)
		{
			GlobeHelpInput.GlobeHelpArrows globeHelpArrows = this._arrowsRotate[i];
			if (globeHelpArrows.Binding == binding)
			{
				if (globeHelpArrows.Arrows != null)
				{
					for (int j = 0; j < globeHelpArrows.Arrows.Length; j++)
					{
						globeHelpArrows.Arrows[j].enabled = flag;
					}
				}
				break;
			}
		}
	}

	public void SetActiveHint(string elementId)
	{
		this._pond = (string.IsNullOrEmpty(elementId) ? null : CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.Asset == elementId && p.PondId != 2));
	}

	private void SetActiveHint(bool flag)
	{
		ShortcutExtensions.DOFade(this._cgHint, (!flag) ? 0f : 1f, 0.1f);
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.UpdateTexts(type == InputModuleManager.InputType.GamePad);
	}

	private void UpdateTexts(bool isGamepad)
	{
		this._zoom.SetActive(!isGamepad);
		this._rotate.SetActive(!isGamepad);
		this._zoomCtrl.SetActive(isGamepad);
		this._rotateCtrl.SetActive(isGamepad);
	}

	private void TravelInit_OnTravelToPond(Action callback)
	{
		this._isTravelToPond = true;
	}

	public void Populate(MissionOnClient mission)
	{
		if (mission == null)
		{
			this._pond = null;
		}
	}

	[SerializeField]
	private CanvasGroup _cgHint;

	[SerializeField]
	private GameObject _zoom;

	[SerializeField]
	private GameObject _rotate;

	[SerializeField]
	private GameObject _zoomCtrl;

	[SerializeField]
	private GameObject _rotateCtrl;

	[SerializeField]
	private Transform _zoomTransform;

	[SerializeField]
	private Transform _rotateTransform;

	[SerializeField]
	private GlobeHelpInput.GlobeHelpArrows[] _arrowsRotate;

	[SerializeField]
	private float _northPoleMax = -450f;

	[SerializeField]
	private float _southPoleMax = 250f;

	[SerializeField]
	private float _rightY0 = 150f;

	[SerializeField]
	private RectTransform _rtArrowRootRight;

	[SerializeField]
	private RectTransform _rtArrowRootDown;

	private Pond _pond;

	private bool _isHintActive;

	private bool _isTravelToPond;

	[Serializable]
	public class GlobeHelpArrows
	{
		public UINavigation.Bindings Binding;

		public ArrowJamping[] Arrows;

		public CanvasGroup[] CanvasGroups;
	}
}
