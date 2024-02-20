using System;
using System.Diagnostics;
using System.Globalization;
using Assets.Scripts.UI._2D.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineLeashLengthController : MessageBoxBase, IScrollHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<LineLeashLengthEventArgs> ConfirmActionCalled;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	public void Init(float currentLength, float minLength, float maxLength, bool isLeader = false)
	{
		this._length = currentLength;
		this._isLeader = isLeader;
		this.SliderControl.direction = ((!this._isLeader) ? 0 : 1);
		if (this.BaseLength != null)
		{
			this.BaseLength.text = ((int)this._length).ToString();
		}
		this.SliderControl.maxValue = ((!this._isLeader) ? maxLength : this._length);
		this.SliderControl.minValue = minLength;
		this.SliderControl.value = this._length;
		this.SliderControl.onValueChanged.AddListener(delegate(float val)
		{
			this._length = MeasuringSystemManager.LineLeashBackLength(val);
			if (this.TargetLength != null)
			{
				Text targetLength = this.TargetLength;
				string text = val.ToString(CultureInfo.InvariantCulture);
				this.TextValueControl.text = text;
				targetLength.text = text;
			}
		});
		this.TextValueControl.text = this.SliderControl.value.ToString();
		if (this.TargetImage != null && this.BaseImage != null)
		{
			this._baseImageLdbl.Image = this.BaseImage;
			this._baseImageLdbl.OnLoaded.RemoveAllListeners();
			this._baseImageLdbl.OnLoaded.AddListener(delegate
			{
				this.TargetImage.overrideSprite = this.BaseImage.overrideSprite;
				this._baseImageLdbl.OnLoaded.RemoveAllListeners();
			});
			this._baseImageLdbl.Load(string.Format("Textures/Inventory/{0}", InitRods.Instance.ActiveRod.Leader.InventoryItem.DollThumbnailBID.Value.ToString()));
		}
	}

	public void ConfirmLength()
	{
		if (this._alphaFade != null && !this._alphaFade.IsHiding)
		{
			if (this.ConfirmActionCalled != null)
			{
				this.ConfirmActionCalled(this, new LineLeashLengthEventArgs
				{
					LeashLength = this.SliderControl.value
				});
			}
			this._alphaFade.HidePanel();
		}
	}

	public void IncreaseClick()
	{
		if (this._isLeader)
		{
			this.Decrease();
			return;
		}
		this.Increase();
	}

	private void Increase()
	{
		this._incFrame = ((Mathf.Abs(this._lastIncFrame - Time.frameCount) >= this._framesToIncreaseStep) ? 0 : (this._incFrame + 1));
		this.SliderControl.value = Mathf.Min(new float[]
		{
			this.SliderControl.value + 1f + (float)Mathf.Min(this._maxStep, this._incFrame / this._framesToIncreaseStep),
			this.SliderControl.maxValue
		});
		this.TextValueControl.text = this.SliderControl.value.ToString();
		this._lastIncFrame = Time.frameCount;
	}

	public void DecreaseClick()
	{
		if (this._isLeader)
		{
			this.Increase();
			return;
		}
		this.Decrease();
	}

	private void Decrease()
	{
		this._decFrame = ((Mathf.Abs(this._lastDecFrame - Time.frameCount) >= this._framesToIncreaseStep) ? 0 : (this._decFrame + 1));
		this.SliderControl.value = Mathf.Max(new float[]
		{
			this.SliderControl.value - 1f - (float)Mathf.Min(this._maxStep, this._decFrame / this._framesToIncreaseStep),
			this.SliderControl.minValue
		});
		this.TextValueControl.text = this.SliderControl.value.ToString();
		this._lastDecFrame = Time.frameCount;
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.SliderControl.value += (float)((int)eventData.scrollDelta.y);
		this.TextValueControl.text = this.SliderControl.value.ToString();
	}

	private bool _isChanged;

	private float _lineCapacity;

	private int _lastIncFrame = -1;

	private int _lastDecFrame = -1;

	private int _incFrame;

	private int _decFrame;

	private int _framesToIncreaseStep = 10;

	private int _maxStep = 5;

	public Text TextValueControl;

	public Slider SliderControl;

	[SerializeField]
	protected Image BaseImage;

	[SerializeField]
	protected Image TargetImage;

	private ResourcesHelpers.AsyncLoadableImage _baseImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected Text BaseLength;

	[SerializeField]
	protected Text TargetLength;

	private float _length;

	private bool _isLeader;
}
