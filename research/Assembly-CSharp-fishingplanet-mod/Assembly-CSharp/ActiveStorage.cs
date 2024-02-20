using System;
using ObjectModel;
using UnityEngine;

public class ActiveStorage : MonoBehaviour
{
	public void Setup(ActiveStorage active)
	{
		this.storage = active.storage;
	}

	public void Setup(StoragePlaces active)
	{
		this.storage = active;
	}

	public StoragePlaces storage = StoragePlaces.Storage;
}
