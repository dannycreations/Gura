using System;
using System.Diagnostics;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItemHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<FriendEventArgs> IsSelectedItem;

	public void Init(Player player)
	{
		this.IsSelectedItem = null;
		this._name = player.UserName;
		this.Name.text = player.UserName;
		if (player.IsOnline)
		{
			this.Status.text = "Online";
			this.Status.color = Color.green;
		}
		else
		{
			this.Status.text = "Offline";
			this.Status.color = Color.red;
		}
	}

	public void IsSelected()
	{
		if (this.Toggle.isOn && this.IsSelectedItem != null)
		{
			this.IsSelectedItem(this, new FriendEventArgs
			{
				Name = this._name
			});
		}
	}

	public Text Name;

	public Toggle Toggle;

	public Text Status;

	private string _name;
}
