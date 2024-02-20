using System;
using UnityEngine;

namespace TPM
{
	public class EyesController
	{
		public EyesController(GameObject[] eyeModels, EyeSettings settings)
		{
			this._eyeModels = eyeModels;
			this._settings = settings;
		}

		public void Update()
		{
			if (this._startMovementAt < 0f)
			{
				this.PrepareNewMovement();
			}
			if (this._startMovementAt < Time.time && GameFactory.Player != null)
			{
				if (this._curFish != null && this._curFish.transform != null)
				{
					for (int i = 0; i < this._eyeModels.Length; i++)
					{
						this._eyeModels[i].transform.LookAt(this._curFish.transform);
					}
				}
				else
				{
					float num = RandomHelper.GetMarsaglia(1f, 0.2f, 0f) * this._settings.sideMaxAngleDelta;
					float num2 = RandomHelper.GetMarsaglia(1f, 0.2f, 0f) * this._settings.verticalMaxAngleDelta;
					for (int j = 0; j < this._eyeModels.Length; j++)
					{
						this._eyeModels[j].transform.localRotation = Quaternion.Euler(num2, num, 0f);
					}
				}
				this.PrepareNewMovement();
			}
		}

		public void UpdateFish(Fish3rdBehaviour fish)
		{
			this._curFish = fish;
		}

		private void PrepareNewMovement()
		{
			this._startMovementAt = Time.time + Random.Range(this._settings.minDelay, this._settings.maxDelay);
		}

		private GameObject[] _eyeModels;

		private EyeSettings _settings;

		private float _startMovementAt = -1f;

		private float _targetDeltaYaw;

		private float _targetDeltaPitch;

		private Fish3rdBehaviour _curFish;
	}
}
