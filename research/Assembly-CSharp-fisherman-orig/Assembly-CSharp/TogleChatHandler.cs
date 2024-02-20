using System;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TogleChatHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<ToggleChatEventArgs> OnCloseClick;

	public void Init(string tournamentName)
	{
		this.CaptionText.text = tournamentName;
		this._closeBtn.SetShow(0);
	}

	public void Init(Player player)
	{
		this.Player = player;
		this.CaptionText.text = player.UserName;
	}

	public void CloseClick()
	{
		if (this.Player == null)
		{
			return;
		}
		if (this.OnCloseClick != null)
		{
			this.OnCloseClick(this, new ToggleChatEventArgs
			{
				player = this.Player
			});
		}
	}

	private void Update()
	{
		if (this.Player == null)
		{
			return;
		}
		if (StaticUserData.ChatController.UnreadChatTabs.Any((Player x) => x.UserId == this.Player.UserId))
		{
			this.tsc.NormalSpriteColor = this.UnreadColor;
			this.tsc.OnPointerExit(null);
		}
		else
		{
			this.tsc.NormalSpriteColor = this.NormalColor;
			this.tsc.OnPointerExit(null);
		}
	}

	[SerializeField]
	private GamePadMouseOnlyDisabler _closeBtn;

	public Text CaptionText;

	public Player Player;

	public ToggleStateChanges tsc;

	public Color UnreadColor;

	public Color NormalColor;
}
