using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteFriendInit : MonoBehaviour
{
	public void Init(Player player)
	{
		this.Name.text = player.UserName;
		this.Location.text = "Unknown location, unknown state";
	}

	public Text Name;

	public Text Location;

	public Toggle InfoButton;

	public Button DeleteButton;
}
