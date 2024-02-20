using System;
using UnityEngine;

public class DebugMouseLook : MonoBehaviour
{
	private void Update()
	{
		if (!Input.GetKey(this._enableBtn) && !Input.GetMouseButton((int)((byte)this._enableMouseButton)))
		{
			return;
		}
		if (this.axes == DebugMouseLook.RotationAxes.MouseXAndY)
		{
			float num = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * this.sensitivityX;
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
			this.rotationY = Mathf.Clamp(this.rotationY, this.minimumY, this.maximumY);
			base.transform.localEulerAngles = new Vector3(-this.rotationY, num, 0f);
		}
		else if (this.axes == DebugMouseLook.RotationAxes.MouseX)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X") * this.sensitivityX, 0f);
		}
		else
		{
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
			this.rotationY = Mathf.Clamp(this.rotationY, this.minimumY, this.maximumY);
			base.transform.localEulerAngles = new Vector3(-this.rotationY, base.transform.localEulerAngles.y, 0f);
		}
	}

	public DebugMouseLook.RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	private float rotationY;

	[SerializeField]
	private DebugMouseLook.MouseButton _enableMouseButton = DebugMouseLook.MouseButton.Right;

	[SerializeField]
	private KeyCode _enableBtn = 308;

	public enum MouseButton
	{
		Left,
		Right,
		Middle
	}

	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}
}
