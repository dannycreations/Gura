using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class WeatherSubMenu : SubMenuFoldoutBase
{
	protected override void Awake()
	{
		base.Awake();
		(this._weatherParent as RectTransform).anchoredPosition = Vector2.zero;
	}

	public void FillContent(IEnumerable<WeatherDesc> dayWeathers, IEnumerable<WeatherDesc> nightWeathers, IEnumerable<int> earlyMorningTemp, IEnumerable<int> lateEveningTemp, IEnumerable<int> earlyNightTemp, IEnumerable<int> lateNightTemp, IEnumerable<int> earlyMorningTempWater, IEnumerable<int> lateEveningTempWater, IEnumerable<int> earlyNightTempWater, IEnumerable<int> lateNightTempWater, int firstDay = 1)
	{
		if (this._weatherPreviews != null && this._weatherPreviews.Length < dayWeathers.Count<WeatherDesc>())
		{
			int num = this._weatherPreviews.Length;
			int num2 = dayWeathers.Count<WeatherDesc>();
			WeatherPreviewPanel[] array = Array.CreateInstance(typeof(WeatherPreviewPanel), num2) as WeatherPreviewPanel[];
			Array.Copy(this._weatherPreviews, array, num);
			this._weatherPreviews = array;
		}
		this._weatherPreviews = ((this._weatherPreviews != null) ? this._weatherPreviews : new WeatherPreviewPanel[dayWeathers.Count<WeatherDesc>()]);
		IEnumerator<WeatherDesc> enumerator = ((dayWeathers != null) ? dayWeathers.GetEnumerator() : null);
		IEnumerator<WeatherDesc> enumerator2 = ((nightWeathers != null) ? nightWeathers.GetEnumerator() : null);
		IEnumerator<int> enumerator3 = ((earlyMorningTemp != null) ? earlyMorningTemp.GetEnumerator() : null);
		IEnumerator<int> enumerator4 = ((lateEveningTemp != null) ? lateEveningTemp.GetEnumerator() : null);
		IEnumerator<int> enumerator5 = ((earlyNightTemp != null) ? earlyNightTemp.GetEnumerator() : null);
		IEnumerator<int> enumerator6 = ((lateNightTemp != null) ? lateNightTemp.GetEnumerator() : null);
		IEnumerator<int> enumerator7 = ((earlyMorningTempWater != null) ? earlyMorningTempWater.GetEnumerator() : null);
		IEnumerator<int> enumerator8 = ((lateEveningTempWater != null) ? lateEveningTempWater.GetEnumerator() : null);
		IEnumerator<int> enumerator9 = ((earlyNightTempWater != null) ? earlyNightTempWater.GetEnumerator() : null);
		IEnumerator<int> enumerator10 = ((lateNightTempWater != null) ? lateNightTempWater.GetEnumerator() : null);
		int num3 = 0;
		while (enumerator.MoveNext())
		{
			if (enumerator2 != null)
			{
				enumerator2.MoveNext();
			}
			enumerator3.MoveNext();
			enumerator4.MoveNext();
			enumerator5.MoveNext();
			enumerator6.MoveNext();
			enumerator7.MoveNext();
			enumerator8.MoveNext();
			enumerator9.MoveNext();
			enumerator10.MoveNext();
			WeatherPreviewPanel weatherPreviewPanel;
			if (this._weatherPreviews[num3] == null)
			{
				GameObject gameObject = GUITools.AddChild(this._weatherParent.gameObject, this._weatherPrefab.gameObject);
				gameObject.name = string.Format("WeatherPreviewPanel{0}", num3);
				weatherPreviewPanel = gameObject.GetComponent<WeatherPreviewPanel>();
				this._weatherPreviews[num3] = weatherPreviewPanel;
			}
			else
			{
				weatherPreviewPanel = this._weatherPreviews[num3];
			}
			weatherPreviewPanel.Init(new WeatherPreviewPanel.WeatherData(enumerator.Current, (nightWeathers == null) ? null : enumerator2.Current, new WeatherPreviewPanel.MinMaxRange(enumerator3.Current, enumerator4.Current), new WeatherPreviewPanel.MinMaxRange(enumerator5.Current, enumerator6.Current), new WeatherPreviewPanel.MinMaxRange(enumerator7.Current, enumerator8.Current), new WeatherPreviewPanel.MinMaxRange(enumerator9.Current, enumerator10.Current)), num3++ + firstDay, false, false);
			weatherPreviewPanel.GetComponent<PointerActionHandler>().OnSelected.RemoveAllListeners();
			weatherPreviewPanel.GetComponent<PointerActionHandler>().OnDeselected.RemoveAllListeners();
		}
	}

	[SerializeField]
	private WeatherPreviewPanel _weatherPrefab;

	[SerializeField]
	private Transform _weatherParent;

	[SerializeField]
	private WeatherPreviewPanel[] _weatherPreviews;
}
