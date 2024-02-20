using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UgcRoomItemTeam : UgcRoomItemSingle
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnLock = delegate(bool b)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDblClick = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnMenuOpen = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnMenuClose = delegate
	{
	};

	protected override void Awake()
	{
		if (this._isInited)
		{
			return;
		}
		base.Awake();
		this._isInited = true;
		this._normal = this.Name.color;
		this.BtnMove.interactable = false;
		this.LockIco.SetActive(false);
		this.InitMenu();
		this.MenuIcos["Red".ToUpper()] = this.MenuIcosRed;
		this.MenuIcos["Blue".ToUpper()] = this.MenuIcosBlue;
		this.MenuIcosGo["Red".ToUpper()] = this.MenuIcoRed;
		this.MenuIcosGo["Blue".ToUpper()] = this.MenuIcoBlue;
	}

	private void Update()
	{
		if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !this.IsPointerOver)
		{
			this.CloseMenu();
		}
	}

	public override void UpdateData(UserCompetitionPlayer user)
	{
		if (!this._isInited)
		{
			this.Awake();
		}
		base.UpdateData(user);
		this.LockIco.SetActive(this.IsLocked);
		Graphic name = this.Name;
		Color color = ((!this.IsLocked) ? this._normal : UgcConsts.UpDownArrowDisabled);
		this.Level.color = color;
		name.color = color;
		this.BtnMove.interactable = this.Tgl.isOn && !this.IsLocked;
		if (this._menuItems.ContainsKey(UgcRoomItemMenuTeam.MenuElems.Lock))
		{
			this._menuItems[UgcRoomItemMenuTeam.MenuElems.Lock].Lock(this.IsLocked);
		}
		this.UpdateMenuIcosVisibility();
		this.BtnDel.interactable = this.IsHost || this.IsPlayer;
	}

	public void CloseMenu()
	{
		if (this._menuGo.activeSelf)
		{
			this._menuGo.SetActive(false);
			this.OnMenuClose();
		}
	}

	public bool IsMenuActive
	{
		get
		{
			return this._menuGo.activeSelf;
		}
	}

	protected override void TglValueChanged(bool flag)
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad && this.IsMenuActive)
		{
			return;
		}
		this.BtnMove.interactable = flag && !this.IsLocked;
		base.TglValueChanged(flag);
	}

	public void Lock()
	{
		if (this.IsInteractable)
		{
			this.OnLock(!this.IsLocked);
		}
	}

	public override void Delete()
	{
		if (!this.IsInteractable)
		{
			return;
		}
		if (!this.Tgl.isOn)
		{
			PlayButtonEffect.SetToogleOn(true, this.Tgl);
		}
		if (this.IsHost)
		{
			if (this.IsPlayer)
			{
				this.Move();
			}
			else
			{
				this._menuGo.SetActive(!this._menuGo.activeSelf);
				if (this._menuGo.activeSelf)
				{
					this.OnMenuOpen();
					if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
					{
						this.HkPr.StopListenForHotKeys();
					}
					UINavigation.SetSelectedGameObject(this._menuItems[UgcRoomItemMenuTeam.MenuElems.Transfer].gameObject);
				}
				else
				{
					this.OnMenuClose();
				}
			}
			return;
		}
		if (this.IsLocked)
		{
			return;
		}
		if (this.IsPlayer)
		{
			this.Move();
		}
	}

	public void Move()
	{
		if (!this.IsInteractable)
		{
			return;
		}
		if (!this.IsLocked && (this.IsHost || this.IsPlayer))
		{
			this.OnDblClick();
		}
	}

	private bool IsLocked
	{
		get
		{
			return base.User.IsLocked || (!this.IsHost && base.User.IsApproved && this.IsPlayer);
		}
	}

	private void InitMenu()
	{
		List<UgcRoomItemMenuTeam.MenuElems> list = UserCompetitionHelper.EnumToList<UgcRoomItemMenuTeam.MenuElems>();
		list.ForEach(delegate(UgcRoomItemMenuTeam.MenuElems p)
		{
			GameObject gameObject = GUITools.AddChild(this._itemMenuContent, this._itemMenuPrefab);
			UgcRoomItemMenuTeam component = gameObject.GetComponent<UgcRoomItemMenuTeam>();
			component.Init(p, this._menuTglGroup);
			component.OnCloseMenu += this.CloseMenu;
			component.OnAction += delegate(UgcRoomItemMenuTeam.MenuElems t)
			{
				this.CloseMenu();
				if (t != UgcRoomItemMenuTeam.MenuElems.Lock)
				{
					if (t != UgcRoomItemMenuTeam.MenuElems.Delete)
					{
						if (t == UgcRoomItemMenuTeam.MenuElems.Transfer)
						{
							this.Move();
						}
					}
					else
					{
						this.<Delete>__BaseCallProxy0();
					}
				}
				else
				{
					this.Lock();
				}
			};
			this._menuItems.Add(p, component);
		});
	}

	protected override void UpdateMenuIcosVisibility()
	{
		bool flag = (this.IsPointerOver || this.Tgl.isOn) && (this.IsHost || (!this.IsLocked && this.IsPlayer));
		foreach (KeyValuePair<string, GameObject> keyValuePair in this.MenuIcosGo)
		{
			keyValuePair.Value.SetActive(keyValuePair.Key == base.User.Team.ToUpper() && flag);
		}
	}

	protected override void UpdateMenuIco()
	{
		string actionIco = base.ActionIco;
		foreach (KeyValuePair<string, TextMeshProUGUI[]> keyValuePair in this.MenuIcos)
		{
			if (keyValuePair.Key == base.User.Team.ToUpper())
			{
				if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					if (keyValuePair.Key == "Red".ToUpper())
					{
						keyValuePair.Value[0].text = ((!this.IsPlayer) ? string.Empty : actionIco);
						keyValuePair.Value[1].text = ((!this.IsPlayer) ? actionIco : "\ue006");
					}
					else
					{
						keyValuePair.Value[0].text = ((!this.IsPlayer) ? string.Empty : "\ue006");
						keyValuePair.Value[1].text = actionIco;
					}
				}
				else if (keyValuePair.Key == "Red".ToUpper())
				{
					keyValuePair.Value[0].text = ((!this.IsPlayer) ? string.Empty : "\ue007");
					keyValuePair.Value[1].text = ((!this.IsPlayer) ? "\ue008" : "\ue006");
				}
				else
				{
					keyValuePair.Value[0].text = ((!this.IsPlayer) ? string.Empty : "\ue006");
					keyValuePair.Value[1].text = ((!this.IsPlayer) ? "\ue008" : "\ue007");
				}
			}
		}
	}

	[SerializeField]
	protected GameObject LockIco;

	[SerializeField]
	protected Button BtnMove;

	[SerializeField]
	protected TextMeshProUGUI[] MenuIcosRed;

	[SerializeField]
	protected TextMeshProUGUI[] MenuIcosBlue;

	[SerializeField]
	protected GameObject MenuIcoRed;

	[SerializeField]
	protected GameObject MenuIcoBlue;

	[SerializeField]
	private GameObject _menuGo;

	[SerializeField]
	private GameObject _itemMenuPrefab;

	[SerializeField]
	private GameObject _itemMenuContent;

	[SerializeField]
	private ToggleGroup _menuTglGroup;

	private const string MenuIcoThreePoints = "\ue008";

	private const string MenuIcoPoint = "\ue007";

	private const string MenuIcoArrow = "\ue006";

	private Color _normal;

	private Dictionary<UgcRoomItemMenuTeam.MenuElems, UgcRoomItemMenuTeam> _menuItems = new Dictionary<UgcRoomItemMenuTeam.MenuElems, UgcRoomItemMenuTeam>();

	private bool _isInited;

	private TimeSpan _ts = TimeSpan.FromTicks(DateTime.Now.Ticks);

	private readonly Dictionary<string, TextMeshProUGUI[]> MenuIcos = new Dictionary<string, TextMeshProUGUI[]>();

	private readonly Dictionary<string, GameObject> MenuIcosGo = new Dictionary<string, GameObject>();
}
