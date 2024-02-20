using System;
using UnityEngine;

public class ClearUnusedAssetResources : MonoBehaviour
{
	private void Start()
	{
		switch (SettingsManager.RenderQuality)
		{
		case RenderQualities.Fastest:
		case RenderQualities.Fast:
			this._clearDeltaTime = 120f;
			break;
		case RenderQualities.Simple:
		case RenderQualities.Good:
			this._clearDeltaTime = 300f;
			break;
		case RenderQualities.Beautiful:
		case RenderQualities.Fantastic:
		case RenderQualities.Ultra:
			this._clearDeltaTime = 600f;
			break;
		}
	}

	private void Update()
	{
		this._currentTime += Time.deltaTime;
		if (this._currentTime >= this._clearDeltaTime)
		{
			Resources.UnloadUnusedAssets();
			this._currentTime = 0f;
		}
	}

	private float _currentTime;

	private float _clearDeltaTime = 300f;
}
