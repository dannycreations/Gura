using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TargetedAdsController : MonoBehaviour
{
	private void Start()
	{
		this.UpdatePrevNextBtnsTexts();
	}

	private void OnEnable()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public void Init(List<TargetedAdSlide> items)
	{
		this.SetupIndicator(items.Count);
		if (items.Count <= 1)
		{
			this.DisableNavigateButtons();
		}
		this._listOfPanels = new List<TargetAdInit>();
		List<TargetedAdSlide> list = items.OrderBy((TargetedAdSlide x) => x.OrderId).ToList<TargetedAdSlide>();
		for (int i = 0; i < list.Count; i++)
		{
			this._listOfPanels.Add(this.GetTargedAd(list[i]));
		}
		if (this._listOfPanels.Count <= 0)
		{
			return;
		}
		this._currentPanel = 0;
		this.ShowPanel(this._listOfPanels[this._currentPanel].gameObject, this._currentPanel);
		this._listOfPanels[this._currentPanel].SetActive(true, this.ConsoleBuyButton);
		this.UpdateBuyButtonText(this._currentPanel);
	}

	private void DisableNavigateButtons()
	{
		this.NextButton.gameObject.SetActive(false);
		this.PrevsButton.gameObject.SetActive(false);
	}

	private TargetAdInit GetTargedAd(TargetedAdSlide item)
	{
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.TargetedAdPanelPrefab.gameObject);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.anchoredPosition = Vector2.zero;
		component.localScale = Vector3.one;
		TargetAdInit component2 = gameObject.GetComponent<TargetAdInit>();
		component2.Init(item);
		return component2;
	}

	private void ShowPanel(GameObject panel, int index)
	{
		this.SetIndicator(index);
		panel.GetComponent<AlphaFade>().ShowPanel();
	}

	private void HidePanel(GameObject panel)
	{
		panel.GetComponent<AlphaFade>().HidePanel();
	}

	public void ShowNextPanel()
	{
		if (this._listOfPanels.Count <= 1)
		{
			return;
		}
		int num = this._currentPanel + 1;
		if (num >= this._listOfPanels.Count)
		{
			num = 0;
		}
		this.UpdatePanel(num);
	}

	public void ShowPrevPanel()
	{
		if (this._listOfPanels.Count <= 1)
		{
			return;
		}
		int num = this._currentPanel - 1;
		if (num < 0)
		{
			num = this._listOfPanels.Count - 1;
		}
		this.UpdatePanel(num);
	}

	public void CloseWindow()
	{
		base.GetComponent<AlphaFade>().HidePanel();
	}

	private void UpdatePanel(int nextPanel)
	{
		this.HidePanel(this._listOfPanels[this._currentPanel].gameObject);
		this.ShowPanel(this._listOfPanels[nextPanel].gameObject, nextPanel);
		this._listOfPanels[this._currentPanel].SetActive(false, null);
		this._listOfPanels[nextPanel].SetActive(true, this.ConsoleBuyButton);
		this.UpdateBuyButtonText(nextPanel);
		this._currentPanel = nextPanel;
	}

	private void UpdateBuyButtonText(int idx)
	{
		this.BuyButtonText.text = ScriptLocalization.Get("BuyButton").ToUpper();
		StoreProduct product = this._listOfPanels[idx].Product;
		if (product != null && product.TypeId == 9 && product.ProductCurrency != "USD")
		{
			this.BuyButtonText.text = string.Format("{0}\n{1} {2}", ScriptLocalization.Get("PremShop_BuyFor"), product.GetPrice(TimeHelper.UtcTime()).ToString("N0"), MeasuringSystemManager.GetCurrencyIcon(product.ProductCurrency));
		}
	}

	private void SetupIndicator(int pagesCount)
	{
		for (int i = pagesCount; i < this.PagesIndicator.Count; i++)
		{
			this.PagesIndicator[i].SetActive(false);
		}
		if (pagesCount == 1)
		{
			this.PagesIndicator[0].SetActive(false);
		}
	}

	private void SetIndicator(int index)
	{
		for (int i = 0; i < this.PagesIndicator.Count; i++)
		{
			this.PagesIndicator[i].GetComponent<Text>().text = "\ue62a";
		}
		this.PagesIndicator[index].GetComponent<Text>().text = "\ue629";
	}

	private void UpdatePrevNextBtnsTexts()
	{
		string text = "\ue696";
		string text2 = "\ue694";
		if (SettingsManager.InputType != InputModuleManager.InputType.GamePad)
		{
			text = "\ue62e";
			text2 = "\ue632";
		}
		this.NextButtonText.text = string.Format("{0} {1}", ScriptLocalization.Get("NextSlideCaption").ToUpper(CultureInfo.InvariantCulture), text);
		this.PrevsButtonText.text = string.Format("{1} {0}", ScriptLocalization.Get("PreviousSlideCaption").ToUpper(CultureInfo.InvariantCulture), text2);
	}

	private void OnInputTypeChanged(InputModuleManager.InputType obj)
	{
		this.UpdatePrevNextBtnsTexts();
	}

	public Text BuyButtonText;

	public TargetAdInit TargetedAdPanelPrefab;

	public GameObject ContentPanel;

	public Text NextButtonText;

	public Text PrevsButtonText;

	public Button NextButton;

	public Button PrevsButton;

	public List<GameObject> PagesIndicator;

	public Button ConsoleBuyButton;

	private List<TargetAdInit> _listOfPanels = new List<TargetAdInit>();

	private int _currentPanel;
}
