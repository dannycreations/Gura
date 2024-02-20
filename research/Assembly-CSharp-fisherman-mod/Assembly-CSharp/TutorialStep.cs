using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStep : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<TutorialEventArgs> StartTriggering;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<TutorialEventArgs> EndTriggering;

	public bool IsActive { get; protected set; }

	public virtual bool IsWaitingHintMessage()
	{
		return false;
	}

	protected virtual void Start()
	{
		if (StaticUserData.CurrentPond.PondId != 2)
		{
			Object.Destroy(base.gameObject);
		}
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
	}

	protected virtual void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
	}

	protected virtual void OnInventoryUpdated()
	{
	}

	protected virtual void Update()
	{
		if ((this.PondActivate == 0 || (StaticUserData.CurrentPond != null && this.PondActivate == StaticUserData.CurrentPond.PondId)) && this.ActivationStepName.Contains(TutorialController.CurrentStep) && this.StarTrigger != null && this.StarTrigger.enabled && this.StarTrigger.IsTriggering() && TutorialController.CurrentStepFinished)
		{
			TutorialController.CurrentStep = this.Name;
			LogHelper.Log("___kocha Name:{0}", new object[] { this.Name });
			if (this.StartTriggering != null)
			{
				this.StartTriggering(this, new TutorialEventArgs
				{
					tutorialStep = this
				});
			}
		}
		if (TutorialController.CurrentStep == this.Name && this.EndTrigger != null && this.EndTrigger.enabled && this.EndTrigger.IsTriggering() && this.EndTriggering != null)
		{
			this.EndTriggering(this, new TutorialEventArgs
			{
				tutorialStep = this
			});
		}
		if (this.StarTrigger != null && this.EndTrigger != null)
		{
			this.StarTrigger.enabled = this.ActivationStepName.Contains(TutorialController.CurrentStep) || TutorialController.CurrentStep == this.Name;
			this.EndTrigger.enabled = this.ActivationStepName.Contains(TutorialController.CurrentStep) || TutorialController.CurrentStep == this.Name;
		}
		if (GameFactory.Player != null && GameFactory.Player.Reel != null && GameFactory.Player.Reel.CurrentReelSpeedSection != 2 && !GameFactory.Player.Reel.IsFightingMode)
		{
			if (GameFactory.Player.Reel.CurrentReelSpeedSection < 2)
			{
				GameFactory.Player.Reel.IncrementReelSpeed(true);
			}
			else
			{
				GameFactory.Player.Reel.DecrementReelSpeed(true);
			}
		}
	}

	public virtual void DoStartAction()
	{
		this.IsActive = true;
	}

	public virtual void DoEndAction()
	{
		this.IsActive = false;
	}

	public virtual void StepUpdate()
	{
	}

	public static ShopMainPageHandler Shop
	{
		get
		{
			if (TutorialStep._shop == null)
			{
				GameObject shopForm = MenuHelpers.Instance.MenuPrefabsList.shopForm;
				TutorialStep._shop = ((!(shopForm != null)) ? null : shopForm.GetComponent<ShopMainPageHandler>());
			}
			return TutorialStep._shop;
		}
	}

	protected Canvas MenuCanvas
	{
		get
		{
			Canvas canvas;
			if ((canvas = this._menuCanvas) == null)
			{
				canvas = (this._menuCanvas = MenuHelpers.Instance.MenuPrefabsList.topMenuForm.GetComponent<Canvas>());
			}
			return canvas;
		}
	}

	protected Toggle ShopToggle
	{
		get
		{
			Toggle toggle;
			if ((toggle = this._shopToggle) == null)
			{
				toggle = (this._shopToggle = DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.Shop).toggle);
			}
			return toggle;
		}
	}

	protected Toggle MapToggle
	{
		get
		{
			Toggle toggle;
			if ((toggle = this._mapToggle) == null)
			{
				toggle = (this._mapToggle = DashboardTabSetter.GetMenuTogglePairByForm(FormsEnum.GlobalMap).toggle);
			}
			return toggle;
		}
	}

	protected GameObject ShopMenuGlow
	{
		get
		{
			GameObject gameObject;
			if ((gameObject = this._shopMenuGlow) == null)
			{
				gameObject = (this._shopMenuGlow = this.ShopToggle.transform.Find("Panel").gameObject);
			}
			return gameObject;
		}
	}

	protected GameObject MapMenuGlow
	{
		get
		{
			GameObject gameObject;
			if ((gameObject = this._mapMenuGlow) == null)
			{
				gameObject = (this._mapMenuGlow = this.MapToggle.transform.Find("Panel").gameObject);
			}
			return gameObject;
		}
	}

	public void SetSelectablesForSkip(List<Selectable> selectablesForSkip)
	{
		this.SelectablesForSkip = selectablesForSkip;
	}

	public string Title;

	public string Name;

	public List<string> ActivationStepName;

	public StartTutorialTriggerContainer StarTrigger;

	public EndTutorialTriggerContainer EndTrigger;

	public string Message;

	public string ShortMessage;

	public Sprite Image;

	public InputControlType ShortControlIcon;

	public InputControlType BigControlIcon;

	public bool AppendIconToBigMessage;

	public bool UseIconBigMessage;

	public bool AppendIconToSmallMessage;

	public bool UseIconSmallMessage;

	public ShortMessagePosition ShortMessagePosition;

	public float TimeShowingFullMessage = 1f;

	public int PondActivate;

	public string ButtonText;

	public Font ButtonFont;

	public FontStyle ButtonTextStyle;

	public bool IsButtonTextUppercase;

	public int ButtonTextSize;

	public int Right;

	public int Left;

	protected static ShopMainPageHandler _shop;

	protected List<Selectable> SelectablesForSkip = new List<Selectable>();

	protected Canvas _menuCanvas;

	protected Toggle _shopToggle;

	protected Toggle _mapToggle;

	protected GameObject _shopMenuGlow;

	protected GameObject _mapMenuGlow;
}
