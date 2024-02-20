using System;
using System.Collections.Generic;
using I2.Loc;

public class LicencesFilter : BaseFilter
{
	internal override void Init()
	{
		this.FilterCategories.Clear();
		this.FilterType = typeof(ShopLicenseContainer);
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeTerminalTackleFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("LicenseTypeBase"),
					FilterFieldName = "IsAdvanced",
					FilterFieldType = typeof(bool),
					Value = false
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("LicenseTypeAdvanced"),
					FilterFieldName = "IsAdvanced",
					FilterFieldType = typeof(bool),
					Value = true
				}
			}
		});
	}
}
