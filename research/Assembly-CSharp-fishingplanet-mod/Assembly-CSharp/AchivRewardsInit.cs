using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class AchivRewardsInit : MonoBehaviour
{
	private void Awake()
	{
		base.GetComponent<AlphaFade>().ShowFinished += this.AchivRewardsInit_ShowFinished;
		this.Caption.text = ScriptLocalization.Get("AchievmentRecievedCaption");
	}

	private void OnDestroy()
	{
		if (CacheLibrary.ItemsCache != null)
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
		}
		base.GetComponent<AlphaFade>().ShowFinished -= this.AchivRewardsInit_ShowFinished;
	}

	private void AchivRewardsInit_ShowFinished(object sender, EventArgs e)
	{
	}

	public void Init(AchivementInfo achivInfo)
	{
		this._achivInfo = achivInfo;
		int num = 0;
		int num2 = 0;
		this.MoneyValue.transform.parent.gameObject.SetActive(false);
		this.GoldValue.transform.parent.gameObject.SetActive(false);
		this.ExpValue.transform.parent.gameObject.SetActive(false);
		if (achivInfo.Amount1 != null && achivInfo.Amount1.Currency == "SC")
		{
			this.MoneyValue.transform.parent.gameObject.SetActive(true);
			num2 = achivInfo.Amount1.Value;
		}
		if (achivInfo.Amount2 != null && achivInfo.Amount2.Currency == "SC")
		{
			this.MoneyValue.transform.parent.gameObject.SetActive(true);
			num2 = achivInfo.Amount2.Value;
		}
		if (achivInfo.Amount1 != null && achivInfo.Amount1.Currency == "GC")
		{
			this.GoldValue.transform.parent.gameObject.SetActive(true);
			num = achivInfo.Amount1.Value;
		}
		if (achivInfo.Amount2 != null && achivInfo.Amount2.Currency == "GC")
		{
			this.GoldValue.transform.parent.gameObject.SetActive(true);
			num = achivInfo.Amount2.Value;
		}
		if (achivInfo.Experience > 0)
		{
			this.ExpValue.transform.parent.gameObject.SetActive(true);
		}
		bool flag = (achivInfo.Amount1 == null || (achivInfo.Amount1 != null && achivInfo.Amount1.Value == 0)) && (achivInfo.Amount2 == null || (achivInfo.Amount2 != null && achivInfo.Amount2.Value == 0)) && achivInfo.Experience == 0;
		this.RewardsTitle.gameObject.SetActive(!flag);
		this.Name.text = string.Format(string.Format("{0} {1}", achivInfo.AchivementName, achivInfo.AchivementStageName), achivInfo.AchivementStageCount).ToUpper();
		this.ExpValue.text = achivInfo.Experience.ToString();
		this.MoneyValue.text = num2.ToString();
		this.GoldValue.text = num.ToString();
		this.IconLdbl.Image = this.Icon;
		this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", achivInfo.ImageId));
		this.DescValue.text = achivInfo.AchivementDesc;
		if (achivInfo.ItemRewards != null && achivInfo.ItemRewards.Length > 0)
		{
			this.FillInventoryItems();
		}
		if (achivInfo.LicenseReward != null && achivInfo.LicenseReward.Length > 0)
		{
			this.FillLicense();
		}
		if (achivInfo.ProductReward != null && achivInfo.ProductReward.Length > 0)
		{
			this.FillProducts();
		}
		this.UnlockAchievement(achivInfo);
	}

	private void FillInventoryItems()
	{
		Random random = new Random();
		this._subscriberId = random.Next(99999);
		CacheLibrary.ItemsCache.OnGotItems += this.ItemsCache_OnGotItems;
		CacheLibrary.ItemsCache.GetItems(this._achivInfo.ItemRewards.Select((ItemReward x) => x.ItemId).ToArray<int>(), this._subscriberId);
	}

	private void ItemsCache_OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId == this._subscriberId)
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
			items.ForEach(delegate(InventoryItem ii)
			{
				RewardItem.SpawnItem(this.RewardsContentPanel, this.RewardItemPrefab, ii);
			});
		}
	}

	private void FillLicense()
	{
		for (int i = 0; i < this._achivInfo.LicenseReward.Length; i++)
		{
			LicenseRef license = this._achivInfo.LicenseReward[i];
			ShopLicense shopLicense = CacheLibrary.MapCache.AllLicenses.FirstOrDefault((ShopLicense x) => x.LicenseId == license.LicenseId);
			if (shopLicense == null)
			{
				LogHelper.Error("Unknown license came in reward LicenseId:{0}", new object[] { license.LicenseId });
			}
			else
			{
				RewardItem.SpawnItem(this.RewardsContentPanel, this.RewardItemPrefab, shopLicense);
			}
		}
	}

	private void FillProducts()
	{
		for (int i = 0; i < this._achivInfo.ProductReward.Length; i++)
		{
			ProductReward p = this._achivInfo.ProductReward[i];
			StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == p.ProductId);
			if (storeProduct == null)
			{
				LogHelper.Error("Unknown product came in reward ProductId:{0}", new object[] { p.ProductId });
			}
			else
			{
				RewardItem.SpawnItem(this.RewardsContentPanel, this.RewardItemPrefab, storeProduct);
			}
		}
	}

	private void UnlockAchievement(AchivementInfo ai)
	{
		if (!string.IsNullOrEmpty(ai.ExternalAchievementId))
		{
			string externalAchievementId = ai.ExternalAchievementId;
			bool flag = AchievementSystem.Instance.IsAchievementUnlocked(externalAchievementId);
			LogHelper.Log("___kocha UnlockAchievement isDone:{0} achievementId:{1}", new object[] { flag, externalAchievementId });
			if (!flag)
			{
				AchievementSystem.Instance.UnlockAchievement(externalAchievementId);
			}
		}
	}

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Caption;

	public Text Name;

	public Text ExpValue;

	public Text MoneyValue;

	public Text GoldValue;

	public Text DescValue;

	public Text RewardsTitle;

	public GameObject RewardItemPrefab;

	public GameObject RewardsContentPanel;

	private int _subscriberId;

	private AchivementInfo _achivInfo;
}
