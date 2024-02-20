using System;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
	private Color FromColor
	{
		get
		{
			return (this._isCorrect != null && !this._isCorrect.Value) ? this.correctCast : this.incorrectCast;
		}
	}

	private Color TargetColor
	{
		get
		{
			return (this._isCorrect != null && !this._isCorrect.Value) ? this.incorrectCast : this.correctCast;
		}
	}

	internal void Start()
	{
		float num = 100f * this.Value;
		this.PrecisionCircle.localScale = new Vector3(num, this.PrecisionCircle.localScale.y, num);
		this._precisionCircleMaterial = this.GetMaterial(this.PrecisionCircle);
		this._precisionCenterMaterial = this.GetMaterial(this.PrecisionCentre);
	}

	private Material GetMaterial(Transform t)
	{
		MeshRenderer component = t.GetComponent<MeshRenderer>();
		return component.material;
	}

	internal void Update()
	{
		float num = 100f * this.Value;
		this.PrecisionCircle.localScale = new Vector3(num, this.PrecisionCircle.localScale.y, num);
		this.PrecisionCentre.localScale = new Vector3(num / 3f, this.PrecisionCentre.localScale.y, num / 3f);
		if (this._switchStartedAt > 0f)
		{
			float num2 = (Time.time - this._switchStartedAt) / this._colorSwitchTime;
			if (num2 > 1f)
			{
				num2 = 1f;
				this._switchStartedAt = -1f;
			}
			Color color = Color.Lerp(this.FromColor, this.TargetColor, num2);
			this._precisionCircleMaterial.SetColor("_TintColor", color);
			this._precisionCenterMaterial.SetColor("_TintColor", color);
		}
	}

	public void SetType(bool isCorrect)
	{
		if (this._isCorrect != null)
		{
			if (this._isCorrect.Value != isCorrect)
			{
				this._isCorrect = new bool?(isCorrect);
				if (this._switchStartedAt > 0f)
				{
					this._switchStartedAt = Time.time - (this._colorSwitchTime - (Time.time - this._switchStartedAt));
				}
				else
				{
					this._switchStartedAt = Time.time;
				}
			}
		}
		else
		{
			this._isCorrect = new bool?(isCorrect);
			Color targetColor = this.TargetColor;
			this._precisionCircleMaterial.SetColor("_TintColor", targetColor);
			this._precisionCenterMaterial.SetColor("_TintColor", targetColor);
			this._switchStartedAt = -1f;
		}
	}

	public void SetActive(bool flag)
	{
		if (flag != base.isActiveAndEnabled)
		{
			base.gameObject.SetActive(flag);
			if (flag)
			{
				this._isCorrect = null;
			}
		}
	}

	public Transform PrecisionCircle;

	public Transform PrecisionCentre;

	public Color correctCast;

	public Color incorrectCast;

	[SerializeField]
	private float _colorSwitchTime;

	private Material _precisionCircleMaterial;

	private Material _precisionCenterMaterial;

	private float _switchStartedAt = -1f;

	private bool? _isCorrect;

	public float Value;
}
