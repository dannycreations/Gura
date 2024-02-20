using System;
using UnityEngine;

public class CompassObject
{
	public int Id { get; set; }

	public Vector3 Pos { get; set; }

	public Vector3 Rot { get; set; }

	public CompassMarker Marker { get; set; }

	public ScreenMarker ScreenMarkerLeft { get; set; }

	public ScreenMarker ScreenMarkerRight { get; set; }
}
