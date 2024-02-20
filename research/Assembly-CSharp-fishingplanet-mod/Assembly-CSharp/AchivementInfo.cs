using System;
using ObjectModel;
using ObjectModel.Common;

public class AchivementInfo
{
	public float ShowDelay { get; set; }

	public ItemReward[] ItemRewards { get; set; }

	public ProductReward[] ProductReward { get; set; }

	public LicenseRef[] LicenseReward { get; set; }

	public string AchivementName;

	public string AchivementDesc;

	public string AchivementStageName;

	public int AchivementStageCount;

	public int ImageId;

	public int Experience;

	public Amount Amount1;

	public Amount Amount2;

	public string ExternalAchievementId;
}
