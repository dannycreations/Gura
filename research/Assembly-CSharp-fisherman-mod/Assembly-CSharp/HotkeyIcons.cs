using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Common;
using InControl;
using UnityEngine;

public class HotkeyIcons
{
	public static string GetOppositeIcon(HotkeyIcons.OppositeIconTypes t)
	{
		return (!HotkeyIcons.OppositeIcons.ContainsKey(t)) ? string.Empty : HotkeyIcons.OppositeIcons[t];
	}

	public static Dictionary<InputControlType, string> KeyMappings
	{
		get
		{
			return HotkeyIcons.xboxMappings;
		}
	}

	public static string GetIcoByActionName(string actionName, out bool fromKeyboard)
	{
		fromKeyboard = false;
		string text = string.Empty;
		PlayerAction playerAction = ControlsController.ControlsActions.Actions.FirstOrDefault((PlayerAction p) => p.Name == actionName);
		if (playerAction == null && ControlsActions.StandaloneToControllerMappings.TryGetValue(actionName, out actionName))
		{
			playerAction = ControlsController.ControlsActions.Actions.FirstOrDefault((PlayerAction p) => p.Name == actionName);
		}
		if (playerAction != null)
		{
			if (SettingsManager.InputType != InputModuleManager.InputType.GamePad)
			{
				Dictionary<BindingSourceHelper.BindingType, List<BindingSource>> bindings = BindingSourceHelper.GetBindings(new BindingSourceHelper.BindingType[]
				{
					BindingSourceHelper.BindingType.Keyboard,
					BindingSourceHelper.BindingType.Mouse
				}, playerAction.Bindings);
				if (bindings[BindingSourceHelper.BindingType.Mouse].Count > 0)
				{
					MouseBindingSource mouseBindingSource = (MouseBindingSource)bindings[BindingSourceHelper.BindingType.Mouse][0];
					return (!HotkeyIcons.MouseIcoMappings.ContainsKey(mouseBindingSource.Control)) ? mouseBindingSource.Name : HotkeyIcons.MouseIcoMappings[mouseBindingSource.Control];
				}
				BindingSource bindingSource;
				BindingSource bindingSource2;
				BindingSourceHelper.GetPrimaryAndSecondaryBindingSource(bindings[BindingSourceHelper.BindingType.Keyboard], out bindingSource, out bindingSource2);
				if (bindingSource != null)
				{
					KeyBindingSource keyBindingSource = (KeyBindingSource)bindingSource;
					if (keyBindingSource.Control.Count > 0)
					{
						Key key = keyBindingSource.Control.Get(0);
						fromKeyboard = !HotkeyIcons.KeyBoardMappings.ContainsKey(key);
						string text2 = ((!fromKeyboard) ? HotkeyIcons.KeyBoardMappings[key] : keyBindingSource.Name);
						string[] array = text2.Split(new char[] { ' ' });
						return array[array.Length - 1];
					}
					fromKeyboard = true;
					string[] array2 = keyBindingSource.Name.Split(new char[] { ' ' });
					return array2[array2.Length - 1];
				}
			}
			List<BindingSource> list = BindingSourceHelper.GetBindings(new BindingSourceHelper.BindingType[] { BindingSourceHelper.BindingType.Controller }, playerAction.Bindings)[BindingSourceHelper.BindingType.Controller];
			if (list.Count == 0 && ControlsActions.StandaloneToControllerMappings.TryGetValue(actionName, out actionName))
			{
				playerAction = ControlsController.ControlsActions.Actions.FirstOrDefault((PlayerAction p) => p.Name == actionName);
				list = BindingSourceHelper.GetBindings(new BindingSourceHelper.BindingType[] { BindingSourceHelper.BindingType.Controller }, playerAction.Bindings)[BindingSourceHelper.BindingType.Controller];
			}
			if (list.Count > 0)
			{
				DeviceBindingSource deviceBindingSource = (DeviceBindingSource)list[0];
				if (!HotkeyIcons.KeyMappings.TryGetValue(deviceBindingSource.Control, out text))
				{
					Debug.Log("Failed to get control icon for: " + deviceBindingSource.Control);
					text = deviceBindingSource.Control.ToString();
					deviceBindingSource = list.FirstOrDefault((BindingSource x) => x is DeviceBindingSource && HotkeyIcons.KeyMappings.ContainsKey((x as DeviceBindingSource).Control)) as DeviceBindingSource;
					if (deviceBindingSource != null)
					{
						text = HotkeyIcons.KeyMappings[deviceBindingSource.Control];
					}
				}
			}
		}
		return text;
	}

	public const string IcoPremiumAccount = "\ue645";

	public const string IcoMarkerBuoys = "\ue745";

	public const string IcoStorageExpansion = "\ue746";

	public const string IcoTemplateSlots = "\ue749";

	public const string IcoRecipeSlot = "\ue723";

	public const string IcoPondPass = "\ue725";

	public const string IcoLicenses = "\ue63a";

	public const string LeftStickIco = "\ue703";

	public const string RightStickIco = "\ue704";

	private static readonly Dictionary<HotkeyIcons.OppositeIconTypes, string> OppositeIcons = new Dictionary<HotkeyIcons.OppositeIconTypes, string>
	{
		{
			HotkeyIcons.OppositeIconTypes.LeftStick,
			"\ue703"
		},
		{
			HotkeyIcons.OppositeIconTypes.RightStick,
			"\ue704"
		},
		{
			HotkeyIcons.OppositeIconTypes.DPadUpDown,
			"\ue70b"
		},
		{
			HotkeyIcons.OppositeIconTypes.DPadLeftRight,
			"\ue70a"
		}
	};

