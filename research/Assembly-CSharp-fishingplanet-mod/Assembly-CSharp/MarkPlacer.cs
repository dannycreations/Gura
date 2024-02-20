using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MarkPlacer
{
	public MarkPlacer(GameObject prefab, GameObject parent, Vector2 mapDimensions, MapRenderer renderer)
	{
		this.renderer = renderer;
		this.SetMinMaxPoints();
		this.SetRefs(prefab, parent, mapDimensions, new List<Vector2>());
	}

	public MarkPlacer(GameObject prefab, GameObject parent, Vector2 mapDimensions, IList<Vector2> mapCoordinates, MapRenderer renderer)
	{
		this.renderer = renderer;
		this.SetMinMaxPoints();
		this.CoordinatesFromPositions(mapCoordinates.Select((Vector2 c) => renderer.GetInGameCoord(c.x, c.y)), ref this.coordinates);
		this.SetRefs(prefab, parent, mapDimensions, this.coordinates);
	}

	public MarkPlacer(GameObject prefab, GameObject parent, Vector2 mapDimensions, List<Transform> transforms, MapRenderer renderer, IList<Color> markColors)
	{
		this.renderer = renderer;
		this.SetMinMaxPoints();
		this.CoordinatesFromPositions(transforms.Select((Transform t) => t.position), ref this.coordinates);
		this.SetRefs(prefab, parent, mapDimensions, this.coordinates);
		this.transforms = transforms;
		this.markColors = markColors;
	}

	public MarkPlacer(GameObject prefab, GameObject parent, Vector2 mapDimensions, IEnumerable<string> assets, MapRenderer renderer)
	{
		this.renderer = renderer;
		this.SetMinMaxPoints();
		this.CoordinatesFromPositions(assets.Select((string asset) => GameObject.Find(asset).transform.position), ref this.coordinates);
		this.SetRefs(prefab, parent, mapDimensions, this.coordinates);
	}

	public List<GameObject> InstantiatedObjects
	{
		get
		{
			return this.objects.Select((RectTransform x) => x.gameObject).ToList<GameObject>();
		}
	}

	private void SetRefs(GameObject prefab, GameObject parent, Vector2 mapDimensions, IList<Vector2> mapCoordinates)
	{
		this.prefab = prefab;
		this.parent = parent;
		this.mapDimensions = mapDimensions;
		this.coordinates = mapCoordinates;
		if (this.objects == null)
		{
			this.objects = new List<RectTransform>();
		}
	}

	private void CoordinatesFromPositions(IEnumerable<Vector3> positions, ref IList<Vector2> mapCoordinates)
	{
		if (mapCoordinates == null)
		{
			mapCoordinates = new List<Vector2>();
		}
		if (LogHelper.Check(this.min != this.max, "min max positions"))
		{
			for (int i = 0; i < positions.Count<Vector3>(); i++)
			{
				Vector3 mapCoord = this.GetMapCoord(positions.ElementAt(i) - this.min, this.min, this.max);
				if (mapCoordinates.Count <= i)
				{
					mapCoordinates.Add(new Vector2(mapCoord.x, mapCoord.z));
				}
				else
				{
					mapCoordinates[i] = new Vector2(mapCoord.x, mapCoord.z);
				}
			}
		}
	}

	private void UpdatePositions(Vector3 oldMin, Vector3 oldMax)
	{
		for (int i = 0; i < this.coordinates.Count; i++)
		{
			Vector3 inGameCoord = this.GetInGameCoord(this.coordinates[i].x, this.coordinates[i].y, oldMin, oldMax);
			Vector3 mapCoord = this.GetMapCoord(inGameCoord - this.min, this.min, this.max);
			this.coordinates[i] = new Vector2(mapCoord.x, mapCoord.z);
		}
	}

	public Vector3 GetCoordinateOffset()
	{
		return this.GetMapCoord(this.renderer.MapOffset - this.min, this.min, this.max) - new Vector3(0.5f, 0f, 0.5f);
	}

	public void UpdateVizualization()
	{
		Vector3 vector = this.min;
		Vector3 vector2 = this.max;
		this.SetMinMaxPoints();
		if (this.transforms != null)
		{
			this.CoordinatesFromPositions(this.transforms.Select((Transform t) => t.position), ref this.coordinates);
		}
		else
		{
			this.UpdatePositions(vector, vector2);
		}
		this.Visualize();
	}

	public void AddMark(Vector2 mapCoordinate)
	{
		if (LogHelper.Check(this.coordinates != null, "coordinates") && LogHelper.Check(this.prefab != null, "prefab"))
		{
			Vector3 vector = Vector3.zero;
			if (LogHelper.Check(this.min != this.max, "min max positions"))
			{
				vector = this.GetCoordinateOffset();
			}
			this.objects.Add(this.CreateMarkButton("mark", mapCoordinate.x - vector.x, mapCoordinate.y - vector.z, Vector3.zero, new Color(0f, 0f, 0f, 0f)));
			this.coordinates.Add(mapCoordinate);
		}
	}

	public Vector2 AddMark(Vector3 worldCoords)
	{
		if (LogHelper.Check(this.coordinates != null, "coordinates") && LogHelper.Check(this.prefab != null, "prefab"))
		{
			Vector3 mapCoord = this.GetMapCoord(worldCoords - this.min, this.min, this.max);
			Vector2 vector;
			vector..ctor(mapCoord.x, mapCoord.z);
			Vector3 vector2 = Vector3.zero;
			if (LogHelper.Check(this.min != this.max, "min max positions"))
			{
				vector2 = this.GetCoordinateOffset();
			}
			this.objects.Add(this.CreateMarkButton("mark", vector.x - vector2.x, vector.y - vector2.z, Vector3.zero, new Color(0f, 0f, 0f, 0f)));
			this.coordinates.Add(vector);
			return vector;
		}
		return Vector2.zero;
	}

	public RectTransform AddMark(Vector3 worldCoords, Vector3 rot, string name)
	{
		if (LogHelper.Check(this.coordinates != null, "coordinates") && LogHelper.Check(this.prefab != null, "prefab"))
		{
			Vector3 mapCoord = this.GetMapCoord(worldCoords - this.min, this.min, this.max);
			Vector2 vector;
			vector..ctor(mapCoord.x, mapCoord.z);
			Vector3 vector2 = Vector3.zero;
			if (LogHelper.Check(this.min != this.max, "min max positions"))
			{
				vector2 = this.GetCoordinateOffset();
			}
			this.objects.Add(this.CreateMarkButton(name, vector.x - vector2.x, vector.y - vector2.z, rot, new Color(0f, 0f, 0f, 0f)));
			this.coordinates.Add(vector);
			return this.objects[this.objects.Count - 1];
		}
		return null;
	}

	public void Visualize()
	{
		if (this.objects == null)
		{
			this.objects = new List<RectTransform>();
		}
		if (LogHelper.Check(this.coordinates != null, "coordinates") && LogHelper.Check(this.prefab != null, "prefab"))
		{
			int num = 0;
			foreach (Vector2 vector in this.coordinates)
			{
				Vector3 vector2 = Vector3.zero;
				Vector3 vector3 = Vector3.zero;
				if (LogHelper.Check(this.min != this.max, "min max positions"))
				{
					vector3 = this.GetCoordinateOffset();
				}
				if (this.transforms != null)
				{
					vector2 = -this.transforms.ElementAt(num).rotation.eulerAngles;
				}
				if (this.objects.Count <= num)
				{
					Color color = ((this.markColors == null || this.markColors.Count <= num) ? new Color(0f, 0f, 0f, 0f) : this.markColors[num]);
					this.objects.Add(this.CreateMarkButton("mark", vector.x - vector3.x, vector.y - vector3.z, vector2, color));
				}
				else
				{
					this.SetPositionAndRotation(this.objects[num], vector.x - vector3.x, vector.y - vector3.z, vector2);
				}
				num++;
			}
		}
	}

	public void UpdateRotation(Vector3 vector3)
	{
		if (this.objects != null)
		{
			for (int i = 0; i < this.objects.Count; i++)
			{
				this.objects[i].transform.localEulerAngles = vector3;
			}
		}
	}

	private void SetMinMaxPoints()
	{
		if (LogHelper.Check(this.renderer != null, "navigator"))
		{
			this.min = this.renderer.GetMin();
			this.max = this.renderer.GetMax();
		}
	}

	private RectTransform CreateMarkButton(string name, float x, float y, Vector3 rotation, Color newColor)
	{
		GameObject gameObject = GUITools.AddChild(this.parent, this.prefab);
		gameObject.name = name;
		RectTransform component = gameObject.GetComponent<RectTransform>();
		this.SetPositionAndRotation(component, x, y, rotation);
		component.localScale = this.prefab.GetComponent<RectTransform>().localScale;
		if (newColor.a != 0f)
		{
			Graphic[] componentsInChildren = gameObject.GetComponentsInChildren<Graphic>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = newColor;
			}
		}
		return component;
	}

	public void SetColorByName(string name, Color c)
	{
		RectTransform rectTransform = this.objects.FirstOrDefault((RectTransform p) => p.name == name);
		if (rectTransform != null)
		{
			Graphic[] componentsInChildren = rectTransform.GetComponentsInChildren<Graphic>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = c;
			}
		}
	}

	public Vector3 TransformCoordinates(Vector2 textureCoord)
	{
		this.SetMinMaxPoints();
		Vector3 inGameCoord = this.GetInGameCoord(textureCoord.x, textureCoord.y, this.min, this.max);
		return this.GetMapCoord(inGameCoord - this.renderer.InitialMin(), this.renderer.InitialMin(), this.renderer.InitialMax());
	}

	private void SetPositionAndRotation(RectTransform go, float x, float y, Vector3 rotation)
	{
		go.anchoredPosition = this.GetAbsolutePosition(x, y);
		((RectTransform)go.GetChild(0)).localRotation = Quaternion.Euler(0f, 0f, rotation.y);
	}

	private Vector3 GetAbsolutePosition(float x, float y)
	{
		return new Vector3(x * this.mapDimensions.x - this.mapDimensions.x / 2f, y * this.mapDimensions.y - this.mapDimensions.y / 2f, 0f);
	}

	public bool RemoveByName(string name)
	{
		int num = this.objects.FindIndex((RectTransform p) => p.name == name);
		if (num != -1)
		{
			this.RemoveAt(num);
			return true;
		}
		return false;
	}

	public void RemoveAt(int currentBuoy)
	{
		if (currentBuoy >= 0 && currentBuoy < this.objects.Count)
		{
			RectTransform rectTransform = this.objects[currentBuoy];
			Object.Destroy(rectTransform.gameObject);
			this.objects.RemoveAt(currentBuoy);
			this.coordinates.RemoveAt(currentBuoy);
		}
	}

	public void Clear()
	{
		foreach (RectTransform rectTransform in this.objects)
		{
			Object.Destroy(rectTransform.gameObject);
		}
		this.objects.Clear();
		this.coordinates.Clear();
		this.markColors.Clear();
		this.transforms.Clear();
	}

	public Vector3 GetMapCoord(Vector3 position, Vector3 min, Vector3 max)
	{
		return new Vector3(position.x / (max.x - min.x), 0f, position.z / (max.z - min.z));
	}

	public Vector3 GetInGameCoord(float x, float y, Vector3 min, Vector3 max)
	{
		return new Vector3(x * (max.x - min.x) + min.x, 0f, y * (max.z - min.z) + min.z);
	}

	private GameObject prefab;

	private GameObject parent;

	private Vector2 mapDimensions;

	private IList<Vector2> coordinates;

	private IEnumerable<Vector3> rotations;

	private List<RectTransform> objects;

	private List<Transform> transforms;

	private IList<Color> markColors;

	private Vector3 min;

	private Vector3 max;

	private MapRenderer renderer;
}
