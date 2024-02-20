using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItemBase : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<FriendEventArgs> IsSelectedItem;

	protected virtual void Awake()
	{
	}

	public virtual void Init(Player player, GameObject friendsContentPanel)
	{
		this.FriendsContentPanel = friendsContentPanel;
		this.Init(player);
	}

	public void Init(Player player)
	{
		this.IsSelectedItem = null;
		this.UserId = player.UserId;
		this._player = player;
		this.Username.text = this._player.UserName;
		this.LevelNumber.text = PlayerProfileHelper.GetPlayerLevelRank(this._player);
		string gender = player.Gender;
		if (gender != null)
		{
			if (gender == "M")
			{
				this.Userpic.sprite = this.maleUserpicSprite;
				goto IL_BA;
			}
			if (gender == "F")
			{
				this.Userpic.sprite = this.femaleUserpicSprite;
				goto IL_BA;
			}
		}
		this.Userpic.sprite = this.notProvidedUserpicSprite;
		IL_BA:
		this.Userpic.overrideSprite = null;
		this.Userpic.StopAllCoroutines();
		PlayerProfileHelper.SetAvatarIco(this._player, this.Userpic);
		if (player.HasPremium)
		{
			this.ProIcon.GetComponent<Text>().color = new Color(0.96862745f, 0.96862745f, 0.96862745f, 1f);
		}
	}

	public void ToggleSelected()
	{
		if (this.IsSelectedItem != null)
		{
			this.IsSelectedItem(this, new FriendEventArgs
			{
				Name = this._player.UserName
			});
		}
	}

	[HideInInspector]
	public string UserId;

	public Text Username;

	public Image Userpic;

	public Text LevelNumber;

	public Text ProIcon;

	public GameObject FriendsContentPanel;

	public Sprite femaleUserpicSprite;

	public Sprite maleUserpicSprite;

	public Sprite notProvidedUserpicSprite;

	public Player _player;
}
