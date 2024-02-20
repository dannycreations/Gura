using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicensesSubMenu : SubMenuFoldoutBase
{
	protected override void Awake()
	{
		this._licensingGuidlinesHeader.text = ScriptLocalization.Get("LicensingGuidelines").ToUpper();
		this._releaseHeader.text = ScriptLocalization.Get("Release").ToUpper();
		this._releaseRules.text = ScriptLocalization.Get("GuidelinesRelease");
		this._takeHeader.text = ScriptLocalization.Get("Take").ToUpper();
		this._takeRules.text = ScriptLocalization.Get("GuidelinesTake");
		this._restrictionsHeader.text = ScriptLocalization.Get("MessageCaption").ToUpper();
		base.Awake();
		if (this.fishTypes == null)
		{
			this.InitFishTypes();
		}
	}

	private void InitFishTypes()
	{
		this.fishTypes = new string[]
		{
			ScriptLocalization.Get("YoungType").ToLower(),
			ScriptLocalization.Get("CommonType").ToLower(),
			ScriptLocalization.Get("TrophyType").ToLower(),
			ScriptLocalization.Get("UniqueType").ToLower()
		};
	}

	private void OnEnable()
	{
		CacheLibrary.MapCache.OnGetBoatDescs += this.GetBoatDesc;
		CacheLibrary.MapCache.GetBoatDescs();
		this.inited = true;
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.subscribedForFish)
		{
			CacheLibrary.MapCache.OnFishes -= this.FillFishRestrictions;
			this.subscribedForFish = false;
		}
		CacheLibrary.MapCache.OnGetBoatDescs -= this.GetBoatDesc;
	}

	private void GetBoatDesc(object sender, GlobalMapBoatDescCacheEventArgs e)
	{
		bool flag = e.Items.Any((BoatDesc x) => x.Prices != null && x.Prices.Any((BoatPriceDesc p) => p.PondId == this.pondID));
		for (int i = 0; i < this._boatFishing.Length; i++)
		{
			this._boatFishing[i].gameObject.SetActive(flag);
		}
	}

	private IEnumerator WaitForInitAndFillContent(ShopLicense basicLicense, ShopLicense advancedLicense, Pond currentPond)
	{
		while (!this.inited)
		{
			yield return null;
		}
		this.FillContent(basicLicense, advancedLicense, currentPond);
		yield break;
	}

	public void FillContent(ShopLicense basicLicense, ShopLicense advancedLicense, Pond currentPond)
	{
		if (!this.inited)
		{
			base.StartCoroutine(this.WaitForInitAndFillContent(basicLicense, advancedLicense, currentPond));
			return;
		}
		this.pondID = currentPond.PondId;
		this._basicLicense = basicLicense;
		this._advancedLicense = advancedLicense;
		this.CollectFishes();
		this.FillInfo(basicLicense, currentPond, 0);
		PlayerLicense playerLicense = this.FillInfo(advancedLicense, currentPond, 1);
		if (this._buy[0].gameObject.activeSelf && playerLicense != null)
		{
			this._buy[0].gameObject.SetActive(false);
		}
		this._toggleEntry[0].SetActive(playerLicense == null);
		this._toggleEntry[1].SetActive(playerLicense != null);
		this._nightFishing[0].text = string.Format(ScriptLocalization.Get("NightCatchBasic"), "\n");
		this._nightFishing[1].text = string.Format(ScriptLocalization.Get("NightCatchAdvanced"), "\n");
		this._boatFishing[0].text = string.Format(ScriptLocalization.Get("RowBoatBasicLicenseText"), "\n");
		this._boatFishing[1].text = string.Format(ScriptLocalization.Get("RowBoatAdvancedLicenseText"), "\n");
	}

	private PlayerLicense FillInfo(ShopLicense license, Pond currentPond, int index)
	{
		PlayerLicense playerLicense = null;
		if (PhotonConnectionFactory.Instance.Profile.ActiveLicenses != null)
		{
			playerLicense = PhotonConnectionFactory.Instance.Profile.ActiveLicenses.FirstOrDefault((PlayerLicense x) => x.LicenseId == license.LicenseId);
		}
		this._licenseName[index].text = license.Name;
		if (this._imageldbs.Count <= index)
		{
			this._imageldbs.Add(new ResourcesHelpers.AsyncLoadableImage());
		}
		this._imageldbs[index].Image = this._image[index];
		this._imageldbs[index].Load(string.Format("Textures/Inventory/{0}", license.LogoBID.Value.ToString()));
		if (playerLicense != null)
		{
			this._statusIcon[index].color = Color.green;
			this._status[index].color = Color.green;
			this._toogleStatus[index].color = Color.green;
			this._toogleStatusIcon[index].color = Color.green;
			this._buy[index].gameObject.SetActive(false);
			if (playerLicense.End != null)
			{
				this._status[index].text = MeasuringSystemManager.DateTimeString(playerLicense.End.Value.ToLocalTime());
				this._toogleStatus[index].text = MeasuringSystemManager.DateTimeString(playerLicense.End.Value.ToLocalTime());
			}
			else
			{
				this._status[index].text = ScriptLocalization.Get("UnlimitedCaption");
				this._toogleStatus[index].text = ScriptLocalization.Get("UnlimitedCaption");
			}
		}
		else
		{
			if (!currentPond.PondLocked())
			{
				this._buy[index].gameObject.SetActive(true);
				this._hint[index].SetElementId("License" + license.LicenseId, null, null);
			}
			else
			{
				this._buy[index].gameObject.SetActive(false);
			}
			this._status[index].color = Color.red;
			this._statusIcon[index].color = Color.red;
			this._toogleStatus[index].color = Color.red;
			this._toogleStatusIcon[index].color = Color.red;
			this._status[index].text = ScriptLocalization.Get("None").ToUpper();
			this._toogleStatus[index].text = ScriptLocalization.Get("None").ToUpper();
		}
		return playerLicense;
	}

	private void CollectFishes()
	{
		int num = this._basicLicense.FreeFish.Length + this._basicLicense.TakeFish.Length;
		int num2 = this._advancedLicense.FreeFish.Length + this._advancedLicense.TakeFish.Length;
		this._fishes = new List<int>(num + num2);
		this._fishes.AddRange(this._basicLicense.FreeFish.Select((FishLicenseConstraint x) => x.FishId));
		this._fishes.AddRange(this._basicLicense.TakeFish.Select((FishLicenseConstraint x) => x.FishId));
		this._fishes.AddRange(this._advancedLicense.FreeFish.Select((FishLicenseConstraint x) => x.FishId));
		this._fishes.AddRange(this._advancedLicense.TakeFish.Select((FishLicenseConstraint x) => x.FishId));
		if (!this.subscribedForFish)
		{
			CacheLibrary.MapCache.OnFishes += this.FillFishRestrictions;
		}
		this.subscribedForFish = true;
		CacheLibrary.MapCache.GetFishes(this._fishes.ToArray());
	}

	private void FillFishRestrictions(object sender, GlobalMapFishCacheEventArgs e)
	{
		this.FillFishesForLicense(e, this._basicLicense, 0);
		this.FillFishesForLicense(e, this._advancedLicense, 1);
		this._restrictions.text = string.Format(ScriptLocalization.Get("Restrictions"), "\n", this._basicLicense.Penalty);
	}

	private void FillFishesForLicense(GlobalMapFishCacheEventArgs e, ShopLicense license, int index)
	{
		this._take[index].text = string.Format("<b>{0}</b>\n", ScriptLocalization.Get("Take").ToUpper());
		if (license.TakeFish == null || license.TakeFish.Length == 0)
		{
			TextMeshProUGUI textMeshProUGUI = this._take[index];
			textMeshProUGUI.text += ScriptLocalization.Get("NoRestriction");
		}
		else
		{
			this.FormTypesString(license.TakeFish, e.Items, this._take[index]);
		}
		this._release[index].text = string.Format("<b>{0}</b>\n", ScriptLocalization.Get("Release").ToUpper());
		if (license.FreeFish == null || license.FreeFish.Length == 0)
		{
			TextMeshProUGUI textMeshProUGUI2 = this._release[index];
			textMeshProUGUI2.text += ScriptLocalization.Get("NoRestriction");
		}
		else
		{
			this.FormTypesString(license.FreeFish, e.Items, this._release[index]);
		}
	}

	private void FormTypesString(FishLicenseConstraint[] licenseFishes, List<Fish> Items, TextMeshProUGUI label)
	{
		if (this.fishTypes == null)
		{
			this.InitFishTypes();
		}
		IEnumerable<int> FishIds = licenseFishes.Select((FishLicenseConstraint x) => x.FishId);
		IEnumerable<IGrouping<int, Fish>> enumerable = from f in Items
			where FishIds.Contains(f.FishId)
			group f by f.CategoryId;
		string text = string.Empty;
		foreach (IGrouping<int, Fish> grouping in enumerable)
		{
			string text2 = string.Empty;
			string[] array = new string[]
			{
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty
			};
			Fish fish = null;
			foreach (Fish fish2 in grouping)
			{
				fish = fish2;
				if (fish2.IsTrophy != null && fish2.IsTrophy.Value)
				{
					array[2] = this.fishTypes[2];
				}
				else if (fish2.IsYoung != null && fish2.IsYoung.Value)
				{
					array[0] = this.fishTypes[0];
				}
				else if (fish2.IsUnique != null && fish2.IsUnique.Value)
				{
					array[3] = this.fishTypes[3];
				}
				else
				{
					array[1] = this.fishTypes[1];
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != string.Empty)
				{
					text2 += ((!(text2 == string.Empty)) ? (", " + array[i]) : array[i]);
				}
			}
			text2 = CacheLibrary.MapCache.GetFishCategory(fish.CategoryId).Name + " (" + text2 + ")";
			text = text + ((!(text == string.Empty)) ? ", " : string.Empty) + text2;
		}
		label.text += text;
	}

	[SerializeField]
	private TextMeshProUGUI[] _licenseName;

	[SerializeField]
	private TextMeshProUGUI[] _release;

	[SerializeField]
	private TextMeshProUGUI[] _take;

	[SerializeField]
	private TextMeshProUGUI[] _nightFishing;

	[SerializeField]
	private TextMeshProUGUI[] _boatFishing;

	[SerializeField]
	private TextMeshProUGUI[] _status;

	[SerializeField]
	private TextMeshProUGUI[] _statusIcon;

	[SerializeField]
	private Button[] _buy;

	[SerializeField]
	private HintElementId[] _hint;

	[SerializeField]
	private Image[] _image;

	[SerializeField]
	private Text[] _toogleStatusIcon;

	[SerializeField]
	private Text[] _toogleStatus;

	[SerializeField]
	private GameObject[] _toggleEntry;

	[SerializeField]
	private TextMeshProUGUI _restrictions;

	[SerializeField]
	private TextMeshProUGUI _licensingGuidlinesHeader;

	[SerializeField]
	private TextMeshProUGUI _releaseHeader;

	[SerializeField]
	private TextMeshProUGUI _releaseRules;

	[SerializeField]
	private TextMeshProUGUI _takeHeader;

	[SerializeField]
	private TextMeshProUGUI _takeRules;

	[SerializeField]
	private TextMeshProUGUI _restrictionsHeader;

	private List<int> _fishes;

	private ShopLicense _basicLicense;

	private ShopLicense _advancedLicense;

	private string[] fishTypes;

	private int pondID;

	private bool inited;

	private List<ResourcesHelpers.AsyncLoadableImage> _imageldbs = new List<ResourcesHelpers.AsyncLoadableImage>();

	private bool subscribedForFish;
}
