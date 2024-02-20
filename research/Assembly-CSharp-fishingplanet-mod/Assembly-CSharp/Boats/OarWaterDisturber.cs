using System;
using UnityEngine;

namespace Boats
{
	public class OarWaterDisturber
	{
		public OarWaterDisturber(PaddleSettings settings)
		{
			this._disturbers = new OarWaterDisturber.Disturber[settings.Disturbers.Length];
			for (int i = 0; i < this._disturbers.Length; i++)
			{
				this._disturbers[i] = new OarWaterDisturber.Disturber(settings.Disturbers[i], settings);
			}
		}

		public void Update()
		{
			for (int i = 0; i < this._disturbers.Length; i++)
			{
				this._disturbers[i].Update();
			}
		}

		public void OnDestroy()
		{
			this._disturbers = null;
		}

		private static string[][] _oarSounds = new string[][]
		{
			new string[] { "Sounds/Actions/Kayak/sfx_boat_kayak_oar_left_shallow", "Sounds/Actions/Kayak/sfx_boat_kayak_oar_right_shallow" },
			new string[] { "Sounds/Actions/Kayak/sfx_boat_kayak_oar_left_deep", "Sounds/Actions/Kayak/sfx_boat_kayak_oar_right_deep" }
		};

		private OarWaterDisturber.Disturber[] _disturbers;

		private class Disturber
		{
			public Disturber(Transform obj, PaddleSettings settings)
			{
				this._t = obj;
				this._isInWater = this._t.position.y < 0f;
				this._settings = settings;
			}

			public void Update()
			{
				if (this._isInWater)
				{
					if (this._t.position.y > 0f)
					{
						this.ChangeState(false);
					}
					else if (this._disturbsCounter == 0)
					{
						this._disturbsCounter = this._settings.TicksBetweenDisturbs;
						GameFactory.Water.AddWaterDisturb(this._t.position, this._settings.DisturbenceRadius, this._settings.DisturbenceForce);
					}
					else
					{
						this._disturbsCounter -= 1;
					}
				}
				else if (this._t.position.y < 0f)
				{
					this._disturbsCounter = 0;
					this.ChangeState(true);
				}
			}

			private void ChangeState(bool isInWater)
			{
				this._isInWater = isInWater;
				DynWaterParticlesController.CreateSplash(null, this._t.position, this._settings.SplashParticlePath, this._settings.SplashSize, 1f, true, true, 1);
				string[] array = OarWaterDisturber._oarSounds[(!isInWater) ? 0 : 1];
				string text = array[Random.Range(0, array.Length)];
				RandomSounds.PlaySoundAtPoint(text, this._t.position, 0.5f, false);
			}

			private Transform _t;

			private bool _isInWater;

			private byte _disturbsCounter;

			private readonly PaddleSettings _settings;
		}
	}
}
