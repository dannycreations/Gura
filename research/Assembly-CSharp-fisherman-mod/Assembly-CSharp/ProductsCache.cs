using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class ProductsCache : MonoBehaviour
{
	public List<ProductCategory> ProductCategories
	{
		get
		{
			return this._productCategories;
		}
	}

	public bool AreProductCategoriesAvailable { get; private set; }

	public List<WhatsNewItem> WhatsNewItems { get; private set; }

	public List<StoreProduct> Products
	{
		get
		{
			return this._products;
		}
	}

	public bool AreProductsAvailable { get; private set; }

	public void SubscribeEvents()
	{
	}

	public void UnsubscribeEvents()
	{
	}

	internal void Update()
	{
		if (!PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated || PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		if (this._productCategories == null)
		{
			this._productCategories = new List<ProductCategory>();
			PhotonConnectionFactory.Instance.OnGotProductCategories += this.OnGotProductCategories;
			PhotonConnectionFactory.Instance.GetProductCategories();
			return;
		}
		if (this._products == null)
		{
			this._products = new List<StoreProduct>();
			PhotonConnectionFactory.Instance.OnGotProducts += this.OnGotProducts;
			PhotonConnectionFactory.Instance.GetProducts(null);
			return;
		}
	}

	private void OnGotProductCategories(List<ProductCategory> categories)
	{
		PhotonConnectionFactory.Instance.OnGotProductCategories -= this.OnGotProductCategories;
		this._productCategories = categories;
		this.AreProductCategoriesAvailable = true;
		Debug.LogFormat("Product categories cache initialized, {0} categories in cache", new object[] { categories.Count });
	}

	private void OnGotProducts(List<StoreProduct> products)
	{
		PhotonConnectionFactory.Instance.OnGotProducts -= this.OnGotProducts;
		this._products = products;
		Debug.LogFormat("Products cache initialized, {0} products in cache", new object[] { products.Count });
		this.AreProductsAvailable = true;
		ManagerScenes.ProgressOfLoad += 0.08f;
	}

	public void SetWhatsNewItem(List<WhatsNewItem> items)
	{
		this.WhatsNewItems = items;
	}

	private List<ProductCategory> _productCategories;

	private List<StoreProduct> _products;
}
