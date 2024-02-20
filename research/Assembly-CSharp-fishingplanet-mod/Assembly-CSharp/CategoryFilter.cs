using System;
using System.Collections.Generic;

public class CategoryFilter
{
	public string CategoryName { get; set; }

	public List<ISelectionFilterBase> Filters { get; set; }
}
