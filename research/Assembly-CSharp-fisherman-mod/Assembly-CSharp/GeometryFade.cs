using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectMask2D))]
public class GeometryFade : MonoBehaviour
{
	public bool IsHiding
	{
		get
		{
			return this._isHiding;
		}
	}

	private void Start()
	{
		this._rectMask = base.GetComponent<RectMask2D>();
	}

	private void Update()
	{
		if (this._isShowing)
		{
			this._currentShowTime += Time.deltaTime;
			float num = this._currentShowTime / this.ShowFadeTime;
			this.GetRectTransform.SetSizeWithCurrentAnchors(1, Mathf.Lerp(0.1f, this._originalSize.y, num));
			this._rectMask.enabled = true;
			if (num >= 1f)
			{
				this._rectMask.enabled = false;
				this._isShow = true;
				this._isShowing = false;
				this._isHiding = false;
			}
		}
		else if (this._isHiding)
		{
			this._currentHideTime += Time.deltaTime;
			float num2 = this._currentHideTime / this.HideFadeTime;
			this.GetRectTransform.SetSizeWithCurrentAnchors(1, Mathf.Lerp(0.1f, this._originalSize.y, Mathf.Max(1f - num2, 0f)));
			this._rectMask.enabled = true;
			if (1f - num2 <= 0f)
			{
				this._isShow = false;
				this._isHiding = false;
				this._isShowing = false;
			}
		}
	}

	public virtual void ShowPanel()
	{
		this._currentShowTime = 0f;
		this._isHiding = false;
		this._isShowing = true;
		base.GetComponent<RectMask2D>().enabled = false;
	}

	public virtual void HidePanel()
	{
		if (!this._isHiding)
		{
			this._currentHideTime = 0f;
		}
		this._isShowing = false;
		this._isHiding = true;
	}

	public void FastHidePanel()
	{
		this.GetRectTransform.SetSizeWithCurrentAnchors(1, 0f);
		this._isShow = false;
		this._isShowing = false;
		this._isHiding = false;
	}

	public void FastShowPanel()
	{
		this.GetRectTransform.SetSizeWithCurrentAnchors(1, this._originalSize.y);
		this._isShow = false;
		this._isShowing = false;
		this._isHiding = false;
	}

	private RectTransform GetRectTransform
	{
		get
		{
			if (this._rectTransform != null)
			{
				return this._rectTransform;
			}
			this._rectTransform = base.GetComponent<RectTransform>();
			this._originalSize = this._rectTransform.sizeDelta;
			return this._rectTransform;
		}
	}

	public float ShowFadeTime = 0.125f;

	public float HideFadeTime = 0.125f;

	private RectTransform _rectTransform;

	private bool _isShowing;

	private bool _isHiding;

	private float _currentHideTime;

	private float _currentShowTime;

	private bool _isShow;

	private Vector2 _originalSize;

	private RectMask2D _rectMask;
}
