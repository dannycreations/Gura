using System;
using System.Diagnostics;
using InControl;
using UnityEngine;

public class ControlsController : MonoBehaviour
{
	public static ControlsActions ControlsActions
	{
		get
		{
			return ControlsController._controlsActions;
		}
	}

	public static bool IsShiftOrControl
	{
		get
		{
			return Input.GetKey(306) || Input.GetKey(304) || Input.GetKey(305) || Input.GetKey(303);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action OnBindingsChanged;

	private void Awake()
	{
		ControlsController.Instance = this;
		ControlsController._controlsActions = ControlsActions.CreateWithDefaultBindings();
		ControlsController.LoadBindings();
		ControlsController._controlsActions.ChangeToRightHanded(SettingsManager.RightHandedLayout);
	}

	public static void SaveBindings()
	{
		ControlsController._saveData = ControlsController._controlsActions.Save();
		CustomPlayerPrefs.SetString("BindingsNew", ControlsController._saveData);
		CustomPlayerPrefs.Save();
		ControlsController.OnBindingsChanged();
	}

	private void Update()
	{
		if (SettingsManager.InputTypeSaved != InputModuleManager.InputType.Mouse && InputManager.IsInputDeviceActivated(InputModuleManager.InputType.Mouse))
		{
			this.OnInputDeviceActivated(InputModuleManager.InputType.Mouse);
		}
		else if (SettingsManager.InputTypeSaved != InputModuleManager.InputType.GamePad && InputManager.IsInputDeviceActivated(InputModuleManager.InputType.GamePad))
		{
			this.OnInputDeviceActivated(InputModuleManager.InputType.GamePad);
		}
	}

	private void OnInputDeviceActivated(InputModuleManager.InputType type)
	{
		CursorManager.Instance.MouseCursor = type == InputModuleManager.InputType.Mouse;
		SettingsManager.InputType = type;
	}

	public static void ResetToDefault()
	{
		ControlsController._controlsActions.Reset();
		ControlsController._controlsActions.ChangeToRightHanded(SettingsManager.RightHandedLayout);
		ControlsController.SaveBindings();
	}

	public static void CancelChanges()
	{
		ControlsController.LoadBindings();
		ControlsController._controlsActions.ChangeToRightHanded(SettingsManager.RightHandedLayout);
	}

	private static void LoadBindings()
	{
		if (CustomPlayerPrefs.HasKey("BindingsNew"))
		{
			ControlsController._saveData = CustomPlayerPrefs.GetString("BindingsNew");
			ControlsController._controlsActions.Load(ControlsController._saveData);
			if (!CustomPlayerPrefs.HasKey("ControlsVersion") || CustomPlayerPrefs.GetInt("ControlsVersion") != 28)
			{
				CustomPlayerPrefs.SetInt("ControlsVersion", 28);
				ControlsController.ResetToDefault();
			}
		}
		else
		{
			ControlsController.SaveBindings();
		}
	}

	// Note: this type is marked as 'beforefieldinit'.
	static ControlsController()
	{
		ControlsController.OnBindingsChanged = delegate
		{
		};
	}

	private static ControlsActions _controlsActions;

	private static string _saveData;

	public static ControlsController Instance;
}
