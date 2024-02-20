using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine.UI;

public class ShowDetailedLicenseInfo : ShowDetailedInfo
{
	protected override void Show()
	{
		CacheLibrary.MapCache.OnFishes += this.FillFishRestrictions;
		this.DetailedPanel.SetActive(true);
		LayoutElement component = base.GetComponent<LayoutElement>();
		component.preferredHeight += this.HeightDetailedPanel;
		component.minHeight = component.preferredHeight;
		this._isShow = true;
		if (this.License != null)
		{
			List<int> list = new List<int>(this.License.TakeFish.Length + this.License.FreeFish.Length);
			list.AddRange(this.License.FreeFish.Select((FishLicenseConstraint x) => x.FishId));
			list.AddRange(this.License.TakeFish.Select((FishLicenseConstraint x) => x.FishId));
			CacheLibrary.MapCache.GetFishes(list.ToArray());
		}
	}

	protected override void Hide()
	{
		base.Hide();
		LayoutElement component = base.GetComponent<LayoutElement>();
		component.preferredHeight -= this.HeightDetailedPanel;
		component.minHeight = component.preferredHeight;
		CacheLibrary.MapCache.OnFishes -= this.FillFishRestrictions;
	}

	private void OnDestroy()
	{
		CacheLibrary.MapCache.OnFishes -= this.FillFishRestrictions;
	}

	private void FillFishRestrictions(object sender, GlobalMapFishCacheEventArgs e)
	{
		Text component = this.DetailedPanel.transform.Find("Description").Find("Mask").Find("Content")
			.GetComponent<Text>();
		if (component.text == string.Empty && this.License != null)
		{
			IEnumerable<ShopLicense> enumerable = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense x) => x.StateId == this.License.StateId);
			string text = string.Empty;
			if (enumerable.First<ShopLicense>().LicenseId == this.License.LicenseId)
			{
				text = string.Format(ScriptLocalization.Get("NightCatchBasic"), "\n");
			}
			else
			{
				text = string.Format(ScriptLocalization.Get("NightCatchAdvanced"), "\n");
			}
			component.text = string.Concat(new string[]
			{
				"<b>",
				ScriptLocalization.Get("Release").ToUpper(),
				" : </b>",
				FishHelper.FillFishes(this.License.FreeFish, e.Items),
				"\n<b>",
				ScriptLocalization.Get("Take").ToUpper(),
				"</b> : ",
				FishHelper.FillFishes(this.License.TakeFish, e.Items),
				"\n\n",
				text
			});
		}
	}

	public PlayerLicense License;
}
