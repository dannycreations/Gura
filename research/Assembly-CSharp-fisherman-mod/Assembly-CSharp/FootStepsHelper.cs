using System;
using UnityEngine;

public class FootStepsHelper
{
	public FootStepsHelper(FootstepsSounds[] soundsBank, int stepPixelsSize)
	{
		this._stepPixelsSize = stepPixelsSize;
		this._soundsBank = new FootstepsSounds[9];
		for (int i = 0; i < soundsBank.Length; i++)
		{
			this._soundsBank[(int)((byte)soundsBank[i].Material)] = soundsBank[i];
		}
	}

	public void Clean()
	{
		this._soundsBank = null;
	}

	public void PlayFootStepAudio(Vector3 footPosition, Vector3 rayEndPosition, AudioSource audioSource)
	{
		RaycastHit raycastHit;
		Physics.Linecast(footPosition, rayEndPosition, ref raycastHit);
		SurfaceMaterial surfaceMaterial;
		if ((double)raycastHit.point.y < 0.01)
		{
			surfaceMaterial = SurfaceMaterial.WATER;
		}
		else
		{
			surfaceMaterial = SurfaceSettings.Instance.GetMaterial(raycastHit, this._stepPixelsSize);
		}
		if (this._soundsBank[(int)((byte)surfaceMaterial)] != null)
		{
			this._soundsBank[(int)((byte)surfaceMaterial)].PlayRandom(audioSource);
		}
		else if (surfaceMaterial != SurfaceMaterial.NONE)
		{
			LogHelper.Error("Can't find sound for material {0}", new object[] { surfaceMaterial });
		}
	}

	private FootstepsSounds[] _soundsBank;

	private int _stepPixelsSize;
}
