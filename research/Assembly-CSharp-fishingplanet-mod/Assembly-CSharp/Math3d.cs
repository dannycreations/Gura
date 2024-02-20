using System;
using System.Collections.Generic;
using System.Globalization;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using UnityEngine;

public class Math3d : MonoBehaviour
{
	public static void Init()
	{
		Math3d.tempChild = new GameObject("Math3d_TempChild").transform;
		Math3d.tempParent = new GameObject("Math3d_TempParent").transform;
		Math3d.tempChild.gameObject.hideFlags = 61;
		Object.DontDestroyOnLoad(Math3d.tempChild.gameObject);
		Math3d.tempParent.gameObject.hideFlags = 61;
		Object.DontDestroyOnLoad(Math3d.tempParent.gameObject);
		Math3d.tempChild.parent = Math3d.tempParent;
	}

	public static float SpeedToDrag(float speed)
	{
		return Math3d.SpeedToDrag(speed, 9.81f);
	}

	public static float DragToSpeed(float drag)
	{
		return Math3d.DragToSpeed(drag, 9.81f);
	}

	public static float SpeedToDrag(float speed, float acceleration)
	{
		return acceleration / (speed + 0.194f);
	}

	public static float DragToSpeed(float drag, float acceleration)
	{
		return acceleration / drag - 0.194f;
	}

	public static Vector3 AddVectorLength(Vector3 vector, float size)
	{
		float num = Vector3.Magnitude(vector);
		num += size;
		Vector3 vector2 = Vector3.Normalize(vector);
		return Vector3.Scale(vector2, new Vector3(num, num, num));
	}

	public static Vector3 SetVectorLength(Vector3 vector, float size)
	{
		Vector3 vector2 = Vector3.Normalize(vector);
		return vector2 * size;
	}

	public static Quaternion SubtractRotation(Quaternion B, Quaternion A)
	{
		return Quaternion.Inverse(A) * B;
	}

	public static Vector3 PlaneNormalFrom3Points(Vector3 pointA, Vector3 pointB, Vector3 pointC)
	{
		Vector3 vector = pointB - pointA;
		Vector3 vector2 = pointC - pointA;
		return Vector3.Normalize(Vector3.Cross(vector, vector2));
	}

	public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
	{
		linePoint = Vector3.zero;
		lineVec = Vector3.zero;
		lineVec = Vector3.Cross(plane1Normal, plane2Normal);
		Vector3 vector = Vector3.Cross(plane2Normal, lineVec);
		float num = Vector3.Dot(plane1Normal, vector);
		if (Mathf.Abs(num) > 0.006f)
		{
			Vector3 vector2 = plane1Position - plane2Position;
			float num2 = Vector3.Dot(plane1Normal, vector2) / num;
			linePoint = plane2Position + num2 * vector;
			return true;
		}
		return false;
	}

	public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
	{
		intersection = Vector3.zero;
		float num = Vector3.Dot(planePoint - linePoint, planeNormal);
		float num2 = Vector3.Dot(lineVec, planeNormal);
		if (num2 != 0f)
		{
			float num3 = num / num2;
			Vector3 vector = Math3d.SetVectorLength(lineVec, num3);
			intersection = linePoint + vector;
			return true;
		}
		return false;
	}

	public static bool LinePlaneIntersection4f(out Vector4f intersection, Vector4f linePoint, Vector4f lineVec, Vector4f planeNormal, Vector4f planePoint)
	{
		intersection = Vector4f.Zero;
		float num = Vector4fExtensions.Dot(planePoint - linePoint, planeNormal);
		float num2 = Vector4fExtensions.Dot(lineVec, planeNormal);
		if (num2 != 0f)
		{
			float num3 = num / num2;
			Vector4f vector4f = lineVec * new Vector4f(num3);
			intersection = linePoint + vector4f;
			return true;
		}
		return false;
	}

	public static bool SegmentPlaneIntersection4f(out Vector4f intersection, Vector4f segmentA, Vector4f segmentB, Vector4f planeNormal, Vector4f planePoint)
	{
		intersection = Vector4f.Zero;
		Vector4f vector4f = segmentB - segmentA;
		float num = (segmentB - segmentA).Magnitude();
		vector4f /= new Vector4f(num);
		float num2 = Vector4fExtensions.Dot(planePoint - segmentA, planeNormal);
		float num3 = Vector4fExtensions.Dot(vector4f, planeNormal);
		if (num3 == 0f)
		{
			return false;
		}
		float num4 = num2 / num3;
		if (num4 > num)
		{
			intersection = segmentB;
			return false;
		}
		Vector4f vector4f2 = vector4f.Normalized() * new Vector4f(num4);
		intersection = segmentA + vector4f2;
		return true;
	}

