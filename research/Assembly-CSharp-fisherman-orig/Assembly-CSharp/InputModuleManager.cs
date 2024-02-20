using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputModuleManager : MonoBehaviour
{
	public static InputModuleManager.InputType GameInputType
	{
		get
		{
			return SettingsManager.InputType;
		}
	}

	public static bool IsConsoleMode
	{
		get
		{
			return InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action<InputModuleManager.InputType> OnInputTypeChanged;

	public static void SetInputType(InputModuleManager.InputType inputType)
	{
		InputModuleManager.ChangeInputType(inputType);
		if (InputModuleManager.OnInputTypeChanged != null)
		{
			InputModuleManager.OnInputTypeChanged(inputType);
		}
		KeysHandlerAction.OnInputTypeChanged();
	}

	private static void ChangeInputType(InputModuleManager.InputType inputType)
	{
		if (InputModuleManager._gamePadModule == null || InputModuleManager._standaloneModule == null)
		{
			return;
		}
		if (inputType != InputModuleManager.InputType.GamePad)
		{
			if (inputType == InputModuleManager.InputType.Mouse)
			{
				InputModuleManager._gamePadModule.enabled = false;
				InputModuleManager._standaloneModule.enabled = true;
			}
		}
		else
		{
			InputModuleManager._gamePadModule.enabled = true;
			InputModuleManager._standaloneModule.enabled = false;
		}
		if (InputModuleManager._currentInputType != inputType)
		{
			InputModuleManager._currentInputType = inputType;
		}
	}

	private void Update()
	{
		if (InputModuleManager._currentInputType != InputModuleManager.GameInputType)
		{
			InputModuleManager.SetInputType(InputModuleManager.GameInputType);
		}
	}

	private void Awake()
	{
		if (InputModuleManager._cursorManager == null)
		{
			InputModuleManager._cursorManager = base.GetComponent<CursorManager>();
		}
		if (InputModuleManager._gamePadModule == null)
		{
			InputModuleManager._gamePadModule = base.GetComponent<InControlInputModule>();
		}
		if (InputModuleManager._emulationModule == null)
		{
			InputModuleManager._emulationModule = base.GetComponent<JoystickAsMouseInputModule>();
		}
		if (InputModuleManager._standaloneModule == null)
		{
			InputModuleManager._standaloneModule = base.GetComponent<StandaloneInputModule>();
			if (!(InputModuleManager._standaloneModule is SRIAStandaloneInputModule))
			{
				Object.Destroy(InputModuleManager._standaloneModule);
				InputModuleManager._standaloneModule = base.gameObject.AddComponent<SRIAStandaloneInputModule>();
			}
		}
		InputModuleManager._gamePadModule.enabled = true;
		InputModuleManager._emulationModule.enabled = false;
		InputModuleManager._standaloneModule.enabled = false;
		InputModuleManager.SetInputType(InputModuleManager.GameInputType);
	}

	private static CursorManager _cursorManager;

	private static InControlInputModule _gamePadModule;

	private static JoystickAsMouseInputModule _emulationModule;

	private static StandaloneInputModule _standaloneModule;

	private static InputModuleManager.InputType _currentInputType = InputModuleManager.InputType.None;

	public enum InputType
	{
		Mouse,
		GamePad,
		None
	}
}
