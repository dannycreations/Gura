using System;
using System.Collections.Generic;
using System.Linq;
using Nss.Udt.Boundaries;
using UnityEngine;

[ExecuteInEditMode]
public class SplineBend : MonoBehaviour
{
	public SplineBendMarker.MarkerType MarkersType
	{
		get
		{
			return this._markersType;
		}
	}

	public void FillContour()
	{
		Transform transform = base.transform.Find("objects");
		while (transform != null)
		{
			Object.DestroyImmediate(transform.gameObject);
			transform = base.transform.Find("objects");
		}
		Transform transform2 = new GameObject("objects").transform;
		transform2.parent = base.transform;
		transform2.SetAsFirstSibling();
		List<Vector3> list = this.markers.Select((SplineBendMarker m) => m.transform.position).ToList<Vector3>();
		int num = 0;
		int num2 = 0;
		for (;;)
		{
			int nextPointOnLine = Math3d.GetNextPointOnLine(list, num2 + 1, list[num2], this._minWidth, 5f, this._maxWidth);
			if (nextPointOnLine < 0)
			{
				break;
			}
			Vector3 position = this.markers[num2].transform.position;
			Vector3 position2 = this.markers[nextPointOnLine].transform.position;
			Vector3 vector = position2 - position;
			Vector3 vector2 = (this._shoreLineDist + this._depth) * Vector3.Cross(vector, Vector3.up).normalized;
			if (this._invertSide)
			{
				vector2 *= -1f;
			}
			GameObject gameObject = Object.Instantiate<GameObject>(this.PChild);
			gameObject.name = string.Format("{0}_{1}", this.PChild.name, ++num);
			gameObject.transform.parent = transform2;
			ISize component = gameObject.GetComponent<ISize>();
			component.SetDepth(this._depth);
			component.SetWidth(vector.magnitude);
			gameObject.transform.position = position + vector * 0.5f + vector2;
			float num3 = Vector3.SignedAngle(Vector3.right, vector, Vector3.up);
			gameObject.transform.rotation = Quaternion.AngleAxis(num3, Vector3.up);
			num2 = nextPointOnLine;
		}
	}

	public Transform RelatedObjectsRoot
	{
		get
		{
			return base.transform.Find("objects");
		}
	}

	private void Awake()
	{
		if (this._disableOnAwake && Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	public void ChangeMarkersType(SplineBendMarker.MarkerType type)
	{
		this._markersType = type;
		for (int i = 0; i < this.markers.Length; i++)
		{
			this.markers[i].type = type;
		}
	}

	private static Vector3 GetBeizerPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		float num = 1f - t;
		return num * num * num * p0 + 3f * t * num * num * p1 + 3f * t * t * num * p2 + t * t * t * p3;
	}

	public static float GetBeizerLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = 0f;
		Vector3 vector = p0;
		float num2 = 0f;
		while ((double)num2 < 1.01)
		{
			Vector3 beizerPoint = SplineBend.GetBeizerPoint(p0, p1, p2, p3, num2);
			num += (vector - beizerPoint).magnitude;
			vector = beizerPoint;
			num2 += 0.1f;
		}
		return num;
	}

	public static float GetBeizerLength(SplineBendMarker marker1, SplineBendMarker marker2)
	{
		return SplineBend.GetBeizerLength(marker1.position, marker1.nextHandle + marker1.position, marker2.prewHandle + marker2.position, marker2.position);
	}

