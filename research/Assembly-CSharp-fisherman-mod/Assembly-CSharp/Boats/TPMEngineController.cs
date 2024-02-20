using System;
using TPM;
using UnityEngine;

namespace Boats
{
	public class TPMEngineController
	{
		public TPMEngineController(TPMEngineSettings engineSettings)
		{
			this._engine = engineSettings;
			this._fHashes = new int[5];
			for (int i = 0; i < this._fHashes.Length; i++)
			{
				int[] fHashes = this._fHashes;
				int num = i;
				TPMMecanimFParameter tpmmecanimFParameter = (TPMMecanimFParameter)i;
				fHashes[num] = Animator.StringToHash(tpmmecanimFParameter.ToString());
			}
			this._bHashes = new int[14];
			for (int j = 0; j < this._bHashes.Length; j++)
			{
				int[] bHashes = this._bHashes;
				int num2 = j;
				TPMMecanimBParameter tpmmecanimBParameter = (TPMMecanimBParameter)j;
				bHashes[num2] = Animator.StringToHash(tpmmecanimBParameter.ToString());
			}
			this._prevFValues = new float[this._fHashes.Length];
			this._targetFValues = new float[this._fHashes.Length];
		}

		public void ServerUpdate(ThirdPersonData data)
		{
			bool flag = data.BoolParameters[7] && !data.BoolParameters[8];
			for (int i = 0; i < TPMEngineController._floatParamsToUpdate.Length; i++)
			{
				byte b = TPMEngineController._floatParamsToUpdate[i];
				this._prevFValues[(int)b] = this._targetFValues[(int)b];
				this._targetFValues[(int)b] = ((!flag) ? 0f : data.FloatParameters[(int)b]);
			}
			for (int j = 0; j < TPMEngineController._boolParamsToUpdate.Length; j++)
			{
				byte b2 = TPMEngineController._boolParamsToUpdate[j];
				this._engine.Animator.SetBool(this._bHashes[(int)b2], data.BoolParameters[(int)b2]);
			}
		}

		public void SyncUpdate(float prc)
		{
			for (int i = 0; i < TPMEngineController._floatParamsToUpdate.Length; i++)
			{
				byte b = TPMEngineController._floatParamsToUpdate[i];
				this._engine.Animator.SetFloat(this._fHashes[(int)b], Mathf.Lerp(this._prevFValues[(int)b], this._targetFValues[(int)b], prc));
			}
		}

		public void OnDestroy()
		{
			this._engine = null;
		}

		private static byte[] _floatParamsToUpdate = new byte[] { 0, 3 };

		private static byte[] _boolParamsToUpdate = new byte[] { 10, 11, 12, 13 };

		private TPMEngineSettings _engine;

		private int[] _fHashes;

		private int[] _bHashes;

		private float[] _prevFValues;

		private float[] _targetFValues;
	}
}
