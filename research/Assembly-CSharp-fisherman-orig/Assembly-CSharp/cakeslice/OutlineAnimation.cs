using System;
using UnityEngine;

namespace cakeslice
{
	public class OutlineAnimation : MonoBehaviour
	{
		private void Start()
		{
		}

		private void Update()
		{
			Color lineColor = base.GetComponent<OutlineEffect>().lineColor0;
			if (this.pingPong)
			{
				lineColor.a += Time.deltaTime;
				if (lineColor.a >= 1f)
				{
					this.pingPong = false;
				}
			}
			else
			{
				lineColor.a -= Time.deltaTime;
				if (lineColor.a <= 0f)
				{
					this.pingPong = true;
				}
			}
			lineColor.a = Mathf.Clamp01(lineColor.a);
			base.GetComponent<OutlineEffect>().lineColor0 = lineColor;
			base.GetComponent<OutlineEffect>().UpdateMaterialsPublicProperties();
		}

		private bool pingPong;
	}
}
