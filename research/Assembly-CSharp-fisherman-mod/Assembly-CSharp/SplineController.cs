using System;
using System.Collections;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class SplineController : MonoBehaviour
{
	public virtual void Awake()
	{
		List<Transform> list = new List<Transform>();
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				list.Add(transform);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		this.transforms = list.ToArray();
		this.masses = new Mass[list.Count];
		for (int i = 0; i < this.transforms.Length; i++)
		{
			this.masses[i] = new Mass(GameFactory.Player.RodSlot.Sim, 0f, this.transforms[i].position, Mass.MassType.Unknown);
		}
	}

	public virtual void Update()
	{
		for (int i = 0; i < this.transforms.Length; i++)
		{
			this.masses[i].Position = this.transforms[i].position;
		}
		SplineBuilder.BuildBezier(this.masses, base.GetComponent<LineRenderer>());
		if (this.RefCube != null)
		{
			BezierConfig bezierConfig = SplineBuilder.InitBezier(this.masses);
			TransformPoint bezierPoint = SplineBuilder.GetBezierPoint(bezierConfig, this.Length);
			this.RefCube.position = bezierPoint.Position;
			this.RefCube.rotation = bezierPoint.Rotation;
		}
	}

	public Transform RefCube;

	public float Length;

	private Transform[] transforms;

	private Mass[] masses;
}
