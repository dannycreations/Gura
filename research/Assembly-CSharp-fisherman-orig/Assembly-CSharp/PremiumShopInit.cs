using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;

public class PremiumShopInit : ActivityStateControlled
{
	private void Awake()
	{
		if (this._empty != null)
		{
			this._empty.SetActive(false);
		}
		this._itemsList.Add(this.PremiumAccounts);
		this._itemsList.Add(this.PremiumMoneyPacks);
		this._itemsList.Add(this.PremiumPasses);
	}

	protected override void Start()
	{
		base.Start();
		this._inited = true;
		this.InitItems(CacheLibrary.ProductCache.Products);
		ScreenManager.Instance.OnScreenChanged += this.Instance_OnScreenChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ScreenManager.Instance.OnScreenChanged -= this.Instance_OnScreenChanged;
	}

	protected override void SetHelp()
	{
		LogHelper.Log("___kocha SetHelp");
	}

	protected override void HideHelp()
	{
		this.ExitStore();
	}

	private void ExitStore()
	{
		LogHelper.Log("___kocha ExitStore");
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void Instance_OnScreenChanged(GameScreenType s)
	{
	}

	private void ShowEmpty()
	{
		this._itemsList.ForEach(delegate(IPremiumProducts p)
		{
			p.SetActive(false);
		});
		if (this._empty != null)
		{
			this._empty.SetActive(true);
		}
		this._emptyText.text = string.Format("<color=#FF1111FF>{0}:</color> {1}", ScriptLocalization.Get("ErrorCaption"), ScriptLocalization.Get("ListIsEmpty"));
	}

	private void InitItems(List<StoreProduct> products)
	{
		if (products == null || products.Count == 0)
		{
			this.ShowEmpty();
		}
		else
		{
			if (this._empty != null)
			{
				this._empty.SetActive(false);
			}
			this._itemsList.ForEach(delegate(IPremiumProducts p)
			{
				p.SetActive(true);
			});
			this._inited = true;
			this._itemsList.ForEach(delegate(IPremiumProducts p)
			{
				p.Init(products);
			});
		}
	}

	[SerializeField]
	private TextMeshProUGUI _emptyText;

	[SerializeField]
	private GameObject _empty;

	public PremiumAccountsInit PremiumAccounts;

	public PremiumMoneyPacksInit PremiumMoneyPacks;

	public PremiumPassesInit PremiumPasses;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private bool _inited;

	private List<IPremiumProducts> _itemsList = new List<IPremiumProducts>();
}
