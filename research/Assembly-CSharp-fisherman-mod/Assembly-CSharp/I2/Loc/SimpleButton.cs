﻿using System;
using UnityEngine;

namespace I2.Loc
{
	public class SimpleButton : MonoBehaviour
	{
		public void OnMouseUp()
		{
			base.gameObject.SendMessage("OnClick", 1);
		}
	}
}
