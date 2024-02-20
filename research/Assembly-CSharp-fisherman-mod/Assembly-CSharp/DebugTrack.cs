using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugTrack : MonoBehaviour
{
	public void DrawBottom(List<Vector2> points)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		Vector2 vector = points.First<Vector2>();
		Vector2 vector2 = points.First<Vector2>();
		float num = (vector.x - vector2.x) * 27f;
		float num2 = vector2.y * 10f;
		vector2..ctor(num, num2);
		for (int i = 1; i < points.Count; i++)
		{
			Vector2 vector3 = points[i];
			float num3 = (vector3.x - vector.x) * 27f;
			float num4 = vector3.y * 40f;
			this.DrawLine(vector2, new Vector2(num3, num4), Color.red);
			vector2..ctor(num3, num4);
		}
		this.DrawGrid(Color.blue);
	}

	public void ClearTrack()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = GameObject.Find("PointPixel(Clone)");
			if (gameObject != null)
			{
				gameObject.SetActive(false);
				Object.Destroy(gameObject);
			}
			else
			{
				gameObject = GameObject.Find("bottom");
				if (gameObject != null)
				{
					gameObject.SetActive(false);
					Object.Destroy(gameObject);
				}
				else
				{
					gameObject = GameObject.Find("GridLine");
					if (gameObject != null)
					{
						gameObject.SetActive(false);
						Object.Destroy(gameObject);
					}
				}
			}
		}
	}

	public void DrawPoint(Vector2 point, Vector2 pointFirst)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		Vector2 vector = pointFirst;
		float num = (vector.x - point.x) * 27f;
		float num2 = point.y * 40f;
		GameObject gameObject = GUITools.AddChild(base.gameObject, this.poinPrefab);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(num, num2, 0f);
		gameObject.GetComponent<RectTransform>().localScale = new Vector3(3f, 3f, 3f);
		gameObject.GetComponent<Image>().color = Color.black;
	}

	private void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
	{
		float magnitude = (pointA - pointB).magnitude;
		float num = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180f / 3.1415927f;
		GameObject gameObject = GUITools.AddChild(base.gameObject, this.poinPrefab);
		gameObject.name = "bottom";
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(pointA.x, pointA.y, 0f);
		gameObject.GetComponent<RectTransform>().localScale = new Vector3(magnitude, 1f, 1f);
		gameObject.GetComponent<RectTransform>().localRotation = Quaternion.AngleAxis(num, Vector3.forward);
		gameObject.GetComponent<Image>().color = color;
	}

	private void DrawGrid(Color color)
	{
		for (int i = 1; i <= 50; i++)
		{
			GameObject gameObject = GUITools.AddChild(base.gameObject, this.poinPrefab);
			gameObject.name = "GridLine";
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, (float)(i * 4 * -1), 0f);
			gameObject.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
			gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 2700f);
			if (i % 10 == 0)
			{
				gameObject.GetComponent<Image>().color = Color.green;
			}
			else
			{
				gameObject.GetComponent<Image>().color = color;
			}
		}
	}

	public GameObject poinPrefab;

	public static bool Drawed;
}
