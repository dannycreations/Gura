using System;
using System.Diagnostics;
using ObjectModel;
using UnityEngine;

namespace Missions
{
	public class MissionsController : MonoBehaviour
	{
		public void StartMission(int id)
		{
			bool flag = true;
			if (id != 1)
			{
				if (id != 2)
				{
					LogHelper.Error("Unsupported mission id {0}", new object[] { id });
					flag = false;
				}
				else
				{
					this._currentMission = new Mission2();
				}
			}
			else
			{
				this._currentMission = new Mission1();
			}
			if (flag)
			{
				LogHelper.Log("Mission {0} started", new object[] { id });
				this._currentMissionId = id;
				this._currentMission.Init(this);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> EMissionFinished = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> EHighlightSomething = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int, bool> EHighlightSomething2 = delegate
		{
		};

		private void Awake()
		{
			if (MissionsController.Instance == null)
			{
				Object.DontDestroyOnLoad(this);
				MissionsController.Instance = this;
			}
			else
			{
				LogHelper.Error("More than 1 instance of MissionsController found!", new object[0]);
			}
		}

		private void Update()
		{
			if (this._currentMission != null && this._currentMission.Update())
			{
				this.EMissionFinished(this._currentMissionId);
				this._currentMissionId = -1;
				this._currentMission = null;
			}
		}

		public GameScreenRecord GameScreen
		{
			get
			{
				return this._gameScreen;
			}
		}

		public GameElementRecord GameElement
		{
			get
			{
				return this._gameElement;
			}
		}

		public bool[] GameSwitches
		{
			get
			{
				return this._gameSwitches;
			}
		}

		public int[] GameIndicators
		{
			get
			{
				return this._gameIndicators;
			}
		}

		public GameActionType GameActionType
		{
			get
			{
				return this._gameActionType;
			}
		}

		public void ChangeGameScreen(GameScreenType gameScreen, GameScreenTabType gameScreenTab = GameScreenTabType.Undefined, int? categoryId = null, int? itemId = null, int[] childCategoryIds = null, string categoryElementId = null, string[] categoryElementsPath = null)
		{
			this._gameScreen = new GameScreenRecord(gameScreen, gameScreenTab, categoryId, itemId, childCategoryIds, categoryElementId, categoryElementsPath);
		}

		public void ChangeSelectedElement(GameElementType type, string value, int? itemId = null)
		{
			this._gameElement = new GameElementRecord(type, value, itemId);
		}

		public void ChangeSwitch(GameSwitchType type, bool value)
		{
			this._gameSwitches[(int)type] = value;
		}

		public void ChangeIndicator(GameIndicatorType type, int value)
		{
			this._gameIndicators[(int)type] = value;
		}

		public void SendGameAction(GameActionType type)
		{
			this._gameActionType = type;
		}

		public void HighlightSomething(string arg)
		{
			this.EHighlightSomething(arg);
		}

		public void HighlightSomething2(int id, bool flag)
		{
			this.EHighlightSomething2(id, flag);
		}

		public static MissionsController Instance;

		private BaseMission _currentMission;

		private int _currentMissionId = -1;

		private GameScreenRecord _gameScreen;

		private GameElementRecord _gameElement;

		private bool[] _gameSwitches = new bool[13];

		private int[] _gameIndicators = new int[5];

		private GameActionType _gameActionType;
	}
}
