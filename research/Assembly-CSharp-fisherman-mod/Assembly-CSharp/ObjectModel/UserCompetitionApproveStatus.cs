using System;

namespace ObjectModel
{
	public enum UserCompetitionApproveStatus
	{
		NoReview,
		InDevelopment,
		PublishedForReview,
		InReview = 4,
		Approved,
		Declined,
		Deleted
	}
}
