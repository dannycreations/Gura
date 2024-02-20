using System;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	internal class TokenAttribute : Attribute
	{
		public TokenAttribute(string value)
		{
			this.Value = value;
		}

		public string Value { get; set; }
	}
}
