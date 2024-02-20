using System;
using ObjectModel;
using UnityEngine;

public class DragNDropType : MonoBehaviour
{
	[HideInInspector]
	public bool IsActive;

	[HideInInspector]
	public int CurrentActiveTypeId;

	[HideInInspector]
	public StoragePlaces CurrentActiveStorage;
}
