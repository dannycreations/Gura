using System;

namespace GameDevWare.Dynamic.Expressions
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExpressionKnownTypeAttribute : Attribute
	{
		public ExpressionKnownTypeAttribute(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			this.Type = type;
		}

		public Type Type { get; private set; }
	}
}
