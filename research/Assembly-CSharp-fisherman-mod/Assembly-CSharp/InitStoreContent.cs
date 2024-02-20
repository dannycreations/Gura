using System;
using UnityEngine;

public class InitStoreContent : MonoBehaviour
{
	private void Start()
	{
		PhotonConnectionFactory.Instance.GetItemCategories(false);
	}
}
