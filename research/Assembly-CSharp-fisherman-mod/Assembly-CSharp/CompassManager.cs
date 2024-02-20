using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;

public class CompassManager : CompassMarkerBase
{
	public static CompassManager Instance { get; private set; }

	private void Awake()
	{
		CompassManager.Instance = this;
		this._compassUiView.OnNorthDegree += this._compassUiView_OnNorthDegree;
	}

	private void Start()
	{
		this._rtNorth.GetComponent<TextMeshProUGUI>().text = ScriptLocalization.Get("N");
		this._rtSouth.GetComponent<TextMeshProUGUI>().text = ScriptLocalization.Get("S");
		this._rtWest.GetComponent<TextMeshProUGUI>().text = ScriptLocalization.Get("W");
		this._rtEast.GetComponent<TextMeshProUGUI>().text = ScriptLocalization.Get("E");
		GameObject gameObject = GameObject.Find("3DSky");
		if (gameObject != null)
		{
			this._compassController.ChangeShift(gameObject.transform.eulerAngles.y);
		}
		this._compassController.Init(GameFactory.PlayerRoot, this._compassUiView, false);
		GameFactory.Player.OnBoarded += this.Player_OnBoarded;
	}

	private void OnDestroy()
	{
		this._compassUiView.OnNorthDegree -= this._compassUiView_OnNorthDegree;
		if (GameFactory.Player != null)
		{
			GameFactory.Player.OnBoarded -= this.Player_OnBoarded;
		}
	}

	private void Update()
	{
		int count = this._data.Count;
		if (count == 0)
		{
			return;
		}
		Camera main = Camera.main;
		PlayerController player = GameFactory.Player;
		if (main == null || player == null || ShowHudElements.Instance == null)
		{
			return;
		}
		float maximumY = player.CameraController.CameraMouseLook.maximumY;
		float minimumY = player.CameraController.CameraMouseLook.minimumY;
		Rect rect = ShowHudElements.Instance.GetComponent<RectTransform>().rect;
		float width = rect.width;
		float height = rect.height;
		float num = (float)Screen.width;
		float num2 = width * 4.16f / 100f;
		float num3 = height * 5.92f / 100f;
		float num4 = height * 5.92f / 100f;
		float num5 = num / this._imageLine.rect.width;
		float num6 = (maximumY - minimumY) / (height - num3 - num4);
		float num7 = this._imageLine.anchoredPosition.x - this._imageLine.rect.width / 2f;
		float num8 = num4;
		for (int i = 0; i < count; i++)
		{
			CompassObject compassObject = this._data[i];
			if (!HintSystem.Instance.IsHintActive(compassObject.Id))
			{
				this.UpdateMarkerVisibility(compassObject.Marker, false);
				this.UpdateMarkersVisibility(compassObject, false, false);
			}
			else
			{
				Vector3 vector = compassObject.Pos - main.transform.position;
				float vectorsYaw = Math3d.GetVectorsYaw(main.transform.forward, vector);
				float num9 = Vector3.Distance(main.transform.position, compassObject.Pos);
				Vector3 vector2 = main.WorldToScreenPoint(compassObject.Pos);
				bool flag = vector2.x >= 0f && vector2.x <= num;
				float num10 = num7;
				if (flag && vector2.z >= 0f)
				{
					num10 += vector2.x / num5;
				}
				else if (vectorsYaw >= 0f)
				{
					num10 += this._imageLine.rect.width;
				}
				RectTransform component = compassObject.Marker.GetComponent<RectTransform>();
				component.anchoredPosition = new Vector2(num10, component.anchoredPosition.y);
				this.UpdateMarkerVisibility(compassObject.Marker, true);
				float num11 = 0f;
				string text = string.Empty;
				bool flag2 = (!flag || vector2.z < 0f) && vectorsYaw <= 0f;
				bool flag3 = (!flag || vector2.z < 0f) && vectorsYaw > 0f;
				if (flag2 || flag3)
				{
					float num12 = Math3d.ClampAngleTo180(Mathf.Asin(vector.y / vector.magnitude) * 57.29578f);
					float num13 = Math3d.ClampAngleTo180(Mathf.Asin(main.transform.forward.y) * 57.29578f);
					float num14 = num12 - num13;
					num11 = num8 + ((maximumY - minimumY) / 2f + num14) / num6;
					text = string.Format("{0} {1}", Mathf.RoundToInt(MeasuringSystemManager.LineLength(num9)), MeasuringSystemManager.LineLengthSufix());
				}
				if (flag2)
				{
					this.UpdateArrow(compassObject.ScreenMarkerLeft, text, new Vector2(num2, num11));
				}
				if (flag3)
				{
					this.UpdateArrow(compassObject.ScreenMarkerRight, text, new Vector2(width - num2, num11));
				}
				this.UpdateMarkersVisibility(compassObject, flag2, flag3);
			}
		}
	}

