using System;
using I2.Loc;

public class JerkbaitMinnowFilter : ConcreteCranckbaitFilter
{
	internal override void Init()
	{
		base.Init();
		CategoryFilter categoryFilter = this.FilterCategories[2];
		this.FilterCategories[2] = this.FilterCategories[0];
		this.FilterCategories[0] = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeLureFilter"),
			Filters = LureFilter.GetJerkbaitMinnowFilters()
		};
		this.FilterCategories.Add(6, categoryFilter);
	}
}
