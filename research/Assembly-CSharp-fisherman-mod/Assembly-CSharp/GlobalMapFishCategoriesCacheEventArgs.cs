using System;
using System.Collections.Generic;

public class GlobalMapFishCategoriesCacheEventArgs : EventArgs
{
	public IEnumerable<FishCategory> Items;
}
