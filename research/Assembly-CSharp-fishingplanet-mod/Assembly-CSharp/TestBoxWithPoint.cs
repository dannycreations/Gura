using System;
using ObjectModel;
using UnityEngine;

public class TestBoxWithPoint : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		Vector3 position = this.Box.transform.position;
		Vector3 eulerAngles = this.Box.transform.rotation.eulerAngles;
		Vector3 localScale = this.Box.transform.localScale;
		Point3 point = this.OutsidePoint.position.ToPoint3();
		if (localScale.x == localScale.y && localScale.x == localScale.z)
		{
			this.StartPoint.position = this.Box.transform.position;
			this.EndPoint.position = this.Box.transform.position;
		}
		else
		{
			Quaternion quaternion = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
			float num = Mathf.Min(new float[] { localScale.x, localScale.y, localScale.z }) / 2f;
			Vector3 vector;
			Vector3 vector2;
			if (localScale.x > localScale.y && localScale.x > localScale.z)
			{
				vector..ctor(localScale.x / 2f - num, 0f, 0f);
				vector2..ctor(-vector.x, 0f, 0f);
			}
			else if (localScale.x > localScale.y && localScale.x > localScale.z)
			{
				vector..ctor(0f, localScale.y / 2f - num, 0f);
				vector2..ctor(0f, -vector.y, 0f);
			}
			else
			{
				vector..ctor(0f, 0f, localScale.z / 2f - num);
				vector2..ctor(0f, 0f, -vector.z);
			}
			vector = position + quaternion * vector;
			vector2 = position + quaternion * vector2;
			this.StartPoint.position = vector;
			this.EndPoint.position = vector2;
		}
		this.PointOnMainAxis.position = Math3d.GetBoxNearestCenterPoint(this.Box.position, this.Box.rotation.eulerAngles, localScale, this.OutsidePoint.position);
		Box box = new Box(position.ToPoint3(), localScale.ToPoint3(), eulerAngles.ToPoint3());
		this.MinDistance = box.Distance(point);
		bool flag = box.Contains(point);
		if (flag && !this.IsInside)
		{
			Debug.Log("ENTER");
		}
		if (!flag && this.IsInside)
		{
			Debug.Log("EXIT");
		}
		this.IsInside = flag;
	}

	public Transform Box;

	public Transform OutsidePoint;

	public Transform PointOnMainAxis;

	public Transform StartPoint;

	public Transform EndPoint;

	public float MinDistance;

	public bool IsInside;
}
