using System;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
	private void Start()
	{
		this._distanceVector = new Vector3(0f, 0f, -this._distance);
		Vector2 vector = base.transform.localEulerAngles;
		this._x = vector.x;
		this._y = vector.y;
		this.Rotate(this._x, this._y);
	}

	private void LateUpdate()
	{
		if (this._target)
		{
			this.RotateControls();
			this.Zoom();
		}
	}

	private void RotateControls()
	{
		if (Input.GetButton("Fire1"))
		{
			this._x += Input.GetAxis("Mouse X") * this._xSpeed;
			this._y += -Input.GetAxis("Mouse Y") * this._ySpeed;
			this.Rotate(this._x, this._y);
		}
	}

	private void Rotate(float x, float y)
	{
		Quaternion quaternion = Quaternion.Euler(y, x, 0f);
		Vector3 vector = quaternion * this._distanceVector + this._target.position;
		base.transform.rotation = quaternion;
		base.transform.position = vector;
	}

	private void Zoom()
	{
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.ZoomOut();
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.ZoomIn();
		}
	}

	private void ZoomIn()
	{
		this._distance -= this._zoomStep;
		this._distanceVector = new Vector3(0f, 0f, -this._distance);
		this.Rotate(this._x, this._y);
	}

	private void ZoomOut()
	{
		this._distance += this._zoomStep;
		this._distanceVector = new Vector3(0f, 0f, -this._distance);
		this.Rotate(this._x, this._y);
	}

	public Transform _target;

	public float _distance = 20f;

	public float _zoomStep = 1f;

	public float _xSpeed = 1f;

	public float _ySpeed = 1f;

	private float _x;

	private float _y;

	private Vector3 _distanceVector;
}
