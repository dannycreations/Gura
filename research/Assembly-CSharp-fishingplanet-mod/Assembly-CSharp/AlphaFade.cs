using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AlphaFade : MonoBehaviour
{
	public bool IsHiding
	{
		get
		{
			return this._isHiding;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			return this._canvasGroup;
		}
	}

	public bool IsShow
	{
		get
		{
			return this._isShow;
		}
	}

	public float Alpha
	{
		get
		{
			return this._canvasGroup.alpha;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ShowFinished;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgsAlphaFade> HideFinished;

	public bool IsShowing
	{
		get
		{
			return this._isShowing;
		}
	}

	protected virtual void Start()
	{
		if (this._isShow && this.OnShow != null)
		{
			this.OnShow.Invoke();
		}
	}

	protected virtual void Awake()
	{
		if (this.BlureBehind != null)
		{
			this.CachedMaterial = this.BlureBehind.material;
		}
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		this._isShow = this._canvasGroup.alpha >= 1f;
	}

	protected virtual void Update()
	{
		if (this._isShowing)
		{
			if (this._currentShowTime == 0f && this.OpenClip != null)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(this.OpenClip, SettingsManager.InterfaceVolume);
			}
			this._currentShowTime += Time.deltaTime;
			float num = this._currentShowTime / this.ShowFadeTime;
			this._canvasGroup.alpha = num;
			if (this._canvasGroup.alpha >= 1f)
			{
				if (this.ShowFinished != null)
				{
					this.ShowFinished(this, new EventArgs());
				}
				this._canvasGroup.alpha = 1f;
				this._isShow = true;
				this._isShowing = false;
				this._isHiding = false;
				if (this.IsDisabled)
				{
					base.gameObject.SetActive(true);
				}
				if (this.OnShow != null)
				{
					this.OnShow.Invoke();
				}
			}
		}
		else if (this._isHiding)
		{
			if (this._currentHideTime == 0f && this.CloseClip != null)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(this.CloseClip, SettingsManager.InterfaceVolume);
			}
			this._currentHideTime += Time.deltaTime;
			float num2 = this._currentHideTime / this.HideFadeTime;
			this._canvasGroup.alpha = 1f - num2;
			if (this._canvasGroup.alpha <= 0f)
			{
				this._isShow = false;
				this._isHiding = false;
				this._isShowing = false;
				if (this.IsDisabled)
				{
					base.gameObject.SetActive(false);
				}
				this.FinishHideInvokes();
			}
		}
		this.UpdateBlurInOut();
	}

	public virtual void ShowPanel()
	{
		if (this._canvasGroup == null)
		{
			this.Awake();
		}
		if (!this.IsShowing && !this._isShow)
		{
			this._currentShowTime = 0f;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		if (this.GeometryFadePanel != null)
		{
			this.GeometryFadePanel.ShowPanel();
		}
		this._isHiding = false;
		this._isShowing = true;
		if (this.OnShowCalled != null)
		{
			this.OnShowCalled.Invoke();
		}
		if (this.SetInteractable)
		{
			this._canvasGroup.interactable = true;
		}
		if (this._canvasGroup.alpha >= 1f)
		{
			return;
		}
		if (this.BlockRaycastOnShow)
		{
			if (this._canvasGroup == null)
			{
				this._canvasGroup = base.GetComponent<CanvasGroup>();
			}
			this._canvasGroup.blocksRaycasts = true;
		}
	}

	public virtual void HidePanel()
	{
		if (this._canvasGroup == null)
		{
			this.Awake();
		}
		if (this.GeometryFadePanel != null)
		{
			this.GeometryFadePanel.HidePanel();
		}
		if (this.SetInteractable)
		{
			this._canvasGroup.interactable = false;
		}
		if (this.BlockRaycastOnShow)
		{
			this._canvasGroup.blocksRaycasts = false;
		}
		if (!this._isHiding)
		{
			this._currentHideTime = 0f;
		}
		if (this._canvasGroup.alpha <= 0f)
		{
			this._currentHideTime = this.HideFadeTime;
		}
		this._isShowing = false;
		this._isHiding = true;
	}

	public void FastHidePanel()
	{
		if (this._canvasGroup == null)
		{
			this.Awake();
		}
		if (this.GeometryFadePanel != null)
		{
			this.GeometryFadePanel.FastHidePanel();
		}
		if (this.SetInteractable)
		{
			this._canvasGroup.interactable = false;
		}
		if (this.BlockRaycastOnShow)
		{
			this._canvasGroup.blocksRaycasts = false;
		}
		this._canvasGroup.alpha = 0f;
		this._isShow = false;
		this._isShowing = false;
		this._isHiding = false;
		if (this.IsDisabled)
		{
			base.gameObject.SetActive(false);
		}
	}

	public virtual void FinishHideInvokes()
	{
		if (this.OnHide != null)
		{
			this.OnHide.Invoke();
		}
		if (this.HideFinished != null)
		{
			this.HideFinished(base.gameObject, new EventArgsAlphaFade
			{
				gameObject = base.gameObject
			});
		}
	}

	public virtual void FastShowPanel()
	{
		if (this._canvasGroup == null)
		{
			this.Awake();
		}
		if (this.GeometryFadePanel != null)
		{
			this.GeometryFadePanel.FastShowPanel();
		}
		base.gameObject.SetActive(true);
		if (this.OnShowCalled != null)
		{
			this.OnShowCalled.Invoke();
		}
		if (this.SetInteractable)
		{
			this._canvasGroup.interactable = true;
		}
		if (this.BlockRaycastOnShow)
		{
			this._canvasGroup.blocksRaycasts = true;
		}
		this._canvasGroup.alpha = 1f;
		this._isShow = true;
		this._isShowing = false;
		this._isHiding = false;
		if (this.IsDisabled)
		{
			base.gameObject.SetActive(true);
		}
	}

	public void Destroy()
	{
		Object.Destroy(this);
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void UpdateBlurInOut()
	{
		if (this.BlureBehind == null)
		{
			return;
		}
		if (Math.Abs(this.AlphaBlurInOut - this._canvasGroup.alpha) > 0.001f)
		{
			this.AlphaBlurInOut = this._canvasGroup.alpha;
			this.BlureBehind.material = ((this.AlphaBlurInOut < 0.8f) ? null : this.CachedMaterial);
		}
	}

	public float ShowFadeTime = 0.5f;

	public float HideFadeTime = 0.5f;

	public bool BlockRaycastOnShow;

	public bool SetInteractable;

	public bool IsDisabled;

	private bool _isShowing;

	private bool _isHiding;

	private float _currentHideTime;

	private float _currentShowTime;

	private bool _isShow;

	private CanvasGroup _canvasGroup;

	public UnityEvent OnShowCalled = new UnityEvent();

	public UnityEvent OnShow = new UnityEvent();

	public UnityEvent OnHide = new UnityEvent();

	public AudioClip OpenClip;

	public AudioClip CloseClip;

	public GeometryFade GeometryFadePanel;

	[SerializeField]
	protected Graphic BlureBehind;

	protected Material CachedMaterial;

	protected float AlphaBlurInOut;
}
