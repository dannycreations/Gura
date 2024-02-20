using System;
using UnityEngine;

public class WaterController : MonoBehaviour, IWaterController
{
	public FishWaterTile FishWaterTileInstance
	{
		get
		{
			return this.fishWaterTile;
		}
	}

	public FishWaterBase FishWaterBaseInstance
	{
		get
		{
			return this.fishWaterBase;
		}
	}

	public Vector3 PlayerPosition { get; set; }

	public Vector3 PlayerForward { get; set; }

	public FishDisturbanceFreq FishDisturbanceFreq { get; set; }

	internal void Awake()
	{
		GameFactory.Water = this;
	}

	internal void Update()
	{
		this.ApplyRandomDisturbances(Time.deltaTime);
	}

	public void SetColor(Color baseCol, Color abyssCol, float time)
	{
		this.fishWaterBase.SetColor(baseCol, abyssCol, time);
	}

	public void ResetColors(float time)
	{
		this.fishWaterBase.SetColor(this.fishWaterBase.Base, this.fishWaterBase.Abyss, time);
	}

	public QueuePool<WaterDisturb> WaterStates
	{
		get
		{
			return this._waterStates;
		}
	}

	public void AddWaterDisturb(Vector3 position, float radius, WaterDisturbForce force)
	{
		this.AddWaterDisturb(position, radius, (float)force);
	}

	public void AddWaterDisturb(Vector3 position, float radius, float force)
	{
		this._putter.GlobalPosition = position;
		this._putter.Radius = radius;
		this._putter.Force = force;
		this._waterStates.Enqueue(this._putter);
	}

	public void SetDynWaterPosition(float x, float z)
	{
		this.fishWaterTile.DynWaterOffsets.Set(x, 0f, z, this.fishWaterTile.DynWaterOffsets.w);
		this.fishWaterTile.UpdateDynWaterPos();
	}

	private void ApplyRandomDisturbances(float dt)
	{
		if (this.FishDisturbanceFreq == FishDisturbanceFreq.None)
		{
			return;
		}
		this.timeSpent += dt;
		int num = 1;
		if (this.FishDisturbanceFreq == FishDisturbanceFreq.Medium)
		{
			num = 2;
		}
		else if (this.FishDisturbanceFreq == FishDisturbanceFreq.High)
		{
			num = 4;
		}
		if (this.timeSpent > 0.2f)
		{
			this.timeSpent = 0f;
			this.PlayerForward = new Vector3(this.PlayerForward.x, 0f, this.PlayerForward.z);
			this.PlayerForward.Normalize();
			for (int i = 0; i < num; i++)
			{
				this.TryCreateDisturb();
			}
		}
	}

	private void TryCreateDisturb()
	{
		int num = 0;
		while (num < 3 && !this.CreateRandomDisturb())
		{
			num++;
		}
	}

	private bool CreateRandomDisturb()
	{
		float num = Random.Range(5f, 25f);
		int num2 = Random.Range(-80, 80);
		Vector3 vector = this.PlayerPosition + Quaternion.Euler(0f, (float)num2, 0f) * this.PlayerForward * num;
		if (GameFactory.FishSpawner == null || GameFactory.FishSpawner.CheckDepth(vector.x, vector.z) == 0f)
		{
			return false;
		}
		vector.y = 0f;
		WaterDisturbForce waterDisturbForce = WaterDisturbForce.XSmall;
		if (Random.Range(0f, 1f) < 0.3f)
		{
			waterDisturbForce = WaterDisturbForce.Small;
		}
		else if (Random.Range(0f, 1f) < 0.3f)
		{
			waterDisturbForce = WaterDisturbForce.Medium;
		}
		Debug.DrawRay(vector, Vector3.up, Color.green);
		this.AddWaterDisturb(vector, 0.1f, waterDisturbForce);
		return true;
	}

	public FishWaterTile fishWaterTile;

	public FishWaterBase fishWaterBase;

	private QueuePool<WaterDisturb> _waterStates = new QueuePool<WaterDisturb>(40);

	private WaterDisturb _putter = new WaterDisturb();

	private float timeSpent;

	private const float MinGenerationPeriod = 0.2f;

	private const float MinDisturbanceRange = 5f;

	private const float MaxDisturbanceRange = 25f;

	private const float DisturbanceRadius = 0.1f;
}
