using System;

public class StatAchievementStage
{
	public int StageId { get; set; }

	public int OrderId { get; set; }

	public string Name { get; set; }

	public string Desc { get; set; }

	public int? SignBID { get; set; }

	public string Unit { get; set; }

	public int? Experience { get; set; }

	public double? Money1 { get; set; }

	public string Currency1 { get; set; }

	public double? Money2 { get; set; }

	public string Currency2 { get; set; }

	public int? SkillId { get; set; }

	public int[] ItemRewards { get; set; }

	public int[] ProductRewards { get; set; }

	public int Count { get; set; }
}
