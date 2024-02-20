using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public abstract class BaseCondition
	{
		[JsonProperty]
		public int oid { get; set; }

		public bool ShowProgress { get; set; }

		public int LastProgress { get; set; }

		public float LastProgressFloat { get; protected set; }

		protected virtual List<IHint> GenerateHints()
		{
			return new List<IHint>();
		}

		protected abstract string[] MonitoringDependencies { get; }

		public abstract bool Check(MissionsContext context);

		public virtual string GetProgress(MissionsContext context)
		{
			return null;
		}

		public virtual void ResetCachedAutoHints()
		{
		}

		public virtual object Clone()
		{
			return (BaseCondition)base.MemberwiseClone();
		}
	}
}
