using System;
using Malee;
using UnityEngine;

public class GameObjectExample : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(32))
		{
			this.list.Add(base.gameObject);
		}
	}

	[Reorderable(paginate = true, pageSize = 2)]
	public GameObjectExample.GameObjectList list;

	[Serializable]
	public class GameObjectList : ReorderableArray<GameObject>
	{
	}
}
