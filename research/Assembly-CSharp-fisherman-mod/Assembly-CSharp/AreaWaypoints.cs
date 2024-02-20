using System;
using UnityEngine;

public class AreaWaypoints
{
	public AreaWaypoints(SchoolController controller, float initialSideK, float initialCoastK)
	{
		this._controller = controller;
		this._zoneWaypoint = new TriangleMovement(initialSideK, initialCoastK);
	}

	public void SetRandomMainWaypoint()
	{
		if (this.randomWaypointsFrom < Time.time)
		{
			this._zoneWaypoint.SetRandom();
		}
	}

	public void SetTimedMainWaypoint(float sideK, float coastK)
	{
		this._zoneWaypoint.Set(sideK, coastK);
		this.randomWaypointsFrom = Time.time + 10f;
	}

	public Vector3 GenerateFishWaypoint()
	{
		float value = Random.value;
		float value2 = Random.value;
		float value3 = Random.value;
		float num = (1f - this._controller._groupWidthPrc) * this._zoneWaypoint.SideK;
		float num2 = (Mathf.Sign(this._zoneWaypoint.SideK) * this._controller._groupWidthPrc * (value - 0.5f) * 2f + num) * this._controller._areaHalfWidth;
		float num3 = (1f - this._controller._groupCoastDistPrc) * this._zoneWaypoint.CoastK;
		float num4 = (this._controller._groupCoastDistPrc * value2 + num3) * this._controller._areaCoastDist;
		float num5 = num4 * Mathf.Tan(this._controller._areaAngle * 0.017453292f) * value3;
		Vector3 vector;
		vector..ctor(num2, -num5, num4);
		return this._controller.transform.TransformPoint(vector);
	}

	public void DrawGizmos()
	{
		this.DrawAreaGizmo(this._controller._areaHalfWidth, this._controller._areaCoastDist, Vector3.zero, Color.green);
		float num;
		float num2;
		if (!Application.isPlaying)
		{
			num = (1f - this._controller._groupWidthPrc) * this._controller._groupInitialPosSidePrc;
			num2 = (1f - this._controller._groupCoastDistPrc) * this._controller._groupInitialPosCoastPrc;
		}
		else
		{
			num = (1f - this._controller._groupWidthPrc) * this._zoneWaypoint.SideK;
			num2 = (1f - this._controller._groupCoastDistPrc) * this._zoneWaypoint.CoastK;
		}
		this.DrawAreaGizmo(this._controller._areaHalfWidth * this._controller._groupWidthPrc, this._controller._areaCoastDist * this._controller._groupCoastDistPrc, new Vector3(this._controller._areaHalfWidth * num, 0f, this._controller._areaCoastDist * num2), Color.gray);
	}

	private void DrawAreaGizmo(float halfWidth, float coastDist, Vector3 localMovement, Color color)
	{
		Vector3 vector = this._controller.transform.TransformDirection(localMovement);
		Vector3 vector2 = this._controller.transform.TransformPoint(new Vector3(-halfWidth, 0f, 0f)) + vector;
		Vector3 vector3 = this._controller.transform.TransformPoint(new Vector3(halfWidth, 0f, 0f)) + vector;
		Vector3 vector4 = this._controller.transform.TransformPoint(new Vector3(-halfWidth, 0f, coastDist)) + vector;
		Vector3 vector5 = this._controller.transform.TransformPoint(new Vector3(halfWidth, 0f, coastDist)) + vector;
		float num = coastDist * Mathf.Tan(this._controller._areaAngle * 0.017453292f);
		Vector3 vector6 = this._controller.transform.TransformPoint(new Vector3(-halfWidth, -num, coastDist)) + vector;
		Vector3 vector7 = this._controller.transform.TransformPoint(new Vector3(halfWidth, -num, coastDist)) + vector;
		Gizmos.color = color;
		Gizmos.DrawLine(vector2, vector4);
		Gizmos.DrawLine(vector2, vector6);
		Gizmos.DrawLine(vector4, vector6);
		Gizmos.DrawLine(vector3, vector7);
		Gizmos.DrawLine(vector3, vector5);
		Gizmos.DrawLine(vector5, vector7);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector4, vector5);
		Gizmos.DrawLine(vector6, vector7);
	}

	private const int DEBUG_POINTS_COUNT = 100;

	private const float DEBUG_POINTS_R = 0.02f;

	private const float TIMED_POINT_DELAY = 10f;

	private SchoolController _controller;

	private TriangleMovement _zoneWaypoint;

	private float randomWaypointsFrom;
}
