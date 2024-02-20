using System;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PlacementLicenses : ActivityStateControlled
{
	public void Refresh()
	{
		if (base.GetComponent<ToggleGroup>() == null)
		{
			base.gameObject.AddComponent<ToggleGroup>();
		}
		base.GetComponent<ToggleGroup>().allowSwitchOff = true;
		this.Clear(this.ContentPanel);
		IOrderedEnumerable<PlayerLicense> orderedEnumerable = PhotonConnectionFactory.Instance.Profile.ActiveLicenses.OrderBy((PlayerLicense x) => x.Name);
		foreach (PlayerLicense playerLicense in orderedEnumerable)
		{
			this.AddItemToLicenses(playerLicense);
		}
	}

	private void AddItemToLicenses(PlayerLicense license)
	{
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.LicensePrefab);
		gameObject.transform.Find("Name").GetComponent<Text>().text = license.Name;
		gameObject.transform.Find("Expire").Find("Date").GetComponent<Text>()
			.text = ((license.End == null) ? ScriptLocalization.Get("UnlimitedCaption") : MeasuringSystemManager.DateTimeString(license.End.Value.ToLocalTime()));
		gameObject.transform.Find("Info").GetComponent<Toggle>().group = base.GetComponent<ToggleGroup>();
		gameObject.GetComponent<ShowDetailedLicenseInfo>().License = license;
		this._height += 80;
		ScrollForwarding scrollForwarding = gameObject.AddComponent<ScrollForwarding>();
		scrollForwarding.ScrollCalled += this.ScrollTriggerOnScrollCalled;
	}

	private void ScrollTriggerOnScrollCalled(object sender, ScrollEventArgs scrollEventArgs)
	{
	}

	private void Clear(GameObject contentsPanel)
	{
		this._height = 0;
		for (int i = 0; i < contentsPanel.transform.childCount; i++)
		{
			Object.Destroy(contentsPanel.transform.GetChild(i).gameObject);
		}
	}

	private void PlacementInventory_HideDetail(object sender, EventArgs e)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		this.SetSizeForContent(component.rect.height - 160f);
	}

	private void PlacementInventory_ShowDetail(object sender, EventArgs e)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		this.SetSizeForContent(component.rect.height + 160f);
	}

	protected override void SetHelp()
	{
		UIStatsCollector.ChangeGameScreen(GameScreenType.Licenses, GameScreenTabType.Undefined, null, null, null, null, null);
		this.Refresh();
	}

	private void SetSizeForContent(float height)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, Mathf.Max(height, base.GetComponent<RectTransform>().rect.height));
		if (component.sizeDelta.y > base.GetComponent<RectTransform>().rect.height)
		{
			base.transform.Find("Scrollbar").gameObject.SetActive(true);
		}
		else
		{
			base.transform.Find("Scrollbar").gameObject.SetActive(false);
		}
	}

	public GameObject LicensePrefab;

	public GameObject ContentPanel;

	private int _height;
}
