using System;
using System.Collections.Generic;
using System.Diagnostics;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UgcRoomItemMenuTeam : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<UgcRoomItemMenuTeam.MenuElems> OnAction = delegate(UgcRoomItemMenuTeam.MenuElems el)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnCloseMenu = delegate
	{
	};

	public void Init(UgcRoomItemMenuTeam.MenuElems elem, ToggleGroup menuTglGroup)
	{
		this._elem = elem;
		this._name.text = ScriptLocalization.Get(this.Localization[elem]);
		this._tgl.group = menuTglGroup;
		this._tgl.onValueChanged.AddListener(new UnityAction<bool>(this.TglValueChanged));
	}

	public void Lock(bool isLocked)
	{
		this._name.text = ((!isLocked) ? ScriptLocalization.Get(this.Localization[this._elem]) : ScriptLocalization.Get("UGC_RoomItemMenuTeamUnlock"));
	}

	public void CloseMenu()
	{
		this.OnCloseMenu();
	}

	public void OnClick()
	{
		this.OnAction(this._elem);
	}

	private void TglValueChanged(bool flag)
	{
		if (flag)
		{
			this._hkPr.StartListenForHotkeys();
		}
		else
		{
			this._hkPr.StopListenForHotKeys();
		}
		if (SettingsManager.InputType != InputModuleManager.InputType.GamePad && flag)
		{
			this.OnAction(this._elem);
		}
	}

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _ico;

	[SerializeField]
	private Toggle _tgl;

	[SerializeField]
	private HotkeyPressRedirect _hkPr;

	private UgcRoomItemMenuTeam.MenuElems _elem;

	private readonly Dictionary<UgcRoomItemMenuTeam.MenuElems, string> Localization = new Dictionary<UgcRoomItemMenuTeam.MenuElems, string>
	{
		{
			UgcRoomItemMenuTeam.MenuElems.Transfer,
			"UGC_RoomItemMenuTeamTransfer"
		},
		{
			UgcRoomItemMenuTeam.MenuElems.Lock,
			"UGC_RoomItemMenuTeamLock"
		},
		{
			UgcRoomItemMenuTeam.MenuElems.Delete,
			"UGC_RoomItemMenuTeamDelete"
		}
	};

	public enum MenuElems : byte
	{
		Transfer,
		Lock,
		Delete
	}
}