	private static Dictionary<InputControlType, string> xboxMappings = new Dictionary<InputControlType, string>
	{
		{
			InputControlType.Action1,
			"\ue685"
		},
		{
			InputControlType.Action2,
			"\ue686"
		},
		{
			InputControlType.Action3,
			"\ue687"
		},
		{
			InputControlType.Action4,
			"\ue688"
		},
		{
			InputControlType.LeftBumper,
			"\ue707"
		},
		{
			InputControlType.RightBumper,
			"\ue701"
		},
		{
			InputControlType.LeftTrigger,
			"\ue699"
		},
		{
			InputControlType.RightTrigger,
			"\ue700"
		},
		{
			InputControlType.DPadUp,
			"\ue695"
		},
		{
			InputControlType.DPadDown,
			"\ue697"
		},
		{
			InputControlType.DPadLeft,
			"\ue694"
		},
		{
			InputControlType.DPadRight,
			"\ue696"
		},
		{
			InputControlType.RightStickDown,
			"\ue753"
		},
		{
			InputControlType.RightStickLeft,
			"\ue750"
		},
		{
			InputControlType.RightStickRight,
			"\ue752"
		},
		{
			InputControlType.RightStickUp,
			"\ue751"
		},
		{
			InputControlType.LeftStickLeft,
			"\ue746"
		},
		{
			InputControlType.LeftStickUp,
			"\ue747"
		},
		{
			InputControlType.LeftStickRight,
			"\ue748"
		},
		{
			InputControlType.LeftStickDown,
			"\ue749"
		},
		{
			InputControlType.LeftStickButton,
			"\ue736"
		},
		{
			InputControlType.RightStickButton,
			"\ue737"
		},
		{
			InputControlType.Start,
			"\ue705"
		},
		{
			InputControlType.Options,
			"\ue705"
		},
		{
			InputControlType.Menu,
			"\ue705"
		},
		{
			InputControlType.Back,
			"\ue705"
		},
		{
			InputControlType.TouchPadButton,
			"\ue706"
		},
		{
			InputControlType.DPadY,
			"\ue697"
		},
		{
			InputControlType.DPadX,
			"\ue695"
		}
	};

	private static Dictionary<InputControlType, string> ps4Mappings = new Dictionary<InputControlType, string>
	{
		{
			InputControlType.Action1,
			"\ue710"
		},
		{
			InputControlType.Action2,
			"\ue709"
		},
		{
			InputControlType.Action3,
			"\ue711"
		},
		{
			InputControlType.Action4,
			"\ue708"
		},
		{
			InputControlType.LeftBumper,
			"\ue721"
		},
		{
			InputControlType.RightBumper,
			"\ue722"
		},
		{
			InputControlType.LeftTrigger,
			"\ue719"
		},
		{
			InputControlType.RightTrigger,
			"\ue720"
		},
		{
			InputControlType.DPadUp,
			"\ue717"
		},
		{
			InputControlType.DPadDown,
			"\ue717"
		},
		{
			InputControlType.DPadLeft,
			"\ue716"
		},
		{
			InputControlType.DPadRight,
			"\ue716"
		},
		{
			InputControlType.RightStickDown,
			"\ue753"
		},
		{
			InputControlType.RightStickLeft,
			"\ue750"
		},
		{
			InputControlType.RightStickRight,
			"\ue752"
		},
		{
			InputControlType.RightStickUp,
			"\ue751"
		},
		{
			InputControlType.LeftStickLeft,
			"\ue746"
		},
		{
			InputControlType.LeftStickUp,
			"\ue747"
		},
		{
			InputControlType.LeftStickRight,
			"\ue748"
		},
		{
			InputControlType.LeftStickDown,
			"\ue749"
		},
		{
			InputControlType.LeftStickButton,
			"\ue732"
		},
		{
			InputControlType.RightStickButton,
			"\ue733"
		},
		{
			InputControlType.Start,
			"\ue718"
		},
		{
			InputControlType.Options,
			"\ue718"
		},
		{
			InputControlType.Menu,
			"\ue718"
		},
		{
			InputControlType.Back,
			"\ue731"
		},
		{
			InputControlType.TouchPadButton,
			"\ue723"
		},
		{
			InputControlType.DPadY,
			"\ue714"
		},
		{
			InputControlType.DPadX,
			"\ue712"
		}
	};

	public static Dictionary<Mouse, string> MouseMappings = new Dictionary<Mouse, string>
	{
		{
			Mouse.LeftButton,
			"MouseLeftButton"
		},
		{
			Mouse.RightButton,
			"MouseRightButton"
		}
	};

	public static Dictionary<Mouse, string> MouseIcoMappings = new Dictionary<Mouse, string>
	{
		{
			Mouse.LeftButton,
			"\ue766"
		},
		{
			Mouse.RightButton,
			"\ue767"
		},
		{
			Mouse.MiddleButton,
			"\ue765"
		},
		{
			Mouse.PositiveScrollWheel,
			"\ue809"
		},
		{
			Mouse.NegativeScrollWheel,
			"\ue809"
		}
	};

	public static readonly Dictionary<Key, string> KeyBoardMappings = new Dictionary<Key, string>
	{
		{
			Key.Escape,
			"\ue769"
		},
		{
			Key.UpArrow,
			"\ue768"
		},
		{
			Key.DownArrow,
			"\ue768"
		},
		{
			Key.LeftArrow,
			"\ue768"
		},
		{
			Key.RightArrow,
			"\ue768"
		},
		{
			Key.PadMinus,
			"\ue771"
		},
		{
			Key.PadPlus,
			"\ue770"
		}
	};

	public enum OppositeIconTypes
	{
		None,
		LeftStick,
		RightStick,
		DPadUpDown,
		DPadLeftRight
	}
}
