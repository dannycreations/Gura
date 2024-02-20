using System;
using ObjectModel;

public class ShopLicenseContainer : ShopLicense
{
	public ShopLicenseContainer(ShopLicense l)
	{
		base.LicenseId = l.LicenseId;
		base.StateId = l.StateId;
		base.PondId = l.PondId;
		base.OriginalMinLevel = l.OriginalMinLevel;
		base.MinLevel = l.MinLevel;
		base.Name = l.Name;
		base.Desc = l.Desc;
		base.Currency = l.Currency;
		base.Penalty = l.Penalty;
		base.LogoBID = l.LogoBID;
		base.LocationIds = l.LocationIds;
		base.IllegalFish = l.IllegalFish;
		base.FreeFish = l.FreeFish;
		base.TakeFish = l.TakeFish;
		base.Costs = l.Costs;
		base.IsAdvanced = l.IsAdvanced;
	}
}
