using System;
using Assets.Scripts._3D.Game.MissionObjects;
using ObjectModel;
using UnityEngine;

public class MOControllerBase : MonoBehaviour
{
	public string ResourceKey { get; protected set; }

	public bool IsHideOnStart
	{
		get
		{
			return this._hideOnStart;
		}
	}

	protected virtual void Awake()
	{
		this.ActionsController.SetParentTransform(base.transform);
		this.Init();
	}

	protected virtual void Start()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		this.OnInputTypeChanged(InputModuleManager.InputType.GamePad);
	}

	protected virtual void OnDestroy()
	{
		this.OnInteractiveObjectHidePanelHidden();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public virtual void Init()
	{
		if (string.IsNullOrEmpty(this.ResourceKey))
		{
			this.ResourceKey = "@" + base.gameObject.name;
			this.Animator = base.GetComponent<Animator>();
		}
	}

	public virtual void Activate(bool flag)
	{
		LogHelper.Log("___kocha MOControllerBase.Activate({0}, {1})", new object[] { this.ResourceKey, flag });
		base.gameObject.SetActive(flag);
		if (!flag)
		{
			this.OnInteractiveObjectHidePanelHidden();
		}
		if (this.IsShowing)
		{
			this.IsShowing = false;
			ShowHudElements.Instance.InteractiveObjectHidePanel(false);
		}
	}

	public virtual void OnChanged(MissionInteractiveObject obj)
	{
		LogHelper.Log("___kocha MOControllerBase.OnChanged({0}, {1})", new object[] { this.ResourceKey, obj.CurrentState });
		this.Context = obj;
		this.Activate(!string.IsNullOrEmpty(obj.CurrentState) && obj.CurrentState != "Hidden");
		if (this.Animator != null && obj.CurrentState != "Transparent")
		{
			this.Animator.SetTrigger(obj.CurrentState);
		}
		if (this._statesActions != null)
		{
			for (int i = 0; i < this._statesActions.Length; i++)
			{
				StateActions stateActions = this._statesActions[i];
				if (stateActions.StateName == obj.CurrentState)
				{
					LogHelper.Log("___kocha MOControllerBase.RunActions({0}, {1})", new object[] { this.ResourceKey, obj.CurrentState });
					this.IsActionsActive = true;
					this.ActionsController.RunActions(stateActions.Actions, delegate
					{
						this.IsActionsActive = false;
					});
					break;
				}
			}
		}
	}

	public virtual void OnInteracted(MissionInteractiveObject obj, MissionInteractiveObject.AllowedInteraction interaction)
	{
		this.IsActionsActive = true;
		this.ActionsController.RunActions(this._interactedActions, delegate
		{
			this.IsActionsActive = false;
		});
		string text = (string.IsNullOrEmpty(interaction.InteractedMessage) ? " " : interaction.InteractedMessage);
		LogHelper.Log("___kocha MOControllerBase.OnInteracted Name:{0} message:{1}", new object[] { interaction.Name, text });
		this.IsInteractiveMsgActive = true;
		ShowHudElements.Instance.OnInteractiveObjectHidePanelHidden += this.OnInteractiveObjectHidePanelHidden;
		ShowHudElements.Instance.InteractiveObjectShowPanelForTime(text, 0.5f);
		if (this._animateOnInteraction && this.Animator != null)
		{
			this.Animator.SetTrigger(obj.CurrentState);
		}
	}

	public virtual void OnInteractionChanged(MissionInteractiveObject obj, string interaction, bool added)
	{
		LogHelper.Log("___kocha MOControllerBase.OnInteractionChanged({0}, {1}, {2})", new object[] { this.ResourceKey, interaction, added });
		this.Context = obj;
		if (!added)
		{
			this.IsShowing = false;
		}
	}

	public virtual bool IsFocused()
	{
		if (GameFactory.Player == null)
		{
			return false;
		}
		Vector3 vector = base.transform.position - GameFactory.Player.Position;
		if (vector.sqrMagnitude < this._distToInteract * this._distToInteract)
		{
			Transform transform = GameFactory.Player.CameraController.Camera.transform;
			Vector2 vector2;
			vector2..ctor(transform.forward.x, transform.forward.z);
			Vector2 normalized = vector2.normalized;
			Vector2 vector3;
			vector3..ctor(vector.x, vector.z);
			Vector2 normalized2 = vector3.normalized;
			float num = Vector2.Dot(normalized, normalized2);
			return num > Mathf.Cos(this._angleToInteract * 0.017453292f);
		}
		return false;
	}

	public virtual bool IsInteractionCue()
	{
		return this._isAutoInteraction || ControlsController.ControlsActions.InteractObject.WasReleased;
	}

	protected virtual void Update()
	{
		bool flag = false;
		if (this.Context != null && this.Context.CurInteraction != null)
		{
			if (this._interactionBlockedDelay > 0f)
			{
				this._interactionBlockedDelay -= Time.deltaTime;
			}
			bool flag2 = this.Context.CurInteraction.ExecutionNextTime != null && this.Context.CurInteraction.ExecutionNextTime > TimeHelper.UtcTime();
			if (this.IsWaitingTime4Actions != flag2)
			{
				this.IsWaitingTime4Actions = flag2;
				if (!flag2)
				{
					LogHelper.Log("MOControllerBase:ExecutionNextTime >> Reset {0}", new object[] { this.Context.ResourceKey });
					this.IsActionsActive = true;
					this.ActionsController.RunActions(this._executionNextTimeFinoshedActions, delegate
					{
						this.IsActionsActive = false;
					});
				}
			}
			if (this.IsFocused())
			{
				if (this.IsWaitingTime != flag2)
				{
					this.IsWaitingTime = flag2;
					if (!this.IsWaitingTime)
					{
						this.IsShowing = false;
					}
				}
				if (this.IsInteractionCue() && !this.IsWaitingTime && !this.IsActionsActive && !this.Context.CurInteraction.IsDisabled && this._interactionBlockedDelay < 0f)
				{
					this._interactionBlockedDelay = 0.5f;
					this.OnInteract();
				}
				if (!this.IsShowing || this.IsWaitingTime)
				{
					this.IsShowing = true;
					this.Message = string.Empty;
					if (!this.IsWaitingTime)
					{
						if (this.Context.CurInteraction.IsDisabled)
						{
							this.Message = this.Context.CurInteraction.BeforeMessage;
						}
						else
						{
							this.Message = string.Format("{0} {1}", this.Context.CurInteraction.BeforeMessage, this.Colored(this.InteractObjectIco));
						}
					}
					else
					{
						TimeSpan timeSpan = this.Context.CurInteraction.ExecutionNextTime.Value - TimeHelper.UtcTime();
						string formated = timeSpan.GetFormated(true, true);
						this.Message = string.Format("{0} {1}", this.Context.CurInteraction.AlternateMessage, this.Colored(formated));
					}
					if (!string.IsNullOrEmpty(this.Message) && !this.IsInteractiveMsgActive && !this._isAutoInteraction)
					{
						ShowHudElements.Instance.InteractiveObjectShowPanel(this.Message);
					}
				}
				flag = true;
				ShowHudElements.Instance.InteractiveObjectChange((!this.IsActionsActive) ? this.Message : string.Empty);
			}
		}
		if (!flag && this.IsShowing)
		{
			this.IsShowing = false;
			ShowHudElements.Instance.InteractiveObjectHidePanel(false);
		}
	}

	protected virtual void OnInteract()
	{
		LogHelper.Log("MOControllerBase.Interaction with object {0}", new object[] { this.Context.ResourceKey });
		ClientMissionsManager.Instance.Interact(this.Context.MissionId, this.Context.ResourceKey, this.Context.CurInteraction.Name, null);
	}

	protected void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		bool flag;
		this.InteractObjectIco = HotkeyIcons.GetIcoByActionName("InteractObject", out flag);
		if (flag)
		{
			this.InteractObjectIco = string.Format("[{0}]", this.InteractObjectIco);
		}
		this.IsShowing = false;
	}

	protected string Colored(string s)
	{
		return string.Format("<color=#F79A44FF>{0}</color>", s);
	}

	protected void OnInteractiveObjectHidePanelHidden()
	{
		if (ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.OnInteractiveObjectHidePanelHidden -= this.OnInteractiveObjectHidePanelHidden;
		}
		this.IsShowing = false;
		this.IsInteractiveMsgActive = false;
	}

	[SerializeField]
	private float _distToInteract = 3f;

	[SerializeField]
	private float _angleToInteract = 45f;

	[SerializeField]
	private bool _animateOnInteraction = true;

	[SerializeField]
	private bool _hideOnStart = true;

	[SerializeField]
	private ActionsSettings _interactedActions;

	[SerializeField]
	private ActionsSettings _executionNextTimeFinoshedActions;

	[SerializeField]
	private StateActions[] _statesActions;

	[SerializeField]
	private bool _isAutoInteraction;

	protected MissionInteractiveObject Context;

	protected bool IsShowing;

	protected Animator Animator;

	protected string InteractObjectIco;

	protected bool IsWaitingTime;

	protected bool IsWaitingTime4Actions;

	protected bool IsInteractiveMsgActive;

	protected bool IsActionsActive;

	private float _interactionBlockedDelay = -1f;

	protected string Message = string.Empty;

	protected MOActionsController ActionsController = new MOActionsController();
}
