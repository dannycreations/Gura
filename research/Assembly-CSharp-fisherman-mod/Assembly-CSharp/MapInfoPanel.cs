using System;
using DG.Tweening;
using UnityEngine;

public class MapInfoPanel
{
	public MapInfoPanel(RectTransform infoPanel)
	{
		this._infoPanel = infoPanel;
		this._infoPanel.gameObject.SetActive(false);
	}

	public bool Openned
	{
		get
		{
			return this._openned;
		}
	}

	public void OpenInfoPanel(Vector2 startPos, Vector2 lastPos)
	{
		this._infoPanel.gameObject.SetActive(true);
		if (this._closed)
		{
			if (!this._inProgress)
			{
				this._inProgress = true;
				this._infoPanel.anchoredPosition = startPos;
				TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._infoPanel, lastPos, this._duration, true), 8), delegate
				{
					this._inProgress = false;
					this._closed = false;
					this.sPos = startPos;
					this.OnComplete();
				});
			}
			else
			{
				this.newSPos = startPos;
				this.newLPos = lastPos;
				this._openNext = true;
			}
		}
		else
		{
			if (!this._inProgress)
			{
				this.Close();
			}
			this.newSPos = startPos;
			this.newLPos = lastPos;
			this._openNext = true;
		}
		this._openned = true;
	}

	public void CloseInfoPanel()
	{
		MonoBehaviour.print("CloseInfoPanel called");
		if (!this._closed && this._openned)
		{
			this._openned = false;
			this.Close();
		}
	}

	private void OnComplete()
	{
		if (!this._closed)
		{
			if (this._openNext)
			{
				this.Close();
			}
		}
		else
		{
			this.OpenInfoPanel(this.newSPos, this.newLPos);
			this._openNext = false;
		}
	}

	private void Close()
	{
		if (!this._inProgress)
		{
			this._inProgress = true;
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._infoPanel, this.sPos, this._duration, true), 8), delegate
			{
				this._inProgress = false;
				this._closed = true;
				this._infoPanel.gameObject.SetActive(false);
				if (this._openned)
				{
					this.OnComplete();
				}
			});
		}
	}

	private RectTransform _infoPanel;

	private float _duration = 0.25f;

	private bool _inProgress;

	private bool _openned;

	private bool _closed = true;

	private bool _openNext;

	private Vector2 sPos;

	private Vector2 newLPos;

	private Vector2 newSPos;
}
