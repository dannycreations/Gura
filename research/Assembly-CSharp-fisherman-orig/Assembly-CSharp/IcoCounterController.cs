using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class IcoCounterController : MonoBehaviour
{
	private void Start()
	{
		this.Init();
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		ClientMissionsManager.OnUpdateMissionCount += this.MissionsContext_OnUpdateMissionCount;
	}

	private void OnDestroy()
	{
		if (this._icoCounter != null)
		{
			Object.Destroy(this._icoCounter.gameObject);
			this._icoCounter = null;
		}
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		ClientMissionsManager.OnUpdateMissionCount -= this.MissionsContext_OnUpdateMissionCount;
		PhotonConnectionFactory.Instance.MissionsListReceived -= this.PhotonServer_OnMissionListReceived;
	}

	private void OnEnable()
	{
		if (StaticUserData.DISABLE_MISSIONS || !this._isUseServerData)
		{
			return;
		}
		PhotonConnectionFactory.Instance.MissionsListReceived += this.PhotonServer_OnMissionListReceived;
		PhotonConnectionFactory.Instance.GetMissionsStarted();
	}

	private void OnDisable()
	{
		if (StaticUserData.DISABLE_MISSIONS || !this._isUseServerData)
		{
			return;
		}
		PhotonConnectionFactory.Instance.MissionsListReceived -= this.PhotonServer_OnMissionListReceived;
	}

	public void SetUseServerData(bool flag)
	{
		this._isUseServerData = flag;
	}

	private void UpdateCount(int count)
	{
		if (base.gameObject == null || this._icoCounter == null || this._icoCounter.gameObject == null)
		{
			return;
		}
		this._icoCounter.UpdateCount(count.ToString());
		this._icoCounter.gameObject.SetActive(count > 0);
	}

	private void PhotonServer_OnMissionListReceived(byte operationcode, List<MissionOnClient> missions)
	{
		if (operationcode != 1)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < missions.Count; i++)
		{
			if (ClientMissionsManager.IsUnreadQuest(missions[i].MissionId))
			{
				num++;
			}
		}
		this.UpdateCount(num);
	}

	private void Init()
	{
		if (this._icoCounter == null)
		{
			this._icoCounter = GUITools.AddChild(this._icoCounterRoot, this._icoCounterPrefab).GetComponent<IcoCounter>();
			this._icoCounter.gameObject.SetActive(false);
		}
	}

	private void MissionsContext_OnUpdateMissionCount(int count)
	{
		this.UpdateCount(count);
	}

	[SerializeField]
	private GameObject _icoCounterPrefab;

	[SerializeField]
	private GameObject _icoCounterRoot;

	private IcoCounter _icoCounter;

	private bool _isUseServerData = true;
}
