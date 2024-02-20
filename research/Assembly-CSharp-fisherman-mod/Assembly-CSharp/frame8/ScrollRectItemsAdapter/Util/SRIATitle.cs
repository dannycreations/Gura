using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class SRIATitle : MonoBehaviour
	{
		private void Start()
		{
			base.GetComponent<Text>().text = "Optimized ScrollView Adapter v3.2";
		}
	}
}
