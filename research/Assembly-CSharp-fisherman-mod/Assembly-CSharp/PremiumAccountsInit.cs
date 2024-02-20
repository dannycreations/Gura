using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class PremiumAccountsInit : MonoBehaviour, IPremiumProducts
{
	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public void Init(List<StoreProduct> products)
	{
		ClientDebugHelper.Log(ProfileFlag.ProductDelivery, "Premium products Count: " + products.Count);
		this.InitPremiumAccountPannel(products, this.Day1, 1);
		this.InitPremiumAccountPannel(products, this.Day3, 3);
		this.InitPremiumAccountPannel(products, this.Day7, 7);
		this.InitPremiumAccountPannel(products, this.Day30, 30);
		this.InitPremiumAccountPannel(products, this.Day180, 180);
		this.InitPremiumAccountPannel(products, this.Day360, 360);
	}

	private void InitPremiumAccountPannel(List<StoreProduct> products, PremiumAccountInit pannelController, int term)
	{
		StoreProduct storeProduct = products.FirstOrDefault((StoreProduct x) => x.TypeId == 3 && x.Term == term);
		if (storeProduct == null)
		{
			ClientDebugHelper.Error(ProfileFlag.ProductDelivery, "Can't find Premium Account product for " + term + " day(s)");
		}
		else
		{
			pannelController.Init(storeProduct);
			ClientDebugHelper.Log(ProfileFlag.ProductDelivery, string.Concat(new object[] { "Product #", storeProduct.ProductId, " is used as Premium Account product for ", term, " day(s)" }));
		}
	}

	public void OnShadow(GameObject UnShadowObject)
	{
		this.Day1.GetComponent<ShowInfoPanel>().ShadowOn();
		this.Day3.GetComponent<ShowInfoPanel>().ShadowOn();
		this.Day7.GetComponent<ShowInfoPanel>().ShadowOn();
		this.Day30.GetComponent<ShowInfoPanel>().ShadowOn();
		this.Day180.GetComponent<ShowInfoPanel>().ShadowOn();
		this.Day360.GetComponent<ShowInfoPanel>().ShadowOn();
		UnShadowObject.GetComponent<ShowInfoPanel>().ShadowOff();
	}

	public void OffShadow()
	{
		this.Day1.GetComponent<ShowInfoPanel>().ShadowOff();
		this.Day3.GetComponent<ShowInfoPanel>().ShadowOff();
		this.Day7.GetComponent<ShowInfoPanel>().ShadowOff();
		this.Day30.GetComponent<ShowInfoPanel>().ShadowOff();
		this.Day180.GetComponent<ShowInfoPanel>().ShadowOff();
		this.Day360.GetComponent<ShowInfoPanel>().ShadowOff();
	}

	public PremiumAccountInit Day1;

	public PremiumAccountInit Day3;

	public PremiumAccountInit Day7;

	public PremiumAccountInit Day30;

	public PremiumAccountInit Day180;

	public PremiumAccountInit Day360;
}
