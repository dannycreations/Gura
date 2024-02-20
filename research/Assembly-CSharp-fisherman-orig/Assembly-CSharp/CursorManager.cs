using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
	private Texture2D StandartCursor
	{
		get
		{
			return this.standartCursor;
		}
	}

	public bool MouseCursor
	{
		get
		{
			return this.mouseCursor;
		}
		set
		{
			this.mouseCursor = value;
			if (this.mouseCursor)
			{
				Cursor.SetCursor(this.activeCursor, this.CursorOffset, 0);
				Cursor.lockState = GamePadCursor.lockState;
				Cursor.visible = GamePadCursor.visible;
				GamePadCursor.visible = false;
			}
			else
			{
				GamePadCursor.SetCursor(this.activeCursor, this.CursorOffset);
				GamePadCursor.lockState = Cursor.lockState;
				GamePadCursor.visible = Cursor.visible;
				Cursor.lockState = 1;
				Cursor.visible = false;
			}
		}
	}

	public static CursorManager Instance
	{
		get
		{
			return CursorManager._instance;
		}
	}

	private void Awake()
	{
		CursorManager._instance = this;
		this.activeCursor = this.StandartCursor;
		bool flag = SettingsManager.InputType == InputModuleManager.InputType.Mouse;
		if (this.MouseCursor != flag)
		{
			this.MouseCursor = flag;
		}
	}

	public void SetCursor(CursorType cursorType)
	{
		switch (cursorType)
		{
		case CursorType.Standart:
			this.activeCursor = this.StandartCursor;
			break;
		case CursorType.Selecting:
			this.activeCursor = this.selectCursor;
			break;
		case CursorType.Loading:
			this.activeCursor = this.loadingCursor;
			break;
		case CursorType.MapMark:
			this.activeCursor = this.mapCanMark;
			break;
		case CursorType.MapCantMark:
			this.activeCursor = this.mapCantMark;
			break;
		}
		this._currentCursorType = cursorType;
		if (this.MouseCursor)
		{
			Vector2 vector = ((cursorType != CursorType.MapMark && cursorType != CursorType.MapCantMark) ? this.CursorOffset : new Vector2((float)this.activeCursor.width / 2f, (float)this.activeCursor.height / 2f));
			Cursor.SetCursor(this.activeCursor, vector, 0);
		}
		else
		{
			GamePadCursor.SetCursor(this.activeCursor, this.CursorOffset);
		}
	}

	private Vector2 CursorOffset
	{
		get
		{
			return this._cursorOffset;
		}
	}

	public CursorType GetCursor
	{
		get
		{
			return this._currentCursorType;
		}
		private set
		{
			this._currentCursorType = value;
		}
	}

	public void BlockForCtrl()
	{
		CursorManager.ShowCursor();
		CursorManager.BlockFPS();
		this.IsCtrlBLocked = true;
	}

	public void UnblockForCtrl()
	{
		this.IsCtrlBLocked = false;
		if (CursorManager._instance._showCursorNumerator >= 0)
		{
			CursorManager.HideCursor();
		}
		if (CursorManager._instance._blockFPSNumerator > 0)
		{
			CursorManager.UnBlockFPS();
		}
	}

	private bool IgnoreCtrl
	{
		get
		{
			return CursorManager.IsModalWindow() || CursorManager.IsRewindHoursActive || (ShowHudElements.Instance != null && ShowHudElements.Instance.IsEquipmentChangeBusy());
		}
	}

	private void Update()
	{
		this.isPressed = false;
		if (ControlsController.ControlsActions == null)
		{
			return;
		}
		this.isPressed = ControlsController.ControlsActions.ShowCursor.IsPressedMandatory;
		if (ControlsController.ControlsActions.ShowCursor.WasPressedMandatory && !this.IsCtrlBLocked)
		{
			if (this.IgnoreCtrl)
			{
				return;
			}
			this.BlockForCtrl();
		}
		else if (ControlsController.ControlsActions.ShowCursor.WasReleasedMandatory && this.IsCtrlBLocked)
		{
			if (this.IgnoreCtrl)
			{
				return;
			}
			this.UnblockForCtrl();
		}
		if (this._isBlockedFPS && !ControlsController.ControlsActions.IsBlockedAxis)
		{
			this._isBlockedFPS = false;
		}
		if (this._showCursorNumerator < 0 && (Cursor.lockState != 1 || Cursor.visible))
		{
			if (this.MouseCursor)
			{
				Cursor.visible = false;
				Cursor.lockState = 1;
			}
			else
			{
				GamePadCursor.visible = false;
				GamePadCursor.lockState = 1;
			}
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		LogHelper.Log("CursorManager.OnApplicationFocus, isPressed: {0}", new object[] { this.isPressed });
		if (this.IgnoreCtrl)
		{
			LogHelper.Log("Ignoring control");
			return;
		}
		if (this.isPressed)
		{
			CursorManager.HideCursor();
			CursorManager.UnBlockFPS();
		}
	}

	public static void ShowCursor()
	{
		if (CursorManager._instance == null || CursorManager._instance._isDisabled)
		{
			return;
		}
		if (CursorManager._instance.IsCtrlBLocked)
		{
			CursorManager._instance.UnblockForCtrl();
		}
		CursorManager._instance._showCursorNumerator++;
		if (CursorManager._instance._showCursorNumerator >= 0)
		{
			if (CursorManager._instance.MouseCursor)
			{
				Cursor.visible = true;
				Cursor.lockState = 0;
			}
			else
			{
				GamePadCursor.visible = true;
				GamePadCursor.lockState = 0;
			}
		}
	}

	public static void HideCursor()
	{
		DebugUtility.Input.Trace("Hide cursor", new object[0]);
		if (CursorManager._instance == null)
		{
			return;
		}
		CursorManager._instance._showCursorNumerator--;
		if (CursorManager._instance._showCursorNumerator < -1)
		{
			CursorManager._instance._showCursorNumerator = -1;
		}
		if (CursorManager._instance._showCursorNumerator < 0)
		{
			if (CursorManager._instance.MouseCursor)
			{
				Cursor.visible = false;
				Cursor.lockState = 1;
			}
			else
			{
				GamePadCursor.visible = false;
				GamePadCursor.lockState = 1;
			}
		}
	}

	public static void ResetCursor()
	{
		DebugUtility.Input.Trace("ResetCursor", new object[0]);
		if (CursorManager._instance != null)
		{
			CursorManager._instance._showCursorNumerator = 0;
		}
	}

	public static void DisableCursor()
	{
		Cursor.visible = false;
		Cursor.lockState = 1;
		CursorManager._instance._isDisabled = true;
	}

	public static void BlockFPS()
	{
		CursorManager._instance._blockFPSNumerator++;
		if (CursorManager._instance._blockFPSNumerator > 0 && !CursorManager._instance._isBlockedFPS)
		{
			CursorManager._instance._isBlockedFPS = true;
			ControlsController.ControlsActions.BlockForCtrl();
		}
	}

	public static void UnBlockFPS()
	{
		CursorManager._instance._blockFPSNumerator--;
		if (CursorManager._instance._blockFPSNumerator <= 0 && CursorManager._instance._isBlockedFPS)
		{
			CursorManager._instance._isBlockedFPS = false;
			ControlsController.ControlsActions.UnblockForCtrl();
		}
	}

	public static void ResetFPS()
	{
		DebugUtility.Input.Trace("ResetFPS", new object[0]);
		if (CursorManager._instance != null)
		{
			CursorManager._instance._blockFPSNumerator = 0;
			CursorManager._instance._isBlockedFPS = false;
			ControlsController.ControlsActions.UnBlockAxis();
			ControlsController.ControlsActions.UnBlockInput();
		}
	}

	public static bool IsShowCursor
	{
		get
		{
			return CursorManager._instance._showCursorNumerator >= 0;
		}
	}

	public static bool IsModalWindow()
	{
		return (ShowHudElements.Instance != null && ShowHudElements.Instance.IsCatchedWindowActive) || (MessageBoxList.Instance != null && MessageBoxList.Instance.IsActive) || (ProlongationOfStay.Instance != null && ProlongationOfStay.Instance.IsActive) || (InfoMessageController.Instance != null && InfoMessageController.Instance.IsActive) || (ExitInGameHandler.Instance != null && ExitInGameHandler.Instance.IsActive);
	}

	public static bool IsRewindHoursActive
	{
		get
		{
			return ShowHudElements.Instance != null && ShowHudElements.Instance.RewindHours != null && ShowHudElements.Instance.RewindHours.IsActive;
		}
	}

	[SerializeField]
	private Texture2D _standartCursorUwp;

	public Texture2D activeCursor;

	public Texture2D selectCursor;

	public Texture2D standartCursor;

	public Texture2D loadingCursor;

	public Texture2D mapCanMark;

	public Texture2D mapCantMark;

	private CursorType _currentCursorType;

	private bool mouseCursor = true;

	private static CursorManager _instance;

	private int _showCursorNumerator;

	private int _blockFPSNumerator;

	private bool _isBlockedFPS;

	private bool _isDisabled;

	private readonly Vector2 _cursorOffset = new Vector2(8f, 0f);

	public bool IsCtrlBLocked;

	private bool isPressed;
}
