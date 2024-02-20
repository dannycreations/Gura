using System;
using UnityEngine;

public class RodPodInclesIndicator : MonoBehaviour
{
	private void Awake()
	{
		this._rightRoot = Object.Instantiate<Transform>(this._leftRoot, this._leftRoot.parent);
		this._rightRoot.name = "right";
		this._rightRoot.rotation *= Quaternion.AngleAxis(180f, Vector3.up);
		Transform transform = this._rightRoot.Find("scaler");
		this._ups = new SpriteRenderer[]
		{
			this._up,
			this.FindTwin(transform, this._up)
		};
		this._downs = new SpriteRenderer[]
		{
			this._down,
			this.FindTwin(transform, this._down)
		};
		this._upLimits = new SpriteRenderer[]
		{
			this._upLimit,
			this.FindTwin(transform, this._upLimit)
		};
		this._downLimits = new SpriteRenderer[]
		{
			this._downLimit,
			this.FindTwin(transform, this._downLimit)
		};
	}

	private SpriteRenderer FindTwin(Transform parent, SpriteRenderer first)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i).name == first.name)
			{
				return parent.GetChild(i).GetComponent<SpriteRenderer>();
			}
		}
		return null;
	}

	public void Init(float startAngle, float minAngle, float maxAngle, float width, Vector3 localPosition, float scale)
	{
		this._curAngle = startAngle;
		this._minAngle = minAngle;
		this._maxAngle = maxAngle;
		this._angleRange = this._maxAngle - this._minAngle;
		base.transform.localPosition = localPosition;
		this._leftRoot.localPosition = new Vector3(-width, 0f, 0f);
		this._rightRoot.localPosition = new Vector3(width, 0f, 0f);
		for (int i = 0; i < this._ups.Length; i++)
		{
			this._ups[i].transform.parent.localScale = new Vector3(scale, scale, scale);
		}
		this.UpdateAngle(startAngle);
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public void UpdateAngle(float newAngle)
	{
		float num = newAngle - this._curAngle;
		if (num > 0f)
		{
			this._curAngle = Mathf.Min(newAngle, this._maxAngle);
			float num2 = (this._maxAngle - this._curAngle) / this._angleRange;
			this.ApplyAngle(this._ups, num * this._indirectSpeedK, num2);
			this.ApplyAngle(this._downs, num, 1f - num2);
		}
		else
		{
			this._curAngle = Mathf.Max(newAngle, this._minAngle);
			float num3 = (this._maxAngle - this._curAngle) / this._angleRange;
			this.ApplyAngle(this._ups, num, num3);
			this.ApplyAngle(this._downs, num * this._indirectSpeedK, 1f - num3);
		}
		for (int i = 0; i < this._upLimits.Length; i++)
		{
			this._downLimits[i].color = (Mathf.Approximately(this._curAngle, this._maxAngle) ? this._limitValueColor : this._downDefaultColor);
		}
		for (int j = 0; j < this._downLimits.Length; j++)
		{
			this._upLimits[j].color = (Mathf.Approximately(this._curAngle, this._minAngle) ? this._limitValueColor : this._upDefaultColor);
		}
	}

	private void ApplyAngle(SpriteRenderer[] dir, float dAngle, float norm)
	{
		float num = Mathf.Lerp(this._minAngleAlpha, this._maxAngleAlpha, norm);
		Color color = dir[0].color;
		Color color2;
		color2..ctor(color.r, color.g, color.b, num);
		dir[0].transform.Rotate(Vector3.forward, dAngle);
		dir[1].transform.Rotate(Vector3.forward, -dAngle);
		dir[0].color = color2;
		dir[1].color = color2;
	}

	[SerializeField]
	private Transform _leftRoot;

	[SerializeField]
	private SpriteRenderer _up;

	[SerializeField]
	private SpriteRenderer _down;

	[SerializeField]
	private SpriteRenderer _upLimit;

	[SerializeField]
	private SpriteRenderer _downLimit;

	[SerializeField]
	private Color _upDefaultColor = new Color(0.3294f, 0.8627f, 1f);

	[SerializeField]
	private Color _downDefaultColor = new Color(0.6078f, 1f, 0.1373f);

	[SerializeField]
	private Color _limitValueColor = new Color(0.8314f, 0.1059f, 0f);

	[SerializeField]
	private float _minAngleAlpha = 0.1f;

	[SerializeField]
	private float _maxAngleAlpha = 0.6f;

	[SerializeField]
	private float _indirectSpeedK = 0.25f;

	private Transform _rightRoot;

	private SpriteRenderer[] _ups;

	private SpriteRenderer[] _downs;

	private SpriteRenderer[] _upLimits;

	private SpriteRenderer[] _downLimits;

	private float _curAngle;

	private float _minAngle;

	private float _maxAngle;

	private float _angleRange;
}
