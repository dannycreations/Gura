using System;
using System.Reflection;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions
{
	internal class MethodCallSignature
	{
		public MethodCallSignature(Type returnType)
		{
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			this.ReturnType = returnType;
			this.hashCode = this.ComputeHashCode();
			this.Count = 0;
		}

		public MethodCallSignature(Type parameter1Type, string parameter1Name, Type returnType)
			: this(returnType)
		{
			if (parameter1Type == null)
			{
				throw new ArgumentNullException("parameter1Type");
			}
			if (parameter1Name == null)
			{
				throw new ArgumentNullException("parameter1Name");
			}
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			this.Parameter1Type = parameter1Type;
			this.Parameter1Name = parameter1Name;
			this.Count = 1;
		}

		public MethodCallSignature(Type parameter1Type, string parameter1Name, Type parameter2Type, string parameter2Name, Type returnType)
			: this(returnType)
		{
			if (parameter1Type == null)
			{
				throw new ArgumentNullException("parameter1Type");
			}
			if (parameter2Type == null)
			{
				throw new ArgumentNullException("parameter2Type");
			}
			if (parameter1Name == null)
			{
				throw new ArgumentNullException("parameter1Name");
			}
			if (parameter2Name == null)
			{
				throw new ArgumentNullException("parameter2Name");
			}
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			this.Parameter1Type = parameter1Type;
			this.Parameter2Type = parameter2Type;
			this.Parameter1Name = parameter1Name;
			this.Parameter2Name = parameter2Name;
			this.Count = 2;
		}

		public MethodCallSignature(Type parameter1Type, string parameter1Name, Type parameter2Type, string parameter2Name, Type parameter3Type, string parameter3Name, Type returnType)
			: this(returnType)
		{
			if (parameter1Type == null)
			{
				throw new ArgumentNullException("parameter1Type");
			}
			if (parameter2Type == null)
			{
				throw new ArgumentNullException("parameter2Type");
			}
			if (parameter3Type == null)
			{
				throw new ArgumentNullException("parameter3Type");
			}
			if (parameter1Name == null)
			{
				throw new ArgumentNullException("parameter1Name");
			}
			if (parameter2Name == null)
			{
				throw new ArgumentNullException("parameter2Name");
			}
			if (parameter3Name == null)
			{
				throw new ArgumentNullException("parameter3Name");
			}
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			this.Parameter1Type = parameter1Type;
			this.Parameter2Type = parameter2Type;
			this.Parameter3Type = parameter3Type;
			this.Parameter1Name = parameter1Name;
			this.Parameter2Name = parameter2Name;
			this.Parameter3Name = parameter3Name;
			this.Count = 3;
		}

		public MethodCallSignature(Type parameter1Type, string parameter1Name, Type parameter2Type, string parameter2Name, Type parameter3Type, string parameter3Name, Type parameter4Type, string parameter4Name, Type returnType)
			: this(returnType)
		{
			if (parameter1Type == null)
			{
				throw new ArgumentNullException("parameter1Type");
			}
			if (parameter2Type == null)
			{
				throw new ArgumentNullException("parameter2Type");
			}
			if (parameter3Type == null)
			{
				throw new ArgumentNullException("parameter3Type");
			}
			if (parameter4Type == null)
			{
				throw new ArgumentNullException("parameter4Type");
			}
			if (parameter1Name == null)
			{
				throw new ArgumentNullException("parameter1Name");
			}
			if (parameter2Name == null)
			{
				throw new ArgumentNullException("parameter2Name");
			}
			if (parameter3Name == null)
			{
				throw new ArgumentNullException("parameter3Name");
			}
			if (parameter4Name == null)
			{
				throw new ArgumentNullException("parameter4Name");
			}
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			this.Parameter1Type = parameter1Type;
			this.Parameter2Type = parameter2Type;
			this.Parameter3Type = parameter3Type;
			this.Parameter4Type = parameter4Type;
			this.Parameter1Name = parameter1Name;
			this.Parameter2Name = parameter2Name;
			this.Parameter3Name = parameter3Name;
			this.Parameter4Name = parameter4Name;
			this.Count = 4;
		}

		public MethodCallSignature(MethodInfo method, bool includeParameterNames = true)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			ParameterInfo[] parameters = method.GetParameters();
			switch (parameters.Length)
			{
			case 0:
				goto IL_105;
			case 1:
				goto IL_D4;
			case 2:
				goto IL_A3;
			case 3:
				break;
			case 4:
				this.Parameter4Name = ((!includeParameterNames) ? string.Empty : parameters[3].Name);
				this.Parameter4Type = parameters[3].ParameterType;
				break;
			default:
				throw new ArgumentException(Resources.EXCEPTION_UNBOUNDEXPR_INVALIDPARAMCOUNT, "method");
			}
			this.Parameter3Name = ((!includeParameterNames) ? string.Empty : parameters[2].Name);
			this.Parameter3Type = parameters[2].ParameterType;
			IL_A3:
			this.Parameter2Name = ((!includeParameterNames) ? string.Empty : parameters[1].Name);
			this.Parameter2Type = parameters[1].ParameterType;
			IL_D4:
			this.Parameter1Name = ((!includeParameterNames) ? string.Empty : parameters[0].Name);
			this.Parameter1Type = parameters[0].ParameterType;
			IL_105:
			this.ReturnType = method.ReturnType;
			this.Count = parameters.Length;
			this.hashCode = this.ComputeHashCode();
		}

		public override bool Equals(object obj)
		{
			MethodCallSignature methodCallSignature = obj as MethodCallSignature;
			return methodCallSignature != null && (object.ReferenceEquals(methodCallSignature, this) || (this.Parameter1Type == methodCallSignature.Parameter1Type && this.Parameter2Type == methodCallSignature.Parameter2Type && this.Parameter3Type == methodCallSignature.Parameter3Type && this.Parameter4Type == methodCallSignature.Parameter4Type && this.Parameter1Name == methodCallSignature.Parameter1Name && this.Parameter2Name == methodCallSignature.Parameter2Name && this.Parameter3Name == methodCallSignature.Parameter3Name && this.Parameter4Name == methodCallSignature.Parameter4Name && this.ReturnType == methodCallSignature.ReturnType));
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		private int ComputeHashCode()
		{
			return ((this.Parameter1Type == null) ? 0 : this.Parameter1Type.GetHashCode()) + ((this.Parameter2Type == null) ? 0 : this.Parameter2Type.GetHashCode()) + ((this.Parameter3Type == null) ? 0 : this.Parameter3Type.GetHashCode()) + ((this.Parameter4Type == null) ? 0 : this.Parameter4Type.GetHashCode()) + ((this.Parameter1Name == null) ? 0 : this.Parameter1Name.GetHashCode()) + ((this.Parameter2Name == null) ? 0 : this.Parameter2Name.GetHashCode()) + ((this.Parameter3Name == null) ? 0 : this.Parameter3Name.GetHashCode()) + ((this.Parameter4Name == null) ? 0 : this.Parameter4Name.GetHashCode()) + this.ReturnType.GetHashCode();
		}

		public override string ToString()
		{
			return string.Concat(new object[] { this.Parameter1Type, ", ", this.Parameter2Type, ", ", this.Parameter3Type, ", ", this.Parameter4Type, ", ", this.ReturnType });
		}

		private readonly int hashCode;

		public readonly Type Parameter1Type;

		public readonly Type Parameter2Type;

		public readonly Type Parameter3Type;

		public readonly Type Parameter4Type;

		public readonly string Parameter1Name;

		public readonly string Parameter2Name;

		public readonly string Parameter3Name;

		public readonly string Parameter4Name;

		public readonly Type ReturnType;

		public readonly int Count;
	}
}
