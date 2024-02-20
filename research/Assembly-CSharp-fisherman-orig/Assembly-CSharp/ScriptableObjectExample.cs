using System;
using Malee;
using UnityEngine;

[CreateAssetMenu(fileName = "New ScriptableObject Example", menuName = "ScriptableObject Example")]
public class ScriptableObjectExample : ScriptableObject
{
	[SerializeField]
	[Reorderable(paginate = true, pageSize = 0, elementNameProperty = "myString")]
	private ScriptableObjectExample.MyList list;

	[Serializable]
	private struct MyObject
	{
		public bool myBool;

		public float myValue;

		public string myString;
	}

	[Serializable]
	private class MyList : ReorderableArray<ScriptableObjectExample.MyObject>
	{
	}
}
