using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class WaterChumController
{
	public void Subscribe()
	{
		if (!this.subscribed)
		{
			PhotonConnectionFactory.Instance.OnChumLost += this.OnChumLostListener;
		}
		this.subscribed = true;
	}

	public void Unsubscribe()
	{
		if (this.subscribed)
		{
			PhotonConnectionFactory.Instance.OnChumLost -= this.OnChumLostListener;
		}
		this.subscribed = false;
	}

	public void AddChumOnPosition(Chum chum, Vector3 position, Vector3 vel)
	{
		if (!this.subscribed)
		{
			this.Subscribe();
		}
		WaterChumController.ChumRecord chumRecord = this._chums.FirstOrDefault((WaterChumController.ChumRecord ch) => ch.Chum.InstanceId.Value == chum.InstanceId.Value);
		if (chumRecord != null)
		{
			chumRecord.Feeding.IsNew = false;
			chumRecord.Feeding.Position = new Point3(position.x, position.y, position.z);
			chumRecord.Velocity = vel;
		}
		else
		{
			if (this.debug)
			{
				this.cube1 = GameObject.CreatePrimitive(3);
				this.cube1.transform.position = position;
				this.cube1.transform.localScale = 0.5f * Vector3.one + Vector3.up * 4.5f;
				this.cube2 = GameObject.CreatePrimitive(3);
				this.cube2.transform.position = position;
				this.cube2.transform.localScale = 0.5f * Vector3.one + Vector3.up * 4.5f;
				this.cube2.GetComponentInChildren<Renderer>().material.color = Color.red;
			}
			Feeding feeding = new Feeding
			{
				IsNew = true,
				ItemId = chum.InstanceId.Value,
				IsDestroyed = false,
				IsExpired = chum.IsExpired,
				Position = new Point3(position.x, position.y, position.z)
			};
			chumRecord = new WaterChumController.ChumRecord(chum, feeding, vel);
			this._chums.Add(chumRecord);
		}
		GameFactory.Player.AddFeeding(chumRecord.Feeding);
	}

	public void UpdatePositions(float dt)
	{
		float time = Time.time;
		for (int i = 0; i < this._chums.Count; i++)
		{
			WaterChumController.ChumRecord chumRecord = this._chums[i];
			Point3 position = chumRecord.Feeding.Position;
			Vector3 vector;
			vector..ctor(position.X, position.Y, position.Z);
			Vector3 velocity = chumRecord.Velocity;
			Vector3 vector2 = ((GameFactory.WaterFlow == null) ? Vector3.zero : GameFactory.WaterFlow.GetStreamSpeed(vector));
			Vector3 vector3 = -Vector3.up * 9.81f * Mathf.Max(0.01f, -chumRecord.Chum.Buoyancy) + (vector2 - velocity) * 10f;
			Vector3 vector4 = velocity + vector3 * dt;
			vector += vector4 * dt + vector3 * dt * dt * 0.5f;
			chumRecord.Feeding.IsNew = false;
			chumRecord.Feeding.Position = new Point3(vector.x, vector.y, vector.z);
			chumRecord.Velocity = vector4;
			if (time > this.lastTime + 1f)
			{
				if (this.debug)
				{
					this.cube2.transform.position = vector;
				}
				GameFactory.Player.AddFeeding(chumRecord.Feeding);
				this.lastTime = time;
			}
		}
	}

	public void OnChumLostListener(InventoryItem item)
	{
		int num = this._chums.FindIndex((WaterChumController.ChumRecord ch) => ch.Chum.InstanceId.Value == item.InstanceId);
		if (num != -1)
		{
			this._chums.RemoveAt(num);
		}
	}

	public const float UpdateInterval = 1f;

	private float lastTime;

	private List<WaterChumController.ChumRecord> _chums = new List<WaterChumController.ChumRecord>();

	private bool debug;

	private GameObject cube1;

	private GameObject cube2;

	private bool subscribed;

	private class ChumRecord
	{
		public ChumRecord(Chum chum, Feeding feeding, Vector3 velocity)
		{
			this.Chum = chum;
			this.Feeding = feeding;
			this.Velocity = velocity;
		}

		public Chum Chum { get; private set; }

		public Feeding Feeding { get; private set; }

		public Vector3 Velocity { get; set; }
	}
}
