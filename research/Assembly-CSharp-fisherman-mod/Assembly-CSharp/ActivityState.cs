using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ActivityState : MonoBehaviour
{
	public bool isActive { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnStart;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnShow;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action HideEvent;

	public static ActivityState GetParentActivityState(Transform tr)
	{
		if (tr == null)
		{
			return null;
		}
		for (int i = 0; i < ActivityState.AllStates.Count; i++)
		{
			if (ActivityState.AllStates[i] != null && tr.IsChildOf(ActivityState.AllStates[i].transform))
			{
				return ActivityState.AllStates[i];
			}
		}
		return null;
	}

	private void Awake()
	{
		this._setupCamera = base.GetComponent<SetupGUICamera>();
		if (base.GetComponent<CanvasGroup>() == null)
		{
			this._canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		if (!ActivityState.AllStates.Contains(this))
		{
			ActivityState.AllStates.Add(this);
		}
		this.overrideSortCanvases = new List<Canvas>(base.GetComponentsInChildren<Canvas>()).Where((Canvas x) => x.overrideSorting).ToList<Canvas>();
	}

	private void OnDestroy()
	{
		if (ActivityState.AllStates.Contains(this))
		{
			ActivityState.AllStates.Remove(this);
		}
	}

	private RectTransform RectTransform
	{
		get
		{
			if (this._rectTransform == null)
			{
				this._rectTransform = this.ActionObject.GetComponent<RectTransform>();
			}
			return this._rectTransform;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			if (this._canvasGroup == null)
			{
				this._canvasGroup = base.GetComponent<CanvasGroup>();
			}
			return this._canvasGroup;
		}
	}

	private void Update()
	{
		if (this.ActionObject == null || this._stop)
		{
			return;
		}
		if (this._isShowing)
		{
			this._alpha += 4f * Time.deltaTime;
			if (this._alpha >= 1f)
			{
				this.FinalizeShow();
			}
		}
		else
		{
			this._alpha -= 4f * Time.deltaTime;
			if (this._alpha <= 0f)
			{
				this.FinalizeHide();
			}
		}
		if (this.isActive)
		{
			this.CanvasGroup.alpha = this._alpha;
		}
	}

	private void FinalizeShow()
	{
		this._stop = true;
		this.CanRun = true;
		this._alpha = 1f;
		this.CanvasGroup.alpha = this._alpha;
		this.CanvasGroup.interactable = true;
		this.CanvasGroup.blocksRaycasts = true;
		this.RectTransform.localPosition = this.BasePosition;
		if (this.OnShow != null)
		{
			this.OnShow();
		}
		if (this.OnStart != null)
		{
			this.OnStart();
		}
		if (this.FormType == FormsEnum.TopDashboard)
		{
			StaticUserData.IsShowDashboard = true;
		}
	}

	private void FinalizeHide()
	{
		this._stop = true;
		this.isActive = false;
		this._alpha = 0f;
		this.CanvasGroup.alpha = this._alpha;
		this.CanvasGroup.interactable = false;
		this.CanvasGroup.blocksRaycasts = false;
		for (int i = 0; i < this.overrideSortCanvases.Count; i++)
		{
			this.overrideSortCanvases[i].overrideSorting = false;
		}
		this.RectTransform.localPosition = this.BasePosition;
		if (this.FormType == FormsEnum.TopDashboard)
		{
			StaticUserData.IsShowDashboard = false;
		}
	}

	public void SetupCamera()
	{
		if (this._setupCamera != null)
		{
			this._setupCamera.Setup();
		}
	}

	public void Show(bool immediate = false)
	{
		this.isActive = true;
		this._isShowing = true;
		if (!ActivityState.AllStates.Contains(this))
		{
			this.Awake();
		}
		for (int i = 0; i < this.overrideSortCanvases.Count; i++)
		{
			this.overrideSortCanvases[i].overrideSorting = true;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		if (StaticUserData.CurrentForm == this && this.IsMainForm)
		{
			this.FinalizeShow();
			return;
		}
		if (this.FormType != FormsEnum.TopDashboard)
		{
			if (this.IsMainForm)
			{
				StaticUserData.CurrentForm = this;
			}
		}
		else
		{
			StaticUserData.IsShowDashboard = true;
		}
		if (immediate)
		{
			this.FinalizeShow();
		}
		else
		{
			this.RectTransform.localPosition = this.BasePosition;
			this._alpha = 0f;
			this.CanvasGroup.alpha = this._alpha;
			this.CanvasGroup.interactable = true;
			this.CanvasGroup.blocksRaycasts = true;
			if (this.OnShow != null)
			{
				this.OnShow();
			}
			this._stop = false;
		}
	}

	public void Hide(bool immediate = false)
	{
		if (base.GetComponent<CanvasGroup>() == null)
		{
			this.Awake();
		}
		if (this.HideEvent != null)
		{
			this.HideEvent();
		}
		this._isShowing = false;
		this.CanvasGroup.interactable = false;
		this.CanvasGroup.blocksRaycasts = false;
		if (immediate)
		{
			this.FinalizeHide();
		}
		else
		{
			this.isActive = true;
			if (this.FormType == FormsEnum.TopDashboard)
			{
				StaticUserData.IsShowDashboard = false;
			}
			this.CanvasGroup.alpha = 1f;
			this._alpha = 1f;
			this._stop = false;
		}
	}

	public GameObject ActionObject;

	public Vector3 StartPosition;

	public Vector3 BasePosition;

	public Vector3 HidePosition;

	private const float Speed = 4f;

	[HideInInspector]
	public bool CanRun;

	public FormsEnum FormType;

	public bool WithoutDisabling;

	public bool IsMainForm = true;

	private bool _isShowing;

	private bool _stop = true;

	private RectTransform _rectTransform;

	private CanvasGroup _canvasGroup;

	private float _alpha;

	public static List<ActivityState> AllStates = new List<ActivityState>();

	[HideInInspector]
	public string PrevPanelName;

	[HideInInspector]
	public string NextPanelName;

	private List<Canvas> overrideSortCanvases;

	private SetupGUICamera _setupCamera;
}
