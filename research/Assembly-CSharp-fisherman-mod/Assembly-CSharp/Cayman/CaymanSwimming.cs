using System;
using UnityEngine;

namespace Cayman
{
	public class CaymanSwimming : CaymanActivity
	{
		public float ScareDistance
		{
			get
			{
				return this._scareDistance;
			}
		}

		public void Init(Vector3 from, Vector3 to, float dirYaw, float spawnDeep)
		{
			base.transform.rotation = Quaternion.AngleAxis(dirYaw, Vector3.up);
			this._swimFrom = new Vector3(from.x, this._maxH, from.z);
			this._swimTo = new Vector3(to.x, this._maxH, to.z);
			base.transform.position = new Vector3(from.x, spawnDeep, from.z);
			this._spawnDeep = spawnDeep;
			this._curTime = 0f;
			this._totalTime = Mathf.Abs(this._spawnDeep - this._maxH) / this.SPEED_V;
			this._curState = CaymanSwimming.States.SwimUp;
		}

		public void Scare()
		{
			if (this._curState == CaymanSwimming.States.SwimDown)
			{
				return;
			}
			this._curState = CaymanSwimming.States.SwimDown;
			this._curTime = 0f;
			this._totalTime = Mathf.Abs(this._spawnDeep - base.transform.position.y) / this.SPEED_V;
			this.DisturbWater();
		}

		private void DisturbWater()
		{
			if (GameFactory.Water != null)
			{
				for (int i = 0; i < this._waterDisturbers.Length; i++)
				{
					Vector3 position = this._waterDisturbers[i].position;
					GameFactory.Water.AddWaterDisturb(position, this._disturbanceRadius, (float)((byte)this._disturbancePower));
				}
			}
		}

		private void Update()
		{
			if (this._curState == CaymanSwimming.States.SwimUp)
			{
				this._curTime += Time.deltaTime;
				float num = this._curTime / this._totalTime;
				float num2 = Mathf.Lerp(this._spawnDeep, this._maxH, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num)));
				base.transform.position = new Vector3(base.transform.position.x, num2, base.transform.position.z);
				if (num > 1f)
				{
					this._curState = CaymanSwimming.States.Swim;
					this._disturbsCounter = (sbyte)this._ticksAfterDisturbs;
					base.transform.position = new Vector3(base.transform.position.x, this._maxH, base.transform.position.z);
					this._curTime = 0f;
					this._totalTime = (this._swimFrom - this._swimTo).magnitude / this.SPEED_H;
					this.DisturbWater();
				}
			}
			else if (this._curState == CaymanSwimming.States.Swim)
			{
				this._curTime += Time.deltaTime;
				float num3 = Mathf.Lerp(0f, this.SPEED_H, Mathf.Clamp01(this._curTime / this.ACCELERATION_TIME));
				base.transform.position += base.transform.forward * num3 * Time.deltaTime;
				if ((int)this._disturbsCounter == 0 && GameFactory.Water != null)
				{
					this._disturbsCounter = (sbyte)this._ticksAfterDisturbs;
					for (int i = 0; i < this._waterDisturbers.Length; i++)
					{
						GameFactory.Water.AddWaterDisturb(this._waterDisturbers[i].position, this._moveDisturbanceRadius, (float)((byte)this._moveDisturbancePower));
					}
				}
				else
				{
					this._disturbsCounter = (sbyte)((int)this._disturbsCounter - 1);
				}
				if (this._curTime > this._totalTime)
				{
					this.Scare();
				}
			}
			else
			{
				this._curTime += Time.deltaTime;
				float num4 = Mathf.Lerp(this.SPEED_H, 0f, Mathf.Clamp01(this._curTime / this.ACCELERATION_TIME));
				base.transform.position += base.transform.forward * num4 * Time.deltaTime - base.transform.up * this.SPEED_V * Time.deltaTime;
				if (this._curTime > this._totalTime)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}

		[SerializeField]
		private float SPEED_H = 0.3f;

		[SerializeField]
		private float ACCELERATION_TIME = 1f;

		[SerializeField]
		private float SPEED_V = 0.1f;

		[SerializeField]
		private float _scareDistance = 5f;

		[SerializeField]
		private float _maxH = -0.38f;

		[SerializeField]
		private Transform[] _waterDisturbers;

		[SerializeField]
		private float _disturbanceRadius = 0.3f;

		[SerializeField]
		private WaterDisturbForce _disturbancePower = WaterDisturbForce.Medium;

		[SerializeField]
		private float _moveDisturbanceRadius = 0.2f;

		[SerializeField]
		private WaterDisturbForce _moveDisturbancePower = WaterDisturbForce.XSmall;

		[SerializeField]
		private byte _ticksAfterDisturbs = 5;

		private sbyte _disturbsCounter;

		private Vector3 _swimFrom;

		private Vector3 _swimTo;

		private float _totalTime;

		private float _curTime;

		private float _spawnDeep;

		private CaymanSwimming.States _curState;

		private enum States
		{
			SwimUp,
			Swim,
			SwimDown
		}
	}
}
