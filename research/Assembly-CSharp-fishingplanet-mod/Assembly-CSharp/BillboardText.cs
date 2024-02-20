using System;
using UnityEngine;

public class BillboardText : MonoBehaviour
{
	private void Awake()
	{
		this._text = base.GetComponent<TextMesh>();
		this._isVisible = true;
		base.transform.localScale = Vector3.one;
	}

	private void Update()
	{
		if (this.cameraToLookAt == null)
		{
			return;
		}
		if (this.isPopup && this._isVisible && this._hideTextAt < Time.time)
		{
			this._isVisible = false;
			this._text.text = string.Empty;
		}
		Vector3 vector = this.cameraToLookAt.transform.position - base.transform.position;
		float magnitude = vector.magnitude;
		float num = Math.Min(1f, magnitude / this._maxDist);
		this._text.characterSize = num * this._maxSize + (1f - num) * this._minSize;
		vector.x = (vector.z = 0f);
		base.transform.LookAt(this.cameraToLookAt.transform.position - vector);
		base.transform.Rotate(0f, 180f, 0f);
	}

	public void SetText(string text)
	{
		this._text.text = text;
		if (this.isPopup)
		{
			this._isVisible = true;
			this._hideTextAt = Time.time + this.showTime;
		}
	}

	public Camera cameraToLookAt;

	public bool isPopup;

	public float showTime = 10f;

	[SerializeField]
	private float _minSize = 0.04f;

	[SerializeField]
	private float _maxSize = 0.14f;

	[SerializeField]
	private float _maxDist = 10f;

	[SerializeField]
	private bool _isDebugInputEnabled;

	private TextMesh _text;

	private float _hideTextAt;

	private bool _isVisible;
}
