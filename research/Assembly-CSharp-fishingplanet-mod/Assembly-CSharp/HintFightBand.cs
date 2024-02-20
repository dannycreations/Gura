using System;
using TMPro;
using UnityEngine;

public class HintFightBand : HintColorText
{
	protected override void Init()
	{
		Transform[] transform4FillData = this.GetTransform4FillData();
		if (transform4FillData != null)
		{
			for (int i = 0; i < transform4FillData.Length; i++)
			{
				base.FillData<TextMeshProUGUI>(this.Data, new Action<TextMeshProUGUI>(base.CloneFontMaterial), true, transform4FillData[i]);
			}
		}
	}

	protected virtual Transform[] GetTransform4FillData()
	{
		return null;
	}
}
