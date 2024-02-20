using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class TabControl : ActivityStateControlled
{
	protected List<TabPage> Pages
	{
		get
		{
			return this._pages;
		}
	}

	protected ToggleLink[] ToggleLinks
	{
		get
		{
			return this._toggleLinks;
		}
	}

	private TabPage CurPage
	{
		get
		{
			return this._pages[this._curPageIndex];
		}
	}

	protected override void Start()
	{
		base.Start();
		this._pages = new List<TabPage>(this._toggleLinks.Length);
		if (this._toggleGroup == null)
		{
			this._toggleGroup = base.GetComponent<ToggleGroup>();
		}
		for (int i = 0; i < this._toggleLinks.Length; i++)
		{
			TabPage page = this._toggleLinks[i].Page;
			page.OnHidden += this.Page_OnHide;
			this._pages.Add(page);
			page.FastHidePanel();
			page.FinishHideInvokes();
			Toggle toggle = this._toggleLinks[i].Toggle;
			toggle.group = this._toggleGroup;
			int i1 = i;
			toggle.onValueChanged.AddListener(delegate(bool tg)
			{
				if (tg)
				{
					this.SetPage(i1);
				}
				if (!this._toggleLinks.Any((ToggleLink item) => item.Toggle.isOn))
				{
					this.Hide();
				}
			});
			toggle.gameObject.SetActive(this.IsPageAvailable(page));
		}
		this.Refresh();
	}

	protected virtual bool IsPageAvailable(TabPage page)
	{
		return true;
	}

	public void Refresh()
	{
		if (this._curPageIndex >= 0 && this._pages != null)
		{
			this._pages[this._curPageIndex].ShowPanel();
			this._pages[this._curPageIndex].RefreshPage();
		}
	}

	public void SetPage(int newPageIndex)
	{
		if (newPageIndex != this._curPageIndex)
		{
			this._waitingPageIndex = newPageIndex;
			if (this._curPageIndex != -1)
			{
				this._pages[this._curPageIndex].HidePanel();
			}
			else
			{
				this.Page_OnHide();
			}
			this._pages[newPageIndex].RefreshPage();
		}
		else
		{
			this._pages[this._curPageIndex].RefreshPage();
		}
	}

	protected override void SetHelp()
	{
		if (this._curPageIndex >= 0 && this._pages != null)
		{
			this._pages[this._curPageIndex].FastHidePanel();
			this.Refresh();
		}
	}

	protected override void HideHelp()
	{
		if (this._curPageIndex >= 0 && this._pages != null)
		{
			this._pages[this._curPageIndex].FastHidePanel();
			this._pages[this._curPageIndex].FinishHideInvokes();
		}
	}

	private void Hide()
	{
		this._pages[this._curPageIndex].HidePanel();
		this._curPageIndex = -1;
		this._waitingPageIndex = -1;
	}

	private void Page_OnHide()
	{
		if (this._waitingPageIndex != -1)
		{
			if (this._disableOnHide)
			{
				if (this._curPageIndex != -1)
				{
					this._pages[this._curPageIndex].gameObject.SetActive(false);
				}
				this._pages[this._waitingPageIndex].gameObject.SetActive(true);
			}
			this._curPageIndex = this._waitingPageIndex;
			this._waitingPageIndex = -1;
			this._pages[this._curPageIndex].ShowPanel();
		}
	}

	protected override void OnDestroy()
	{
		for (int i = 0; i < this._toggleLinks.Length; i++)
		{
			this._toggleLinks[i].Page.OnHidden -= this.Page_OnHide;
			this._toggleLinks[i].Toggle.onValueChanged.RemoveAllListeners();
		}
		base.OnDestroy();
	}

	private List<TabPage> _pages;

	[SerializeField]
	private ToggleLink[] _toggleLinks;

	[SerializeField]
	private bool _disableOnHide;

	private int _curPageIndex;

	private int _waitingPageIndex = -1;

	[Tooltip("It's normal to have toggleGroup field empty when use ToogleGroup component attached to this game object")]
	[SerializeField]
	private ToggleGroup _toggleGroup;
}
