using System;
using Assets.Scripts.UI._2D.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CutControllerBase : WindowBase, IScrollHandler, IEventSystemHandler
{
	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	protected override void Update()
	{
		this.SliderControl.value = (float)((int)Mathf.Min((float)((int)this.SliderControl.maxValue), Mathf.Max(Mathf.Ceil(this.SliderControl.minValue), this.SliderControl.value)));
		base.Update();
	}

	public void IncreaseClick()
	{
		if ((int)this.SliderControl.value >= (int)this.SliderControl.maxValue)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			return;
		}
		this.SliderControl.value += 1f;
	}

	public void DecreaseClick()
	{
		if ((int)this.SliderControl.value <= (int)this.SliderControl.minValue)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			return;
		}
		this.SliderControl.value -= 1f;
	}

	public virtual void SetMax()
	{
		this.SliderControl.value = this.SliderControl.maxValue;
	}

	public virtual void SetMin()
	{
		this.SliderControl.value = this.SliderControl.minValue;
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.SliderControl.value += (float)((int)eventData.scrollDelta.y);
		this.Update();
	}

	[SerializeField]
	protected Slider SliderControl;

	[SerializeField]
	protected Text TextValueControl;

	[SerializeField]
	protected Image BaseImage;

	protected ResourcesHelpers.AsyncLoadableImage BaseImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected Image TargetImage;

	[SerializeField]
	protected Text BaseLength;

	[SerializeField]
	protected Text TargetLength;

	[SerializeField]
	protected BorderedButton AcceptBtn;
}
