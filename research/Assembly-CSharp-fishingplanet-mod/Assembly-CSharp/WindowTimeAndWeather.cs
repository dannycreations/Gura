using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowTimeAndWeather : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<MetadataForUserCompetitionPondWeather.DayWeather[], MetadataForUserCompetitionPondWeather.DayWeather> OnSelected = delegate(MetadataForUserCompetitionPondWeather.DayWeather[] weathers, MetadataForUserCompetitionPondWeather.DayWeather weather)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		int num = 5;
		for (int i = 0; i < this._daysPos.Length; i++)
		{
			this._daysPos[i].text = MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, num, 0, 0));
			num += 2;
			if (num > 23)
			{
				num = 1;
			}
		}
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionPondWeather += this.Instance_OnGetMetadataForCompetitionPondWeather;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionPondWeather += new OnFailureUserCompetition(this.Instance_OnFailureGetMetadataForCompetitionPondWeather);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionPondWeather -= this.Instance_OnGetMetadataForCompetitionPondWeather;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionPondWeather -= new OnFailureUserCompetition(this.Instance_OnFailureGetMetadataForCompetitionPondWeather);
	}

	public void Init(int pondId, MetadataForUserCompetitionPondWeather.DayWeather[] dayWeathers, MetadataForUserCompetitionPondWeather.DayWeather selectedDayWeather)
	{
		this._pondId = pondId;
		if (dayWeathers != null && selectedDayWeather != null)
		{
			MetadataForUserCompetitionPondWeather metadataForUserCompetitionPondWeather = new MetadataForUserCompetitionPondWeather
			{
				DayWeathers = dayWeathers
			};
			this.MetadataPondWeather = metadataForUserCompetitionPondWeather;
			this.RefreshWeathers(selectedDayWeather);
		}
		else
		{
			this.Refresh();
		}
	}

	public void Inc(int v)
	{
		int num = this._index + v;
		if (num == this._pointPosx.Count)
		{
			num = 0;
		}
		else if (num < 0)
		{
			num = this._pointPosx.Count - 1;
		}
		this.UpdateIndex(num, 0.12f, false);
	}

	public void Refresh()
	{
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.GetMetadataForCompetitionPondWeather(this._pondId, null);
	}

	public void Click(int i)
	{
		this.UpdateIndex(i, 0.12f, false);
	}

	protected override void AcceptActionCalled()
	{
		if (this._weatherDesc != null)
		{
			this.OnSelected(this.MetadataPondWeather.DayWeathers, this._weatherDesc);
		}
	}

	private void UpdateIndex(int i, float animSpeed = 0.12f, bool forceUpdate = false)
	{
		if (this._dayFinal.Count > 0 && (i != this._index || forceUpdate))
		{
			this._index = i;
			this._weatherDesc = this._dayFinal[this._index];
			ShortcutExtensions.DOKill(this._pointer, false);
			ShortcutExtensions.DOLocalMoveX(this._pointer, this._pointPosx[this._index], animSpeed, false);
			this.UpdateData();
		}
	}

	private void UpdateData()
	{
		if (this._weatherDesc == null)
		{
			return;
		}
		this._time.text = MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, this._weatherDesc.Hour, 0, 0));
		TournamentHelper.TournamentWeatherDesc tournamentWeather = TournamentHelper.GetTournamentWeather(this._weatherDesc);
		this._tempIco.text = tournamentWeather.WeatherIcon;
		this._tempIco2.text = tournamentWeather.PressureIcon;
		this._temp.text = tournamentWeather.WeatherTemperature;
		this._tempWater.text = tournamentWeather.WaterTemperature;
		this._wind.text = string.Format("{0} {1} {2}", tournamentWeather.WeatherWindDirection, tournamentWeather.WeatherWindPower, tournamentWeather.WeatherWindSuffix);
	}

	private void Instance_OnGetMetadataForCompetitionPondWeather(MetadataForUserCompetitionPondWeather metadataPondWeather)
	{
		this.MetadataPondWeather = metadataPondWeather;
		this.RefreshWeathers(null);
		UIHelper.Waiting(false, null);
	}

	private void RefreshWeathers(MetadataForUserCompetitionPondWeather.DayWeather selectedDayWeather)
	{
		this._dayFinal.Clear();
		List<MetadataForUserCompetitionPondWeather.DayWeather> list = new List<MetadataForUserCompetitionPondWeather.DayWeather>();
		List<MetadataForUserCompetitionPondWeather.DayWeather> list2 = new List<MetadataForUserCompetitionPondWeather.DayWeather>();
		for (int i = 0; i < this.MetadataPondWeather.DayWeathers.Length; i++)
		{
			MetadataForUserCompetitionPondWeather.DayWeather dayWeather = this.MetadataPondWeather.DayWeathers[i];
			bool flag = TimeSpanExtension.IsNightFrame(dayWeather.TimeOfDay);
			if (flag)
			{
				list2.Add(dayWeather);
			}
			else
			{
				list.Add(dayWeather);
			}
			if (dayWeather.FishingDiagramImageId != null)
			{
				if (flag)
				{
					this._nightImgldbl.Load(dayWeather.FishingDiagramImageId, this._nightImg, "Textures/Inventory/{0}");
				}
				else
				{
					this._imgldbl.Load(dayWeather.FishingDiagramImageId, this._dayImg, "Textures/Inventory/{0}");
				}
			}
		}
		List<MetadataForUserCompetitionPondWeather.DayWeather> list3 = new List<MetadataForUserCompetitionPondWeather.DayWeather>();
		List<MetadataForUserCompetitionPondWeather.DayWeather> list4 = new List<MetadataForUserCompetitionPondWeather.DayWeather>();
		this.CorrectDays(5, 21, list, list3, null);
		this.CorrectDays(21, 24, list2, list4, list3.Last<MetadataForUserCompetitionPondWeather.DayWeather>());
		this.CorrectDays(0, 5, list2, list4, list4.Last<MetadataForUserCompetitionPondWeather.DayWeather>());
		this._dayFinal.AddRange(list3);
		this._dayFinal.AddRange(list4);
		this.UpdateIndex((selectedDayWeather != null) ? this._dayFinal.FindIndex((MetadataForUserCompetitionPondWeather.DayWeather p) => p.Hour == selectedDayWeather.Hour) : 0, 0f, true);
	}

	private void CorrectDays(int start, int end, List<MetadataForUserCompetitionPondWeather.DayWeather> data, List<MetadataForUserCompetitionPondWeather.DayWeather> container, MetadataForUserCompetitionPondWeather.DayWeather prev = null)
	{
		MetadataForUserCompetitionPondWeather.DayWeather dayWeather = prev;
		while (start < end)
		{
			MetadataForUserCompetitionPondWeather.DayWeather dayWeather2 = data.FirstOrDefault((MetadataForUserCompetitionPondWeather.DayWeather p) => p.Hour == start);
			if (dayWeather2 != null)
			{
				dayWeather = dayWeather2;
				container.Add(dayWeather2);
			}
			else if (dayWeather != null)
			{
				container.Add(this.CopyDayWeather(dayWeather, start));
			}
			start++;
		}
	}

	private MetadataForUserCompetitionPondWeather.DayWeather CopyDayWeather(MetadataForUserCompetitionPondWeather.DayWeather d, int hour)
	{
		return new MetadataForUserCompetitionPondWeather.DayWeather
		{
			Hour = hour,
			PondId = d.PondId,
			Date = d.Date,
			TimeOfDay = d.TimeOfDay,
			Icon = d.Icon,
			Name = d.Name,
			AirTemperature = d.AirTemperature,
			WaterTemperature = d.WaterTemperature,
			Pressure = d.Pressure,
			WindSpeed = d.WindSpeed,
			WindDirection = d.WindDirection,
			Precipitation = d.Precipitation,
			FishPlayFreq = d.FishPlayFreq,
			Sky = d.Sky,
			FishingDiagramImageId = d.FishingDiagramImageId,
			FishingDiagramDesc = d.FishingDiagramDesc
		};
	}

	private void Instance_OnFailureGetMetadataForCompetitionPondWeather(Failure failure)
	{
		UIHelper.Waiting(false, null);
	}

	[SerializeField]
	private TextMeshProUGUI _time;

	[SerializeField]
	private TextMeshProUGUI _temp;

	[SerializeField]
	private TextMeshProUGUI _tempIco;

	[SerializeField]
	private TextMeshProUGUI _tempIco2;

	[SerializeField]
	private TextMeshProUGUI _tempWater;

	[SerializeField]
	private TextMeshProUGUI _wind;

	[SerializeField]
	private Image _dayImg;

	[SerializeField]
	private Image _nightImg;

	private ResourcesHelpers.AsyncLoadableImage _imgldbl = new ResourcesHelpers.AsyncLoadableImage();

	private ResourcesHelpers.AsyncLoadableImage _nightImgldbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private RectTransform _pointer;

	[SerializeField]
	private Text[] _daysPos;

	private int _pondId;

	private int _index;

	private MetadataForUserCompetitionPondWeather.DayWeather _weatherDesc;

	protected MetadataForUserCompetitionPondWeather MetadataPondWeather;

	private const float AnimSpeed = 0.12f;

	private readonly List<float> _pointPosx = new List<float>
	{
		-260f, -227f, -194f, -162f, -129f, -96f, -63f, -30f, 2f, 35f,
		68f, 101f, 134f, 167f, 199f, 232f, 296f, 328f, 360f, 389f,
		421f, 452f, 484f, 514f
	};

	private readonly List<MetadataForUserCompetitionPondWeather.DayWeather> _dayFinal = new List<MetadataForUserCompetitionPondWeather.DayWeather>();
}
