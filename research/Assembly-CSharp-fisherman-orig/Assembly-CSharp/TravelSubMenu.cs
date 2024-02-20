using System;
using I2.Loc;
using TMPro;
using UnityEngine;

public class TravelSubMenu : SubMenuFoldoutBase
{
	protected override void Awake()
	{
		base.Awake();
		this._transfer.text = string.Format("{0}:", ScriptLocalization.Get("Transfer"));
		this._total.text = string.Format("{0}:", ScriptLocalization.Get("[TotalTravelCaption]"));
	}

	public override void SetOpened(bool opened, Action callback = null)
	{
		base.SetOpened(opened, callback);
		if (opened)
		{
		}
	}

	[SerializeField]
	private TextMeshProUGUI _transfer;

	[SerializeField]
	private TextMeshProUGUI _total;
}
