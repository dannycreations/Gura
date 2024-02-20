using System;

namespace Assets
{
	public static class PatternString
	{
		public static string TransformPattern<InstanceT>(this string pattern, InstanceT instance)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			return new PatternString<InstanceT>(pattern).Tranform(instance);
		}
	}
}