	public void AddObject(int id, Vector3 pos, Vector3 rot, HintArrowType3D type)
	{
		CompassMarker component = GUITools.AddChild(this._objParent, this._objPref).GetComponent<CompassMarker>();
		component.Init(type, pos);
		RectTransform component2 = component.GetComponent<RectTransform>();
		float num = this._imageLine.anchoredPosition.y + component2.rect.height / 2f;
		component2.anchoredPosition = new Vector2(this._imageLine.anchoredPosition.x, num);
		ScreenMarker component3 = GUITools.AddChild(this._arrowParent, this._arrowPref).GetComponent<ScreenMarker>();
		component3.Init(ScreenMarker.DirectionTypes.Left, component.HintType);
		ScreenMarker component4 = GUITools.AddChild(this._arrowParent, this._arrowPref).GetComponent<ScreenMarker>();
		component4.Init(ScreenMarker.DirectionTypes.Right, component.HintType);
		this._data.Add(new CompassObject
		{
			Id = id,
			Pos = pos,
			Rot = rot,
			Marker = component,
			ScreenMarkerLeft = component3,
			ScreenMarkerRight = component4
		});
	}

	public void RemoveObject(int id)
	{
		int num = this._data.FindIndex((CompassObject p) => p.Id == id);
		if (num != -1)
		{
			Object.Destroy(this._data[num].Marker.gameObject);
			Object.Destroy(this._data[num].ScreenMarkerLeft.gameObject);
			Object.Destroy(this._data[num].ScreenMarkerRight.gameObject);
			this._data.RemoveAt(num);
		}
	}

	private void UpdateMarkersVisibility(CompassObject o, bool left, bool right)
	{
		this.UpdateMarkerVisibility(o.ScreenMarkerLeft, left);
		this.UpdateMarkerVisibility(o.ScreenMarkerRight, right);
	}

	private void UpdateMarkerVisibility(CompassMarkerBase m, bool flag)
	{
		if (flag && !m.IsActive)
		{
			m.SetActive(true);
		}
		else if (!flag && m.IsActive)
		{
			m.SetActive(false);
		}
	}

	private void _compassUiView_OnNorthDegree(float degree)
	{
		degree %= 360f;
		if (degree < 0f)
		{
			degree += 360f;
		}
		float num = this._imageLine.rect.width / 2f;
		float num2 = this._imageLine.anchoredPosition.x - num;
		float y = this._rtNorth.anchoredPosition.y;
		float num3 = num2;
		bool flag = false;
		bool flag2 = false;
		if (degree >= 0f && degree <= 90f)
		{
			num3 += num - degree * num / 90f;
			this._rtNorth.anchoredPosition = new Vector2(num3, y);
			this._rtEast.anchoredPosition = new Vector2(num3 + num, y);
			flag = true;
		}
		else if (degree <= 360f && degree >= 270f)
		{
			num3 += num + (360f - degree) * num / 90f;
			this._rtNorth.anchoredPosition = new Vector2(num3, y);
			this._rtWest.anchoredPosition = new Vector2(num3 - num, y);
			flag = true;
			flag2 = true;
		}
		else if (degree >= 90f && degree <= 270f)
		{
			num3 += this._imageLine.rect.width - (degree - 90f) * this._imageLine.rect.width / 180f;
			this._rtSouth.anchoredPosition = new Vector2(num3, y);
			this._rtWest.anchoredPosition = new Vector2(num3 + num, y);
			this._rtEast.anchoredPosition = new Vector2(num3 - num, y);
			flag2 = degree >= 180f;
		}
		this._rtNorth.gameObject.SetActive(flag);
		this._rtSouth.gameObject.SetActive(!flag);
		this._rtWest.gameObject.SetActive(flag2);
		this._rtEast.gameObject.SetActive(!flag2);
	}

	private void UpdateArrow(ScreenMarker m, string textDist, Vector2 pos)
	{
		m.UpdateText(textDist);
		m.GetComponent<RectTransform>().anchoredPosition = pos;
	}

	private void Player_OnBoarded(bool flag)
	{
		this._compassController.Init(GameFactory.PlayerRoot, this._compassUiView, false);
	}

	[SerializeField]
	private CompassController _compassController;

	[SerializeField]
	private RectTransform _rtNorth;

	[SerializeField]
	private RectTransform _rtSouth;

	[SerializeField]
	private RectTransform _rtWest;

	[SerializeField]
	private RectTransform _rtEast;

	[SerializeField]
	private RectTransform _imageLine;

	[SerializeField]
	private GameObject _objPref;

	[SerializeField]
	private GameObject _objParent;

	[SerializeField]
	private GameObject _arrowPref;

	[SerializeField]
	private GameObject _arrowParent;

	private const float OffsetX = 4.16f;

	private const float OffsetYdown = 5.92f;

	private const float OffsetYUp = 5.92f;

	private List<CompassObject> _data = new List<CompassObject>();

	private CompassUIView _compassUiView = new CompassUIView();
}
