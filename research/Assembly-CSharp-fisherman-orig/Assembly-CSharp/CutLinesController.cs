using System;
using System.Diagnostics;
using System.Globalization;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class CutLinesController : CutControllerBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float> ConfirmActionCalled;

	protected override void Awake()
	{
		base.Awake();
		CutLinesController._instance = this;
	}

	protected override void Update()
	{
		if (Math.Abs(this._length - this.SliderControl.value) > 0.01f)
		{
			this._length = MeasuringSystemManager.LineBackLength(this.SliderControl.value);
			Text targetLength = this.TargetLength;
			string text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
			this.TextValueControl.text = text;
			targetLength.text = text;
			this.BaseLength.text = ((int)MeasuringSystemManager.LineLength(this._lineCapacity - this._length)).ToString();
		}
		base.Update();
	}

	public void Init(float reelCapacity, float lineCapacity, InventoryItem line)
	{
		this._length = 15f;
		this._lineCapacity = lineCapacity;
		this._lineId = line.InstanceId.Value;
		this.SliderControl.minValue = MeasuringSystemManager.LineLength(15f);
		this.SliderControl.maxValue = MeasuringSystemManager.LineLength(reelCapacity);
		this.SliderControl.value = this.SliderControl.maxValue;
		Text targetLength = this.TargetLength;
		string text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
		this.TextValueControl.text = text;
		targetLength.text = text;
		this.BaseLength.text = ((int)MeasuringSystemManager.LineLength(this._lineCapacity - this._length)).ToString();
		this.BaseImageLdbl.Image = this.BaseImage;
		this.BaseImageLdbl.OnLoaded.RemoveAllListeners();
		this.BaseImageLdbl.OnLoaded.AddListener(delegate
		{
			Sprite sprite = this.BaseImage.overrideSprite;
			DropMe component = InitRods.Instance.ActiveRod.Reel.GetComponent<DropMe>();
			if (component != null && component.receivingImage != null && component.receivingImage.overrideSprite != null)
			{
				sprite = component.receivingImage.overrideSprite;
			}
			this.TargetImage.overrideSprite = sprite;
			this.BaseImageLdbl.OnLoaded.RemoveAllListeners();
		});
		this.BaseImageLdbl.Load(string.Format("Textures/Inventory/{0}", line.DollThumbnailBID.Value.ToString()));
		base.Open();
	}

	public static float GetMaxLineLength()
	{
		if (CutLinesController._instance == null)
		{
			return 100f;
		}
		return Mathf.Floor(CutLinesController._instance.SliderControl.maxValue);
	}

	public static Guid GetCurrentLineInstanceID()
	{
		if (CutLinesController._instance == null)
		{
			return Guid.Empty;
		}
		return CutLinesController._instance._lineId;
	}

	public static float GetCurrentLineLength()
	{
		if (CutLinesController._instance == null)
		{
			return 0f;
		}
		return CutLinesController._instance.SliderControl.value;
	}

	protected override void AcceptActionCalled()
	{
		if (this.ConfirmActionCalled != null)
		{
			this.ConfirmActionCalled(this._length);
		}
	}

	public const int MinLength = 15;

	private float _length;

	private float _lineCapacity;

	private static CutLinesController _instance;

	private Guid _lineId;
}
