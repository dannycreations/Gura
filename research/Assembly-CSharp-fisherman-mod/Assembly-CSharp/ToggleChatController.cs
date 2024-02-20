using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ToggleChatController : MonoBehaviour
{
	public Toggle ActiveToggle
	{
		get
		{
			return this._activeToggle;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActivate = delegate
	{
	};

	private void Awake()
	{
		ToggleChatController.Instance = this;
		this._activeToggle = this.GeneralToggle;
		this._lastActiveToggle = this._activeToggle;
		this.SetOnActiveToggle(true);
	}

	private void Start()
	{
		this._leftCornerX = 0f;
		base.GetComponent<RectTransform>().offsetMin += new Vector2(this._leftCornerX, 0f);
	}

	private void Update()
	{
		if (this._toggles.Count != StaticUserData.ChatController.OpenPrivates.Count)
		{
			int i;
			for (i = this._toggles.Count - 1; i >= 0; i--)
			{
				if (!StaticUserData.ChatController.OpenPrivates.Any((Player p) => p.UserId == this._toggles[i].UserId) && !this._toggles[i].IsChannel)
				{
					this.RemovePrivateChat(this._toggles[i].UserId);
				}
			}
			for (int j = 0; j < StaticUserData.ChatController.OpenPrivates.Count; j++)
			{
				Player p = StaticUserData.ChatController.OpenPrivates[j];
				if (this._toggles.All((ToggleChatController.ToggleRecord tr) => tr.UserId != p.UserId))
				{
					this.AddPrivateChat(p);
				}
			}
		}
		this.UpdateToggles();
	}

	public void SetOnActiveToggle(bool isOn)
	{
		if (this._activeToggle != null && this._activeToggle.isOn != isOn)
		{
			this._activeToggle.isOn = isOn;
			LogHelper.Log("___kocha SetOnActiveToggle _activeToggle:{0} isOn:{1}", new object[]
			{
				this._activeToggle.name,
				isOn
			});
		}
	}

	public void NextToggle()
	{
		this.ContentPanel.anchoredPosition = new Vector3(Mathf.Max(this.ContentPanel.anchoredPosition.x - 103f, base.GetComponent<RectTransform>().rect.width - this.ContentPanel.rect.width), this.ContentPanel.anchoredPosition.y, 0f);
	}

	public void PrevToggle()
	{
		this.ContentPanel.anchoredPosition = new Vector3(Mathf.Min(this.ContentPanel.anchoredPosition.x + 103f, 0f), this.ContentPanel.anchoredPosition.y, 0f);
	}

	public bool IsTournamentToggle(Toggle t)
	{
		ToggleChatController.ToggleRecord toggleRecord = this._toggles.FirstOrDefault((ToggleChatController.ToggleRecord p) => p.IsChannel);
		return toggleRecord != null && t.Equals(toggleRecord.Toggle);
	}

	public void AddPrivateChat(Player player)
	{
		TogleChatHandler togleChatHandler = this.Add(player.UserId, false);
		if (togleChatHandler != null)
		{
			togleChatHandler.Init(player);
		}
	}

	public void AddTournamentChat(string tournamentName)
	{
		TogleChatHandler togleChatHandler = this.Add(tournamentName, true);
		if (togleChatHandler != null)
		{
			togleChatHandler.Init(tournamentName);
		}
	}

	public void ActivatePrivateChat(Player player)
	{
		ToggleChatController.ToggleRecord toggleRecord = this._toggles.FirstOrDefault((ToggleChatController.ToggleRecord t) => t.UserId == player.UserId);
		if (toggleRecord != null)
		{
			toggleRecord.Toggle.isOn = true;
		}
	}

	private void ToggleChatController_OnCloseClick(object sender, ToggleChatEventArgs e)
	{
		this.RemovePrivateChat(e.player.UserId);
	}

	public void RemovePrivateChat(string userId)
	{
		LogHelper.Log("___kocha RemovePrivateChat userId:{0}", new object[] { userId });
		int num = this._toggles.FindIndex((ToggleChatController.ToggleRecord x) => x.UserId == userId);
		if (num >= 0)
		{
			StaticUserData.ChatController.OpenPrivates.RemoveAll((Player p) => p.UserId == userId);
			ToggleChatController.ToggleRecord toggleRecord = this._toggles[num];
			this.GeneralToggle.group.UnregisterToggle(toggleRecord.Toggle);
			if (toggleRecord.Toggle != null && toggleRecord.Toggle.gameObject != null)
			{
				Object.Destroy(toggleRecord.Toggle.gameObject);
			}
			this._toggles.RemoveAt(num);
			this.GeneralToggle.isOn = true;
			this.UpdateToggles();
		}
	}

	public void RemovePrivateChat()
	{
		ToggleChatController.ToggleRecord toggleRecord = this._toggles.FirstOrDefault((ToggleChatController.ToggleRecord o) => o.Toggle.isOn && !o.IsChannel);
		if (toggleRecord != null)
		{
			this.RemovePrivateChat(toggleRecord.UserId);
		}
	}

	public void RemoveChannelChat()
	{
		this._toggles.Where((ToggleChatController.ToggleRecord p) => p.IsChannel).ToList<ToggleChatController.ToggleRecord>().ForEach(delegate(ToggleChatController.ToggleRecord p)
		{
			this.RemovePrivateChat(p.UserId);
		});
	}

	private void UpdateToggles()
	{
		if (this.GeneralToggle.isOn || this._toggles.Count == 0)
		{
			this._activeToggle = this.GeneralToggle;
		}
		else
		{
			ToggleChatController.ToggleRecord toggleRecord = this._toggles.FirstOrDefault((ToggleChatController.ToggleRecord t) => t.Toggle.isOn);
			if (toggleRecord != null)
			{
				this._activeToggle = toggleRecord.Toggle;
			}
			else if (this._lastActiveToggle != null)
			{
				this._activeToggle = this._lastActiveToggle;
			}
			else
			{
				this._activeToggle = this._toggles.First<ToggleChatController.ToggleRecord>().Toggle;
			}
		}
		this._lastActiveToggle = this._activeToggle;
		if (!this._activeToggle.isOn)
		{
			this.SetOnActiveToggle(true);
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			RectTransform component = this._activeToggle.GetComponent<RectTransform>();
			Rect rect = base.GetComponent<RectTransform>().rect;
			float num = 0f;
			if (0f > component.anchoredPosition.x + this.ContentPanel.anchoredPosition.x)
			{
				num = component.anchoredPosition.x + this.ContentPanel.anchoredPosition.x;
			}
			if (rect.width < component.anchoredPosition.x + component.rect.width + this.ContentPanel.anchoredPosition.x)
			{
				num = component.anchoredPosition.x + component.rect.width - rect.width + this.ContentPanel.anchoredPosition.x;
			}
			this.ContentPanel.anchoredPosition -= new Vector2(num, 0f);
		}
		bool flag = true;
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
		{
			flag = false;
		}
		if (this.L1 != null && this.L1.gameObject != null)
		{
			this.L1.gameObject.SetActive(flag && this._toggles.Count > 0);
			this.R1.gameObject.SetActive(flag && this._toggles.Count > 0);
		}
	}

	private TogleChatHandler Add(string id, bool isChannel)
	{
		if (this._toggles.All((ToggleChatController.ToggleRecord x) => x.UserId != id))
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel.gameObject, this.TogglePrefab);
			Toggle component = gameObject.GetComponent<Toggle>();
			component.group = this.GeneralToggle.group;
			PointerActionHandler component2 = gameObject.GetComponent<PointerActionHandler>();
			component2.OnSelected.AddListener(delegate
			{
				this.OnActivate(true);
			});
			component2.OnDeselected.AddListener(delegate
			{
				this.OnActivate(false);
			});
			this.GeneralToggle.group.RegisterToggle(component);
			this.GeneralToggle.group.SetAllTogglesOff();
			component.isOn = true;
			this.GeneralToggle.isOn = false;
			TogleChatHandler component3 = component.GetComponent<TogleChatHandler>();
			if (!isChannel)
			{
				component3.OnCloseClick += this.ToggleChatController_OnCloseClick;
			}
			this._toggles.Add(new ToggleChatController.ToggleRecord(id, component, isChannel));
			UINavigation component4 = base.GetComponent<UINavigation>();
			if (component4 != null)
			{
				component4.ForceUpdate();
			}
			return component3;
		}
		return null;
	}

	private IEnumerator RecalculateBumperPosition()
	{
		yield return new WaitForSeconds(0.5f);
		float minValue = Mathf.Min(base.GetComponent<RectTransform>().rect.width, this.ContentPanel.rect.width);
		this.R1.GetComponent<RectTransform>().anchoredPosition = new Vector2(minValue + 60f, 0f);
		yield break;
	}

	public Toggle GeneralToggle;

	public RectTransform ContentPanel;

	public GameObject TogglePrefab;

	public Text L1;

	public Text R1;

	private float _leftCornerX;

	public static ToggleChatController Instance;

	private List<ToggleChatController.ToggleRecord> _toggles = new List<ToggleChatController.ToggleRecord>();

	private Toggle _activeToggle;

	private Toggle _lastActiveToggle;

	private class ToggleRecord
	{
		public ToggleRecord(string userId, Toggle toggle, bool isChannel)
		{
			this.UserId = userId;
			this.Toggle = toggle;
			this.IsChannel = isChannel;
		}

		public string UserId { get; private set; }

		public Toggle Toggle { get; private set; }

		public bool IsChannel { get; set; }
	}
}
