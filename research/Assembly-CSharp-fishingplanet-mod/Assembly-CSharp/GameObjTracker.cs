using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameObjTracker : MonoBehaviour
{
	private void Start()
	{
		this.PrintGameObjectTree();
	}

	private void Update()
	{
		if (Input.GetKeyUp(289))
		{
			this.dumpNow = true;
		}
		if (this.dumpNow)
		{
			this.PrintGameObjectTree();
			this.dumpNow = false;
		}
	}

	private List<GameObject> FindAllRootGO()
	{
		Transform[] array = Object.FindObjectsOfType(typeof(Transform)) as Transform[];
		List<GameObject> list = new List<GameObject>();
		int num = 0;
		int num2 = 0;
		foreach (Transform transform in array)
		{
			if (transform.parent == null)
			{
				list.Add(transform.gameObject);
				num++;
			}
			if (transform.childCount == 0)
			{
				num2++;
			}
		}
		return list;
	}

	private string SubTreeToString(GameObject go, int indentLevel)
	{
		string text = string.Empty;
		for (int i = 0; i < indentLevel; i++)
		{
			text += "    ";
		}
		this.gameobjectcount++;
		text = text + "+ " + go.name;
		if (go.GetComponent<Renderer>())
		{
			if (go.GetComponent<Renderer>().material)
			{
				text += " using materials: ( ";
				foreach (Material material in go.GetComponent<Renderer>().materials)
				{
					string text2 = text;
					text = string.Concat(new object[] { text2, material.name, " ", material.mainTexture, ", " });
				}
				text += " )";
			}
			if (go.GetComponent<Renderer>().sharedMaterial)
			{
				text += " using shared materials: ( ";
				foreach (Material material2 in go.GetComponent<Renderer>().sharedMaterials)
				{
					string text2 = text;
					text = string.Concat(new object[] { text2, material2.name, " ", material2.mainTexture, ", " });
				}
				text += " )";
			}
		}
		text += "\n";
		IEnumerator enumerator = go.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				text += this.SubTreeToString(transform.gameObject, indentLevel + 1);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		return text;
	}

	private void PrintGameObjectTree()
	{
		this.gameobjectcount = 0;
		List<GameObject> list = this.FindAllRootGO();
		string text = string.Empty;
		foreach (GameObject gameObject in list)
		{
			text += this.SubTreeToString(gameObject, 0);
		}
		Debug.Log(string.Concat(new object[]
		{
			"Level '",
			Application.loadedLevelName,
			"' has now ",
			this.gameobjectcount,
			" ACTIVE gameobjects:\n",
			text
		}));
	}

	private static string GetObjectsOfType(Type type)
	{
		string text = string.Empty;
		Object[] array = Resources.FindObjectsOfTypeAll(type);
		string text2 = text;
		text = string.Concat(new object[] { text2, " - Found ", array.Length, " ", type, "\n" });
		int num = 0;
		foreach (Object @object in array)
		{
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"     (#",
				num,
				") ",
				@object,
				" [ ",
				@object.GetInstanceID(),
				"\n"
			});
			num++;
		}
		return text;
	}

	public static void DumpObjects()
	{
		string text = "Unity objects total: " + Resources.FindObjectsOfTypeAll(typeof(Object)).Length + "\n";
		text += GameObjTracker.GetObjectsOfType(typeof(Texture));
		text += GameObjTracker.GetObjectsOfType(typeof(AudioClip));
		text += GameObjTracker.GetObjectsOfType(typeof(Mesh));
		text += GameObjTracker.GetObjectsOfType(typeof(Material));
		text += GameObjTracker.GetObjectsOfType(typeof(GameObject));
		text += GameObjTracker.GetObjectsOfType(typeof(Component));
		Debug.Log(text);
	}

	private Object[] GetObjectLinks(Object obj)
	{
		List<Object> list = new List<Object>();
		if (obj.GetType() == typeof(GameObject))
		{
			GameObject gameObject = obj as GameObject;
			Component[] components = gameObject.GetComponents(typeof(Component));
			foreach (Component component in components)
			{
				list.Add(component);
			}
		}
		else if (obj.GetType() == typeof(Transform))
		{
			Transform transform = obj as Transform;
			list.Add(transform.gameObject);
			IEnumerator enumerator = transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj2 = enumerator.Current;
					list.Add(obj2 as Object);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}
		else if (obj.GetType() == typeof(Renderer))
		{
			Renderer renderer = obj as Renderer;
			foreach (Material material in renderer.materials)
			{
				list.Add(material);
			}
			foreach (Material material2 in renderer.sharedMaterials)
			{
				list.Add(material2);
			}
		}
		else if (obj.GetType() == typeof(Material))
		{
			Material material3 = obj as Material;
			Texture texture = material3.GetTexture("_MainTex");
			if (texture != null)
			{
				list.Add(texture);
			}
			texture = material3.GetTexture("_BumpMap");
			if (texture != null)
			{
				list.Add(texture);
			}
			texture = material3.GetTexture("_Cube");
			if (texture != null)
			{
				list.Add(texture);
			}
		}
		else
		{
			Debug.LogError("wtf ... unhandled component type: " + obj.GetType());
		}
		return list.ToArray();
	}

	public static void CollectObjectInfo()
	{
		Debug.Log("CollectObjectInfo");
		Dictionary<int, GameObjTracker.ObjectInfo> dictionary = new Dictionary<int, GameObjTracker.ObjectInfo>();
		Object[] array = Resources.FindObjectsOfTypeAll(typeof(Object));
		foreach (Object @object in array)
		{
			if (GameObjTracker.objects.ContainsKey(@object.GetInstanceID()))
			{
				GameObjTracker.ObjectInfo objectInfo = GameObjTracker.objects[@object.GetInstanceID()];
				objectInfo.timesSeen++;
				dictionary.Add(@object.GetInstanceID(), objectInfo);
			}
			else
			{
				GameObjTracker.ObjectInfo objectInfo2 = new GameObjTracker.ObjectInfo();
				objectInfo2.id = @object.GetInstanceID();
				objectInfo2.name = @object.name;
				objectInfo2.type = @object.GetType();
				objectInfo2.timesSeen = 0;
				if (objectInfo2.type == typeof(GameObject))
				{
					objectInfo2.rootId = (@object as GameObject).transform.root.gameObject.GetInstanceID();
				}
				if (objectInfo2.type == typeof(Texture2D))
				{
					Texture2D texture2D = @object as Texture2D;
					int num = 8;
					TextureFormat format = texture2D.format;
					switch (format)
					{
					case 1:
						goto IL_17D;
					case 2:
					case 7:
						num = 16;
						break;
					case 3:
						num = 24;
						break;
					default:
						switch (format)
						{
						case 30:
						case 31:
							num = 2;
							goto IL_1B8;
						case 32:
						case 33:
						case 34:
							break;
						default:
							if (format == 47)
							{
								goto IL_17D;
							}
							if (format != 64)
							{
								goto IL_1B8;
							}
							break;
						}
						num = 4;
						break;
					case 5:
						num = 32;
						break;
					case 10:
					case 12:
						num = 8;
						break;
					}
					IL_1B8:
					objectInfo2.notes = string.Concat(new object[]
					{
						texture2D.format.ToString(),
						" (",
						texture2D.width,
						" x ",
						texture2D.height,
						")"
					});
					objectInfo2.textureSize = texture2D.width * texture2D.height * num / 8;
					goto IL_234;
					IL_17D:
					num = 8;
					goto IL_1B8;
				}
				IL_234:
				dictionary.Add(objectInfo2.id, objectInfo2);
			}
		}
		GameObjTracker.objects = dictionary;
		GameObjTracker.PrintObjectInfo();
		Debug.Log("CollectObjectInfo - end");
	}

	private static void PrintObjectInfo()
	{
		List<GameObjTracker.ObjectInfo> list = new List<GameObjTracker.ObjectInfo>(GameObjTracker.objects.Values);
		list.Sort((GameObjTracker.ObjectInfo x, GameObjTracker.ObjectInfo y) => y.timesSeen.CompareTo(x.timesSeen));
		string text = "Number of objects found: " + list.Count + "\n";
		foreach (GameObjTracker.ObjectInfo objectInfo in list)
		{
			text += objectInfo.GetDescription();
		}
		string text2 = Application.persistentDataPath + "/objectinfo.csv";
		File.WriteAllText(text2, text);
		Debug.LogWarning("Wrote object info data to " + text2);
	}

	public bool dumpNow;

	private float _time;

	private int gameobjectcount;

	private static Dictionary<int, GameObjTracker.ObjectInfo> objects = new Dictionary<int, GameObjTracker.ObjectInfo>();

	public class ObjectInfo
	{
		public string GetDescription()
		{
			return string.Concat(new object[]
			{
				string.Empty,
				this.timesSeen,
				",\"",
				this.name,
				"\",\"",
				this.type,
				"\",",
				this.id,
				",",
				this.rootId,
				",",
				this.textureSize,
				",",
				this.notes,
				"\n"
			});
		}

		public int id;

		public string name;

		public Type type;

		public int timesSeen;

		public int rootId;

		public int textureSize;

		public string notes;
	}
}