	public static bool SegmentPlaneIntersection(out Vector3 intersection, Vector3 segmentA, Vector3 segmentB, Vector3 planeNormal, Vector3 planePoint)
	{
		intersection = Vector3.zero;
		Vector3 vector = segmentB - segmentA;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		float num = Vector3.Dot(planePoint - segmentA, planeNormal);
		float num2 = Vector3.Dot(vector, planeNormal);
		if (num2 == 0f)
		{
			return false;
		}
		float num3 = num / num2;
		if (num3 > magnitude)
		{
			intersection = segmentB;
			return false;
		}
		Vector3 vector2 = vector.normalized * num3;
		intersection = segmentA + vector2;
		return true;
	}

	public static bool SegmentSegmentIntersectionParams2D(Vector2 s1start, Vector2 s1end, Vector2 s2start, Vector2 s2end, out float t1, out float t2)
	{
		float x = s1start.x;
		float x2 = s1end.x;
		float x3 = s2start.x;
		float x4 = s2end.x;
		float y = s1start.y;
		float y2 = s1end.y;
		float y3 = s2start.y;
		float y4 = s2end.y;
		float num = 1f / ((x - x2) * (y3 - y4) - (y - y2) * (x3 - x4));
		if (!Mathf.Approximately(num, 0f))
		{
			t1 = ((x - x3) * (y3 - y4) - (y - y3) * (x3 - x4)) * num;
			t2 = ((x - x2) * (y - y3) - (y - y2) * (x - x3)) * num;
			return true;
		}
		t1 = 0f;
		t2 = 0f;
		return false;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		intersection = Vector3.zero;
		Vector3 vector = linePoint2 - linePoint1;
		Vector3 vector2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 vector3 = Vector3.Cross(vector, lineVec2);
		float num = Vector3.Dot(vector, vector2);
		if (num >= 1E-05f || num <= -1E-05f)
		{
			return false;
		}
		float num2 = Vector3.Dot(vector3, vector2) / vector2.sqrMagnitude;
		if (num2 >= 0f && num2 <= 1f)
		{
			intersection = linePoint1 + lineVec1 * num2;
			return true;
		}
		return false;
	}

