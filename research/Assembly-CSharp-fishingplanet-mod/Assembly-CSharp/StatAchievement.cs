using System;

public class StatAchievement
{
	public int AchivementId { get; set; }

	public int? OrderId { get; set; }

	public string Name { get; set; }

	public string Desc { get; set; }

	public bool IsCompleted { get; set; }

	public StatAchievementStage CurrentStage { get; set; }

	public StatAchievementStage NextStage { get; set; }

	public int CurrentCount { get; set; }

	public override string ToString()
	{
		return string.Format("StatAchievement #{0} '{1}', IsCompleted: {2}", this.AchivementId, this.Name, this.IsCompleted);
	}
}
