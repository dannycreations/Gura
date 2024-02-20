using System;
using System.Collections.Generic;
using ObjectModel;
using ObjectModel.Common;

public class LevelInfo
{
	public int Level;

	public bool IsLevel;

	public long ExpToThisLevel;

	public long ExpToNextLevel;

	public Amount Amount1;

	public Amount Amount2;

	public List<InventoryItem> GlobalItemsForLevel;
}
