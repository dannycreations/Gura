using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteTrophyInit : MonoBehaviour
{
	public void Refresh(FishStat fishStat, List<InventoryItem> tackles)
	{
		this._fishAsset = fishStat.MaxFish.Asset;
		this.InitPanel(fishStat.MaxFish.Name, fishStat.MaxFishPond, fishStat.MaxFishDate.ToLocalTime(), fishStat.MaxFish.Weight, fishStat.MaxFish.Length, fishStat.MaxFish.ThumbnailBID, tackles);
	}

	public void Refresh(CaughtFish fish, DateTime catchDate, int pondId)
	{
		this._fishToRefresh = fish;
		this._subscriberId = pondId;
		this._lastCatchDate = catchDate;
		this._fishAsset = fish.Fish.Asset;
		string text = ScriptLocalization.Get("Info Labels/[Loading]");
		string text2 = text;
		int subscriberId = this._subscriberId;
		DateTime lastCatchDate = this._lastCatchDate;
		float weight = this._fishToRefresh.Fish.Weight;
		float length = this._fishToRefresh.Fish.Length;
		int? thumbnailBID = this._fishToRefresh.Fish.ThumbnailBID;
		this.InitPanel(text2, subscriberId, lastCatchDate, weight, length, (thumbnailBID == null) ? 0 : thumbnailBID.Value, new List<InventoryItem>
		{
			new InventoryItem
			{
				Name = text,
				ThumbnailBID = null
			}
		});
		if (!this.subscribed)
		{
			CacheLibrary.ItemsCache.OnGotItems += this.OnGotItems;
			this.subscribed = true;
		}
		CacheLibrary.ItemsCache.GetItems(fish.AllBaitIds, this._subscriberId);
	}

	private void Unsubscribe()
	{
		this.subscribed = false;
		CacheLibrary.ItemsCache.OnGotItems -= this.OnGotItems;
	}

	private void OnDestroy()
	{
		if (this.subscribed)
		{
			this.Unsubscribe();
		}
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId == this._subscriberId)
		{
			this.Unsubscribe();
			List<InventoryItem> list = new List<InventoryItem>();
			for (int i = 0; i < this._fishToRefresh.AllBaitIds.Length; i++)
			{
				int baitId = this._fishToRefresh.AllBaitIds[i];
				InventoryItem inventoryItem = items.FirstOrDefault((InventoryItem x) => x.ItemId == baitId);
				if (inventoryItem != null)
				{
					list.Add(inventoryItem);
				}
			}
			string name = this._fishToRefresh.Fish.Name;
			int subscriberId2 = this._subscriberId;
			DateTime lastCatchDate = this._lastCatchDate;
			float weight = this._fishToRefresh.Fish.Weight;
			float length = this._fishToRefresh.Fish.Length;
			int? thumbnailBID = this._fishToRefresh.Fish.ThumbnailBID;
			this.InitPanel(name, subscriberId2, lastCatchDate, weight, length, (thumbnailBID == null) ? 0 : thumbnailBID.Value, list);
		}
	}

	private void InitPanel(string fishName, int pondId, DateTime catchDate, float fishWeight, float fishLength, int fishThumbnailId, List<InventoryItem> tackles)
	{
		this._pondId = pondId;
		this._date = catchDate;
		this._fishWeight = fishWeight;
		this._fishLength = fishLength;
		if (string.IsNullOrEmpty(fishName))
		{
			FishBrief fishBrief = CacheLibrary.MapCache.FishesLight.FirstOrDefault((FishBrief x) => x.CodeName == this._fishToRefresh.Fish.CodeName);
			if (fishBrief != null)
			{
				fishName = fishBrief.Name;
			}
		}
		this._fishName = fishName;
		if (this.NameAlt != null)
		{
			this.NameAlt.text = fishName;
			if (this.Name != null)
			{
				this.Name.gameObject.SetActive(false);
			}
		}
		else
		{
			this.Name.text = fishName;
		}
		this.Pond.text = ((pondId == 0) ? string.Empty : CacheLibrary.MapCache.CachedPonds.First((Pond x) => x.PondId == pondId).Name);
		this.Date.text = catchDate.ToShortDateString();
		this.Weight.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(fishWeight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		this.Length.text = string.Format("{0} {1}", MeasuringSystemManager.FishLength(fishLength).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		this.IconLdbl.Image = this.Icon;
		this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", fishThumbnailId.ToString(CultureInfo.InvariantCulture)));
		if (tackles.Count > 0)
		{
			InventoryItem inventoryItem = tackles[0];
			string name = inventoryItem.Name;
			this.LureName.text = (string.IsNullOrEmpty(name) ? string.Empty : string.Format("{0}", name));
			this.LureValueLdbl.Load(inventoryItem.ThumbnailBID, this.LureValue, "Textures/Inventory/{0}");
		}
		else
		{
			PhotonConnectionFactory.Instance.PinError(string.Format("ConcreteTrophyInit::InitPanel tackles.Count=0 fishName:{0} pondId:{1} AllBaitIds:{2}", fishName, pondId, string.Join(",", this._fishToRefresh.AllBaitIds.Select((int p) => p.ToString()).ToArray<string>())), Environment.StackTrace);
		}
		if (this._lure2 != null)
		{
			this._lure2.SetActive(tackles.Count > 1);
		}
		if (tackles.Count > 1)
		{
			InventoryItem inventoryItem2 = tackles[1];
			this._lureName2.text = inventoryItem2.Name;
			this._lureValueLdbl2.Load(inventoryItem2.ThumbnailBID, this._lureValue2, "Textures/Inventory/{0}");
			if (this._mainLe != null)
			{
				this._mainLe.preferredHeight += 45f;
			}
		}
		if (this.TimeOfDay != null)
		{
			this.TimeOfDay.text = TimeAndWeatherManager.GetTimeOfDayCaption(this._fishToRefresh.Time);
		}
		if (this.Weather != null)
		{
			this.Weather.text = WeatherHelper.GetWeatherIcon(this._fishToRefresh.WeatherIcon);
		}
		if (this.PreviewButton != null)
		{
			this.PreviewButton.SetActive(true);
		}
	}

	public void ShowPreview()
	{
		string text = ScriptLocalization.Get("TrophiesPondStatCaption") + " " + ((this._pondId == 0) ? string.Empty : CacheLibrary.MapCache.CachedPonds.First((Pond x) => x.PondId == this._pondId).Name);
		text = string.Concat(new string[]
		{
			text,
			"\n",
			ScriptLocalization.Get("TrophyStatDateCaption"),
			" ",
			this._date.ToShortDateString()
		});
		text = text + "\n" + ScriptLocalization.Get("TrophyStatWeghtCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishWeight(this._fishWeight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		text = text + "\n" + ScriptLocalization.Get("TrophyStatLengthCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishLength(this._fishLength).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		ModelInfo modelInfo = new ModelInfo
		{
			Title = this._fishName,
			Info = text
		};
		base.GetComponent<LoadPreviewScene>().LoadScene(this._fishAsset, modelInfo, null);
	}

	public void SetPanelEmpty()
	{
		if (this.NameAlt != null)
		{
			this.NameAlt.text = ScriptLocalization.Get("NoFishDataTitle");
			if (this.Name != null)
			{
				this.Name.gameObject.SetActive(false);
			}
		}
		else
		{
			this.Name.text = ScriptLocalization.Get("NoFishDataTitle");
		}
		this.Pond.text = "- - - - -";
		this.Date.text = "- - - - -";
		if (this.TimeOfDay != null)
		{
			this.TimeOfDay.text = "- - - - -";
		}
		if (this.Weather != null)
		{
			this.Weather.text = string.Empty;
		}
		this.Weight.text = string.Format("{0} {1}", "- - -", MeasuringSystemManager.FishWeightSufix());
		this.Length.text = string.Format("{0} {1}", "- - -", MeasuringSystemManager.FishLengthSufix());
		this.Icon.overrideSprite = this.emptyFish;
		this.LureName.text = string.Empty;
		this.LureValue.overrideSprite = ResourcesHelpers.GetTransparentSprite();
	}

	[SerializeField]
	private GameObject _lure2;

	[SerializeField]
	private Image _lureValue2;

	[SerializeField]
	private Text _lureName2;

	[SerializeField]
	private LayoutElement _mainLe;

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Name;

	public TextMeshProUGUI NameAlt;

	public Text Pond;

	public Text Date;

	public Text TimeOfDay;

	public Text Weather;

	public Text Weight;

	public Text Length;

	public Text LureName;

	public Image LureValue;

	private ResourcesHelpers.AsyncLoadableImage LureValueLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public GameObject PreviewButton;

	private ResourcesHelpers.AsyncLoadableImage _lureValueLdbl2 = new ResourcesHelpers.AsyncLoadableImage();

	private int _pondId;

	private DateTime _date;

	private float _fishWeight;

	private float _fishLength;

	private string _fishName;

	private string _fishAsset;

	private CaughtFish _fishToRefresh;

	private int _subscriberId;

	private DateTime _lastCatchDate;

	private bool subscribed;

	[SerializeField]
	private Sprite emptyFish;
}
