using System;

namespace GameDevWare.Dynamic.Expressions
{
	internal struct ExpressionPosition : ILineInfo, IEquatable<ExpressionPosition>, IEquatable<ILineInfo>
	{
		public ExpressionPosition(int lineNumber, int columnNumber, int tokenLength)
		{
			this.LineNumber = lineNumber;
			this.ColumnNumber = columnNumber;
			this.TokenLength = tokenLength;
		}

		public static ExpressionPosition Parse(string positionString)
		{
			if (positionString == null)
			{
				throw new ArgumentNullException("positionString");
			}
			int num = positionString.IndexOf(':');
			int num2 = positionString.IndexOf('+');
			if (num < 0 || num2 < 0)
			{
				throw new FormatException();
			}
			int num3 = int.Parse(positionString.Substring(0, num), Constants.DefaultFormatProvider);
			int num4 = int.Parse(positionString.Substring(num + 1, num2 - num - 1), Constants.DefaultFormatProvider);
			int num5 = int.Parse(positionString.Substring(num2 + 1), Constants.DefaultFormatProvider);
			return new ExpressionPosition(num3, num4, num5);
		}

		public override bool Equals(object obj)
		{
			if (obj is ExpressionPosition)
			{
				return this.Equals((ExpressionPosition)obj);
			}
			return obj is ILineInfo && this.Equals((ILineInfo)obj);
		}

		public bool Equals(ExpressionPosition other)
		{
			return this.LineNumber == other.LineNumber && this.ColumnNumber == other.ColumnNumber && this.TokenLength == other.TokenLength;
		}

		public bool Equals(ILineInfo other)
		{
			return other != null && (this.LineNumber == other.GetLineNumber() && this.ColumnNumber == other.GetColumnNumber()) && this.TokenLength == other.GetTokenLength();
		}

		public override int GetHashCode()
		{
			int num = this.LineNumber;
			num = (num * 397) ^ this.ColumnNumber;
			return (num * 397) ^ this.TokenLength;
		}

		int ILineInfo.GetLineNumber()
		{
			return this.LineNumber;
		}

		int ILineInfo.GetColumnNumber()
		{
			return this.ColumnNumber;
		}

		int ILineInfo.GetTokenLength()
		{
			return this.TokenLength;
		}

		public override string ToString()
		{
			return string.Format(Constants.DefaultFormatProvider, "{0}:{1}+{2}", new object[]
			{
				this.LineNumber.ToString(),
				this.ColumnNumber.ToString(),
				this.TokenLength.ToString()
			});
		}

		public readonly int LineNumber;

		public readonly int ColumnNumber;

		public readonly int TokenLength;
	}
}