	public static Vector3 AlignPoint(SplineBendMarker marker1, SplineBendMarker marker2, float percent, Vector3 coords)
	{
		Vector3 beizerPoint = SplineBend.GetBeizerPoint(marker1.position, marker1.nextHandle + marker1.position, marker2.prewHandle + marker2.position, marker2.position, Mathf.Max(0f, percent - 0.01f));
		Vector3 beizerPoint2 = SplineBend.GetBeizerPoint(marker1.position, marker1.nextHandle + marker1.position, marker2.prewHandle + marker2.position, marker2.position, Mathf.Min(1f, percent + 0.01f));
		Vector3 beizerPoint3 = SplineBend.GetBeizerPoint(marker1.position, marker1.nextHandle + marker1.position, marker2.prewHandle + marker2.position, marker2.position, percent);
		Vector3 vector = beizerPoint - beizerPoint2;
		Vector3 vector2 = Vector3.Slerp(marker1.up, marker2.up, percent);
		Vector3 normalized = Vector3.Cross(vector, vector2).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, vector).normalized;
		Vector3 one = Vector3.one;
		if (marker1.expandWithScale || marker2.expandWithScale)
		{
			float num = percent * percent;
			float num2 = 1f - (1f - percent) * (1f - percent);
			float num3 = num2 * percent + num * (1f - percent);
			one.x = marker1.transform.localScale.x * (1f - num3) + marker2.transform.localScale.x * num3;
			one.y = marker1.transform.localScale.y * (1f - num3) + marker2.transform.localScale.y * num3;
		}
		return beizerPoint3 + normalized * coords.x * one.x + normalized2 * coords.y * one.y;
	}

	public void BuildMesh(Mesh mesh, Mesh initialMesh, int num, float offset)
	{
		Vector3[] vertices = initialMesh.vertices;
		Vector2[] uv = initialMesh.uv;
		Vector2[] uv2 = initialMesh.uv2;
		int[] triangles = initialMesh.triangles;
		Vector4[] tangents = initialMesh.tangents;
		Vector3[] array = new Vector3[vertices.Length * num];
		Vector2[] array2 = new Vector2[vertices.Length * num];
		Vector2[] array3 = new Vector2[vertices.Length * num];
		Vector4[] array4 = new Vector4[vertices.Length * num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < vertices.Length; j++)
			{
				array[i * vertices.Length + j] = vertices[j];
				array2[i * vertices.Length + j] = uv[j];
				array4[i * vertices.Length + j] = tangents[j];
			}
		}
		int[] array5 = new int[triangles.Length * num];
		for (int k = 0; k < num; k++)
		{
			for (int j = 0; j < triangles.Length; j++)
			{
				array5[k * triangles.Length + j] = triangles[j] + vertices.Length * k;
			}
		}
		mesh.Clear();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.uv2 = array3;
		mesh.triangles = array5;
		mesh.tangents = array4;
		mesh.RecalculateNormals();
	}

	public void RebuildMeshes()
	{
		if (this.renderMesh != null)
		{
			MeshFilter component = base.GetComponent<MeshFilter>();
			if (component == null)
			{
				return;
			}
			this.renderMesh.Clear();
			this.BuildMesh(this.renderMesh, this.initialRenderMesh, this.tiles, this.tileOffset);
			component.sharedMesh = this.renderMesh;
			this.renderMesh.RecalculateBounds();
			this.renderMesh.RecalculateNormals();
		}
		if (this.collisionMesh != null)
		{
			MeshCollider component2 = base.GetComponent<MeshCollider>();
			if (component2 == null)
			{
				return;
			}
			this.collisionMesh.Clear();
			this.BuildMesh(this.collisionMesh, this.initialCollisionMesh, this.tiles, this.tileOffset);
			component2.sharedMesh = null;
			component2.sharedMesh = this.collisionMesh;
			this.collisionMesh.RecalculateBounds();
			this.collisionMesh.RecalculateNormals();
		}
	}

	public void Align(Mesh mesh, Mesh initialMesh)
	{
		Vector3[] array = new Vector3[mesh.vertexCount];
		Vector3[] vertices = initialMesh.vertices;
		for (int i = 0; i < this.tiles; i++)
		{
			for (int j = 0; j < vertices.Length; j++)
			{
				int num = i * vertices.Length + j;
				array[num] = vertices[j] + this.axisVector * this.tileOffset * (float)i;
				if (this.axis == SplineBend.SplineBendAxis.x)
				{
					array[num] = new Vector3(-array[num].z, array[num].y, array[num].x);
				}
				else if (this.axis == SplineBend.SplineBendAxis.y)
				{
					array[num] = new Vector3(-array[num].x, array[num].z, array[num].y);
				}
			}
		}
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		for (int k = 0; k < array.Length; k++)
		{
			num2 = Mathf.Min(num2, array[k].z);
			num3 = Mathf.Max(num3, array[k].z);
		}
		float num4 = num3 - num2;
		for (int l = 0; l < array.Length; l++)
		{
			float num5 = (array[l].z - num2) / num4;
			num5 = Mathf.Clamp01(num5);
			if (Mathf.Approximately(num4, 0f))
			{
				num5 = 0f;
			}
			int num6 = 0;
			for (int m = 1; m < this.markers.Length; m++)
			{
				if (this.markers[m].percent >= num5)
				{
					num6 = m - 1;
					break;
				}
			}
			if (this.closed && num5 < this.markers[1].percent)
			{
				num6 = 0;
			}
			float num7 = (num5 - this.markers[num6].percent) / (this.markers[num6 + 1].percent - this.markers[num6].percent);
			if (this.closed && num5 < this.markers[1].percent)
			{
				num7 = num5 / this.markers[1].percent;
			}
			if (this.equalize)
			{
				int num8 = 0;
				for (int n = 1; n < this.markers[num6].subPoints.Length; n++)
				{
					if (this.markers[num6].subPointPercents[n] >= num7)
					{
						num8 = n - 1;
						break;
					}
				}
				float num9 = (num7 - this.markers[num6].subPointPercents[num8]) * this.markers[num6].subPointFactors[num8];
				num7 = this.markers[num6].subPointMustPercents[num8] + num9;
			}
			array[l] = SplineBend.AlignPoint(this.markers[num6], this.markers[num6 + 1], num7, array[l]);
		}
		mesh.vertices = array;
	}

	public void FallToTerrain(Mesh mesh, Mesh initialMesh, float seekDist, int layer, float offset)
	{
		Vector3[] vertices = mesh.vertices;
		float[] array = new float[mesh.vertexCount];
		Vector3[] vertices2 = initialMesh.vertices;
		SplineBend.SplineBendAxis splineBendAxis = this.axis;
		if (splineBendAxis != SplineBend.SplineBendAxis.z && splineBendAxis != SplineBend.SplineBendAxis.x)
		{
			if (splineBendAxis == SplineBend.SplineBendAxis.y)
			{
				for (int i = 0; i < this.tiles; i++)
				{
					for (int j = 0; j < vertices2.Length; j++)
					{
						array[i * vertices2.Length + j] = vertices2[j].z;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < this.tiles; k++)
			{
				for (int l = 0; l < vertices2.Length; l++)
				{
					array[k * vertices2.Length + l] = vertices2[l].y;
				}
			}
		}
		int layer2 = base.gameObject.layer;
		base.gameObject.layer = 4;
		for (int m = 0; m < vertices.Length; m++)
		{
			Vector3 vector = base.transform.TransformPoint(vertices[m]);
			vector.y = base.transform.position.y;
			RaycastHit raycastHit;
			if (Physics.Raycast(vector + new Vector3(0f, seekDist * 0.5f, 0f), -Vector3.up, ref raycastHit, seekDist, 1 << layer))
			{
				vertices[m].y = array[m] + base.transform.InverseTransformPoint(raycastHit.point).y + offset;
			}
		}
		base.gameObject.layer = layer2;
		mesh.vertices = vertices;
	}

	private void ResetMarkers()
	{
		this.ResetMarkers(this.markers.Length);
	}

	private void ResetMarkers(int count)
	{
		this.markers = new SplineBendMarker[count];
		if (this.initialRenderMesh != null)
		{
			Mesh mesh = this.initialRenderMesh;
		}
		else if (this.initialCollisionMesh != null)
		{
			Mesh mesh = this.initialCollisionMesh;
		}
		Bounds bounds;
		bounds..ctor(Vector3.zero, Vector3.one * 5f);
		bool flag = false;
		if (this.initialRenderMesh != null)
		{
			bounds = this.initialRenderMesh.bounds;
			flag = true;
		}
		else if (this.initialCollisionMesh != null)
		{
			bounds = this.initialCollisionMesh.bounds;
			flag = true;
		}
		MeshFilter component = base.GetComponent<MeshFilter>();
		MeshCollider component2 = base.GetComponent<MeshCollider>();
		if (!flag && component != null)
		{
			bounds = component.sharedMesh.bounds;
			flag = true;
		}
		if (!flag && component2 != null)
		{
			bounds = component2.sharedMesh.bounds;
		}
		float z = bounds.min.z;
		float num = bounds.size.z / (float)(count - 1);
		for (int i = 0; i < count; i++)
		{
			Transform transform = new GameObject("Marker" + i).transform;
			transform.parent = base.transform;
			transform.localPosition = new Vector3(0f, 0f, z + num * (float)i);
			this.markers[i] = transform.gameObject.AddComponent<SplineBendMarker>();
			this.markers[i].type = this._markersType;
		}
	}

	public void AddMarker(Vector3 coords)
	{
		int num = 0;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < this.markers.Length; i++)
		{
			float sqrMagnitude = (this.markers[i].position - coords).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num = i;
				num2 = sqrMagnitude;
			}
		}
		this.AddMarker(num, coords);
	}

	public void RecreateMarkers()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			SplineBendMarker component = base.transform.GetChild(i).GetComponent<SplineBendMarker>();
			this.markers[i] = component;
		}
		this.UpdateNow();
	}

	private bool IsPointExtendContour(Vector3 p1, Vector3 p2, Vector3 p)
	{
		Vector3 vector = p - p2;
		Vector3 vector2 = p1 - p2;
		float num = Vector3.Angle(vector, vector2);
		if (num < 90f)
		{
			vector = p2 - p;
			vector2 = p1 - p;
			return Vector3.Angle(vector, vector2) < 160f;
		}
		return true;
	}

	public void AddMarker(Ray camRay)
	{
		float num = float.PositiveInfinity;
		int num2 = 0;
		Vector3 vector;
		if (Math3d.LinePlaneIntersection(out vector, camRay.origin, camRay.direction, Vector3.up, base.transform.position))
		{
			for (int i = 0; i < this.markers.Length; i++)
			{
				float magnitude = (vector - this.markers[i].transform.position).magnitude;
				if (magnitude < num)
				{
					num2 = i;
					num = magnitude;
				}
			}
			if (num2 == 0)
			{
				if (this.IsPointExtendContour(this.markers[1].transform.position, this.markers[0].transform.position, vector))
				{
					SplineBendMarker[] array = new SplineBendMarker[this.markers.Length + 1];
					for (int j = 0; j < this.markers.Length; j++)
					{
						array[j + 1] = this.markers[j];
					}
					array[0] = this.CreateMarker(0, vector);
					this.markers = array;
					this.UpdateNow();
					return;
				}
			}
			else if (num2 == this.markers.Length - 1)
			{
				if (this.IsPointExtendContour(this.markers[this.markers.Length - 2].transform.position, this.markers[this.markers.Length - 1].transform.position, vector))
				{
					SplineBendMarker[] array2 = new SplineBendMarker[this.markers.Length + 1];
					Array.Copy(this.markers, array2, this.markers.Length);
					array2[this.markers.Length] = this.CreateMarker(this.markers.Length, vector);
					this.markers = array2;
					this.UpdateNow();
					return;
				}
				num2--;
			}
			else
			{
				float num3 = Math3d.PointToSegmentDistance(vector, this.markers[num2].transform.position, this.markers[num2 - 1].transform.position);
				float num4 = Math3d.PointToSegmentDistance(vector, this.markers[num2].transform.position, this.markers[num2 + 1].transform.position);
				if (num3 < num4)
				{
					num2--;
				}
			}
			this.AddMarker(num2, vector);
			this.UpdateNow();
		}
	}

	public void AddMarker(int prewMarkerNum, Vector3 coords)
	{
		SplineBendMarker[] array = new SplineBendMarker[this.markers.Length + 1];
		for (int i = 0; i < this.markers.Length; i++)
		{
			if (i <= prewMarkerNum)
			{
				array[i] = this.markers[i];
			}
			else
			{
				array[i + 1] = this.markers[i];
			}
		}
		array[prewMarkerNum + 1] = this.CreateMarker(prewMarkerNum + 1, coords);
		this.markers = array;
	}

	public SplineBendMarker CreateMarker(int index, Vector3 pos)
	{
		Transform transform = new GameObject("Marker" + (index + 1)).transform;
		transform.parent = base.transform;
		transform.position = pos;
		SplineBendMarker splineBendMarker = transform.gameObject.AddComponent<SplineBendMarker>();
		splineBendMarker.type = this._markersType;
		transform.SetSiblingIndex(index);
		return splineBendMarker;
	}

	public void ImportMarkers(List<Segment> segments, bool isClosed)
	{
		for (int i = 0; i < this.markers.Length; i++)
		{
			Object.DestroyImmediate(this.markers[i].gameObject);
		}
		this.markers = new SplineBendMarker[(isClosed || segments.Count <= 0) ? segments.Count : (segments.Count + 1)];
		for (int j = 0; j < segments.Count; j++)
		{
			this.markers[j] = this.CreateMarker(j, segments[j].start);
		}
		if (!isClosed && segments.Count > 0)
		{
			this.markers[this.markers.Length - 1] = this.CreateMarker(this.markers.Length - 1, segments[segments.Count - 1].end);
		}
		this.UpdateNow();
	}

	public List<Vector3> Split(SplineBendMarker from, SplineBendMarker to)
	{
		List<Segment> list = new List<Segment>();
		List<Vector3> list2 = new List<Vector3>();
		int num = Array.IndexOf<SplineBendMarker>(this.markers, from);
		int num2 = Array.IndexOf<SplineBendMarker>(this.markers, to);
		if (num >= 0 && num2 >= 0)
		{
			for (int i = 0; i < this.markers.Length; i++)
			{
				if (i < num || i > num2)
				{
					list.Add(new Segment
					{
						start = this.markers[i].position,
						end = ((i >= this.markers.Length - 1) ? this.markers[0].position : this.markers[i + 1].position)
					});
				}
				else
				{
					list2.Add(this.markers[i].position);
				}
			}
		}
		this.ImportMarkers(list, true);
		return list2;
	}

	private void RefreshMarkers()
	{
		int num = 0;
		for (int i = 0; i < this.markers.Length; i++)
		{
			if (this.markers[i] != null)
			{
				num++;
			}
		}
		SplineBendMarker[] array = new SplineBendMarker[num];
		int num2 = 0;
		for (int j = 0; j < this.markers.Length; j++)
		{
			if (!(this.markers[j] == null))
			{
				array[num2] = this.markers[j];
				num2++;
			}
		}
		this.markers = array;
	}

	private void RemoveMarker(int num)
	{
		Object.DestroyImmediate(this.markers[num].gameObject);
		SplineBendMarker[] array = new SplineBendMarker[this.markers.Length - 1];
		for (int i = 0; i < this.markers.Length - 1; i++)
		{
			if (i < num)
			{
				array[i] = this.markers[i];
			}
			else
			{
				array[i] = this.markers[i + 1];
			}
		}
		this.markers = array;
	}

	private void CloseMarkers()
	{
		if (this.closed || this.markers[0] == this.markers[this.markers.Length - 1])
		{
			return;
		}
		SplineBendMarker[] array = new SplineBendMarker[this.markers.Length + 1];
		for (int i = 0; i < this.markers.Length; i++)
		{
			array[i] = this.markers[i];
		}
		this.markers = array;
		this.markers[this.markers.Length - 1] = this.markers[0];
		this.UpdateNow();
		this.closed = true;
	}

	private void UnCloseMarkers()
	{
		if (!this.closed || this.markers[0] != this.markers[this.markers.Length - 1])
		{
			return;
		}
		SplineBendMarker[] array = new SplineBendMarker[this.markers.Length - 1];
		for (int i = 0; i < this.markers.Length - 1; i++)
		{
			array[i] = this.markers[i];
		}
		this.markers = array;
		this.UpdateNow();
		this.closed = false;
	}

	private void OnEnable()
	{
		this.renderMesh = null;
		this.collisionMesh = null;
		this.ForceUpdate();
		MeshFilter component = base.GetComponent<MeshFilter>();
		MeshCollider component2 = base.GetComponent<MeshCollider>();
		if (this.renderMesh != null && component != null)
		{
			component.sharedMesh = this.renderMesh;
		}
		if (this.collisionMesh != null && component2 != null)
		{
			component2.sharedMesh = null;
			component2.sharedMesh = this.collisionMesh;
		}
	}

	private void OnDisable()
	{
		MeshFilter component = base.GetComponent<MeshFilter>();
		MeshCollider component2 = base.GetComponent<MeshCollider>();
		if (this.initialRenderMesh != null && component != null)
		{
			component.sharedMesh = this.initialRenderMesh;
		}
		if (this.initialCollisionMesh != null && component2 != null)
		{
			component2.sharedMesh = null;
			component2.sharedMesh = this.initialCollisionMesh;
		}
	}

	public void UpdateNow()
	{
		this.ForceUpdate(true);
	}

	public void ForceUpdate()
	{
		this.ForceUpdate(true);
	}

	public void ForceUpdate(bool refreshCollisionMesh)
	{
		MeshCollider component = base.GetComponent<MeshCollider>();
		MeshFilter component2 = base.GetComponent<MeshFilter>();
		SplineBend.SplineBendAxis splineBendAxis = this.axis;
		if (splineBendAxis != SplineBend.SplineBendAxis.x)
		{
			if (splineBendAxis != SplineBend.SplineBendAxis.y)
			{
				if (splineBendAxis == SplineBend.SplineBendAxis.z)
				{
					this.axisVector = Vector3.forward;
				}
			}
			else
			{
				this.axisVector = Vector3.up;
			}
		}
		else
		{
			this.axisVector = Vector3.right;
		}
		if (this.initialRenderMesh != null)
		{
			this.tiles = Mathf.Min(this.tiles, Mathf.FloorToInt(65000f / (float)this.initialRenderMesh.vertices.Length));
		}
		else if (this.initialCollisionMesh != null)
		{
			this.tiles = Mathf.Min(this.tiles, Mathf.FloorToInt(65000f / (float)this.initialCollisionMesh.vertices.Length));
		}
		this.tiles = Mathf.Max(this.tiles, 1);
		if (this.markers == null)
		{
			this.ResetMarkers(2);
		}
		for (int i = 0; i < this.markers.Length; i++)
		{
			if (this.markers[i] == null)
			{
				this.RefreshMarkers();
			}
		}
		if (this.markers.Length < 2)
		{
			this.ResetMarkers(2);
		}
		for (int j = 0; j < this.markers.Length; j++)
		{
			this.markers[j].Init(this, j);
		}
		if (this.closed)
		{
			this.markers[0].dist = this.markers[this.markers.Length - 2].dist + SplineBend.GetBeizerLength(this.markers[this.markers.Length - 2], this.markers[0]);
		}
		float num = this.markers[this.markers.Length - 1].dist;
		if (this.closed)
		{
			num = this.markers[0].dist;
		}
		for (int k = 0; k < this.markers.Length; k++)
		{
			this.markers[k].percent = this.markers[k].dist / num;
		}
		if (this.closed && !this.wasClosed)
		{
			this.CloseMarkers();
		}
		if (!this.closed && this.wasClosed)
		{
			this.UnCloseMarkers();
		}
		this.wasClosed = this.closed;
		if (component2 != null && this.renderMesh == null)
		{
			if (this.initialRenderMesh == null)
			{
				this.initialRenderMesh = component2.sharedMesh;
			}
			else
			{
				if (this.tileOffset < 0f)
				{
					this.tileOffset = this.initialRenderMesh.bounds.size.z;
				}
				this.renderMesh = Object.Instantiate<Mesh>(this.initialRenderMesh);
				this.renderMesh.hideFlags = 61;
				component2.sharedMesh = this.renderMesh;
			}
		}
		if (component != null && this.collisionMesh == null)
		{
			if (this.initialCollisionMesh == null)
			{
				this.initialCollisionMesh = component.sharedMesh;
			}
			else
			{
				if (this.tileOffset < 0f)
				{
					this.tileOffset = this.initialCollisionMesh.bounds.size.z;
				}
				this.collisionMesh = Object.Instantiate<Mesh>(this.initialCollisionMesh);
				this.collisionMesh.hideFlags = 61;
				component.sharedMesh = this.collisionMesh;
			}
		}
		if (this.renderMesh != null && this.initialRenderMesh != null && component2 != null)
		{
			if (this.renderMesh.vertexCount != this.initialRenderMesh.vertexCount * this.tiles)
			{
				this.BuildMesh(this.renderMesh, this.initialRenderMesh, this.tiles, 0f);
			}
			this.Align(this.renderMesh, this.initialRenderMesh);
			if (this.dropToTerrain)
			{
				this.FallToTerrain(this.renderMesh, this.initialRenderMesh, this.terrainSeekDist, this.terrainLayer, this.terrainOffset);
			}
			this.renderMesh.RecalculateBounds();
			this.renderMesh.RecalculateNormals();
		}
		if (this.collisionMesh != null && this.initialCollisionMesh != null && component != null)
		{
			if (this.collisionMesh.vertexCount != this.initialCollisionMesh.vertexCount * this.tiles)
			{
				this.BuildMesh(this.collisionMesh, this.initialCollisionMesh, this.tiles, 0f);
			}
			this.Align(this.collisionMesh, this.initialCollisionMesh);
			if (this.dropToTerrain)
			{
				this.FallToTerrain(this.collisionMesh, this.initialCollisionMesh, this.terrainSeekDist, this.terrainLayer, this.terrainOffset);
			}
			if (refreshCollisionMesh && component.sharedMesh == this.collisionMesh)
			{
				this.collisionMesh.RecalculateBounds();
				this.collisionMesh.RecalculateNormals();
				component.sharedMesh = null;
				component.sharedMesh = this.collisionMesh;
			}
		}
	}

	[SerializeField]
	private bool _disableOnAwake;

	[HideInInspector]
	public SplineBendMarker[] markers;

	[SerializeField]
	private SplineBendMarker.MarkerType _markersType;

	[HideInInspector]
	public bool showMeshes;

	[HideInInspector]
	public bool showTiles;

	[HideInInspector]
	public bool showTerrain;

	[HideInInspector]
	public bool showUpdate;

	[HideInInspector]
	public bool showExport;

	[HideInInspector]
	public Mesh initialRenderMesh;

	[HideInInspector]
	public Mesh renderMesh;

	[HideInInspector]
	public Mesh initialCollisionMesh;

	[HideInInspector]
	public Mesh collisionMesh;

	[HideInInspector]
	public int tiles = 1;

	[HideInInspector]
	public float tileOffset = -1f;

	[HideInInspector]
	private int oldTiles = 1;

	[HideInInspector]
	public bool dropToTerrain;

	[HideInInspector]
	public float terrainSeekDist = 1000f;

	[HideInInspector]
	public float terrainOffset;

	[HideInInspector]
	public int terrainLayer;

	[HideInInspector]
	public bool equalize = true;

	[HideInInspector]
	public bool closed;

	[HideInInspector]
	public bool wasClosed;

	[HideInInspector]
	public float markerSize = 1f;

	[HideInInspector]
	public bool displayRolloutOpen;

	[HideInInspector]
	public bool settingsRolloutOpen;

	[HideInInspector]
	public bool terrainRolloutOpen;

	[HideInInspector]
	[SerializeField]
	private float _shoreLineDist = 1f;

	[HideInInspector]
	[SerializeField]
	private float _depth = 3f;

	[HideInInspector]
	[SerializeField]
	private float _minWidth = 8f;

	[HideInInspector]
	[SerializeField]
	private float _maxWidth = 20f;

	[HideInInspector]
	[SerializeField]
	private bool _invertSide;

	[HideInInspector]
	public GameObject PChild;

	public SplineBend.SplineBendAxis axis = SplineBend.SplineBendAxis.z;

	private Vector3 axisVector;

	private Transform objFile;

	public enum SplineBendAxis
	{
		x,
		y,
		z
	}
}
