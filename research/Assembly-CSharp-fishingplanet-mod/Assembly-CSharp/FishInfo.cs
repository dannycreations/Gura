using System;

public class FishInfo
{
	public int FishId { get; set; }

	public string Name { get; set; }

	public int CategoryId { get; set; }

	public string Asset { get; set; }

	public float Weight { get; set; }

	public float Length { get; set; }

	public bool? IsYoung { get; set; }

	public bool? IsTrophy { get; set; }

	public bool? IsUnique { get; set; }

	public int ThumbnailBID { get; set; }
}
