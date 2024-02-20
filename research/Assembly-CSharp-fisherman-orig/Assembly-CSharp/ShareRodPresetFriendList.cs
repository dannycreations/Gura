using System;
using System.Globalization;
using System.Linq;
using FriendsSRIA.ViewsHolders;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ShareRodPresetFriendList : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		if (this.cancelButtonText != null)
		{
			this.CancelButtonText = ScriptLocalization.Get("CancelButton");
		}
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		this.ConfirmButton.interactable = false;
		MessageFactory.MessageBoxQueue.Enqueue(this);
		if (this.FriendsView == null)
		{
			this.FriendsView = base.GetComponentInChildren<FriendsHandlerNew>();
		}
		this.FriendsView.IsSelectedItem += this.FriendSelected;
		this.FriendsView.UpdatedItem += this.ItemUpdated;
	}

	public void AcceptPressed()
	{
		if (!string.IsNullOrEmpty(this._currentSelectedFriendName))
		{
			this.ConfirmAction(this._currentSelectedFriendName);
		}
		else
		{
			MonoBehaviour.print("no friend selected");
		}
		this.Close();
	}

	public void Init(RodSetup setup)
	{
		this.Preview.Show(setup);
		this._setup = setup;
		string text = string.Empty;
		this.NameText.text = setup.Name;
		foreach (InventoryItem inventoryItem in setup.Items)
		{
			if (inventoryItem.Brand != null)
			{
				text += string.Format("<b>[{0}]</b> : {1}\n", inventoryItem.BrandName, inventoryItem.Name);
			}
			else
			{
				text += string.Format("<b>{0}</b>\n", inventoryItem.Name);
			}
		}
		text = text.Trim(new char[] { '\n', ' ' });
		this.DetailsText.text = string.Format("<color=#c3986d>{0}</color>", text);
	}

	public new void Start()
	{
		if (this._setup != null)
		{
			this.Preview.Show(this._setup);
		}
	}

	public new void Open()
	{
		this._alphaFade.ShowPanel();
	}

	public new void Close()
	{
		this.closing = true;
		this._alphaFade.HidePanel();
	}

	public string Caption
	{
		set
		{
			this.caption.text = value;
		}
	}

	public string CancelButtonText
	{
		set
		{
			this.cancelButtonText.text = value.ToUpper(CultureInfo.InvariantCulture);
		}
	}

	public string AcceptButtonText
	{
		set
		{
			this.confirmButtonText.text = value.ToUpper(CultureInfo.InvariantCulture);
		}
	}

	public void Update()
	{
		this.CancelButton.enabled = !this.ClearSearchButton.isActiveAndEnabled;
	}

	private void ItemUpdated(object sender, FriendEventArgs e)
	{
		if (sender == null)
		{
			return;
		}
		FriendItemVH friendItemVH = sender as FriendItemVH;
		if (friendItemVH.current.Username.text.Length > 10)
		{
			friendItemVH.current.Username.text = friendItemVH.current.Username.text.Substring(0, 10) + "...";
		}
		if (!string.IsNullOrEmpty(this._currentSelectedFriendName) && e.Name == this._currentSelectedFriendName)
		{
			(sender as FriendItemVH)._background.color = this.HighlightColor;
		}
	}

	private void FriendSelected(object sender, FriendEventArgs e)
	{
		if (this._lastSelected != null && this._lastSelected.root != null && this._lastSelected.root.gameObject.activeInHierarchy && this._lastSelected.current._player.UserName == this._currentSelectedFriendName)
		{
			this._lastSelected.UpdateViews(this._lastSelected.model);
		}
		this._lastSelected = sender as FriendItemVH;
		this._currentSelectedFriendName = e.Name;
		this.ItemUpdated(sender, e);
		if (PhotonConnectionFactory.Instance.Profile.Friends == null)
		{
			this.ConfirmButton.interactable = false;
			return;
		}
		if (!PhotonConnectionFactory.Instance.Profile.Friends.Any((Player x) => x.UserName == this._currentSelectedFriendName && x.Status == FriendStatus.Friend))
		{
			this.ConfirmButton.interactable = false;
			if (!this.closing && !string.IsNullOrEmpty(e.Name))
			{
				GameFactory.Message.ShowMessage(ScriptLocalization.Get("ShareRodSetupOnlyFriends"), null, 2f, false);
			}
		}
		else
		{
			this.ConfirmButton.interactable = true;
		}
	}

	public Text caption;

	public Text cancelButtonText;

	public Text confirmButtonText;

	public Button ConfirmButton;

	public HotkeyPressRedirect CancelButton;

	public Button ClearSearchButton;

	public Action<string> ConfirmAction;

	public RodPresetPreview Preview;

	public Text DetailsText;

	public Text NameText;

	public FriendsHandlerNew FriendsView;

	private RodSetup _setup;

	public Color HighlightColor;

	private string _currentSelectedFriendName;

	private bool closing;

	private FriendItemVH _lastSelected;
}
