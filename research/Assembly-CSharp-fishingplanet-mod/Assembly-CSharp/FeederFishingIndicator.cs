using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FeederFishingIndicator : FishingIndicatorBase, IDisposable
{
	public float QuiverPart
	{
		get
		{
			return (this._quiverPart <= 0f || this._quiverPart > 1f) ? 1f : this._quiverPart;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this._width = this._rt.rect.width;
		this._height = this._rt.rect.height;
		this._widthHalf = this._width * 0.5f;
		this._heightHalf = this._height * 0.5f;
	}

	private void Start()
	{
		GameFactory.Player.OnChangeRod += this.Player_OnChangeRod;
	}

	private void OnDestroy()
	{
		this.Dispose();
		if (GameFactory.Player != null)
		{
			GameFactory.Player.OnChangeRod -= this.Player_OnChangeRod;
		}
	}

	public void Init(Rod1stBehaviour rod)
	{
		if (rod != null)
		{
			this._isHigh = false;
			this._enhancedTime = 0f;
			this._tBezier = new float[11];
			this._p = new Vector2[11];
			this._points = new Vector2[11];
			if (rod.RodObject.RodMass == null || rod.RodObject.TipMass == null)
			{
				float num = 1f - 0.2f * this.QuiverPart;
				float num2 = (1f - num) / 10f;
				this._tBezier[0] = num;
				for (int i = 1; i < 11; i++)
				{
					num += num2;
					this._tBezier[i] = num;
				}
			}
			else
			{
				float num3 = rod.transform.InverseTransformPoint(rod.RodObject.RodMass.Position).z;
				float num4 = (rod.transform.InverseTransformPoint(rod.RodObject.TipMass.Position).z - num3) / 10f;
				this._tBezier[0] = rod.GetTParam(num3);
				for (int j = 1; j < 11; j++)
				{
					num3 += num4;
					this._tBezier[j] = rod.GetTParam(num3);
				}
			}
		}
	}

	public void SetHidden(bool isHidden)
	{
		if (isHidden)
		{
			this._time += Time.deltaTime;
			if (this._time >= this._notActiveHideTime)
			{
				base.Hide();
			}
		}
		else
		{
			this.StopHiding();
		}
	}

	public void SetPositions()
	{
		if (this._curPointsCount != 11)
		{
			this._curPointsCount = 11;
			this.CreateLines();
		}
		this.CalcPoints();
		for (int i = 0; i < 11; i++)
		{
			Vector2 vector = this.CalcPos(this._points[i]);
			if (i > 0)
			{
				int num = i - 1;
				RectTransform rectTransform = this._linesRectTransform[num];
				Vector2 vector2 = this.CalcPos(this._points[num]);
				Vector2 vector3 = vector - vector2;
				float num2 = vector3.magnitude;
				if (num2 <= 0f)
				{
					num2 = 2f;
				}
				rectTransform.anchoredPosition = new Vector2(vector2.x + vector3.x * 0.5f, vector2.y + vector3.y * 0.5f);
				rectTransform.sizeDelta = new Vector2(num2, rectTransform.rect.height);
				float num3 = -Mathf.Acos(vector3.x / num2) * 57.29578f;
				if (vector.y > vector2.y)
				{
					num3 *= -1f;
				}
				rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num3 + this._rt.rotation.eulerAngles.z));
				if (this._circlesRectTransform.ContainsKey(num))
				{
					RectTransform rectTransform2 = this._circlesRectTransform[num];
					rectTransform2.anchoredPosition = new Vector2(rectTransform.rect.width * 0.5f - rectTransform2.rect.width * 0.5f, rectTransform.rect.height / 2f + this._lineCircleOffsetY);
				}
			}
		}
	}

	public void HighSensitivity(bool isHigh = true)
	{
		this._isHigh = isHigh;
	}

	public void Dispose()
	{
		this.Clear(ref this._linesRectTransform);
	}

	public override void Hide()
	{
		if (this.InState(FeederFishingIndicator.States.IsActive))
		{
			this._state ^= FeederFishingIndicator.States.IsActive;
		}
		base.Hide();
	}

	public override void Show()
	{
		this._state |= FeederFishingIndicator.States.IsActive;
		base.Show();
	}

	public override bool IsShow
	{
		get
		{
			return this.AlphaFade.IsShow || this.InState(FeederFishingIndicator.States.IsActive);
		}
	}

	private void CalcPoints()
	{
		Rod1stBehaviour rod = GameFactory.Player.Rod;
		if (rod == null)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		Quaternion quaternion = Quaternion.Inverse(rod.GetBezierRotation(this._tBezier[0]));
		Vector3 vector = quaternion * rod.GetBezierPoint(this._tBezier[0]);
		this._p[0].x = num;
		this._p[0].y = num2;
		float num3 = 0f;
		float num4;
		for (int i = 0; i < 11; i++)
		{
			Vector3 vector2 = quaternion * rod.GetBezierPoint(this._tBezier[i]);
			Vector3 vector3 = vector2 - vector;
			num3 += vector3.magnitude;
			num4 = num - Mathf.Sqrt(vector3.x * vector3.x + vector3.z * vector3.z);
			float num5 = num2 - vector3.y;
			this._p[i].x = num4;
			this._p[i].y = num5;
			vector = vector2;
			num = num4;
			num2 = num5;
		}
		float num6 = ((num3 <= 0f) ? 1f : (1f / num3));
		float num7 = 1f;
		if (this._isHigh)
		{
			if (this._prevDivergence > 0f && num2 - this._prevDivergence > 0.00015f)
			{
				this._enhancedTime = Time.time + 0.2f;
			}
			if (Time.time < this._enhancedTime)
			{
				float num8 = num3 * 0.7f;
				float num9 = ((num2 < 0f) ? (-num2) : num2);
				if (num9 < num8)
				{
					num7 = 2.5f - 1.5f * num9 / num8;
				}
			}
		}
		this._prevDivergence = num2;
		float num10 = 1f;
		float num11 = 0f;
		num = num10;
		num2 = num11;
		this._p[0].x = num;
		this._p[0].y = num2;
		for (int j = 1; j < 11; j++)
		{
			float num12 = this._p[j].x * num6;
			float num13 = this._p[j].y * num6 * num7;
			if (this._isHigh && num7 > 1f && num12 > 0f)
			{
				float num14 = num13 / num12;
				num14 = 1f + num14 * num14 * (1f - num7 * num7);
				if (num14 <= 0f)
				{
					num12 = 0f;
				}
				else
				{
					num12 *= Mathf.Sqrt(num14);
				}
			}
			num4 = num10 + num12;
			float num5 = num11 + num13;
			num4 = Mathf.Clamp(num4, 0f, 1f);
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			else if (num5 > 1f)
			{
				num4 = num + (num4 - num) * (1f - num2) / (num5 - num2);
				num5 = 1f;
			}
			this._p[j].x = num4;
			this._p[j].y = num5;
			num = num4;
			num2 = num5;
		}
		int num15 = 10;
		float num16 = (this._p[num15].x - this._p[0].x) / (float)num15;
		this._points[0] = this._p[0];
		this._points[num15] = this._p[num15];
		int num17 = 1;
		num = this._p[0].x;
		num2 = this._p[0].y;
		float num18 = this._p[1].x;
		float num19 = this._p[1].y;
		num4 = num;
		for (int k = 1; k < num15; k++)
		{
			num4 += num16;
			if (num17 < num15 && num18 > num4)
			{
				while (num17 < num15 && this._p[++num17].x > num4)
				{
				}
				num = num18;
				num2 = num19;
				num18 = this._p[num17].x;
				num19 = this._p[num17].y;
			}
			this._points[k].x = num4;
			this._points[k].y = num2 + (num19 - num2) * (num4 - num) / (num18 - num);
		}
	}

	private bool InState(FeederFishingIndicator.States s)
	{
		return (this._state & s) == s;
	}

	private void Clear(ref RectTransform[] rtData)
	{
		if (rtData != null)
		{
			for (int i = 0; i < rtData.Length; i++)
			{
				Object.Destroy(rtData[i].gameObject);
			}
			rtData = null;
		}
	}

	private Vector2 CalcPos(Vector2 v2)
	{
		return new Vector2(v2.x * this._width - this._widthHalf, this._heightHalf - v2.y * this._height);
	}

	private void CreateLines()
	{
		if (this._linesRectTransform == null)
		{
			this._linesRectTransform = new RectTransform[this._curPointsCount - 1];
		}
		for (int i = 0; i < this._linesRectTransform.Length; i++)
		{
			this.Add(new GameObject
			{
				name = string.Format("line {0}_{1}", i, i + 1)
			}, this._spLine, this._linesRectTransform, i);
		}
	}

	private void SetColor()
	{
		if (GameFactory.Player.Rod != null)
		{
			AssembledRod assembledRod = GameFactory.Player.Rod.AssembledRod;
			if (assembledRod != null && assembledRod.Quiver != null)
			{
				QuiverTip quiver = assembledRod.Quiver;
				if (!this._colors.ContainsKey(quiver.Color) || this._color != this._colors[quiver.Color])
				{
				}
			}
		}
	}

	private void UpdateColors(RectTransform[] rtData)
	{
		if (rtData != null)
		{
			for (int i = 0; i < rtData.Length; i++)
			{
				rtData[i].GetComponent<Image>().color = this._color;
			}
		}
	}

	private void Player_OnChangeRod(AssembledRod rod)
	{
		this.SetColor();
	}

	private RectTransform Add(GameObject go, Sprite sp, RectTransform[] rtData, int i)
	{
		Image image = go.AddComponent<Image>();
		image.raycastTarget = false;
		image.color = this._color;
		image.sprite = sp;
		RectTransform component = go.GetComponent<RectTransform>();
		component.SetParent(this._rt);
		rtData[i] = component;
		float num;
		if (i < 3)
		{
			num = 12f;
		}
		else if (i < 6)
		{
			num = 10f;
		}
		else
		{
			num = 8f;
			image.color = Color.red;
		}
		component.sizeDelta = new Vector2(component.rect.width, num);
		if (i == 2 || i == 5 || i == 9)
		{
			GameObject gameObject = new GameObject
			{
				name = string.Format("circle {0}", i)
			};
			Image image2 = gameObject.AddComponent<Image>();
			image2.color = image.color;
			image2.sprite = this._spLineCircle;
			RectTransform component2 = gameObject.GetComponent<RectTransform>();
			component2.SetParent(component);
			component2.sizeDelta = new Vector2(2f, num / 2f);
			this._circlesRectTransform[i] = component2;
		}
		return rtData[i];
	}

	private void StopHiding()
	{
		this._time = 0f;
		if (this.InState(FeederFishingIndicator.States.IsActive))
		{
			base.Show();
		}
	}

	[SerializeField]
	private float _lineCircleOffsetY;

	[SerializeField]
	private RectTransform _rt;

	[SerializeField]
	private Sprite _spLineCircle;

	[SerializeField]
	private Sprite _spLine;

	[SerializeField]
	private float _lineHeight = 1f;

	[SerializeField]
	private float _notActiveHideTime = 2f;

	[SerializeField]
	private float _quiverPart = 1f;

	private RectTransform[] _linesRectTransform;

	private Dictionary<int, RectTransform> _circlesRectTransform = new Dictionary<int, RectTransform>();

	private readonly Dictionary<QuiverTipColor, Color> _colors = new Dictionary<QuiverTipColor, Color>
	{
		{
			QuiverTipColor.White,
			Color.white
		},
		{
			QuiverTipColor.Red,
			Color.red
		},
		{
			QuiverTipColor.Orange,
			new Color32(254, 161, 0, 1)
		},
		{
			QuiverTipColor.Yellow,
			Color.yellow
		},
		{
			QuiverTipColor.Green,
			Color.green
		},
		{
			QuiverTipColor.Blue,
			Color.blue
		},
		{
			QuiverTipColor.Purple,
			new Color32(143, 0, 254, 1)
		}
	};

	private Color _color = Color.white;

	private float _width;

	private float _height;

	private float _widthHalf;

	private float _heightHalf;

	private int _curPointsCount;

	private FeederFishingIndicator.States _state;

	private float _time;

	private const int _pointsCount = 11;

	private float[] _tBezier;

	private Vector2[] _p;

	private Vector2[] _points;

	private const float _smallDivergence = 0.7f;

	private const float _highSensitivity = 2.5f;

	private const float _deltaDivergence = 0.00015f;

	private const float _enhancedPeriod = 0.2f;

	private float _enhancedTime;

	private bool _isHigh;

	private float _prevDivergence;

	[Flags]
	private enum States : byte
	{
		None = 0,
		IsActive = 1
	}
}
