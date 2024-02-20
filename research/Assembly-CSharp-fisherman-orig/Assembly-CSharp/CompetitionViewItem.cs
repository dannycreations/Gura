using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionViewItem : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSelect = delegate
	{
	};

	public virtual void Init(string title, string hint, string value, ToggleGroup tg, bool separActive)
	{
		this.Separ.SetActive(separActive);
		if (this.Title != null)
		{
			this.Title.text = title;
		}
		this.UpdateData(value, hint);
		if (this.Tgl != null)
		{
			this.Tgl.group = tg;
			this.Tgl.onValueChanged.AddListener(delegate(bool b)
			{
				if (b && SettingsManager.InputType != InputModuleManager.InputType.GamePad)
				{
					this.OnSelect();
				}
			});
			if (this.Btn != null)
			{
				this.Btn.onClick.AddListener(delegate
				{
					if (this.Tgl != null && this.Tgl.isOn)
					{
						this.OnSelect();
					}
				});
			}
		}
	}

	public virtual void UpdateData(string value, string hint = null)
	{
		if (this.Value != null)
		{
			this.Value.text = value;
			this.Value.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}
		if (this.Hint != null)
		{
			this.Hint.text = hint;
			this.Hint.gameObject.SetActive(string.IsNullOrEmpty(value));
		}
	}

	public virtual void SetBlocked(bool flag)
	{
		if (this.Ico != null)
		{
			this.Ico.color = ((!flag) ? this.NormalIco : this.BlockedIco);
		}
		if (this.Value != null)
		{
			this.Value.color = ((!flag) ? this.NormalIco : this.BlockedText);
		}
		this.Img.color = ((!flag) ? this.Normal : this.Blocked);
		if (this.Tgl != null)
		{
			this.Tgl.interactable = !flag;
		}
	}

	public virtual void SetStarIcoActive(bool flag)
	{
		this.StarIco.SetActive(flag);
	}

	[SerializeField]
	protected TextMeshProUGUI Title;

	[SerializeField]
	protected TextMeshProUGUI Value;

	[SerializeField]
	protected TextMeshProUGUI Hint;

	[SerializeField]
	protected Toggle Tgl;

	[SerializeField]
	protected GameObject Active;

	[SerializeField]
	protected Button Btn;

	[SerializeField]
	protected Image Img;

	[SerializeField]
	protected GameObject Separ;

	[SerializeField]
	protected GameObject StarIco;

	[SerializeField]
	protected TextMeshProUGUI Ico;

	protected readonly Color Normal = new Color(0.20784314f, 0.21176471f, 0.22352941f, 0.7490196f);

	protected readonly Color Blocked = new Color(0.15294118f, 0.15294118f, 0.15294118f, 0.8f);

	protected readonly Color BlockedIco = new Color(0.8901961f, 0.8901961f, 0.8901961f, 0f);

	protected readonly Color NormalIco = new Color(0.8901961f, 0.8901961f, 0.8901961f);

	protected readonly Color BlockedText = new Color(0.3764706f, 0.3764706f, 0.3764706f);
}
