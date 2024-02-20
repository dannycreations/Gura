using System;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;

public class ManagedHintObject : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ManagedHintObject> OnDestroyed = delegate
	{
	};

	public bool Displayed
	{
		get
		{
			if (this.check == null)
			{
				return this.observer == null || this.observer.CanShow(this.inhintId);
			}
			return this.check() && (this.observer == null || this.observer.CanShow(this.inhintId));
		}
	}

	public virtual void SetObserver(ManagedHint observer, int id)
	{
		this.observer = observer;
		this.inhintId = id;
		if (!this.Displayed)
		{
			this.lastCheck = false;
			this.Hide();
		}
		else
		{
			this.lastCheck = true;
		}
	}

	protected virtual void Show()
	{
		if (this.ShowFunc != null)
		{
			this.ShowFunc();
		}
		this.shouldShow = true;
		if (this.group != null)
		{
			ShortcutExtensions.DOFade(this.group, 1f, 0.2f);
		}
	}

	protected virtual void Hide()
	{
		this.shouldShow = false;
		if (this.group != null)
		{
			ShortcutExtensions.DOFade(this.group, 0f, 0.2f);
		}
	}

	protected virtual void Update()
	{
		if (!this._isUpdateable)
		{
			return;
		}
		if (this.baseHintColor != HintSystem.BaseHintsColor)
		{
			this.baseHintColor = HintSystem.BaseHintsColor;
			foreach (Transform transform in this.objectsToSetColor)
			{
				transform.GetComponent<Renderer>().material.color = this.baseHintColor;
			}
		}
		if (this._skipServerScale && base.transform.localScale != Vector3.one)
		{
			base.transform.localScale = Vector3.one;
		}
		bool displayed = this.Displayed;
		if ((!this.lastCheck || !this.checkedOnce) && displayed)
		{
			this.Show();
			this.checkedOnce = true;
		}
		if ((this.lastCheck || !this.checkedOnce) && !displayed)
		{
			this.Hide();
			this.checkedOnce = true;
		}
		this.lastCheck = displayed;
	}

	public void SetFilterFunction(ManagedHint.FilterFunction ff, ManagedHint.FilterFunction showFunc = null)
	{
		this.ShowFunc = showFunc;
		if (ff == null)
		{
			return;
		}
		this.check = ff;
		if (!this.Displayed)
		{
			this.lastCheck = false;
			this.Hide();
		}
		else
		{
			this.lastCheck = true;
		}
	}

	protected virtual void OnEnable()
	{
		this.BeenActive = true;
	}

	protected virtual void OnDestroy()
	{
		this.shouldShow = false;
		if (this.observer != null)
		{
			this.observer.ElementRemoved(this.inhintId);
		}
		this.inhintId = -1;
		this.OnDestroyed(this);
	}

	public void RemoveManual()
	{
		if (this.observer != null)
		{
			this.observer.ElementRemoved(this.inhintId);
		}
	}

	[SerializeField]
	protected CanvasGroup group;

	[SerializeField]
	protected bool _skipServerScale;

	[SerializeField]
	protected bool _isUpdateable = true;

	protected ManagedHint observer;

	protected int inhintId = -1;

	protected bool shouldShow;

	protected ManagedHint.FilterFunction check;

	protected ManagedHint.FilterFunction ShowFunc;

	protected bool lastCheck;

	protected bool checkedOnce;

	[SerializeField]
	protected Color baseHintColor;

	[SerializeField]
	protected Transform[] objectsToSetColor;

	public bool BeenActive;
}
