using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SubMenuFoldoutBase : MonoBehaviour
{
	public RectTransform Content
	{
		get
		{
			return base.transform as RectTransform;
		}
	}

	public bool Opened { get; private set; }

	protected virtual void Awake()
	{
		this.Navigation = this.Navigation ?? base.GetComponent<UINavigation>();
		this.Hotkeys = this.Hotkeys ?? base.GetComponent<HotkeyPressRedirect>();
		if (this.CanvasGroup == null)
		{
			this.CanvasGroup = base.GetComponent<CanvasGroup>() ?? base.gameObject.AddComponent<CanvasGroup>();
		}
		this.CanvasGroup.alpha = 0f;
		this.CanvasGroup.interactable = false;
		this.CanvasGroup.blocksRaycasts = false;
	}

	public virtual void SetOpened(bool opened, Action callback = null)
	{
		this.Opened = opened;
		this._callback = callback;
		float num = ((!opened) ? 0f : 1f);
		float num2 = ((!opened) ? (this._duration * 0.5f) : this._duration);
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOFade(this.CanvasGroup, num, num2), 8);
		this.CanvasGroup.interactable = opened;
		this.CanvasGroup.blocksRaycasts = opened;
		if (!opened)
		{
			this.SetFocused(false);
		}
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this.Content, new Vector2((!opened) ? (-this.Content.rect.width) : 0f, this.Content.anchoredPosition.y), num2, true), 8), delegate
		{
			if (callback != null)
			{
				callback();
			}
		});
	}

	public void ForceClose()
	{
		DOTween.Kill(this.Content, false);
		DOTween.Kill(this.CanvasGroup, false);
		this.CanvasGroup.alpha = 0f;
		this.CanvasGroup.interactable = false;
		this.CanvasGroup.blocksRaycasts = false;
		this.SetFocused(false);
		this.Content.anchoredPosition = new Vector2(-this.Content.rect.width, this.Content.anchoredPosition.y);
		this.Opened = false;
		if (this._callback != null)
		{
			this._callback();
		}
	}

	public bool HasFocus { get; private set; }

	public bool HasContent()
	{
		bool flag = false;
		if (this.Navigation != null)
		{
			this.Navigation.PreheatSelectables();
			flag = new List<Selectable>(this.Navigation.Selectables).Where((Selectable x) => x.gameObject.activeInHierarchy).ToList<Selectable>().Count > 0;
		}
		return flag;
	}

	public virtual void SetFocused(bool active)
	{
		this.HasFocus = active;
		if (this.Hotkeys != null)
		{
			this.Hotkeys.enabled = active;
		}
		if (this.Navigation != null)
		{
			this.Navigation.enabled = active;
		}
	}

	public UINavigation Navigation;

	public CanvasGroup CanvasGroup;

	public HotkeyPressRedirect Hotkeys;

	private float _duration = 0.3f;

	private Action _callback;
}
