using System;
using Boats;
using UnityEngine;

namespace TPM
{
	public class TPMSenderDataCache : TPMDataCache
	{
		public TPMSenderDataCache()
		{
			this._packageCurIndex = 0;
			this._nextUpdateTimeAt = -1f;
			this._curFraction = new ThirdPersonData();
			this._photoModeFractions = new ThirdPersonData[]
			{
				new ThirdPersonData(),
				new ThirdPersonData()
			};
		}

		public ThirdPersonData CurFraction
		{
			get
			{
				return this._curFraction;
			}
		}

		private Package UpdateCurFraction(Vector3 colliderPosition, Transform cameraTransform, IBoatController boatController, bool forceSend)
		{
			this._curFraction.SetSetverTime();
			this._curFraction.playerPosition = colliderPosition;
			this._curFraction.playerRotation = cameraTransform.rotation;
			this._curFraction.UpdateBoat(boatController);
			this._package[this._packageCurIndex++].Clone(this._curFraction);
			if (forceSend || this._packageCurIndex == 3)
			{
				this._package.SetPackageLength(this._packageCurIndex);
				this._packageCurIndex = 0;
				return this._package;
			}
			return null;
		}

		public Package SetViewPause(bool flag, Vector3 colliderPosition, Transform cameraTransform, IBoatController boatController)
		{
			if (this._lastPauseFlag == flag)
			{
				return null;
			}
			this._lastPauseFlag = flag;
			this._curFraction.isPaused = flag;
			this._nextUpdateTimeAt = Time.realtimeSinceStartup + 0.1f;
			return this.UpdateCurFraction(colliderPosition, cameraTransform, boatController, true);
		}

		public Package Update(Vector3 colliderPosition, Transform cameraTransform, IBoatController boatController)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (this._nextUpdateTimeAt <= realtimeSinceStartup)
			{
				if (this._nextUpdateTimeAt < 0f)
				{
					this._nextUpdateTimeAt = realtimeSinceStartup + 0.1f;
				}
				else
				{
					this._nextUpdateTimeAt += 0.1f;
				}
				return this.UpdateCurFraction(colliderPosition, cameraTransform, boatController, false);
			}
			return null;
		}

		public ThirdPersonData GetPhotoModeData(Vector3 colliderPosition, Quaternion playerRotation, IBoatController boatController, bool isHoldFishInHands)
		{
			if ((this._curPhotoModeFractionIndex += 1) == 2)
			{
				this._curPhotoModeFractionIndex = 0;
			}
			ThirdPersonData thirdPersonData = this._photoModeFractions[(int)this._curPhotoModeFractionIndex];
			thirdPersonData.UpdateBoat(boatController);
			thirdPersonData.Clone(this._curFraction);
			thirdPersonData.ReplacePhotoModeData(isHoldFishInHands);
			thirdPersonData.playerPosition = colliderPosition;
			thirdPersonData.playerRotation = playerRotation;
			return thirdPersonData;
		}

		private readonly Package _package = new Package();

		private readonly ThirdPersonData _curFraction;

		private readonly ThirdPersonData[] _photoModeFractions;

		private byte _curPhotoModeFractionIndex;

		private float _nextUpdateTimeAt;

		private int _packageCurIndex;

		private bool _lastPauseFlag = true;
	}
}
