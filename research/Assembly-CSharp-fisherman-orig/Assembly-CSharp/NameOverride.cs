using System;
using Malee;
using UnityEngine;

public class NameOverride : MonoBehaviour
{
	public string nameOverride = "Car";

	public string nestedNameOverride = "Car Part";

	[Reorderable(null, "Car", null)]
	public NameOverride.ExampleChildList autoNameList;

	[Reorderable]
	public NameOverride.DynamicExampleChildList dynamicNameList;

	[Serializable]
	public class ExampleChild
	{
		[Reorderable(null, "Car Part", null)]
		public NameOverride.StringList nested;
	}

	[Serializable]
	public class DynamicExampleChild
	{
		[Reorderable]
		public NameOverride.StringList nested;
	}

	[Serializable]
	public class ExampleChildList : ReorderableArray<NameOverride.ExampleChild>
	{
	}

	[Serializable]
	public class DynamicExampleChildList : ReorderableArray<NameOverride.DynamicExampleChild>
	{
	}

	[Serializable]
	public class StringList : ReorderableArray<string>
	{
	}
}
