using System;
using System.Collections;
using ObjectModel;
using UnityEngine;

public class ShowInfoPanel : MonoBehaviour
{
	private void Start()
	{
		this.ShadowOff();
	}

	public void ShowPanel()
	{
		if (this.ProductInfo == null)
		{
			return;
		}
		this.TrianglePanel.GetComponent<RectTransform>().localPosition = new Vector3(this.TriangleXPosition, this.TrianglePanel.GetComponent<RectTransform>().localPosition.y, this.TrianglePanel.GetComponent<RectTransform>().localPosition.z);
		this.InfoPanel.GetComponent<AlphaFade>().ShowPanel();
		ShowInfoPanel.InfoPanelType panelType = this.PanelType;
		if (panelType != ShowInfoPanel.InfoPanelType.MoneyPack)
		{
			if (panelType != ShowInfoPanel.InfoPanelType.PremiumAccount)
			{
				if (panelType == ShowInfoPanel.InfoPanelType.PassPack)
				{
					this.InfoPanel.GetComponent<PremiumPassInfoPanelInit>().Init(this.ProductInfo);
				}
			}
			else
			{
				this.InfoPanel.GetComponent<PremiumAccountInfoPanelInit>().Init(this.ProductInfo);
			}
		}
		else
		{
			this.InfoPanel.GetComponent<PremiumMoneyPackInfoPanelInit>().Init(this.ProductInfo);
		}
	}

	public void ShowTimed()
	{
		base.StartCoroutine(this.ShowAfterTimeout());
	}

	private IEnumerator ShowAfterTimeout()
	{
		yield return new WaitForSeconds((float)this._timeout);
		this.ShowPanel();
		yield break;
	}

	public void HidePanel()
	{
		base.StopAllCoroutines();
		this.InfoPanel.GetComponent<AlphaFade>().HidePanel();
	}

	public void ShadowOn()
	{
		this.Shadow.SetActive(true);
	}

	public void ShadowOff()
	{
		this.Shadow.SetActive(false);
	}

	public GameObject InfoPanel;

	public GameObject TrianglePanel;

	public GameObject Shadow;

	public float TriangleXPosition;

	public ShowInfoPanel.InfoPanelType PanelType;

	[HideInInspector]
	public StoreProduct ProductInfo;

	private int _timeout = 2;

	public enum InfoPanelType
	{
		PremiumAccount,
		MoneyPack,
		PassPack
	}
}
