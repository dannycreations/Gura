using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteFishInFishkeeper : MonoBehaviour
{
	private void Start()
	{
		this.Release.onClick.AddListener(delegate
		{
			this.ReleaseFish();
		});
	}

	public void OnSelected()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.Hotkey.SetActive(true);
		}
		if (this.Info.isOn)
		{
			this.Preview.StartListenForHotkeys();
		}
	}

	public void OnDeselected()
	{
		this.Preview.StopListenForHotKeys();
		this.Hotkey.SetActive(false);
	}

	private void ReleaseFish()
	{
		if (this.CaughtFish == null || !PhotonConnectionFactory.Instance.Profile.FishCage.Cage.Safety)
		{
			return;
		}
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage += this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed += this.OnFishReleaseFailed;
		PhotonConnectionFactory.Instance.ReleaseFishFromFishCage(this.CaughtFish.Fish);
		if (this.CaughtFish.Fish != null && this.CaughtFish.Fish.NoRelease)
		{
			FishPenaltyHelper fishPenaltyHelper = MessageBoxList.Instance.gameObject.AddComponent<FishPenaltyHelper>();
			fishPenaltyHelper.CheckPenalty(true);
		}
	}

	public void OnFishReleaseFailed(Failure failure)
	{
		LogHelper.Error(failure.ErrorMessage, new object[0]);
		UIHelper.Waiting(false, null);
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnFishReleaseFailed;
	}

	public void OnFishReleased(Guid fishId)
	{
		UIHelper.Waiting(false, null);
		if (fishId != this.CaughtFish.Fish.InstanceId.Value)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnFishReleaseFailed;
	}

	public void InitItem(CaughtFish fish, List<InventoryItem> tackles)
	{
		this.CaughtFish = fish;
		this.Name.text = fish.Fish.Name;
		this.Weight.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(fish.Fish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		this.Length.text = string.Format("{0} {1}", MeasuringSystemManager.FishLength(fish.Fish.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		if (fish.Fish.GoldCost != null)
		{
			this.Money.text = string.Format("{0}", fish.Fish.GoldCost);
			this.MoneySuffix.text = "\ue62c";
		}
		else
		{
			this.Money.text = string.Format("{0}", fish.Fish.SilverCost);
			this.MoneySuffix.text = "\ue62b";
		}
		this.Expiriences.text = string.Format("{0}", fish.Fish.Experience);
		this.Weather.text = WeatherHelper.GetWeatherIcon(fish.WeatherIcon);
		this.TimeOfDate.text = TimeAndWeatherManager.GetTimeOfDayCaption(fish.Time);
		this.IconLoadable.Load(fish.Fish.ThumbnailBID, this.Icon, "Textures/Inventory/{0}");
		InventoryItem inventoryItem = ((tackles.Count <= 0) ? null : tackles[0]);
		if (inventoryItem != null)
		{
			this.LureName.text = string.Format("{0}", inventoryItem.Name);
			this.LureValueLoadable.Load(inventoryItem.ThumbnailBID, this.LureValue, "Textures/Inventory/{0}");
		}
		else
		{
			this.LureName.text = string.Empty;
			this.LureValue.overrideSprite = ResourcesHelpers.GetTransparentSprite();
		}
		InventoryItem inventoryItem2 = ((tackles.Count <= 1) ? null : tackles[1]);
		if (inventoryItem2 != null)
		{
			this._detailRt.anchoredPosition = new Vector2(this._detailRt.anchoredPosition.x, this._detailRt.anchoredPosition.y - 30f);
			this._lureName2.text = string.Format("{0}", inventoryItem2.Name);
			this._lureValueLoadable2.Load(inventoryItem2.ThumbnailBID, this._lureValue2, "Textures/Inventory/{0}");
			this._leMain.preferredHeight = this._leMain.preferredHeight + 35f;
			this._lureRt2.anchoredPosition = new Vector2(this._lureRt2.anchoredPosition.x, this._lureRt2.anchoredPosition.y - 10f);
			this._lureRt2.gameObject.SetActive(true);
		}
		if (!PhotonConnectionFactory.Instance.Profile.FishCage.Cage.Safety)
		{
			this.Release.gameObject.SetActive(false);
		}
		else
		{
			this.Release.gameObject.SetActive(true);
		}
	}

	public void ShowPreview()
	{
		if (this.CaughtFish != null)
		{
			string text = ScriptLocalization.Get("TrophiesPondStatCaption") + " " + StaticUserData.CurrentPond.Name;
			text = text + "\n" + ScriptLocalization.Get("TrophyStatWeghtCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishWeight(this.CaughtFish.Fish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
			text = text + "\n" + ScriptLocalization.Get("TrophyStatLengthCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishLength(this.CaughtFish.Fish.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
			ModelInfo modelInfo = new ModelInfo
			{
				Title = this.CaughtFish.Fish.Name,
				Info = text
			};
			base.GetComponent<LoadPreviewScene>().LoadScene(this.CaughtFish.Fish.Asset, modelInfo, null);
		}
	}

	[SerializeField]
	private Text _lureName2;

	[SerializeField]
	private Image _lureValue2;

	[SerializeField]
	private RectTransform _lureRt2;

	[SerializeField]
	private RectTransform _detailRt;

	[SerializeField]
	private LayoutElement _leMain;

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text Name;

	public Text Weight;

	public Text Length;

	public Text LureName;

	public Image LureValue;

	private ResourcesHelpers.AsyncLoadableImage LureValueLoadable = new ResourcesHelpers.AsyncLoadableImage();

	private ResourcesHelpers.AsyncLoadableImage _lureValueLoadable2 = new ResourcesHelpers.AsyncLoadableImage();

	public Text Weather;

	public Text TimeOfDate;

	public Text Money;

	public Text MoneySuffix;

	public Text Expiriences;

	public Toggle Info;

	public Button Release;

	public GameObject TournamentIcon;

	public HotkeyPressRedirect Preview;

	public CaughtFish CaughtFish;

	public GameObject Hotkey;
}
