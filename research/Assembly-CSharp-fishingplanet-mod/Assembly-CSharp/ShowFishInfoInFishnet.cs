using System;
using System.Diagnostics;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ShowFishInfoInFishnet : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ShowDetail;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> HideDetail;

	public void InfoClick()
	{
		Toggle component = base.transform.Find("Info").GetComponent<Toggle>();
		if (component.isOn)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}

	private void Show()
	{
		if (this._isShow)
		{
			this.Hide();
			return;
		}
		this.DetailedPanel.SetActive(true);
		base.GetComponent<LayoutElement>().preferredHeight += this.HeightDetailedPanel;
		if (this.ShowDetail != null)
		{
			this.ShowDetail(this, new EventArgs());
		}
		this._isShow = true;
		CaughtFish caughtFish = base.GetComponent<ConcreteFishInFishkeeper>().CaughtFish;
		this.DetailedPanel.transform.Find("Description").Find("Mask").Find("Content")
			.GetComponent<Text>()
			.text = caughtFish.Fish.Desc.Replace("<br>", "\n");
	}

	private void Hide()
	{
		if (!this._isShow)
		{
			return;
		}
		this.DetailedPanel.SetActive(false);
		base.GetComponent<LayoutElement>().preferredHeight -= this.HeightDetailedPanel;
		if (this.HideDetail != null)
		{
			this.HideDetail(this, new EventArgs());
		}
		this._isShow = false;
	}

	private bool _isShow;

	public float HeightDetailedPanel = 130f;

	public GameObject DetailedPanel;
}
