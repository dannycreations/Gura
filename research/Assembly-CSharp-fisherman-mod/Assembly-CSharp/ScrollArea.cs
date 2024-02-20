using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollArea : MonoBehaviour
{
	public GameObject ContentObject
	{
		get
		{
			return this._contentPanel;
		}
	}

	private void Awake()
	{
		if (this._contentPanel)
		{
			Transform transform = this._contentPanel.transform.parent.Find("Scrollbar");
			if (transform != null)
			{
				this._scrollbar = transform.gameObject.GetComponent<Scrollbar>();
			}
			this._canvas = this._contentPanel.GetComponent<CanvasGroup>();
			this._vLayout = this._contentPanel.GetComponent<VerticalLayoutGroup>();
			this._sizeFitter = this._contentPanel.GetComponent<ContentSizeFitter>();
		}
	}

	public void SetContentVisibility(bool flag)
	{
		if (this._canvas != null)
		{
			this._canvas.alpha = (float)((!flag) ? 0 : 1);
		}
	}

	public void PrepareFilling()
	{
		this._canvas.alpha = 0f;
		this._vLayout.enabled = true;
		this._sizeFitter.enabled = true;
	}

	public void FinishFilling()
	{
		this._canvas.alpha = 1f;
		base.StartCoroutine(this.DisableLayout());
	}

	public void ScrollContentUp()
	{
		base.StartCoroutine(this.ScrollUp());
	}

	private IEnumerator ScrollUp()
	{
		this._scrollbar.value = 1f;
		yield return new WaitForSeconds(0.5f);
		this._scrollbar.value = 1f;
		yield break;
	}

	private IEnumerator DisableLayout()
	{
		yield return new WaitForSeconds(0.5f);
		this._vLayout.enabled = false;
		this._sizeFitter.enabled = false;
		if (this._scrollbar != null)
		{
			this._scrollbar.value = 1f;
		}
		yield break;
	}

	[SerializeField]
	private GameObject _contentPanel;

	private Scrollbar _scrollbar;

	private CanvasGroup _canvas;

	private VerticalLayoutGroup _vLayout;

	private ContentSizeFitter _sizeFitter;
}
