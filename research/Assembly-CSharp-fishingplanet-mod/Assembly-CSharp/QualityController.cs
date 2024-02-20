using System;
using UnityEngine;

public class QualityController : MonoBehaviour
{
	private void Start()
	{
		this.InitParams();
		this.UpdateAllParams(this._currentQualities);
	}

	private void InitParams()
	{
		if (GameFactory.Player != null && GameFactory.Player.CameraController != null)
		{
			this._camera = GameFactory.Player.CameraController.Camera;
		}
		if (GameFactory.Water != null)
		{
			this._waterTile = GameFactory.Water.FishWaterTileInstance;
			this._fishWaterBase = GameFactory.Water.FishWaterBaseInstance;
		}
		this._currentQualities = SettingsManager.RenderQuality;
		this._skyController = GameFactory.SkyControllerInstance;
		this._currentSSAO = SettingsManager.SSAO;
	}

	private void Update()
	{
		if (this._currentQualities != SettingsManager.RenderQuality || !this._initedQuality)
		{
			if (this._camera != null && this._waterTile != null && this._fishWaterBase && this._skyController != null)
			{
				this._currentQualities = SettingsManager.RenderQuality;
				this._initedQuality = true;
				this.UpdateAllParams(this._currentQualities);
			}
			else
			{
				this.InitParams();
			}
		}
		if ((this._currentSSAO != SettingsManager.SSAO || !this._initedSSAO) && this._camera != null)
		{
			this._initedSSAO = true;
			this._currentSSAO = SettingsManager.SSAO;
			this._camera.GetComponent<SSAOPro>().enabled = this._currentSSAO;
		}
		if ((this._currentDynWater != SettingsManager.DynWater || !this._initedDynWater) && this._fishWaterBase != null && this._waterTile != null)
		{
			this._initedDynWater = true;
			this._currentDynWater = SettingsManager.DynWater;
			this.ChangeDynWater(this._currentDynWater);
		}
		if ((this._currentAntialiasing != SettingsManager.Antialiasing || !this._initedAntialiasing) && this._camera != null)
		{
			this._initedAntialiasing = true;
			this._currentAntialiasing = SettingsManager.Antialiasing;
			this.ChangeAntialiasingWater(this._currentAntialiasing);
		}
	}

	private void ChangeAntialiasingWater(AntialiasingValue antialiasing)
	{
		if (antialiasing != AntialiasingValue.Off)
		{
			if (antialiasing != AntialiasingValue.High)
			{
				if (antialiasing == AntialiasingValue.Low)
				{
					DebugUtility.Settings.Trace("Antialiasing low", new object[0]);
				}
			}
			else
			{
				DebugUtility.Settings.Trace("Antialiasing high", new object[0]);
			}
		}
		else
		{
			DebugUtility.Settings.Trace("Antialiasing off", new object[0]);
		}
	}

	private void ChangeDynWater(DynWaterValue dynWater)
	{
		if (dynWater != DynWaterValue.Off)
		{
			if (dynWater != DynWaterValue.High)
			{
				if (dynWater == DynWaterValue.Low)
				{
					DebugUtility.Settings.Trace("Dyn water low", new object[0]);
					if (this._fishWaterBase != null)
					{
						this._fishWaterBase.dynWaterQuality = FishDynWaterQuality.Low;
					}
				}
			}
			else
			{
				DebugUtility.Settings.Trace("Dyn water high", new object[0]);
				if (this._fishWaterBase != null)
				{
					this._fishWaterBase.dynWaterQuality = FishDynWaterQuality.High;
				}
			}
		}
		else
		{
			DebugUtility.Settings.Trace("Dyn water off", new object[0]);
			if (this._fishWaterBase != null)
			{
				this._fishWaterBase.dynWaterQuality = FishDynWaterQuality.None;
			}
		}
	}

	public void UpdateAllParams(RenderQualities quality)
	{
		switch (quality)
		{
		case RenderQualities.Fastest:
			this.SetFastestQuality();
			break;
		case RenderQualities.Fast:
			this.SetFastQuality();
			break;
		case RenderQualities.Simple:
			this.SetSimpleQuality();
			break;
		case RenderQualities.Good:
			this.SetGoodQuality();
			break;
		case RenderQualities.Beautiful:
			this.SetBeautifulQuality();
			break;
		case RenderQualities.Fantastic:
			this.SetFantasticQuality();
			break;
		case RenderQualities.Ultra:
			this.SetUltraQuality();
			break;
		}
	}

	private void SetFastestQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.VeryLow;
			this._camera.GetComponent<SSAOPro>().Downsampling = 3;
		}
	}

	private void SetFastQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.Low;
			this._camera.GetComponent<SSAOPro>().Downsampling = 3;
		}
	}

	private void SetSimpleQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.Low;
			this._camera.GetComponent<SSAOPro>().Downsampling = 2;
		}
	}

	private void SetGoodQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.Medium;
			this._camera.GetComponent<SSAOPro>().Downsampling = 2;
		}
	}

	private void SetBeautifulQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.Medium;
			this._camera.GetComponent<SSAOPro>().Downsampling = 1;
		}
	}

	private void SetFantasticQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.High;
			this._camera.GetComponent<SSAOPro>().Downsampling = 1;
		}
	}

	private void SetUltraQuality()
	{
		if (this._camera != null)
		{
			this._camera.GetComponent<SSAOPro>().NoiseTexture = null;
			this._camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.Ultra;
			this._camera.GetComponent<SSAOPro>().Downsampling = 1;
		}
	}

	private RenderQualities _currentQualities;

	private bool _initedQuality;

	private bool _currentSSAO;

	private bool _initedSSAO;

	private AntialiasingValue _currentAntialiasing;

	private bool _initedAntialiasing;

	private DynWaterValue _currentDynWater;

	private bool _initedDynWater;

	private Camera _camera;

	private FishWaterTile _waterTile;

	private FishWaterBase _fishWaterBase;

	private ISkyController _skyController;
}
