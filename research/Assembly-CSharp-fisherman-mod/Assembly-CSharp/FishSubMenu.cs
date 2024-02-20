using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishSubMenu : SubMenuFoldoutBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnScene3D = delegate(bool b)
	{
	};

	private void Start()
	{
		this._scene3D = base.GetComponent<LoadPreviewScene>();
		LoadPreviewScene.OnLoad += this.Scene3D_OnLoad;
	}

	private void OnDestroy()
	{
		LoadPreviewScene.OnLoad -= this.Scene3D_OnLoad;
	}

	public override void SetOpened(bool opened, Action callback = null)
	{
		this.Help3DIcon.SetShow(0);
		base.SetOpened(opened, callback);
		if (opened)
		{
			this.RequestData();
		}
	}

	private void RequestData()
	{
		if (!this.subscribed)
		{
			CacheLibrary.MapCache.OnFishes += this.OnGetFishes;
		}
		this.subscribed = true;
		CacheLibrary.MapCache.GetFishes((!(ShowPondInfo.Instance != null)) ? StaticUserData.CurrentPond.FishIds : ShowPondInfo.Instance.CurrentPond.FishIds);
	}

	private void OnEnable()
	{
		if (this.fishTypes == null)
		{
			this.fishTypes = new string[]
			{
				ScriptLocalization.Get("YoungType"),
				ScriptLocalization.Get("CommonType"),
				ScriptLocalization.Get("TrophyType"),
				ScriptLocalization.Get("UniqueType")
			};
		}
		if (base.Opened)
		{
			this.RequestData();
		}
	}

	private void OnDisable()
	{
		if (this.subscribed)
		{
			this.subscribed = false;
			CacheLibrary.MapCache.OnFishes -= this.OnGetFishes;
		}
		for (int i = 0; i < this._fishToggles.Count; i++)
		{
			this._fishToggles[i].gameObject.SetActive(false);
		}
	}

	private void OnGetFishes(object sender, GlobalMapFishCacheEventArgs e)
	{
		Pond currentPond = ((!(ShowPondInfo.Instance != null)) ? StaticUserData.CurrentPond : ShowPondInfo.Instance.CurrentPond);
		Func<Fish, bool> func = (Fish fish) => fish.IsActive && currentPond.FishIds.Contains(fish.FishId);
		IEnumerable<IGrouping<int, Fish>> enumerable = from f in e.Items.Where(func)
			orderby CacheLibrary.MapCache.GetFishCategory(f.CategoryId).Name
			group f by f.CategoryId;
		this.FillContent(enumerable);
	}

	public void FillContent(IEnumerable<IGrouping<int, Fish>> fishes)
	{
		this._fishDescription.text = string.Empty;
		int num = 0;
		foreach (IGrouping<int, Fish> grouping in fishes)
		{
			FishToggle fishToggle;
			if (this._fishToggles.Count <= num)
			{
				GameObject gameObject = GUITools.AddChild(this._fishParent.gameObject, this._fishPrefab.gameObject);
				gameObject.name = string.Format("FishToggle {0}", num++);
				fishToggle = gameObject.GetComponent<FishToggle>();
				fishToggle.Toggle.group = this._toggleGroup;
				this._fishToggles.Add(fishToggle);
			}
			else
			{
				fishToggle = this._fishToggles[num++];
				fishToggle.gameObject.SetActive(true);
			}
			Fish current = grouping.First<Fish>();
			string typesColored = this.FormTypesString(grouping, false);
			string typesWhite = this.FormTypesString(grouping, true);
			if (this._fishToggles[0] == fishToggle)
			{
				this.FillFishDetails(current, typesColored, typesWhite);
			}
			fishToggle.Init(current, typesColored);
			fishToggle.Toggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					this.FillFishDetails(current, typesColored, typesWhite);
					this.Help3DIcon.SetShow(1);
				}
				else
				{
					this.Help3DIcon.SetShow(0);
				}
			});
		}
		while (this._fishToggles.Count > num)
		{
			this._fishToggles[num++].gameObject.SetActive(false);
		}
	}

	public void Show3D()
	{
		if (this._opennedFish == null)
		{
			return;
		}
		FishCategory fishCategory = CacheLibrary.MapCache.GetFishCategory(this._opennedFish.CategoryId);
		string text = ((fishCategory != null) ? fishCategory.Name : string.Empty);
		ModelInfo modelInfo = new ModelInfo
		{
			Title = string.Format("<b>{0}</b>\n{1}", text.ToUpper(), this._opennedTypes),
			Info = string.Empty
		};
		this._scene3D.LoadScene(this._opennedFish.Asset, modelInfo, null);
	}

	private void Scene3D_OnLoad(bool isActive)
	{
		this.OnScene3D(isActive);
	}

	private string GetColor(Fish fish, bool allWhite = false)
	{
		this.sb.Length = 0;
		int num = ((!(ShowPondInfo.Instance != null)) ? StaticUserData.CurrentPond.PondId : ShowPondInfo.Instance.CurrentPond.PondId);
		this.sb.Append(num);
		if (fish.IsTrophy != null && fish.IsTrophy.Value)
		{
			this.sb.Append("t");
		}
		else if (fish.IsYoung != null && fish.IsYoung.Value)
		{
			this.sb.Append("y");
		}
		else if (fish.IsUnique != null && fish.IsUnique.Value)
		{
			this.sb.Append("u");
		}
		else
		{
			this.sb.Append("c");
		}
		this.sb.Append(fish.FishId);
		if (allWhite || (ObscuredPrefs.HasKey(this.sb.ToString()) && ObscuredPrefs.GetBool(this.sb.ToString())))
		{
			return "<color=\"#FFFFFFFF\">{0}</color>";
		}
		return "<color=\"#B9B9B9FF\">{0}</color>";
	}

	private string FormTypesString(IGrouping<int, Fish> fishGroup, bool allWhite)
	{
		if (this.fishTypes == null)
		{
			this.fishTypes = new string[]
			{
				ScriptLocalization.Get("YoungType"),
				ScriptLocalization.Get("CommonType"),
				ScriptLocalization.Get("TrophyType"),
				ScriptLocalization.Get("UniqueType")
			};
		}
		string text = string.Empty;
		string[] array = new string[]
		{
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty
		};
		foreach (Fish fish in fishGroup)
		{
			string color = this.GetColor(fish, allWhite);
			if (fish.IsTrophy != null && fish.IsTrophy.Value)
			{
				array[2] = string.Format(color, this.fishTypes[2]);
			}
			else if (fish.IsYoung != null && fish.IsYoung.Value)
			{
				array[0] = string.Format(color, this.fishTypes[0]);
			}
			else if (fish.IsUnique != null && fish.IsUnique.Value)
			{
				array[3] = string.Format(color, this.fishTypes[3]);
			}
			else
			{
				array[1] = string.Format(color, this.fishTypes[1]);
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != string.Empty)
			{
				text += ((!(text == string.Empty)) ? (", " + array[i]) : array[i]);
			}
		}
		return text;
	}

	private void FillFishDetails(Fish fish, string typesColored, string typesWhite)
	{
		this._opennedFish = fish;
		this._opennedTypes = typesWhite;
		FishCategory fishCategory = CacheLibrary.MapCache.GetFishCategory(fish.CategoryId);
		string text = ((fishCategory != null) ? fishCategory.Name : string.Empty);
		string text2 = string.Format("<size=33><b>{0}</b></size>\n{1} \n\n{2}", text.ToUpper(), typesColored, fish.Desc.Replace("<br>", "\n"));
		if (text2.EndsWith("<b>"))
		{
			text2 = text2.Substring(0, text2.Length - "<b>".Length);
		}
		this._fishDescription.text = text2;
	}

	public const string GreyedColor = "#B9B9B9FF";

	public const string GreyedMask = "<color=\"#B9B9B9FF\">{0}</color>";

	[SerializeField]
	private FishToggle _fishPrefab;

	[SerializeField]
	private Transform _fishParent;

	[SerializeField]
	private Text _fishDescription;

	[SerializeField]
	private ToggleGroup _toggleGroup;

	[SerializeField]
	private List<FishToggle> _fishToggles = new List<FishToggle>();

	[SerializeField]
	private GamePadMouseOnlyDisabler Help3DIcon;

	private string[] fishTypes;

	private Fish _opennedFish;

	private string _opennedTypes = string.Empty;

	private LoadPreviewScene _scene3D;

	private bool subscribed;

	private StringBuilder sb = new StringBuilder();
}
