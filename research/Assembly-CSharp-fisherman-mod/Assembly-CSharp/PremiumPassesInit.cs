using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class PremiumPassesInit : MonoBehaviour, IPremiumProducts
{
	public void Init(List<StoreProduct> products)
	{
		List<StoreProduct> list = products.Where((StoreProduct x) => x.TypeId == 4 && x.PondsUnlocked != null && x.PondsUnlocked.Length == 1 && x.Term > 1).ToList<StoreProduct>();
		this.Pass1.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 113).ToList<StoreProduct>());
		this.Pass2.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 118).ToList<StoreProduct>());
		this.Pass3.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 115).ToList<StoreProduct>());
		this.Pass4.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 123).ToList<StoreProduct>());
		this.Pass5.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 114).ToList<StoreProduct>());
		this.Pass6.Init(list.Where((StoreProduct x) => x.PondsUnlocked[0] == 121).ToList<StoreProduct>());
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public PremiumPassInit Pass1;

	public PremiumPassInit Pass2;

	public PremiumPassInit Pass3;

	public PremiumPassInit Pass4;

	public PremiumPassInit Pass5;

	public PremiumPassInit Pass6;
}
