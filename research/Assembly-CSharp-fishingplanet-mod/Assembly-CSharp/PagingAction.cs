using System;
using UnityEngine;
using UnityEngine.UI;

public class PagingAction : MonoBehaviour
{
	private void Update()
	{
		if (this._currentPage != this.updateContentItems.CurrentPage || this.updateContentItems.Pages != this._pages)
		{
			this._currentPage = this.updateContentItems.CurrentPage;
			this._pages = this.updateContentItems.Pages;
			this.pageText.text = string.Format("{0} of {1}", this._currentPage, this.updateContentItems.Pages);
		}
		if (this.SortAndSearchPanel != null)
		{
			bool flag = this.updateContentItems.ProductItems == null;
			if (this._isShouldShowSearch != flag)
			{
				this._isShouldShowSearch = flag;
				this.SortAndSearchPanel.SetActive(this._isShouldShowSearch);
			}
		}
	}

	public void FirstPage()
	{
		if (this.updateContentItems.Pages == 0)
		{
			return;
		}
		this.updateContentItems.CurrentPage = 1;
		this.updateContentItems.ChangePage();
	}

	public void PrevsPage()
	{
		if (this.updateContentItems.Pages == 0)
		{
			return;
		}
		this.updateContentItems.CurrentPage = (ushort)Mathf.Max(1, (int)(this.updateContentItems.CurrentPage - 1));
		this.updateContentItems.ChangePage();
	}

	public void LastPage()
	{
		if (this.updateContentItems.Pages == 0)
		{
			return;
		}
		this.updateContentItems.CurrentPage = this.updateContentItems.Pages;
		this.updateContentItems.ChangePage();
	}

	public void NextPage()
	{
		if (this.updateContentItems.Pages == 0)
		{
			return;
		}
		this.updateContentItems.CurrentPage = (ushort)Mathf.Min((int)this.updateContentItems.Pages, (int)(this.updateContentItems.CurrentPage + 1));
		this.updateContentItems.ChangePage();
	}

	private ushort _currentPage;

	private ushort _pages;

	public UpdateContentItems updateContentItems;

	public Text pageText;

	public GameObject SortAndSearchPanel;

	private bool _isShouldShowSearch = true;
}
