using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BrandDescriptionHandlerShopMainPage : MonoBehaviour
{
	internal void Start()
	{
		base.GetComponent<AlphaFade>().FastHidePanel();
		PhotonConnectionFactory.Instance.GetBrands();
		PhotonConnectionFactory.Instance.OnGotBrands += new OnGotBrands(this.OnGotBrands);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotBrands -= new OnGotBrands(this.OnGotBrands);
	}

	private void OnGotBrands(IEnumerable<Brand> brands)
	{
		if (brands == null)
		{
			return;
		}
		this._flaggman = brands.First((Brand x) => x.BrandId == 1).Desc;
		this._garryScott = brands.First((Brand x) => x.BrandId == 2).Desc;
		this._magFin = brands.First((Brand x) => x.BrandId == 3).Desc;
		this._rivertex = brands.First((Brand x) => x.BrandId == 4).Desc;
	}

	public void ShowFlaggmanDesc()
	{
		base.GetComponent<AlphaFade>().ShowPanel();
		this.NameText.text = "Flaggman";
		this.DescriptionText.text = this._flaggman;
	}

	public void ShowMagFinDesc()
	{
		base.GetComponent<AlphaFade>().ShowPanel();
		this.NameText.text = "MagFin";
		this.DescriptionText.text = this._magFin;
	}

	public void ShowRivertexDesc()
	{
		base.GetComponent<AlphaFade>().ShowPanel();
		this.NameText.text = "Rivertex";
		this.DescriptionText.text = this._rivertex;
	}

	public void ShowGSDesc()
	{
		base.GetComponent<AlphaFade>().ShowPanel();
		this.NameText.text = "GarryScott";
		this.DescriptionText.text = this._garryScott;
	}

	public void Hide()
	{
		base.GetComponent<AlphaFade>().HidePanel();
	}

	public Text DescriptionText;

	public Text NameText;

	private string _garryScott = string.Empty;

	private string _rivertex = string.Empty;

	private string _magFin = string.Empty;

	private string _flaggman = string.Empty;
}
