using System;
using ObjectModel;
using UnityEngine;

public class RequestLocationDescriptioAction : MonoBehaviour
{
	public void ValueChanged(bool isOn = true)
	{
		if (isOn && PhotonConnectionFactory.Instance != null && this.TransferToLocation != null)
		{
			ShowLocationInfo.Instance.CurrentLocation = new Location
			{
				LocationId = this.CurrentLocationBrief.LocationId,
				Pond = StaticUserData.CurrentPond,
				Asset = this.CurrentLocationBrief.Asset
			};
			PhotonConnectionFactory.Instance.GetLocationInfo(this.CurrentLocationBrief.LocationId);
			PhotonConnectionFactory.Instance.ChangeSelectedElement(GameElementType.LocationPin, this.CurrentLocationBrief.Asset, null);
		}
	}

	public LocationBrief CurrentLocationBrief;

	[HideInInspector]
	public TransferToLocation TransferToLocation;
}
