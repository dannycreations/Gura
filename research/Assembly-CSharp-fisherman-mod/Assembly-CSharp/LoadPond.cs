using System;
using System.Collections;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class LoadPond : MonoBehaviour
{
	internal void Awake()
	{
		LoadPond.IsMoving = true;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
	}

	internal void OnDestroy()
	{
		LoadPond.IsMoving = false;
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.Instance_OnGotAvailableLocations;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
	}

	internal void Update()
	{
		if (this.PondInfo != null && !this._isMoved && base.GetComponent<ActivityState>().CanRun)
		{
			this._isMoved = true;
			PhotonConnectionFactory.Instance.MoveToPond(this.PondInfo.Pond.PondId, this.PondInfo.Days, new bool?(this.PondInfo.HasInCar));
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		if (this.PondInfo != null)
		{
			StaticUserData.CurrentPond = this.PondInfo.Pond;
			if (StaticUserData.CurrentPond != null)
			{
				TimeAndWeatherManager.ResetTimeAndWeather();
				PhotonConnectionFactory.Instance.OnGotAvailableLocations += this.Instance_OnGotAvailableLocations;
				PhotonConnectionFactory.Instance.GetAvailableLocations(new int?(StaticUserData.CurrentPond.PondId));
			}
			base.StartCoroutine(this.LoadPondScene());
		}
	}

	private void Instance_OnGotAvailableLocations(IEnumerable<LocationBrief> locations)
	{
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.Instance_OnGotAvailableLocations;
		GameFactory.SetPondLocationsInfo(locations);
	}

	private void OnMoveFailed(Failure failure)
	{
		SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoGameMenuFromTravel, this, null);
		CacheLibrary.MapCache.RefreshPondsCache();
	}

	private IEnumerator LoadPondScene()
	{
		while (!CacheLibrary.AllChachesInited)
		{
			yield return new WaitForSeconds(1f);
		}
		ManagerScenes.Instance.LoadScene(this.PondInfo.Pond.Asset);
		yield break;
	}

	[HideInInspector]
	public PondTransferInfo PondInfo;

	private bool _isMoved;

	public static bool IsMoving;
}
