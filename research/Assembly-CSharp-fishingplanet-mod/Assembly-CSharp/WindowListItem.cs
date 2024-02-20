using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowListItem : MonoBehaviour, IWindowListItem, IListItemBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSelect = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnOk = delegate
	{
	};

	public bool IsActive
	{
		get
		{
			return this.Active.activeSelf;
		}
	}

	public int RadioId { get; set; }

	public void Init(string t, ToggleGroup tg, bool interactable, int radioId)
	{
		this.RadioId = radioId;
		this.HkPr.SetPausedFromScript(true);
		this.TglSwitcher.OnStateChanged += delegate(ToggleColorTransitionChanges.SelectionState s)
		{
			this.HkPr.SetPausedFromScript(s != ToggleColorTransitionChanges.SelectionState.Highlighted);
		};
		this.UpdateText(t);
		this.Tgl.group = tg;
		this.Tgl.interactable = interactable;
		if (!this.Tgl.interactable)
		{
			this.Text.color = this.Disabled;
		}
		this.TglAddListener();
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	public void UpdateText(string t)
	{
		this.Text.text = t;
		if (this.TextActive != null)
		{
			this.TextActive.text = t;
		}
	}

	public void SetToggle(bool value)
	{
		if (this.Tgl.isOn != value)
		{
			this.TglClearListener();
			this.Tgl.isOn = value;
			this.TglAddListener();
		}
	}

	public void SetActive(bool flag)
	{
		this.Active.SetActive(flag);
		for (int i = 0; i < this.HideWhenActive.Length; i++)
		{
			this.HideWhenActive[i].SetActive(!flag);
		}
	}

	public void Ok()
	{
		this.OnOk();
	}

	public int GetSiblingIndex()
	{
		return base.GetComponent<RectTransform>().GetSiblingIndex();
	}

	protected void TglAddListener()
	{
		this.Tgl.onValueChanged.AddListener(delegate(bool b)
		{
			if (b)
			{
				this.OnSelect();
			}
		});
	}

	protected void TglClearListener()
	{
		this.Tgl.onValueChanged.RemoveAllListeners();
	}

	[SerializeField]
	protected TextMeshProUGUI Text;

	[SerializeField]
	protected TextMeshProUGUI TextActive;

	[SerializeField]
	protected GameObject Active;

	[SerializeField]
	protected Toggle Tgl;

	[SerializeField]
	protected GameObject[] HideWhenActive;

	[SerializeField]
	protected HotkeyPressRedirect HkPr;

	[SerializeField]
	protected ToggleColorTransitionChanges TglSwitcher;

	private readonly Color Disabled = new Color(0.4862745f, 0.4862745f, 0.4862745f);
}
