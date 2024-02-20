using System;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChatMenuHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> PointerEnter;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> PointerExit;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickShowProfile;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickAddToFriend;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickReport;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickToIgnore;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickToUnIgnore;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickPrivateChat;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ClickMute;

	private void Start()
	{
	}

	private void Update()
	{
		if (this.PointerEnter == null)
		{
			Object.Destroy(this);
		}
	}

	public void UpdateVoice(bool isMuted, bool isTalking, bool forceUpdate = false)
	{
		if (this._isTalking != isTalking || this._isMuted != isMuted || forceUpdate)
		{
			this._muteText.text = ScriptLocalization.Get((!isMuted) ? "ChatMutePlayer" : "ChatUnMutePlayer");
			this._muteIco.text = ((!isMuted) ? "\ue778" : "\ue777");
		}
	}

	public void Close()
	{
		Object.Destroy(this);
	}

	public void Init(Player player)
	{
		if (PhotonConnectionFactory.Instance.Profile.Friends != null && PhotonConnectionFactory.Instance.Profile.Friends.Any((Player x) => x.UserId == player.UserId && (x.Status == FriendStatus.FriendshipRequested || x.Status == FriendStatus.Friend)))
		{
			this._isLockedAddToFriend = true;
			this.AddToFriendButtonText.color = Color.gray;
		}
		if (PhotonConnectionFactory.Instance.Profile.Friends != null && PhotonConnectionFactory.Instance.Profile.Friends.Any((Player x) => x.UserId == player.UserId && x.Status == FriendStatus.Ignore))
		{
			this._isIgnoredUser = true;
			this.IgnoreButtonText.text = ScriptLocalization.Get("UnignoreCaption");
		}
		else
		{
			this.IgnoreButtonText.text = ScriptLocalization.Get("IgnoreCaption");
		}
	}

	public void OnPointerEnter()
	{
		if (this.PointerEnter != null && this.PointerEnter.Target != null)
		{
			this.PointerEnter(this, new EventArgs());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void OnPointerExit()
	{
		if (this.PointerExit != null && this.PointerExit.Target != null)
		{
			this.PointerExit(this, new EventArgs());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void OnClick()
	{
		if (this.PointerExit != null)
		{
			this.PointerExit(this, new EventArgs());
		}
	}

	public void OnClickShowProfile()
	{
		if (this.ClickShowProfile != null)
		{
			this.ClickShowProfile(this, new EventArgs());
		}
	}

	public void OnClickAddToFriend()
	{
		if (this.ClickAddToFriend != null && !this._isLockedAddToFriend)
		{
			this.ClickAddToFriend(this, new EventArgs());
		}
	}

	public void OnClickToIgnore()
	{
		if (this.ClickToIgnore != null && !this._isIgnoredUser)
		{
			this.ClickToIgnore(this, new EventArgs());
			return;
		}
		if (this.ClickToUnIgnore != null && this._isIgnoredUser)
		{
			this.ClickToUnIgnore(this, new EventArgs());
		}
	}

	public void OnClickReport()
	{
		if (this.ClickReport != null)
		{
			this.ClickReport(this, new EventArgs());
		}
	}

	public void OnClickPrivateChat()
	{
		if (this.ClickPrivateChat != null)
		{
			this.ClickPrivateChat(this, new EventArgs());
		}
	}

	public void OnClickMute()
	{
	}

	[SerializeField]
	private Text _muteIco;

	[SerializeField]
	private Text _muteText;

	[SerializeField]
	private RectTransform _menuList;

	public Text AddToFriendButtonText;

	private bool _isLockedAddToFriend;

	public Text IgnoreButtonText;

	private bool _isIgnoredUser;

	private bool _isMuted;

	private bool _isTalking;
}
