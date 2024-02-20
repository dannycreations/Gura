using System;

public class EmptyFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Clear();
	}
}
