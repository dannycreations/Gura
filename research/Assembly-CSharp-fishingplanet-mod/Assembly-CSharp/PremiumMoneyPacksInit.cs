using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class PremiumMoneyPacksInit : MonoBehaviour, IPremiumProducts
{
	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public void Init(List<StoreProduct> products)
	{
		StoreProduct[] array = (from p in products
			where p.TypeId == 1 && this._productIds.Contains(p.ProductId)
			orderby p.Gold
			select p).ToArray<StoreProduct>();
		if (array.Length != 6)
		{
			string text = "Incorrect number of money packs available. Having " + array.Length + " packs in place.";
			Debug.LogError(text);
			PhotonConnectionFactory.Instance.PinError("Error displaying Money Packs in Premium Shop", text);
			return;
		}
		for (int i = 0; i < this._packs.Length; i++)
		{
			this._packs[i].Init(array[i]);
		}
	}

	[SerializeField]
	private PremiumMoneyPackInit[] _packs;

	private readonly int[] _productIds = new int[] { 1, 2, 3, 12, 13, 19 };
}
