using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSMeter : MonoBehaviour
{
	private Text p_text
	{
		get
		{
			if (this.m_text == null)
			{
				this.m_text = base.GetComponent<Text>();
			}
			return this.m_text;
		}
	}

	private void Update()
	{
		float unscaledTime = Time.unscaledTime;
		float num = unscaledTime - this.m_cycleStartTime;
		if (num >= 0.5f)
		{
			int frameCount = Time.frameCount;
			this.p_text.text = Mathf.RoundToInt((float)(frameCount - this.m_cycleStartFrame) / num).ToString() + " fps";
			this.m_cycleStartFrame = frameCount;
			this.m_cycleStartTime = unscaledTime;
		}
	}

	private Text m_text;

	private const float updatePeriod = 0.5f;

	private float m_cycleStartTime;

	private int m_cycleStartFrame;
}
