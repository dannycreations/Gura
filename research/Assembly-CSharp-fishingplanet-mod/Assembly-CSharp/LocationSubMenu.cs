using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LocationSubMenu : SubMenuFoldoutBase
{
	private void OnGotLocationInfo(Location location)
	{
		if (this.locationImage != null)
		{
			this.locationImageLoadable.Image = this.locationImage;
			this.locationImageLoadable.Load(string.Format("Textures/Inventory/{0}", location.PhotoBID));
		}
	}

	protected override void Awake()
	{
		base.Awake();
		PhotonConnectionFactory.Instance.OnGotLocationInfo += this.OnGotLocationInfo;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotLocationInfo -= this.OnGotLocationInfo;
	}

	[SerializeField]
	private Image locationImage;

	private ResourcesHelpers.AsyncLoadableImage locationImageLoadable = new ResourcesHelpers.AsyncLoadableImage();
}
