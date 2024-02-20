using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class AchivementsInit : StatsInitBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnActive = delegate
	{
	};

	private void Awake()
	{
		this.Caption.text = ScriptLocalization.Get("AchivementsCaption").ToUpper();
	}

	public void Refresh(PlayerStats stats)
	{
		if (stats == null)
		{
			return;
		}
		int num = 0;
		this.list.Clear();
		this.l2.Clear();
		this.l3.Clear();
		for (int i = 0; i < stats.Achievements.Length; i++)
		{
			StatAchievement statAchievement = stats.Achievements[i];
			if (statAchievement.OrderId < 100)
			{
				this.list.Add(statAchievement);
			}
			else if (statAchievement.CurrentStage != null)
			{
				this.l2.Add(statAchievement);
			}
			else
			{
				this.l3.Add(statAchievement);
			}
			if (statAchievement.CurrentStage != null)
			{
				num++;
			}
		}
		this.list = this.list.OrderBy((StatAchievement p) => p.OrderId).ToList<StatAchievement>();
		this.list.AddRange(this.l2.OrderBy((StatAchievement p) => p.OrderId));
		this.list.AddRange(this.l3.OrderBy((StatAchievement p) => p.OrderId));
		this.AchivementsCountText.text = string.Format("({0})", num);
		this.Init<StatAchievement>(this.list, this.OnActive);
	}

	protected override void Add(int i)
	{
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.AchivPrefab);
		gameObject.name = string.Format("AchivItemPanel{0}", i);
		Image component = gameObject.GetComponent<Image>();
		if (!component.enabled)
		{
			component.enabled = true;
		}
		component.color = ((i % 2 != 1) ? Color.clear : this.ListLineColor);
		gameObject.GetComponent<AchivInit>().Refresh(this.list[i]);
	}

	public GameObject AchivPrefab;

	public Color ListLineColor;

	public Text AchivementsCountText;

	public Text Caption;

	public Scrollbar ScrollBar;

	public ScrollRect ScrollRect;

	private List<StatAchievement> list = new List<StatAchievement>();

	private List<StatAchievement> l2 = new List<StatAchievement>();

	private List<StatAchievement> l3 = new List<StatAchievement>();
}
