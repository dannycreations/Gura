using System;
using System.Diagnostics;
using I2.Loc;
using InControl;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UgcRoomItemSingle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDelete = delegate
	{
	};

	public UserCompetitionPlayer User { get; protected set; }

	protected virtual void Awake()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected virtual void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public virtual void Init(UserCompetitionPlayer user, bool isPlayer, ToggleGroup group, bool isHost, bool isPlayerHost)
	{
		this.User = user;
		this.IsHost = isHost;
		this.IsPlayer = isPlayer;
		this.IsPlayerHost = isPlayerHost;
		if (this.IsPlayer)
		{
			this.Bg.color = new Color(0.96862745f, 0.96862745f, 0.96862745f, 0.1f);
		}
		this.Level.text = user.Level.ToString();
		this.Name.text = user.Name;
		this.UpdateData(user);
		this.Tgl.group = group;
		this.Tgl.onValueChanged.AddListener(new UnityAction<bool>(this.TglValueChanged));
	}

	public void SetInteractable(bool flag)
	{
		this.IsInteractable = flag;
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	public virtual void UpdateData(UserCompetitionPlayer user)
	{
		this.User = user;
		this.HostIco.SetActive(this.IsPlayerHost);
		this.ReadyIco.SetActive(user.IsApproved && !this.IsPlayerHost);
		this.BtnDel.interactable = this.IsHost && !this.IsPlayer;
		this.UpdateMenuIcosVisibility();
		this.UpdateMenuIco();
	}

	public virtual void Delete()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad && (!(EventSystem.current != null) || !(EventSystem.current.currentSelectedGameObject.name == base.gameObject.name)))
		{
			return;
		}
		if (this.IsInteractable)
		{
			UIHelper.ShowYesNo(ScriptLocalization.Get("UGC_DeleteUserQuestionCaption"), delegate
			{
				this.OnDelete();
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	protected virtual void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.UpdateMenuIco();
	}

	protected virtual void UpdateMenuIco()
	{
		this.DelIcoBtn.text = ((SettingsManager.InputType != InputModuleManager.InputType.GamePad) ? "\ue613" : this.ActionIco);
	}

	protected string ActionIco
	{
		get
		{
			return HotkeyIcons.KeyMappings[InputControlType.Action1];
		}
	}

	protected virtual void TglValueChanged(bool flag)
	{
		this.UpdateMenuIcosVisibility();
		this.HotkeyPressRedirectUpdate(flag);
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		this.IsPointerOver = true;
		this.UpdateMenuIcosVisibility();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		this.IsPointerOver = false;
		this.UpdateMenuIcosVisibility();
	}

	protected virtual void HotkeyPressRedirectUpdate(bool flag)
	{
		if (flag)
		{
			this.HkPr.StartListenForHotkeys();
		}
		else
		{
			this.HkPr.StopListenForHotKeys();
		}
	}

	protected virtual void UpdateMenuIcosVisibility()
	{
		bool flag = (this.IsPointerOver || this.Tgl.isOn) && this.BtnDel.interactable;
		this.DelIcoBtn.gameObject.SetActive(flag);
	}

	[SerializeField]
	protected HotkeyPressRedirect HkPr;

	[SerializeField]
	protected Image Bg;

	[SerializeField]
	protected GameObject ReadyIco;

	[SerializeField]
	protected GameObject HostIco;

	[SerializeField]
	protected TextMeshProUGUI DelIcoBtn;

	[SerializeField]
	protected TextMeshProUGUI Level;

	[SerializeField]
	protected TextMeshProUGUI Name;

	[SerializeField]
	protected Toggle Tgl;

	[SerializeField]
	protected Button BtnDel;

	protected bool IsHost;

	protected bool IsPlayerHost;

	protected bool IsPlayer;

	protected const float DblClickTime = 0.5f;

	protected static float CurDblClickTime;

	private const string MenuIcoDel = "\ue613";

	protected bool IsPointerOver;

	protected bool IsInteractable = true;
}
