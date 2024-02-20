using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class MessageBoxBase : MonoBehaviour
{
	public AlphaFade AlphaFade
	{
		get
		{
			return this._alphaFade;
		}
	}

	private void ShowCalled()
	{
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockInput(this.ControlsActionsToUse);
	}

	protected void EnsureMessageRemovedFromQueue()
	{
		if (MessageFactory.MessageBoxQueue.Contains(this))
		{
			MessageFactory.MessageBoxQueue.Remove(this);
		}
	}

	protected virtual void Awake()
	{
		this._alphaFade = base.GetComponent<AlphaFade>();
		if (this._alphaFade != null)
		{
			this._alphaFade.FastHidePanel();
			this._alphaFade.OnShowCalled.AddListener(new UnityAction(this.ShowCalled));
			this._alphaFade.HideFinished += this.MessageBoxBase_HideFinished;
			this._blockInputListenersAdded = true;
		}
	}

	public void SetInteractable(bool value)
	{
		this._alphaFade.CanvasGroup.interactable = value;
	}

	public void Open()
	{
		this._alphaFade.ShowPanel();
		this.EnsureMessageRemovedFromQueue();
		MessageBoxList.Instance.VerifyMessageSetToCurrent(this);
	}

	public virtual void Close()
	{
		this.SetInteractable(false);
		this._alphaFade.HidePanel();
	}

	public virtual void CloseFast()
	{
		this.SetInteractable(false);
		this.HideFast();
	}

	public void HideFast()
	{
		this._alphaFade.FastHidePanel();
		this._alphaFade.FinishHideInvokes();
	}

	public void OpenFast()
	{
		this._alphaFade.FastShowPanel();
		this.EnsureMessageRemovedFromQueue();
	}

	internal virtual void Start()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void MessageBoxBase_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this.EnsureMessageRemovedFromQueue();
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
		this.AfterFullyHidden.Invoke();
		Object.Destroy(base.gameObject);
	}

	protected virtual void OnDestroy()
	{
		this.AfterFullyHidden.RemoveAllListeners();
		if (this._alphaFade != null && this._blockInputListenersAdded)
		{
			this._alphaFade.OnShowCalled.RemoveListener(new UnityAction(this.ShowCalled));
			this._alphaFade.HideFinished -= this.MessageBoxBase_HideFinished;
		}
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	protected virtual void OnInputTypeChanged(InputModuleManager.InputType type)
	{
	}

	public static string GetGameObjectPath(Transform transform)
	{
		string text = transform.name;
		while (transform.parent != null)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return text;
	}

	[HideInInspector]
	public bool OnPriority;

	public UnityEvent AfterFullyHidden = new UnityEvent();

	protected AlphaFade _alphaFade;

	private bool _blockInputListenersAdded;

	protected List<string> ControlsActionsToUse;
}
