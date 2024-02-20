using System;
using System.Collections;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class LoadLocation : MonoBehaviour
{
	internal void Awake()
	{
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
		CacheLibrary.MapCache.OnGetPond += this.MapCache_OnGetPond;
		PhotonConnectionFactory.Instance.OnGotLocationInfo += this.OnGotLocationInfo;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.Instance_OnGotAvailableLocations;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
		CacheLibrary.MapCache.OnGetPond -= this.MapCache_OnGetPond;
		PhotonConnectionFactory.Instance.OnGotLocationInfo -= this.OnGotLocationInfo;
	}

	public void Action()
	{
		Debug.Log("LoadLocation.Action");
		int? locationId = this.LocationId;
		if (locationId != null && this.LocationId > 0)
		{
			PhotonConnectionFactory.Instance.MoveToRoom(this.PondId, "top");
		}
		else
		{
			PhotonConnectionFactory.Instance.MoveToPond(this.PondId, PhotonConnectionFactory.Instance.Profile.PondStayTime, PhotonConnectionFactory.Instance.Profile.IsTravelingByCar);
		}
	}

	private void OnGotLocationInfo(Location location)
	{
		if (location != null)
		{
			StaticUserData.CurrentLocation = location;
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		Debug.LogFormat("OnMoved, Destination: {0}, IsTutorialFinished: {1}", new object[]
		{
			destination,
			PhotonConnectionFactory.Instance.Profile.IsTutorialFinished
		});
		if (destination != TravelDestination.Pond)
		{
			if (!PhotonConnectionFactory.Instance.Profile.IsTutorialFinished)
			{
				base.StartCoroutine(this.LoadPondScene());
			}
			return;
		}
		PhotonConnectionFactory.Instance.GetPondWeather(this.PondId, 1);
		PhotonConnectionFactory.Instance.GetTime();
		if (!PhotonConnectionFactory.Instance.Profile.IsTutorialFinished)
		{
			PhotonConnectionFactory.Instance.GetLocationInfo(10120);
			PhotonConnectionFactory.Instance.MoveToRoom(this.PondId, "top");
			return;
		}
		base.StartCoroutine(this.LoadPondScene());
	}

	private void OnMoveFailed(Failure failure)
	{
		throw new Exception(failure.ErrorMessage);
	}

	private void MapCache_OnGetPond(object sender, GlobalMapPondCacheEventArgs e)
	{
		Debug.Log("MapCache_OnGetPond");
		StaticUserData.CurrentPond = e.Pond;
		if (StaticUserData.CurrentPond != null)
		{
			PhotonConnectionFactory.Instance.OnGotAvailableLocations += this.Instance_OnGotAvailableLocations;
			PhotonConnectionFactory.Instance.GetAvailableLocations(new int?(StaticUserData.CurrentPond.PondId));
		}
		ManagerScenes.Instance.LoadScene(e.Pond.Asset);
	}

	private void Instance_OnGotAvailableLocations(IEnumerable<LocationBrief> locations)
	{
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.Instance_OnGotAvailableLocations;
		GameFactory.SetPondLocationsInfo(locations);
	}

	private IEnumerator LoadPondScene()
	{
		while (TimeAndWeatherManager.CurrentWeather == null || !CacheLibrary.AllChachesInited)
		{
			yield return new WaitForSeconds(1f);
		}
		Debug.Log("LoadPondScene");
		CacheLibrary.MapCache.GetPondInfo(this.PondId);
		if (PhotonConnectionFactory.Instance.Game != null)
		{
			PhotonConnectionFactory.Instance.Game.Resume(true);
		}
		yield break;
	}

	[HideInInspector]
	public int? LocationId;

	[HideInInspector]
	public int PondId;
}
