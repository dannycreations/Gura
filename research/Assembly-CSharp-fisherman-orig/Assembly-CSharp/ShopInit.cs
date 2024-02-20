using System;
using UnityEngine;

public class ShopInit : MonoBehaviour
{
	public static int MinTravelCost
	{
		get
		{
			if (StaticUserData.CurrentPond == null)
			{
				return ShopInit._minTravelCost;
			}
			return (int)StaticUserData.CurrentPond.StayFee.Value;
		}
		set
		{
			ShopInit._minTravelCost = value;
		}
	}

	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGotMinTravelCost += this.OnGotMinTravelCost;
		PhotonConnectionFactory.Instance.GetItemCategories(false);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotMinTravelCost -= this.OnGotMinTravelCost;
	}

	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.GetMinTravelCost();
	}

	private void OnGotMinTravelCost(int cost)
	{
		ShopInit._minTravelCost = cost;
	}

	private static int _minTravelCost;
}
