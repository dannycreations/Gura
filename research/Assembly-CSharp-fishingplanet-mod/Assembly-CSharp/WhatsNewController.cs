using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WhatsNewController : MonoBehaviour
{
	private void Awake()
	{
		this._pagesIndicatorTexts = this.PagesIndicator.Select((GameObject p) => p.GetComponent<Text>()).ToList<Text>();
	}

	private void Start()
	{
		this.NextButtonText.text = string.Format("{0} \ue62e", ScriptLocalization.Get("NextSlideCaption").ToUpper(CultureInfo.InvariantCulture));
		this.PrevsButtonText.text = string.Format("\ue632 {0}", ScriptLocalization.Get("PreviousSlideCaption").ToUpper(CultureInfo.InvariantCulture));
	}

	public void Init(List<WhatsNewItem> items)
	{
		this.SetupIndicator(items.Count);
		if (items.Count <= 1)
		{
			this.DisableNavigateButtons();
		}
		this._listOfPanels = new List<WhatsNewInit>();
		List<WhatsNewItem> list = items.OrderBy((WhatsNewItem x) => x.OrderId).ToList<WhatsNewItem>();
		for (int i = 0; i < list.Count; i++)
		{
			this._listOfPanels.Add(this.GetWhatsNew(list[i]));
		}
		if (this._listOfPanels.Count <= 0)
		{
			return;
		}
		this._currentPanel = 0;
		this.ShowPanel(this._listOfPanels[this._currentPanel], this._currentPanel);
	}

	private void DisableNavigateButtons()
	{
		this.NextButton.gameObject.SetActive(false);
		this.PrevsButton.gameObject.SetActive(false);
	}

	private WhatsNewInit GetWhatsNew(WhatsNewItem item)
	{
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.WhatsNewPanelPrefab.gameObject);
		gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		gameObject.GetComponent<WhatsNewInit>().Init(item, this.BuyButton);
		return gameObject.GetComponent<WhatsNewInit>();
	}

	private void ShowPanel(WhatsNewInit panel, int index)
	{
		this.SetIndicator(index);
		if (panel.Product != null && panel.Product.TypeId == 9 && panel.Product.ProductCurrency != "USD")
		{
			this.BuyButtonText.text = string.Format("{0}\n{1} {2}", ScriptLocalization.Get("PremShop_BuyFor"), panel.Product.GetPrice(TimeHelper.UtcTime()).ToString("N0"), MeasuringSystemManager.GetCurrencyIcon(panel.Product.ProductCurrency));
		}
		else
		{
			this.BuyButtonText.text = ((panel.HasActualProduct || !panel.HasUrl) ? ScriptLocalization.Get("BuyButton").ToUpper() : ScriptLocalization.Get("ViewInfoTitle").ToUpper());
		}
		panel.Show();
		panel.GetComponent<AlphaFade>().ShowPanel();
	}

	private void HidePanel(WhatsNewInit panel)
	{
		panel.Hide();
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
		this.HidePanel(this._listOfPanels[this._currentPanel]);
		this.ShowPanel(this._listOfPanels[num], num);
		this._currentPanel = num;
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
		this.HidePanel(this._listOfPanels[this._currentPanel]);
		this.ShowPanel(this._listOfPanels[num], num);
		this._currentPanel = num;
	}

	public void CloseWindow()
	{
		base.GetComponent<AlphaFade>().HidePanel();
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
		if (this._pagesIndicatorTexts == null)
		{
			this.Awake();
		}
		for (int i = 0; i < this._pagesIndicatorTexts.Count; i++)
		{
			this._pagesIndicatorTexts[i].text = ((index != i) ? "\ue62a" : "\ue629");
		}
		if (index >= this.PagesIndicator.Count || index < 0)
		{
			LogHelper.Error("WhatsNewController:SetIndicator index:[{0}] out of range; Panels:{1}", new object[]
			{
				index,
				this._listOfPanels.Count
			});
		}
	}

	public WhatsNewInit WhatsNewPanelPrefab;

	public GameObject ContentPanel;

	public Text NextButtonText;

	public Text PrevsButtonText;

	public Text BuyButtonText;

	public Button NextButton;

	public Button PrevsButton;

	public Button BuyButton;

	public List<GameObject> PagesIndicator;

	private List<WhatsNewInit> _listOfPanels = new List<WhatsNewInit>();

	private int _currentPanel;

	private List<Text> _pagesIndicatorTexts;
}
