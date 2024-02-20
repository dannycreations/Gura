using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using I2.Loc;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpLinePanel : MonoBehaviour
{
	private void OnEnable()
	{
		HelpLinePanel.instance = this;
	}

	private void Awake()
	{
		if (this._cgController == null)
		{
			this._cgController = base.GetComponent<CanvasGroup>();
		}
		if (HelpLinePanel.instance == null)
		{
			HelpLinePanel.instance = this;
		}
		this.SetAlpha(InputModuleManager.GameInputType);
		InputModuleManager.OnInputTypeChanged += this.SetAlpha;
		HelpLinePanel.SetActionHelp(new HotkeyBinding
		{
			Hotkey = InputControlType.Action1,
			LocalizationKey = "Select"
		});
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.SetAlpha;
	}

	private void SetAlpha(InputModuleManager.InputType inputType)
	{
		this._cgController.alpha = (float)((inputType != InputModuleManager.InputType.GamePad) ? 0 : 1);
		if (this._cgMouse != null)
		{
			this._cgMouse.alpha = 1f - this._cgController.alpha;
		}
	}

	public static void SetMouseHelp(string text)
	{
		if (HelpLinePanel.instance != null)
		{
			HelpLinePanel.instance._helpMouse.text = text;
		}
	}

	public static void SetActionHelp(HotkeyBinding binding)
	{
		if (HelpLinePanel.instance == null || string.IsNullOrEmpty(binding.LocalizationKey))
		{
			return;
		}
		for (int i = 0; i < HelpLinePanel.instance.textMappings.Length; i++)
		{
			if (HelpLinePanel.instance.textMappings[i].Hotkey == binding.Hotkey && (!HelpLinePanel.LongPressedItems.Contains(binding.Hotkey) || binding.isLongPressed == HelpLinePanel.instance.textMappings[i].IsLongPressed))
			{
				TextActionMapping textActionMapping = HelpLinePanel.instance.textMappings[i];
				textActionMapping.text.gameObject.SetActive(true);
				textActionMapping.keysDisplayed.AddFirst(binding.LocalizationKey);
				string text = ScriptLocalization.Get(binding.LocalizationKey);
				string ico = HelpLinePanel.GetIco(binding);
				if (!binding.isLongPressed)
				{
					textActionMapping.text.text = HelpLinePanel.GetMappingHint(binding, ico, text);
				}
				else
				{
					textActionMapping.text.text = string.Format("{0} {1} {2}", ScriptLocalization.Get("HoldControllerButton"), ico, (!string.IsNullOrEmpty(text)) ? text : binding.LocalizationKey);
				}
				return;
			}
		}
	}

	public static void HideActionHelp(HotkeyBinding binding)
	{
		if (HelpLinePanel.instance == null)
		{
			return;
		}
		for (int i = 0; i < HelpLinePanel.instance.textMappings.Length; i++)
		{
			if (HelpLinePanel.instance.textMappings[i].Hotkey == binding.Hotkey && (!HelpLinePanel.LongPressedItems.Contains(binding.Hotkey) || binding.isLongPressed == HelpLinePanel.instance.textMappings[i].IsLongPressed))
			{
				LinkedListNode<string> linkedListNode = HelpLinePanel.instance.textMappings[i].keysDisplayed.Find(binding.LocalizationKey);
				if (linkedListNode != null)
				{
					TextActionMapping textActionMapping = HelpLinePanel.instance.textMappings[i];
					if (textActionMapping.keysDisplayed.Count > 0)
					{
						textActionMapping.keysDisplayed.Remove(linkedListNode);
					}
					if (textActionMapping.keysDisplayed.Count <= 0)
					{
						textActionMapping.text.gameObject.SetActive(false);
						for (int j = 0; j < HelpLinePanel.instance.textMappings.Length; j++)
						{
							if (HelpLinePanel.instance.textMappings[j].keysDisplayed.Count > 0)
							{
								HelpLinePanel.instance.textMappings[j].text.gameObject.SetActive(true);
							}
						}
					}
					else
					{
						textActionMapping.text.text = HelpLinePanel.GetMappingHint(binding, HelpLinePanel.GetIco(binding), ScriptLocalization.Get(textActionMapping.keysDisplayed.First.Value));
					}
					return;
				}
			}
		}
	}

	internal static void DisablePanel()
	{
		if (HelpLinePanel.instance != null)
		{
			HelpLinePanel._disabledPanel = HelpLinePanel.instance.gameObject;
			HelpLinePanel._disabledPanel.SetActive(false);
		}
	}

	internal static void EnablePanel()
	{
		if (HelpLinePanel._disabledPanel != null)
		{
			HelpLinePanel._disabledPanel.SetActive(true);
			HelpLinePanel._disabledPanel = null;
		}
	}

	private static string GetIco(HotkeyBinding binding)
	{
		InputControlType hotkey = binding.Hotkey;
		string text = HotkeyIcons.KeyMappings[hotkey];
		if (binding.IsOpposite)
		{
			InputControlType oppositeBinding = HelpLinePanel.GetOppositeBinding(binding.Hotkey);
			if (oppositeBinding != InputControlType.None && text != HotkeyIcons.KeyMappings[oppositeBinding])
			{
				if (binding.OppositeIcon != HotkeyIcons.OppositeIconTypes.None)
				{
					text = HotkeyIcons.GetOppositeIcon(binding.OppositeIcon);
				}
				else
				{
					text = string.Format("{0} {1}", text, HotkeyIcons.KeyMappings[oppositeBinding]);
				}
			}
		}
		return string.Format("<color=#FFDD77FF>{0}</color>", text);
	}

	public static void SetVisibleBg(bool v)
	{
		if (HelpLinePanel.instance != null)
		{
			HelpLinePanel.instance._bgImage.gameObject.SetActive(v);
		}
	}

	private static string GetMappingHint(HotkeyBinding binding, string ico, string hint)
	{
		return ico + ((binding.AlternativeHotkey == InputControlType.None) ? string.Empty : (" " + string.Format("<color=#FFDD77FF>{0}</color>", HotkeyIcons.KeyMappings[binding.AlternativeHotkey]))) + " " + ((!string.IsNullOrEmpty(hint)) ? hint : binding.LocalizationKey);
	}

	private static InputControlType GetOppositeBinding(InputControlType hotkey)
	{
		InputControlType[] array = HelpLinePanel.OppositeBindings.FirstOrDefault((InputControlType[] p) => p[0] == hotkey || p[1] == hotkey);
		return (array == null) ? InputControlType.None : ((array[0] != hotkey) ? array[0] : array[1]);
	}

	[SerializeField]
	private CanvasGroup _cgController;

	[SerializeField]
	private CanvasGroup _cgMouse;

	[SerializeField]
	private TextMeshProUGUI _helpMouse;

	[SerializeField]
	private Image _bgImage;

	public const string IcoFormatColor = "<color=#FFDD77FF>{0}</color>";

	private static string spacing = "      ";

	[SerializeField]
	private TextActionMapping[] textMappings = new TextActionMapping[0];

	private static HelpLinePanel instance;

	private static GameObject _disabledPanel = null;

	private static readonly IList<InputControlType[]> OppositeBindings = new ReadOnlyCollection<InputControlType[]>(new List<InputControlType[]>
	{
		new InputControlType[]
		{
			InputControlType.LeftTrigger,
			InputControlType.RightTrigger
		},
		new InputControlType[]
		{
			InputControlType.LeftStickUp,
			InputControlType.LeftStickDown
		},
		new InputControlType[]
		{
			InputControlType.LeftStickLeft,
			InputControlType.LeftStickRight
		},
		new InputControlType[]
		{
			InputControlType.RightStickDown,
			InputControlType.RightStickUp
		},
		new InputControlType[]
		{
			InputControlType.RightStickLeft,
			InputControlType.RightStickRight
		},
		new InputControlType[]
		{
			InputControlType.DPadDown,
			InputControlType.DPadUp
		},
		new InputControlType[]
		{
			InputControlType.DPadLeft,
			InputControlType.DPadRight
		}
	});

	private static readonly IList<InputControlType> LongPressedItems = new ReadOnlyCollection<InputControlType>(new List<InputControlType>
	{
		InputControlType.Action1,
		InputControlType.Action2,
		InputControlType.Action3,
		InputControlType.Action4
	});
}
