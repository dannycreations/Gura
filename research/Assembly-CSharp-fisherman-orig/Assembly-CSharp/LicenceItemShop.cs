using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LicenceItemShop : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private event EventHandler<EventArgs> OnChoose = delegate
	{
	};

	private void Awake()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		this.Init(0);
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public void FillData(EventHandler<EventArgs> onChoose)
	{
		this.OnInputTypeChanged(SettingsManager.InputType);
		string text = "License" + this.Licence.LicenseId;
		HintElementId component = base.GetComponent<HintElementId>();
		if (component == null)
		{
			base.gameObject.AddComponent<HintElementId>().SetElementId(text, null, null);
			if (this._terms != null && this._terms.Count > 0)
			{
				foreach (KeyValuePair<Text, int> keyValuePair in this._terms)
				{
					if (keyValuePair.Value != 0)
					{
						keyValuePair.Key.transform.parent.gameObject.AddComponent<HintElementId>();
					}
				}
			}
		}
		else
		{
			component.SetElementId(text, null, null);
		}
		this.SaleIcon.SetActive(false);
		this.Title.text = this.Licence.Name;
		this.Level.text = string.Format("{0}{1}", "\ue62d", this.Licence.OriginalMinLevel);
		this.OnChoose = onChoose;
		bool flag = PhotonConnectionFactory.Instance.Profile.HomeState == this.Licence.StateId;
		this.SetCost(this.Licence.Costs.FirstOrDefault((ShopLicenseCost x) => x.Term == 0), this.UnlimCost.transform, flag);
		if (this._ico != null && this.Licence.LogoBID != null)
		{
			this._icoLdbl.Image = this._ico;
			this._icoLdbl.Load(string.Format("Textures/Inventory/{0}", this.Licence.LogoBID.Value.ToString()));
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		if (this.circleNavigation != null)
		{
			this.circleNavigation.SetSelectablesInteractable(type == InputModuleManager.InputType.Mouse);
		}
	}

	private void SetCost(ShopLicenseCost lCost, Transform t, bool isResidence)
	{
		if (lCost != null && lCost.Term == this.Term)
		{
			if (isResidence)
			{
				float? discountResidentCost = lCost.DiscountResidentCost;
				this.Cost = ((discountResidentCost == null) ? lCost.ResidentCost : discountResidentCost.Value);
			}
			else
			{
				float? discountNonResidentCost = lCost.DiscountNonResidentCost;
				this.Cost = ((discountNonResidentCost == null) ? lCost.NotResidentCost : discountNonResidentCost.Value);
			}
			this.Currency = lCost.Currency;
		}
		float? num = ((!isResidence) ? lCost.DiscountNonResidentCost : lCost.DiscountResidentCost);
		float num2 = ((!isResidence) ? lCost.NotResidentCost : lCost.ResidentCost);
		if (num != null)
		{
			this.SaleIcon.SetActive(true);
			this.SetValue(t, num.ToString(), true, lCost.Currency == "GC");
		}
		else
		{
			this.SetValue(t, num2.ToString(CultureInfo.InvariantCulture), false, lCost.Currency == "GC");
		}
	}

	private void SetValue(Transform item, string value, bool isDiscount = false, bool isGold = false)
	{
		item.Find("Value").GetComponent<Text>().text = value;
		item.Find("Value").GetComponent<Text>().color = (isDiscount ? Color.red : this._notDiscountColor);
		item.Find("Currency").GetComponent<Text>().text = (isGold ? "\ue62c" : "\ue62b");
		item.Find("Currency").GetComponent<Text>().color = (isGold ? this._goldColor : this._notDiscountColor);
	}

	public void Init(int term)
	{
		this.Term = term;
		if (this.Licence == null || this.Licence.Costs == null)
		{
			return;
		}
		ShopLicenseCost shopLicenseCost = this.Licence.Costs.FirstOrDefault((ShopLicenseCost x) => x.Term == term);
		if (shopLicenseCost != null)
		{
			if (PhotonConnectionFactory.Instance.Profile.HomeState == this.Licence.StateId)
			{
				float? discountResidentCost = shopLicenseCost.DiscountResidentCost;
				this.Cost = ((discountResidentCost == null) ? shopLicenseCost.ResidentCost : discountResidentCost.Value);
			}
			else
			{
				float? discountNonResidentCost = shopLicenseCost.DiscountNonResidentCost;
				this.Cost = ((discountNonResidentCost == null) ? shopLicenseCost.NotResidentCost : discountNonResidentCost.Value);
			}
			this.Currency = shopLicenseCost.Currency;
		}
	}

	public void OnToggleValueChanged(bool isOn)
	{
		MonoBehaviour.print("isOn: " + isOn);
		if (isOn)
		{
			this.ElementInputListener.StartListenForHotkeys();
			if (this.circleNavigation != null)
			{
				this.circleNavigation.enabled = true;
			}
			if (!this.justChanged)
			{
				this.Change(this.Term);
			}
		}
		else
		{
			this.ElementInputListener.StopListenForHotKeys();
			if (this.circleNavigation != null)
			{
				this.circleNavigation.enabled = false;
			}
		}
	}

	public void BackPressed()
	{
		ShopMainPageHandler.Instance.SetFiltersNavigationEnabled(true);
		this.ElementInputListener.StopListenForHotKeys();
		if (this.circleNavigation != null)
		{
			this.circleNavigation.enabled = false;
		}
	}

	public void Change(int term)
	{
		this.Init(term);
		this.justChanged = true;
		this.OnToggleValueChanged(true);
		this.justChanged = false;
		if (this.OnChoose != null)
		{
			this.OnChoose(this, null);
		}
		this.ShowDetails();
	}

	public void ShowDetails()
	{
		if (this.OnChoose != null)
		{
			this.OnChoose(this, null);
		}
	}

	public void Change1(bool isOn)
	{
		if (isOn)
		{
			this.Change(1);
		}
	}

	public void Change3(bool isOn)
	{
		if (isOn)
		{
			this.Change(3);
		}
	}

	public void Change7(bool isOn)
	{
		if (isOn)
		{
			this.Change(7);
		}
	}

	public void Change30(bool isOn)
	{
		if (isOn)
		{
			this.Change(30);
		}
	}

	public void Change0(bool isOn)
	{
		if (isOn)
		{
			this.Change(0);
		}
	}

	[SerializeField]
	private Image _ico;

	private ResourcesHelpers.AsyncLoadableImage _icoLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Title;

	public Toggle Border;

	public Text DayCost;

	public Text ThreeDayCost;

	public Text WeekCost;

	public Text MonthCost;

	public Text UnlimCost;

	public Text Level;

	public ShopLicense Licence;

	public GameObject SaleIcon;

	[HideInInspector]
	public int Term = 1;

	[HideInInspector]
	public float Cost;

	[HideInInspector]
	public string Currency;

	public HotkeyPressRedirect ElementInputListener;

	public CircleNavigation circleNavigation;

	private Dictionary<Text, int> _terms;

	private readonly Color _notDiscountColor = new Color(0.24313726f, 0.25490198f, 0.28627452f, 1f);

	private readonly Color _goldColor = new Color(0.72156864f, 0.6117647f, 0f, 1f);

	private bool justChanged;
}
