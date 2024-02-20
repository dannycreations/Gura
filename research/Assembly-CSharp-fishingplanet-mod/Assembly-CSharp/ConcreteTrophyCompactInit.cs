using System;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteTrophyCompactInit : MonoBehaviour
{
	public void Refresh(FishStat fishStat, bool is3DInit = false)
	{
		this._fishStat = fishStat;
		this.Name.text = fishStat.MaxFish.Name;
		this.ParamsValue.text = string.Format("{0} {1}      {2} {3}", new object[]
		{
			MeasuringSystemManager.FishWeight(fishStat.MaxFish.Weight).ToString("N3"),
			MeasuringSystemManager.FishWeightSufix(),
			MeasuringSystemManager.FishLength(fishStat.MaxFish.Length).ToString("N3"),
			MeasuringSystemManager.FishLengthSufix()
		});
		this.IconLdbl.Image = this.Icon;
		this.IconLdbl.Load(string.Format("Textures/Inventory/{0}", fishStat.MaxFish.ThumbnailBID.ToString(CultureInfo.InvariantCulture)));
		this.PreviewButton.SetActive(true);
	}

	public void ShowPreview()
	{
		if (this._fishStat != null)
		{
			string text = ScriptLocalization.Get("TrophiesPondStatCaption") + " " + ((this._fishStat.MaxFishPond == 0) ? string.Empty : CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == this._fishStat.MaxFishPond).Name);
			text = string.Concat(new string[]
			{
				text,
				"\n",
				ScriptLocalization.Get("TrophyStatDateCaption"),
				" ",
				this._fishStat.MaxFishDate.ToLocalTime().ToShortDateString()
			});
			text = text + "\n" + ScriptLocalization.Get("TrophyStatWeghtCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishWeight(this._fishStat.MaxFish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
			text = text + "\n" + ScriptLocalization.Get("TrophyStatLengthCaption") + string.Format(" {0} {1}", MeasuringSystemManager.FishLength(this._fishStat.MaxFish.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
			ModelInfo modelInfo = new ModelInfo
			{
				Title = this._fishStat.MaxFish.Name,
				Info = text
			};
			base.GetComponent<LoadPreviewScene>().LoadScene(this._fishStat.MaxFish.Asset, modelInfo, null);
		}
	}

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Name;

	public Text ParamsValue;

	private FishStat _fishStat;

	public GameObject PreviewButton;
}
