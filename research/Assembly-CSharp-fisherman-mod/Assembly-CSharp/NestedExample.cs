using System;
using Malee;
using UnityEngine;

public class NestedExample : MonoBehaviour
{
	[Reorderable]
	public NestedExample.ExampleChildList list;

	[Serializable]
	public class ExampleChild
	{
		[Reorderable(singleLine = true)]
		public NestedExample.NestedChildList nested;
	}

	[Serializable]
	public class NestedChild
	{
		public float myValue;
	}

	[Serializable]
	public class NestedChildCustomDrawer
	{
		public bool myBool;

		public GameObject myGameObject;
	}

	[Serializable]
	public class ExampleChildList : ReorderableArray<NestedExample.ExampleChild>
	{
	}

	[Serializable]
	public class NestedChildList : ReorderableArray<NestedExample.NestedChildCustomDrawer>
	{
	}
}
