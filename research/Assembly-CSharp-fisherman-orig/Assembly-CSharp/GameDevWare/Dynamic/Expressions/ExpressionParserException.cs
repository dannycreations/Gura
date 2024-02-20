using System;

namespace GameDevWare.Dynamic.Expressions
{
	public sealed class ExpressionParserException : Exception, ILineInfo
	{
		internal ExpressionParserException()
		{
		}

		internal ExpressionParserException(string message, int lineNumber = 0, int columnNumber = 0, int tokenLength = 0)
			: base(message)
		{
			this.LineNumber = lineNumber;
			this.ColumnNumber = columnNumber;
			this.TokenLength = tokenLength;
		}

		internal ExpressionParserException(string message, Exception innerException, int lineNumber = 0, int columnNumber = 0, int tokenLength = 0)
			: base(message, innerException)
		{
			this.LineNumber = lineNumber;
			this.ColumnNumber = columnNumber;
			this.TokenLength = tokenLength;
		}

		internal ExpressionParserException(string message, ILineInfo lineInfo)
			: base(message)
		{
			if (lineInfo == null)
			{
				return;
			}
			this.LineNumber = lineInfo.GetLineNumber();
			this.ColumnNumber = lineInfo.GetColumnNumber();
			this.TokenLength = lineInfo.GetTokenLength();
		}

		internal ExpressionParserException(string message, Exception innerException, ILineInfo lineInfo)
			: base(message, innerException)
		{
			if (lineInfo == null)
			{
				return;
			}
			this.LineNumber = lineInfo.GetLineNumber();
			this.ColumnNumber = lineInfo.GetColumnNumber();
			this.TokenLength = lineInfo.GetTokenLength();
		}

		public int LineNumber { get; set; }

		public int ColumnNumber { get; set; }

		public int TokenLength { get; set; }

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
			if (this.TokenLength != 0)
			{
				return string.Format("[{0},{1}+{2}]{3}", new object[]
				{
					this.LineNumber.ToString(),
					this.ColumnNumber.ToString(),
					this.TokenLength.ToString(),
					base.ToString()
				});
			}
			return base.ToString();
		}
	}
}
