using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
	public static ScreenManager Instance { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<GameScreenType> OnScreenChanged = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnTransfer = delegate
	{
	};

	public bool IsTransfer { get; private set; }

	public GameScreenType GameScreen
	{
		get
		{
			return this._currentGameScreen;
		}
	}

	public GameScreenType GameScreenPrev
	{
		get
		{
			return this._prevGameScreen;
		}
	}

	public bool IsIn3D
	{
		get
		{
			return ScreenManager.Game3DScreens.Contains(this.GameScreen);
		}
	}

	private void Awake()
	{
		ScreenManager.Instance = this;
		this.IsTransfer = false;
	}

	public void SetGameScreen(GameScreenType gameScreen)
	{
		if (this._currentGameScreen != gameScreen)
		{
			this._prevGameScreen = this._currentGameScreen;
			this._currentGameScreen = gameScreen;
			this.OnScreenChanged(this._currentGameScreen);
		}
	}

	public void SetTransfer(bool flag)
	{
		this.IsTransfer = flag;
		this.OnTransfer(flag);
	}

	public static void OpenMissions()
	{
		if (GameFactory.Player != null && GameFactory.Player.IsTPMInitialized)
		{
			KeysHandlerAction.MissionsHandler();
		}
		Transform transform = MenuHelpers.Instance.GetFormByName(FormsEnum.TopDashboard).transform.Find("Image");
		Toggle component = transform.Find("TopMenuLeft").Find("btnMissions (1)").GetComponent<Toggle>();
		component.group.SetAllTogglesOff();
		component.isOn = true;
	}

	public static readonly IList<GameScreenType> Game3DScreens = new ReadOnlyCollection<GameScreenType>(new List<GameScreenType>
	{
		GameScreenType.Game,
		GameScreenType.Map,
		GameScreenType.ExtendTime,
		GameScreenType.Time,
		GameScreenType.PhotoMode,
		GameScreenType.BoatRent
	});

	private GameScreenType _currentGameScreen;

	private GameScreenType _prevGameScreen;
}