	public static int SegmentSphereIntersection(Vector4f segA, Vector4f segB, Vector4f sphereCenter, float sphereRadius, out float i1, out float i2)
	{
		Vector4f vector4f = segA - sphereCenter;
		Vector4f vector4f2 = segB - segA;
		float num = vector4f2.SqrMagnitude();
		float num2 = 2f * Vector4fExtensions.Dot(vector4f2, vector4f);
		float num3 = vector4f.SqrMagnitude() - sphereRadius * sphereRadius;
		float num4 = num2 * num2 - 4f * num * num3;
		if (num4 < 0f)
		{
			i1 = 0f;
			i2 = 0f;
			return 0;
		}
		if (Mathf.Approximately(num4, 0f))
		{
			i1 = -0.5f * num2 / num;
			i2 = 0f;
			return 1;
		}
		num4 = Mathf.Sqrt(num4);
		i1 = 0.5f * (-num2 - num4) / num;
		i2 = 0.5f * (-num2 + num4) / num;
		return 2;
	}

	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;
		float num = Vector3.Dot(lineVec1, lineVec1);
		float num2 = Vector3.Dot(lineVec1, lineVec2);
		float num3 = Vector3.Dot(lineVec2, lineVec2);
		float num4 = num * num3 - num2 * num2;
		if (num4 != 0f)
		{
			Vector3 vector = linePoint1 - linePoint2;
			float num5 = Vector3.Dot(lineVec1, vector);
			float num6 = Vector3.Dot(lineVec2, vector);
			float num7 = (num2 * num6 - num5 * num3) / num4;
			float num8 = (num * num6 - num5 * num2) / num4;
			closestPointLine1 = linePoint1 + lineVec1 * num7;
			closestPointLine2 = linePoint2 + lineVec2 * num8;
			return true;
		}
		return false;
	}

	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
	{
		Vector3 vector = point - linePoint;
		float num = Vector3.Dot(vector, lineVec);
		return linePoint + lineVec * num;
	}

	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{
		Vector3 vector = Math3d.ProjectPointOnLine(linePoint1, (linePoint2 - linePoint1).normalized, point);
		int num = Math3d.PointOnWhichSideOfLineSegment(linePoint1, linePoint2, vector);
		if (num == 0)
		{
			return vector;
		}
		if (num == 1)
		{
			return linePoint1;
		}
		if (num == 2)
		{
			return linePoint2;
		}
		return Vector3.zero;
	}

	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		float num = Math3d.SignedDistancePlanePoint(planeNormal, planePoint, point);
		num *= -1f;
		Vector3 vector = Math3d.SetVectorLength(planeNormal, num);
		return point + vector;
	}

	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{
		return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
	}

	public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		return Vector3.Dot(planeNormal, point - planePoint);
	}

	public static float SignedDotProduct(Vector3 vectorA, Vector3 vectorB, Vector3 normal)
	{
		Vector3 vector = Vector3.Cross(normal, vectorA);
		return Vector3.Dot(vector, vectorB);
	}

	public static float SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector, Vector3 normal)
	{
		Vector3 vector = Vector3.Cross(normal, referenceVector);
		float num = Vector3.Angle(referenceVector, otherVector);
		return num * Mathf.Sign(Vector3.Dot(vector, otherVector));
	}

	public static float AngleVectorPlane(Vector3 vector, Vector3 normal)
	{
		float num = Vector3.Dot(vector, normal);
		float num2 = (float)Math.Acos((double)num);
		return 1.5707964f - num2;
	}

	public static float DotProductAngle(Vector3 vec1, Vector3 vec2)
	{
		double num = (double)Vector3.Dot(vec1, vec2);
		if (num < -1.0)
		{
			num = -1.0;
		}
		if (num > 1.0)
		{
			num = 1.0;
		}
		double num2 = Math.Acos(num);
		return (float)num2;
	}

	public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC)
	{
		planeNormal = Vector3.zero;
		planePoint = Vector3.zero;
		Vector3 vector = pointB - pointA;
		Vector3 vector2 = pointC - pointA;
		planeNormal = Vector3.Normalize(Vector3.Cross(vector, vector2));
		Vector3 vector3 = pointA + vector / 2f;
		Vector3 vector4 = pointA + vector2 / 2f;
		Vector3 vector5 = pointC - vector3;
		Vector3 vector6 = pointB - vector4;
		Vector3 vector7;
		Math3d.ClosestPointsOnTwoLines(out planePoint, out vector7, vector3, vector5, vector4, vector6);
	}

	public static float DistanceXZ(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static Vector3 GetForwardVector(Quaternion q)
	{
		return q * Vector3.forward;
	}

	public static Vector3 GetUpVector(Quaternion q)
	{
		return q * Vector3.up;
	}

	public static Vector3 GetRightVector(Quaternion q)
	{
		return q * Vector3.right;
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}

	public static Vector3 PositionFromMatrix(Matrix4x4 m)
	{
		Vector4 column = m.GetColumn(3);
		return new Vector3(column.x, column.y, column.z);
	}

	public static void LookRotationExtended(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 customForward, Vector3 customUp)
	{
		Quaternion quaternion = Quaternion.LookRotation(alignWithVector, alignWithNormal);
		Quaternion quaternion2 = Quaternion.LookRotation(customForward, customUp);
		gameObjectInOut.transform.rotation = quaternion * Quaternion.Inverse(quaternion2);
	}

	public static void TransformWithParent(out Quaternion childRotation, out Vector3 childPosition, Quaternion parentRotation, Vector3 parentPosition, Quaternion startParentRotation, Vector3 startParentPosition, Quaternion startChildRotation, Vector3 startChildPosition)
	{
		childRotation = Quaternion.identity;
		childPosition = Vector3.zero;
		Math3d.tempParent.rotation = startParentRotation;
		Math3d.tempParent.position = startParentPosition;
		Math3d.tempParent.localScale = Vector3.one;
		Math3d.tempChild.rotation = startChildRotation;
		Math3d.tempChild.position = startChildPosition;
		Math3d.tempChild.localScale = Vector3.one;
		Math3d.tempParent.rotation = parentRotation;
		Math3d.tempParent.position = parentPosition;
		childRotation = Math3d.tempChild.rotation;
		childPosition = Math3d.tempChild.position;
	}

	public static void PreciseAlign(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 alignWithPosition, Vector3 triangleForward, Vector3 triangleNormal, Vector3 trianglePosition)
	{
		Math3d.LookRotationExtended(ref gameObjectInOut, alignWithVector, alignWithNormal, triangleForward, triangleNormal);
		Vector3 vector = gameObjectInOut.transform.TransformPoint(trianglePosition);
		Vector3 vector2 = alignWithPosition - vector;
		gameObjectInOut.transform.Translate(vector2, 0);
	}

	private void VectorsToTransform(ref GameObject gameObjectInOut, Vector3 positionVector, Vector3 directionVector, Vector3 normalVector)
	{
		gameObjectInOut.transform.position = positionVector;
		gameObjectInOut.transform.rotation = Quaternion.LookRotation(directionVector, normalVector);
	}

	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
	{
		Vector3 vector = linePoint2 - linePoint1;
		Vector3 vector2 = point - linePoint1;
		float num = Vector3.Dot(vector2, vector);
		if (num <= 0f)
		{
			return 1;
		}
		if (vector2.magnitude <= vector.magnitude)
		{
			return 0;
		}
		return 2;
	}

	public static float MouseDistanceToLine(Vector3 linePoint1, Vector3 linePoint2)
	{
		Camera main = Camera.main;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = main.WorldToScreenPoint(linePoint1);
		Vector3 vector2 = main.WorldToScreenPoint(linePoint2);
		Vector3 vector3 = Math3d.ProjectPointOnLineSegment(vector, vector2, mousePosition);
		vector3..ctor(vector3.x, vector3.y, 0f);
		return (vector3 - mousePosition).magnitude;
	}

	public static float MouseDistanceToCircle(Vector3 point, float radius)
	{
		Camera main = Camera.main;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = main.WorldToScreenPoint(point);
		vector..ctor(vector.x, vector.y, 0f);
		float magnitude = (vector - mousePosition).magnitude;
		return magnitude - radius;
	}

	public static bool IsLineInRectangle(Vector3 linePoint1, Vector3 linePoint2, Vector3 rectA, Vector3 rectB, Vector3 rectC, Vector3 rectD)
	{
		bool flag = false;
		bool flag2 = Math3d.IsPointInRectangle(linePoint1, rectA, rectC, rectB, rectD);
		if (!flag2)
		{
			flag = Math3d.IsPointInRectangle(linePoint2, rectA, rectC, rectB, rectD);
		}
		if (!flag2 && !flag)
		{
			bool flag3 = Math3d.AreLineSegmentsCrossing(linePoint1, linePoint2, rectA, rectB);
			bool flag4 = Math3d.AreLineSegmentsCrossing(linePoint1, linePoint2, rectB, rectC);
			bool flag5 = Math3d.AreLineSegmentsCrossing(linePoint1, linePoint2, rectC, rectD);
			bool flag6 = Math3d.AreLineSegmentsCrossing(linePoint1, linePoint2, rectD, rectA);
			return flag3 || flag4 || flag5 || flag6;
		}
		return true;
	}

	public static bool IsPointInRectangle(Vector3 point, Vector3 rectA, Vector3 rectC, Vector3 rectB, Vector3 rectD)
	{
		Vector3 vector = rectC - rectA;
		float num = -(vector.magnitude / 2f);
		vector = Math3d.AddVectorLength(vector, num);
		Vector3 vector2 = rectA + vector;
		Vector3 vector3 = rectB - rectA;
		float num2 = vector3.magnitude / 2f;
		Vector3 vector4 = rectD - rectA;
		float num3 = vector4.magnitude / 2f;
		Vector3 vector5 = Math3d.ProjectPointOnLine(vector2, vector3.normalized, point);
		float magnitude = (vector5 - point).magnitude;
		vector5 = Math3d.ProjectPointOnLine(vector2, vector4.normalized, point);
		float magnitude2 = (vector5 - point).magnitude;
		return magnitude2 <= num2 && magnitude <= num3;
	}

	public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2)
	{
		Vector3 vector = pointA2 - pointA1;
		Vector3 vector2 = pointB2 - pointB1;
		Vector3 vector3;
		Vector3 vector4;
		bool flag = Math3d.ClosestPointsOnTwoLines(out vector3, out vector4, pointA1, vector.normalized, pointB1, vector2.normalized);
		if (flag)
		{
			int num = Math3d.PointOnWhichSideOfLineSegment(pointA1, pointA2, vector3);
			int num2 = Math3d.PointOnWhichSideOfLineSegment(pointB1, pointB2, vector4);
			return num == 0 && num2 == 0;
		}
		return false;
	}

	public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
	}

	public static Vector3 GetBoxNearestCenterPoint(Vector3 boxPosition, Vector3 boxRotation, Vector3 boxScale, Vector3 point)
	{
		if (boxScale.x == boxScale.y && boxScale.x == boxScale.z)
		{
			return boxPosition;
		}
		Quaternion quaternion = Quaternion.Euler(boxRotation.x, boxRotation.y, boxRotation.z);
		float num = Mathf.Min(new float[] { boxScale.x, boxScale.y, boxScale.z }) / 2f;
		Vector3 vector;
		Vector3 vector2;
		if (boxScale.x > boxScale.y && boxScale.x > boxScale.z)
		{
			vector..ctor(boxScale.x / 2f - num, 0f, 0f);
			vector2..ctor(-vector.x, 0f, 0f);
		}
		else if (boxScale.x > boxScale.y && boxScale.x > boxScale.z)
		{
			vector..ctor(0f, boxScale.y / 2f - num, 0f);
			vector2..ctor(0f, -vector.y, 0f);
		}
		else
		{
			vector..ctor(0f, 0f, boxScale.z / 2f - num);
			vector2..ctor(0f, 0f, -vector.z);
		}
		vector = boxPosition + quaternion * vector;
		vector2 = boxPosition + quaternion * vector2;
		Vector3 normalized = (vector2 - vector).normalized;
		Vector3 vector3 = Math3d.ProjectPointOnLine(vector, normalized, point);
		int num2 = Math3d.PointOnWhichSideOfLineSegment(vector, vector2, vector3);
		if (num2 == 0)
		{
			return vector3;
		}
		if (num2 == 1)
		{
			return vector;
		}
		return vector2;
	}

	public static void GetConstraintPointCollisionInfo(ConstraintPointOnRigidBody cp)
	{
		for (int i = 0; i < cp.RaycastLocalDirections.Length; i++)
		{
			Vector3 vector = cp.RigidBody.Rotation * cp.RaycastLocalDirections[i];
			RaycastHit raycastHit;
			if (Physics.Raycast(cp.Position - vector * 0.1f, vector, ref raycastHit, 1f, GlobalConsts.FishMask))
			{
				cp.CollisionPlanesPoints[i] = raycastHit.point.AsVector4f();
				cp.CollisionPlanesNormals[i] = raycastHit.normal.AsVector4f();
			}
			else
			{
				cp.CollisionPlanesPoints[i] = (cp.RaycastLocalDirections[i] * 10000f).AsVector4f();
				cp.CollisionPlanesNormals[i] = (-cp.RaycastLocalDirections[i]).AsVector4f();
			}
		}
	}

	public static float GetGroundHight(Vector3 position)
	{
		float num = -1000f;
		Vector3 vector;
		vector..ctor(position.x, position.y + 5f, position.z);
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, Vector3.down, ref raycastHit, 50f, GlobalConsts.FishMask))
		{
			num = raycastHit.point.y;
		}
		return num;
	}

	public static void UpdateTerrainHeightChunk(Vector3 position, Terrain terrain, HeightFieldChunk heightFieldChunk)
	{
		float num = terrain.terrainData.size.x / (float)(terrain.terrainData.heightmapWidth - 1);
		float num2 = terrain.terrainData.size.z / (float)(terrain.terrainData.heightmapHeight - 1);
		float num3 = (float)(heightFieldChunk.DataSizeX - 1) * num;
		Vector3 vector = position - terrain.transform.position - new Vector3(num3 * 0.5f, 0f, num3 * 0.5f);
		float num4 = vector.x / num;
		float num5 = vector.z / num2;
		int num6 = (int)num4;
		int num7 = (int)num5;
		Vector3 vector2 = new Vector3((float)num6 * num, 0f, (float)num7 * num2) + terrain.transform.position;
		if (num6 != heightFieldChunk.hmIndexX || num7 != heightFieldChunk.hmIndexZ)
		{
			heightFieldChunk.Move(vector2, num, num6, num7);
			for (int i = 0; i < heightFieldChunk.DataSizeX; i++)
			{
				for (int j = 0; j < heightFieldChunk.DataSizeZ; j++)
				{
					heightFieldChunk.heightData[i, j] = terrain.terrainData.GetHeight(num6 + i, num7 + j) + 0.02f;
				}
			}
		}
	}

	public static Vector3? GetGroundCollision(Vector3 origin)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(origin + new Vector3(0f, 100f, 0f), Vector3.down, ref raycastHit, 200f, GlobalConsts.GroundObstacleMask))
		{
			return new Vector3?(raycastHit.point);
		}
		return null;
	}

	public static void GetGroundCollision(Vector3 origin, out Vector3 point, out Vector3 normal, float raycastHeight = 5f)
	{
		Vector3 vector;
		vector..ctor(origin.x, origin.y + raycastHeight, origin.z);
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, Vector3.down, ref raycastHit, 50f, GlobalConsts.FishMask))
		{
			point = raycastHit.point;
			normal = raycastHit.normal;
		}
		else
		{
			point = origin;
			point.y = -1000f;
			normal = Vector3.up;
		}
	}

	public static Vector3? GetUDTContactPoint(Vector3 from, Vector3 to)
	{
		Vector3 vector = to - from;
		RaycastHit raycastHit;
		if (Physics.Raycast(from, vector.normalized, ref raycastHit, vector.magnitude, GlobalConsts.WallsMask))
		{
			return new Vector3?(raycastHit.point);
		}
		return null;
	}

	public static Vector3? GetMaskedRayContactPoint(Vector3 from, Vector3 to, int layerMask)
	{
		Vector3 vector = to - from;
		RaycastHit raycastHit;
		if (Physics.Raycast(from, vector.normalized, ref raycastHit, vector.magnitude, layerMask))
		{
			return new Vector3?(raycastHit.point);
		}
		return null;
	}

	public static RaycastHit GetMaskedRayHit(Vector3 from, Vector3 to, int layerMask)
	{
		Vector3 vector = to - from;
		RaycastHit raycastHit;
		Physics.Raycast(from, vector.normalized, ref raycastHit, vector.magnitude, layerMask);
		return raycastHit;
	}

	public static Vector3? GetMaskedCylinderContactPoint(Vector3 from, Vector3 to, float r, int layerMask)
	{
		Vector3 vector = to - from;
		RaycastHit raycastHit;
		if (Physics.CapsuleCast(from, from + new Vector3(0f, vector.magnitude, 0f), r, vector.normalized, ref raycastHit, vector.magnitude, layerMask))
		{
			return new Vector3?(raycastHit.point);
		}
		return null;
	}

	public static Vector3? GetBoatUnboardingPosition(Vector3 from, Vector3 to)
	{
		Vector3? udtcontactPoint = Math3d.GetUDTContactPoint(from, to);
		if (udtcontactPoint == null)
		{
			return null;
		}
		Vector3 normalized = (to - udtcontactPoint.Value).normalized;
		Vector3 vector = udtcontactPoint.Value + normalized * 0.05f;
		Vector3? udtcontactPoint2 = Math3d.GetUDTContactPoint(vector, to);
		if (udtcontactPoint2 == null || (udtcontactPoint2.Value - udtcontactPoint.Value).magnitude > 1f)
		{
			Vector3 normalized2 = (to - from).normalized;
			Vector3 vector2;
			vector2..ctor(normalized2.x, 0f, normalized2.z);
			Vector3 vector3 = udtcontactPoint.Value + vector2 * 0.5f;
			vector3..ctor(vector3.x, 10f, vector3.z);
			float groundHight = Math3d.GetGroundHight(vector3);
			if (groundHight > -0.25f)
			{
				return new Vector3?(new Vector3(vector3.x, groundHight + 2f, vector3.z));
			}
		}
		return null;
	}

	public static float GetVectorsYaw(Vector3 v1, Vector3 v2)
	{
		Vector3 vector = Vector3.ProjectOnPlane(v1, Vector3.up);
		Vector3 vector2 = Vector3.ProjectOnPlane(v2, Vector3.up);
		return Vector3.Angle(vector, vector2) * (float)Math.Sign(Vector3.Cross(vector, vector2).y);
	}

	public static Vector3 ProjectOXZ(Vector3 v)
	{
		return Vector3.ProjectOnPlane(v, Vector3.up);
	}

	public static Vector2 Rotate2DPoint(float x, float y, float angle)
	{
		return new Vector2(x * Mathf.Cos(angle) - y * Mathf.Sin(angle), x * Mathf.Sin(angle) + y * Mathf.Cos(angle));
	}

	public static Vector3 RotatePointAroundOY(Vector3 p, float angle)
	{
		Vector2 vector = Math3d.Rotate2DPoint(p.x, p.z, angle);
		return new Vector3(vector.x, p.y, vector.y);
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		return Mathf.Clamp(Math3d.ClampAngleTo360(angle), min, max);
	}

	public static float ClampAngleTo360(float angle)
	{
		return (Mathf.Abs(angle) <= 360f) ? angle : (angle - (float)((int)angle / 360 * 360));
	}

	public static float ClampAngleTo180(float angle)
	{
		if (Mathf.Abs(angle) <= 180f)
		{
			return angle;
		}
		return (angle <= 0f) ? (angle + 360f) : (angle - 360f);
	}

	public static float CrossXZ(Vector3 a, Vector3 b)
	{
		return a.x * b.z - a.z * b.x;
	}

	public static bool PointInsideTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = a - c;
		Vector3 vector2 = b - c;
		float num = Math3d.CrossXZ(vector, vector2);
		float num2 = (Math3d.CrossXZ(p, vector2) - Math3d.CrossXZ(c, vector2)) / num;
		float num3 = -(Math3d.CrossXZ(p, vector) - Math3d.CrossXZ(c, vector)) / num;
		return num2 > 0f && num3 > 0f && num2 + num3 < 1f;
	}

	public static float PointToSegmentDistance(Vector3 p, Vector3 a, Vector3 b)
	{
		Vector3 vector = b - a;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		float num = Mathf.Clamp(Vector3.Dot(p - a, vector), 0f, magnitude);
		Vector3 vector2 = a + num * vector;
		return (vector2 - p).magnitude;
	}

	public static Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
	{
		Vector3 vector = b - a;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		float num = Mathf.Clamp(Vector3.Dot(p - a, vector), 0f, magnitude);
		return a + num * vector;
	}

	public static float PointToTriangleDistanceXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		if (Math3d.PointInsideTriangleXZ(p, a, b, c))
		{
			return 0f;
		}
		return Mathf.Min(new float[]
		{
			Math3d.PointToSegmentDistance(p, a, b),
			Math3d.PointToSegmentDistance(p, b, c),
			Math3d.PointToSegmentDistance(p, c, a)
		});
	}

	public static Vector3 ClosestPointOnTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		if (Math3d.PointInsideTriangleXZ(p, a, b, c))
		{
			return p;
		}
		Vector3 vector = Math3d.ClosestPointOnSegment(p, a, b);
		Vector3 vector2 = Math3d.ClosestPointOnSegment(p, b, c);
		Vector3 vector3 = Math3d.ClosestPointOnSegment(p, c, a);
		float magnitude = (vector - p).magnitude;
		float magnitude2 = (vector2 - p).magnitude;
		float magnitude3 = (vector3 - p).magnitude;
		if (magnitude <= magnitude2 && magnitude <= magnitude3)
		{
			return vector;
		}
		if (magnitude2 <= magnitude && magnitude2 <= magnitude3)
		{
			return vector2;
		}
		return vector3;
	}

	public static bool SegmentSegmentOverlap1D(float minA, float maxA, float minB, float maxB)
	{
		return Mathf.Max(minA, minB) < Mathf.Min(maxA, maxB);
	}

	public static bool PointOverlapAABBXZ(Vector3 p, float minX, float maxX, float minZ, float maxZ)
	{
		return p.x >= minX && p.z >= minZ && p.x < maxX && p.z < maxZ;
	}

	public static bool BoxBoxOverlapAABBXZ(float minX1, float maxX1, float minZ1, float maxZ1, float minX2, float maxX2, float minZ2, float maxZ2)
	{
		return Math3d.SegmentSegmentOverlap1D(minX1, maxX1, minX2, maxX2) && Math3d.SegmentSegmentOverlap1D(minZ1, maxZ1, minZ2, maxZ2);
	}

	public static float PointToAABBXZDistance(float px, float pz, float minX, float minZ, float maxX, float maxZ)
	{
		bool flag = px >= minX && px < maxX;
		bool flag2 = pz >= minZ && pz < maxZ;
		if (flag && flag2)
		{
			return 0f;
		}
		if (flag)
		{
			return Mathf.Min(minZ - pz, pz - maxZ);
		}
		if (flag2)
		{
			return Mathf.Min(minX - px, px - maxX);
		}
		if (px < minX)
		{
			if (pz < minZ)
			{
				return MathHelper.Hypot(minX - px, minZ - pz);
			}
			if (pz >= maxZ)
			{
				return MathHelper.Hypot(minX - px, pz - maxZ);
			}
		}
		if (pz < minZ)
		{
			return MathHelper.Hypot(px - maxX, minZ - pz);
		}
		return MathHelper.Hypot(px - maxX, pz - maxZ);
	}

	public static bool TriangleOverlapAABBXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 boxMinXZ, Vector3 boxMaxXZ)
	{
		Vector3 vector = Vector3.Min(Vector3.Min(a, b), c);
		Vector3 vector2 = Vector3.Max(Vector3.Max(a, b), c);
		if (!Math3d.SegmentSegmentOverlap1D(vector.x, vector2.x, boxMinXZ.x, boxMaxXZ.x))
		{
			return false;
		}
		if (!Math3d.SegmentSegmentOverlap1D(vector.z, vector2.z, boxMinXZ.z, boxMaxXZ.z))
		{
			return false;
		}
		Vector3 vector3;
		vector3..ctor(boxMinXZ.x, 0f, boxMinXZ.z);
		Vector3 vector4;
		vector4..ctor(boxMinXZ.x, 0f, boxMaxXZ.z);
		Vector3 vector5;
		vector5..ctor(boxMaxXZ.x, 0f, boxMaxXZ.z);
		Vector3 vector6;
		vector6..ctor(boxMaxXZ.x, 0f, boxMinXZ.z);
		Vector3 vector7 = b - a;
		float num = Mathf.Sign(Math3d.CrossXZ(vector7, c - a));
		if ((float)Math.Sign(Math3d.CrossXZ(vector7, vector3 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector4 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector5 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector6 - a)) != num)
		{
			return false;
		}
		vector7 = c - a;
		num = Mathf.Sign(Math3d.CrossXZ(vector7, b - a));
		if ((float)Math.Sign(Math3d.CrossXZ(vector7, vector3 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector4 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector5 - a)) != num && (float)Math.Sign(Math3d.CrossXZ(vector7, vector6 - a)) != num)
		{
			return false;
		}
		vector7 = b - c;
		num = Mathf.Sign(Math3d.CrossXZ(vector7, a - c));
		return (float)Math.Sign(Math3d.CrossXZ(vector7, vector3 - c)) == num || (float)Math.Sign(Math3d.CrossXZ(vector7, vector4 - c)) == num || (float)Math.Sign(Math3d.CrossXZ(vector7, vector5 - c)) == num || (float)Math.Sign(Math3d.CrossXZ(vector7, vector6 - c)) == num;
	}

	public static int GetNextPointOnLine(List<Vector3> points, int startIndex, Vector3 fromPoint, float minDist, float dAngle, float maxDist = -1f)
	{
		if (startIndex >= points.Count || startIndex < 0)
		{
			return -1;
		}
		int farEnoughPointIndex = Math3d.GetFarEnoughPointIndex(points, startIndex, fromPoint, minDist);
		Vector3 vector = points[farEnoughPointIndex];
		if (Mathf.Approximately(fromPoint.x, vector.x))
		{
			for (int i = farEnoughPointIndex + 1; i < points.Count; i++)
			{
				vector = points[i];
				if (maxDist > 0f && (fromPoint - vector).magnitude > maxDist)
				{
					return i - 1;
				}
				if (!Mathf.Approximately(fromPoint.x, vector.x))
				{
					float num = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
					float num2 = Mathf.Atan(num) * 57.29578f;
					if (num2 < 180f)
					{
						if (Mathf.Abs(90f - num2) > dAngle)
						{
							return i - 1;
						}
					}
					else if (Mathf.Abs(270f - num2) > dAngle)
					{
						return i - 1;
					}
				}
			}
		}
		else
		{
			float num3 = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
			float num4 = Mathf.Atan(num3) * 57.29578f;
			for (int j = farEnoughPointIndex + 1; j < points.Count; j++)
			{
				vector = points[j];
				if (maxDist > 0f && (fromPoint - vector).magnitude > maxDist)
				{
					return j - 1;
				}
				float num5 = ((num4 >= 180f) ? 270f : 90f);
				if (!Mathf.Approximately(fromPoint.x, vector.x))
				{
					num3 = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
					num5 = Mathf.Atan(num3) * 57.29578f;
				}
				if (Mathf.Abs(num5 - num4) > dAngle)
				{
					return j - 1;
				}
			}
		}
		return points.Count - 1;
	}

	private static int GetFarEnoughPointIndex(List<Vector3> points, int startIndex, Vector3 fromPoint, float minDist)
	{
		for (int i = startIndex; i < points.Count; i++)
		{
			if ((fromPoint - points[i]).magnitude >= minDist)
			{
				return i;
			}
		}
		return points.Count - 1;
	}

	public static float ParseFloat(string v)
	{
		return float.Parse(v.Replace(',', '.'), CultureInfo.InvariantCulture);
	}

	private static Transform tempChild;

	private static Transform tempParent;

	private const float GROUND_TEST_DY = 100f;

	private const float UNBOARDING_MIN_HEIGHT = -0.25f;

	private const float UNBOARDING_MIN_SPACE = 1f;
}
