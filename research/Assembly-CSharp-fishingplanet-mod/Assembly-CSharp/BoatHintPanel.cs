using System;
using System.Linq;
using I2.Loc;
using InControl;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AlphaFade))]
public class BoatHintPanel : MonoBehaviour
{
	public bool IsActive
	{
		get
		{
			return this._hideAt >= 0f;
		}
	}

	private void Start()
	{
		this.CorrectPosition();
		this._panel = base.GetComponent<AlphaFade>();
		this._panel.FastHidePanel();
	}

	public void ShowPanel(string text, float duration)
	{
		if (ShowHudElements.Instance != null && ShowHudElements.Instance.CurrentState == ShowHudStates.HideAll)
		{
			return;
		}
		this.TextInfo.text = text;
		this.Show(duration);
	}

	public void ShowPanel(string locLabel, CustomPlayerAction playerAction = null, float duration = 0.1f)
	{
		if (ShowHudElements.Instance != null && ShowHudElements.Instance.CurrentState == ShowHudStates.HideAll)
		{
			return;
		}
		this.TextInfo.text = this.GetString(locLabel, playerAction);
		if (!string.IsNullOrEmpty(this.TextInfo.text))
		{
			this.Show(duration);
		}
		else
		{
			this.Hide();
		}
	}

	public void ShowPanel(string locLabel1, string locLabel2, CustomPlayerAction playerAction1 = null, CustomPlayerAction playerAction2 = null, float duration = 0.1f)
	{
		if (ShowHudElements.Instance != null && ShowHudElements.Instance.CurrentState == ShowHudStates.HideAll)
		{
			return;
		}
		string @string = this.GetString(locLabel1, playerAction1);
		string string2 = this.GetString(locLabel2, playerAction2);
		if (!string.IsNullOrEmpty(@string) && string.IsNullOrEmpty(string2))
		{
			this.TextInfo.text = @string;
		}
		else if (string.IsNullOrEmpty(@string) && !string.IsNullOrEmpty(string2))
		{
			this.TextInfo.text = string2;
		}
		else if (!string.IsNullOrEmpty(@string) && !string.IsNullOrEmpty(string2))
		{
			this.TextInfo.text = string.Format("{0}\n{1}", @string, string2);
		}
		else
		{
			this.TextInfo.text = string.Empty;
		}
		if (!string.IsNullOrEmpty(this.TextInfo.text))
		{
			this.Show(duration);
		}
		else
		{
			this.Hide();
		}
	}

	private void Show(float duration)
	{
		this._hideAt = Time.time + duration;
		if (this._panel != null)
		{
			this._panel.ShowPanel();
		}
	}

	public void Hide()
	{
		this._hideAt = Time.time - 1f;
		this.Update();
	}

	private void Update()
	{
		if (this._hideAt > 0f && this._hideAt < Time.time)
		{
			this._hideAt = -1f;
			this._panel.FastHidePanel();
		}
	}

	private string GetString(string locLabel, CustomPlayerAction playerAction)
	{
		string text = ScriptLocalization.Get(locLabel);
		if (playerAction == null)
		{
			return text;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
		{
			BindingSource bindingSource = playerAction.Bindings.FirstOrDefault((BindingSource b) => b.GetType() == typeof(KeyBindingSource));
			if (bindingSource != null)
			{
				KeyCombo control = (bindingSource as KeyBindingSource).Control;
				if (control.Count > 0)
				{
					return this.Localize(text, control.Get(0).ToString(), true);
				}
			}
		}
		else
		{
			BindingSource bindingSource2 = playerAction.Bindings.FirstOrDefault((BindingSource b) => b.GetType() == typeof(DeviceBindingSource));
			if (bindingSource2 != null)
			{
				return this.Localize(text, HotkeyIcons.KeyMappings[(bindingSource2 as DeviceBindingSource).Control], true);
			}
		}
		return null;
	}

	private void CorrectPosition()
	{
		RectTransform component = base.GetComponent<RectTransform>();
		component.anchoredPosition = new Vector2(component.anchoredPosition.x, -36f);
	}

	private string Localize(string localized, string v, bool isColored)
	{
		int num = ((SettingsManager.InputType != InputModuleManager.InputType.GamePad) ? 0 : 2);
		if (isColored)
		{
			return string.Format(localized, string.Format("<size=+{1}><color=#FFEE44FF>{0}</color></size>", v, num));
		}
		return string.Format(localized, string.Format("<size=+{1}>{0}</size>", v, num));
	}

	public TextMeshProUGUI TextInfo;

	private AlphaFade _panel;

	private float _hideAt = -1f;
}
