using System;
using System.Collections.Generic;
using ObjectModel;

public delegate void OnInteractedWithObject(IEnumerable<InventoryItem> itemsGained, int? amount, string currency);
