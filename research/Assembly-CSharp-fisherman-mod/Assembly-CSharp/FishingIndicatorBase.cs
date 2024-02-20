using System;
using UnityEngine;

public class FishingIndicatorBase : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.AlphaFade.FastHidePanel();
	}

	public virtual bool IsShow
	{
		get
		{
			return this.AlphaFade.IsShow;
		}
	}

	public virtual void Show()
	{
		this.SetActive(true);
	}

	public virtual void Hide()
	{
		this.SetActive(false);
	}

	public virtual void SetActive(bool flag)
	{
		if (flag)
		{
			this.AlphaFade.ShowPanel();
		}
		else
		{
			this.AlphaFade.HidePanel();
		}
	}

	[SerializeField]
	protected AlphaFade AlphaFade;
}
