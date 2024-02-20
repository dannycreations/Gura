using System;
using UnityEngine;

public class Freeflight : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetAxis("Vertical") != 0f)
		{
			float axis = Input.GetAxis("Vertical");
			base.transform.localPosition += base.transform.localRotation * new Vector3(0f, 0f, axis) * this.moveSpeed * Time.deltaTime;
		}
		if (Input.GetAxis("Horizontal") != 0f)
		{
			float axis2 = Input.GetAxis("Horizontal");
			base.transform.localPosition += base.transform.localRotation * new Vector3(axis2, 0f, 0f) * this.moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey(101))
		{
			base.transform.localPosition += Vector3.up * this.moveSpeed * Time.deltaTime * 0.6f;
		}
		else if (Input.GetKey(113))
		{
			base.transform.localPosition += Vector3.down * this.moveSpeed * Time.deltaTime * 0.6f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			this.moveSpeed = Mathf.Clamp(this.moveSpeed + Input.GetAxis("Mouse ScrollWheel") * 2f, 0.01f, 10f);
		}
		if (this.controlAxis == Freeflight.ControlAxis.MouseXandY)
		{
			this.rotationX += Input.GetAxis("Mouse X") * this.sensitivityX;
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
			this.Adjust360andClamp();
			this.KeyLookAround();
			this.KeyLookUp();
		}
		else if (this.controlAxis == Freeflight.ControlAxis.MouseX)
		{
			this.rotationX += Input.GetAxis("Mouse X") * this.sensitivityX;
			this.Adjust360andClamp();
			this.KeyLookAround();
			this.KeyLookUp();
		}
		else
		{
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
			this.Adjust360andClamp();
			this.KeyLookAround();
			this.KeyLookUp();
		}
	}

	private void KeyLookAround()
	{
		this.Adjust360andClamp();
		base.transform.localRotation = Quaternion.AngleAxis(this.rotationX, Vector3.up);
	}

	private void KeyLookUp()
	{
		this.Adjust360andClamp();
		base.transform.localRotation *= Quaternion.AngleAxis(this.rotationY, Vector3.left);
	}

	private void Adjust360andClamp()
	{
		if (this.rotationX < -360f)
		{
			this.rotationX += 360f;
		}
		else if (this.rotationX > 360f)
		{
			this.rotationX -= 360f;
		}
		if (this.rotationY < -360f)
		{
			this.rotationY += 360f;
		}
		else if (this.rotationY > 360f)
		{
			this.rotationY -= 360f;
		}
		this.rotationX = Mathf.Clamp(this.rotationX, this.minimumX, this.maximumX);
		this.rotationY = Mathf.Clamp(this.rotationY, this.minimumY, this.maximumY);
	}

	private void Start()
	{
		if (base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	private Freeflight.ControlAxis controlAxis;

	[SerializeField]
	private float moveSpeed = 3f;

	[SerializeField]
	private float sensitivityX = 3f;

	[SerializeField]
	private float sensitivityY = 3f;

	[SerializeField]
	private float minimumX = -360f;

	[SerializeField]
	private float maximumX = 360f;

	[SerializeField]
	private float minimumY = -90f;

	[SerializeField]
	private float maximumY = 90f;

	private float rotationX;

	private float rotationY;

	private enum ControlAxis
	{
		MouseXandY,
		MouseX,
		MouseY
	}
}
