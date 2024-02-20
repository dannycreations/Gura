using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MissionAccomplishedInit : MonoBehaviour
{
	private void Start()
	{
		if (this._type != InfoMessageTypes.MissionNew && this._type != InfoMessageTypes.LevelUp)
		{
			this.Caption.text = ScriptLocalization.Get("QuestAccomplishedCaption");
		}
	}

	private void OnDestroy()
	{
		base.GetComponent<AlphaFade>().HideFinished -= this.Af_HideFinished;
	}

	public void Init(string missionName, string congratsText, int? ImageBID, AchivementInfo info, InfoMessageTypes type = InfoMessageTypes.Unknown, List<InventoryItem> ii = null, Pond pond = null)
	{
		this._itemRewards.Clear();
		this._type = type;
		if (this._type == InfoMessageTypes.LevelUp)
		{
			this.Caption.text = missionName;
			if (ii != null)
			{
				this.ItemsCache_OnGotItems(ii, this.RewardID);
			}
			if (pond != null)
			{
				this._textPondName.text = pond.Name;
				this._textPondDesc.text = pond.State.Name;
				this._textPondIco.text = WeatherHelper.GetPondWeatherIcon(pond);
			}
		}
		if (this.Name != null)
		{
			this.Name.text = missionName;
		}
		this.IconLdbl.Image = this.Icon;
		if (ImageBID != null)
		{
			this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", ImageBID));
		}
		if (this.DescValue != null)
		{
			this.DescValue.text = congratsText;
		}
		int num = 0;
		int num2 = 0;
		UIHelper.ParseAmount(info.Amount1, ref num2, ref num);
		UIHelper.ParseAmount(info.Amount2, ref num2, ref num);
		this.SetRewardValue(this.ExperienceValue, info.Experience);
		this.SetRewardValue(this.SilverValue, num2);
		this.SetRewardValue(this.GoldValue, num);
		if (this._rewardsGameObject != null)
		{
			this._rewardsGameObject.SetActive((info.ItemRewards != null && info.ItemRewards.Length > 0) || (info.ProductReward != null && info.ProductReward.Length > 0) || (info.LicenseReward != null && info.LicenseReward.Length > 0) || (ii != null && ii.Count > 0));
		}
		if (info.ItemRewards != null)
		{
			IEnumerable<int> enumerable = info.ItemRewards.Select((ItemReward x) => x.ItemId);
			this._itemRewards.AddRange(enumerable);
			CacheLibrary.ItemsCache.OnGotItems += this.ItemsCache_OnGotItems;
			CacheLibrary.ItemsCache.GetItems(enumerable.ToArray<int>(), this.RewardID);
		}
		if (info.ProductReward != null)
		{
			ProductReward[] productReward = info.ProductReward;
			for (int i = 0; i < productReward.Length; i++)
			{
				ProductReward p = productReward[i];
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == p.ProductId);
				if (storeProduct == null)
				{
					Debug.LogWarning("Unknown product came in reward");
				}
				else
				{
					RewardItem component = GUITools.AddChild(this.Content, this.ItemPrefab).GetComponent<RewardItem>();
					component.gameObject.SetActive(true);
					component.FillData(storeProduct);
				}
			}
		}
		if (info.LicenseReward != null)
		{
			LicenseRef[] licenseReward = info.LicenseReward;
			for (int j = 0; j < licenseReward.Length; j++)
			{
				LicenseRef license = licenseReward[j];
				ShopLicense shopLicense = CacheLibrary.MapCache.AllLicenses.FirstOrDefault((ShopLicense x) => x.LicenseId == license.LicenseId);
				GameObject gameObject = GUITools.AddChild(this.Content, this.ItemPrefab);
				RewardItem component2 = gameObject.GetComponent<RewardItem>();
				component2.FillData(shopLicense);
			}
		}
		this.SetVisibilityRewardsButtons();
	}

	private void ItemsCache_OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId == this.RewardID)
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
			int i;
			for (i = 0; i < items.Count; i++)
			{
				RewardItem component = GUITools.AddChild(this.Content, this.ItemPrefab).GetComponent<RewardItem>();
				component.gameObject.SetActive(true);
				int num = this._itemRewards.Count((int p) => p == items[i].ItemId);
				component.FillData(items[i], num);
			}
			this._itemRewards.Clear();
			this.SetVisibilityRewardsButtons();
		}
	}

	public void Init(HintMessage message)
	{
		this.Name.text = message.MissionName;
		this.IconLdbl.Image = this.Icon;
		if (!string.IsNullOrEmpty(message.Image))
		{
			this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", message.Image));
		}
		this.DescValue.text = message.Description;
	}

	public void OpenMissions()
	{
		AlphaFade component = base.GetComponent<AlphaFade>();
		component.HideFinished += this.Af_HideFinished;
		component.HidePanel();
	}

	private void Af_HideFinished(object sender, EventArgsAlphaFade e)
	{
		base.GetComponent<AlphaFade>().HideFinished -= this.Af_HideFinished;
		ScreenManager.OpenMissions();
	}

	private void SetRewardValue(Text t, int v)
	{
		t.transform.parent.gameObject.SetActive(v > 0);
		if (v > 0)
		{
			t.text = v.ToString();
		}
	}

	private void SetVisibilityRewardsButtons()
	{
		bool flag = this.Content.transform.childCount > 3;
		for (int i = 0; i < this._rewardsButtons.Length; i++)
		{
			this._rewardsButtons[i].SetActive(flag);
		}
	}

	[SerializeField]
	private Text _textPondName;

	[SerializeField]
	private Text _textPondDesc;

	[SerializeField]
	private Text _textPondIco;

	[SerializeField]
	private GameObject _rewardsGameObject;

	[SerializeField]
	private GameObject[] _rewardsButtons;

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Caption;

	public Text Name;

	public Text DescValue;

	public Text GoldValue;

	public Text SilverValue;

	public Text ExperienceValue;

	public GameObject Content;

	public GameObject ItemPrefab;

	public int RewardID;

	private InfoMessageTypes _type;

	private const int RewardsCount4Scroll = 3;

	private List<int> _itemRewards = new List<int>();
}
