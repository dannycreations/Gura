using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class LicenseMenuClick : MonoBehaviour
{
	public string CategoryElementId
	{
		get
		{
			HintElementId hintElementId = base.GetComponent<HintElementId>();
			if (hintElementId == null)
			{
				hintElementId = base.gameObject.AddComponent<HintElementId>();
				hintElementId.SetElementId("Licenses_" + this.StateId, null, null);
			}
			return hintElementId.GetElementId();
		}
	}

	public void OnClick()
	{
		LogHelper.Log("___kocha LicenseMenuClick:OnClick StateId:{0} RegionId:{1}", new object[] { this.StateId, this.RegionId });
		if (ShopMainPageHandler.Instance != null)
		{
			ShopMainPageHandler.Instance.MenuClick(false);
		}
		LicenseMenuClick.LastSelectedCategoryId = new int?(this.StateId);
		LicenseMenuClick.LastSelectedCategoryElementId = this.CategoryElementId;
		GlobalMapCache mc = CacheLibrary.MapCache;
		IEnumerable<ShopLicense> enumerable = null;
		if (mc.AllMapChachesInited)
		{
			if (this.RegionId != null)
			{
				List<int> ponds = (from p in mc.CachedPonds
					where p.Region.RegionId == this.RegionId
					select p.PondId).ToList<int>();
				enumerable = mc.AllLicenses.Where((ShopLicense l) => ponds.Contains(l.PondId.Value));
			}
			else if (this.StateId > 0)
			{
				enumerable = mc.AllLicenses.Where((ShopLicense l) => l.StateId == this.StateId);
			}
			else if (this.StateId == -1 && PhotonConnectionFactory.Instance.CurrentPondId != null)
			{
				enumerable = mc.AllLicenses.Where(delegate(ShopLicense l)
				{
					int? pondId = l.PondId;
					int valueOrDefault = pondId.GetValueOrDefault();
					int? currentPondId = PhotonConnectionFactory.Instance.CurrentPondId;
					return valueOrDefault == currentPondId.GetValueOrDefault() && pondId != null == (currentPondId != null);
				});
			}
			else
			{
				enumerable = mc.AllLicenses;
			}
		}
		if (enumerable != null)
		{
			enumerable = enumerable.SkipWhile((ShopLicense l) => mc.CachedPonds.FirstOrDefault((Pond p) => p.PondId == l.PondId).PondPaidLocked());
			ShopMainPageHandler.Instance.ContentUpdater.SetLicenses(enumerable);
		}
		else
		{
			LogHelper.Error("___kocha LicenseMenuClick:OnClick licenses = null; AllMapChachesInited:{0}", new object[] { mc.AllMapChachesInited });
		}
	}

	public int StateId = -1;

	public int? RegionId;

	public static int? LastSelectedCategoryId;

	public static string LastSelectedCategoryElementId;

	public static string[] LastSelectedCategoryElementIdsPath;

	public static int[] LastSelectedFullCategoryIdsList;
}
