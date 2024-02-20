using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSetAlphaForContent : MonoBehaviour
{
	private void Awake()
	{
		this.ResetView();
	}

	public void OnChange()
	{
		if (this.content == null)
		{
			return;
		}
		if (this._tgl == null)
		{
			this.ResetView();
		}
		if (this._tgl == null)
		{
			return;
		}
		if (this._tgl.isOn)
		{
			this.content.gameObject.SetActive(true);
		}
		else
		{
			this.content.gameObject.SetActive(false);
		}
	}

	public void OnChangeByAlpha()
	{
		if (this._tgl == null)
		{
			this.ResetView();
		}
		if (this._tgl.isOn)
		{
			this.content.gameObject.SetActive(true);
			this._alphaFade.ShowPanel();
		}
		else
		{
			this._alphaFade.HideFinished += this._alphaFade_HideFinished;
			this._alphaFade.HidePanel();
		}
	}

	private void _alphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._alphaFade.HideFinished -= this._alphaFade_HideFinished;
		this.content.gameObject.SetActive(false);
	}

	public void ResetView()
	{
		this._tgl = base.GetComponent<Toggle>();
		this._alphaFade = this.content.GetComponent<AlphaFade>();
	}

	public void SetContent(Graphic c)
	{
		this.content = c;
	}

	public Graphic content;

	private Toggle _tgl;

	private AlphaFade _alphaFade;
}
