using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class WindowInitialPrizePool : WindowEntryFee
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<WindowEntryFee.CurrencyData, int> OnSelectedPrizePool = delegate(WindowEntryFee.CurrencyData cd, int i)
	{
	};

	public void Init(List<WindowList.WindowListElem> data, string title, string dataTitle, string descTitle, WindowEntryFee.EntryFeeData d, int index = 0)
	{
		base.Init(d);
		this.Data = data;
		this.SelectedIndex = index;
		WindowList.CreateList(data, this.ItemsRoot, this.ItemPrefab, this.Items, this.ItemsRoot.GetComponent<ToggleGroup>(), this.SelectedIndex, new Action<int>(this.SetActiveItem), new Action<int>(this.SetActiveItem), null, null);
	}

	protected override void AcceptActionCalled()
	{
		this.OnSelectedPrizePool(new WindowEntryFee.CurrencyData
		{
			Currency = this.CurrencyIcos[this.IfCheckerMinLevel.Value],
			EntranceFee = new double?((double)this.IfCheckerMaxLevel.Value)
		}, this.SelectedIndex);
	}

	protected void SetActiveItem(int i)
	{
		this.Items[this.SelectedIndex].SetActive(false);
		this.SelectedIndex = i;
		this.Items[this.SelectedIndex].SetActive(true);
	}

	[SerializeField]
	protected GameObject ItemsRoot;

	[SerializeField]
	protected GameObject ItemPrefab;

	protected int SelectedIndex;

	protected List<IWindowListItem> Items = new List<IWindowListItem>();

	protected List<WindowList.WindowListElem> Data;
}
