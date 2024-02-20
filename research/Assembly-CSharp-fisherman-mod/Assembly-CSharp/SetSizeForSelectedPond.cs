using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetSizeForSelectedPond : MonoBehaviour
{
	private Toggle Tgl
	{
		get
		{
			if (this._tgl == null)
			{
				this._tgl = base.GetComponent<Toggle>();
			}
			return this._tgl;
		}
	}

	private void Start()
	{
		if (this.TextsToScale.Length != 0)
		{
			this._textNormal = this.TextsToScale[0].fontSize;
			this._textScaled = this.ScaleCoef * this._textNormal;
		}
	}

	private void Update()
	{
		if (this.Tgl.isOn)
		{
			this.SetObjectsScale(Vector3.one * this.ScaleCoef);
			this.SetTextsScale((int)this._textScaled);
		}
		else if (this.Tgl.group != null && this.Tgl.group.AnyTogglesOn())
		{
			this.SetObjectsScale(Vector3.one);
			this.SetTextsScale((int)this._textNormal);
		}
	}

	private void SetObjectsScale(Vector3 scale)
	{
		for (int i = 0; i < this.ObjectsToScale.Length; i++)
		{
			this.ObjectsToScale[i].localScale = scale;
		}
	}

	private void SetTextsScale(int fontSize)
	{
		for (int i = 0; i < this.TextsToScale.Length; i++)
		{
			this.TextsToScale[i].fontSize = (float)fontSize;
		}
	}

	public Transform[] ObjectsToScale;

	public TextMeshProUGUI[] TextsToScale;

	public float ScaleCoef = 1.5f;

	private float _textScaled;

	private float _textNormal;

	private Toggle _tgl;
}
