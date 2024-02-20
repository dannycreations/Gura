using System;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ManualPhotonViewAllocator : MonoBehaviour
{
	public void AllocateManualPhotonView()
	{
	}

	public void InstantiateRpc(int viewID)
	{
	}

	public GameObject Prefab;
}
