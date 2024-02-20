using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions
{
	internal static class Executor
	{
		static Executor()
		{
			if (typeof(Executor).Name == string.Empty)
			{
				Executor.Expression(null, null, null);
				Executor.Conditional(null, null, null);
				Executor.Constant(null, null, null);
				Executor.Invocation(null, null, null);
				Executor.Lambda(null, null, null);
				Executor.ListInit(null, null, null);
				Executor.MemberAccess(null, null, null);
				Executor.MemberInit(null, null, null);
				Executor.MemberAssignments(null, null, null);
				Executor.MemberListBindings(null, null, null);
				Executor.MemberMemberBindings(null, null, null);
				Executor.Call(null, null, null);
				Executor.New(null, null, null);
				Executor.NewArray(null, null, null);
				Executor.Parameter(null, null, null);
				Executor.TypeIs(null, null, null);
				Executor.TypeAs(null, null, null);
				Executor.Convert(null, null, null);
				Executor.Unary(null, null, null);
				Executor.Binary(null, null, null);
				Executor.CreateUnaryOperationFn(null);
				Executor.CreateBinaryOperationFn(null);
				Executor.WrapUnaryOperation(null, null);
				Executor.WrapUnaryOperation(null);
				Executor.WrapBinaryOperation(null, null);
				Executor.WrapBinaryOperation(null);
			}
		}

		public static Func<ResultT> Prepare<ResultT>(Expression body, ReadOnlyCollection<ParameterExpression> parameters = null)
		{
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}
			Executor.ConstantsCollector constantsCollector = new Executor.ConstantsCollector();
			constantsCollector.Visit(body);
			ConstantExpression[] array = constantsCollector.Constants.ToArray();
			ParameterExpression[] emptyParameters = Constants.EmptyParameters;
			Executor.ExecuteFunc compiledFn = Executor.Expression(body, array, emptyParameters);
			object[] constants = Array.ConvertAll<ConstantExpression, object>(array, (ConstantExpression c) => c.Value);
			return delegate
			{
				object[] array2 = new object[3];
				Executor.Closure closure = new Executor.Closure(constants, array2);
				ResultT resultT = (ResultT)((object)compiledFn(closure));
				Array.Clear(array2, 0, array2.Length);
				return resultT;
			};
		}

		public static Func<Arg1T, ResultT> Prepare<Arg1T, ResultT>(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
		{
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Executor.ConstantsCollector constantsCollector = new Executor.ConstantsCollector();
			constantsCollector.Visit(body);
			ConstantExpression[] array = constantsCollector.Constants.ToArray();
			ParameterExpression[] array2 = parameters.ToArray<ParameterExpression>();
			Executor.ExecuteFunc compiledFn = Executor.Expression(body, array, array2);
			object[] constants = new object[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				constants[i] = array[i].Value;
			}
			return delegate(Arg1T arg1)
			{
				object[] array3 = new object[] { null, null, null, arg1 };
				Executor.Closure closure = new Executor.Closure(constants, array3);
				ResultT resultT = (ResultT)((object)compiledFn(closure));
				Array.Clear(array3, 0, array3.Length);
				return resultT;
			};
		}

		public static Func<Arg1T, Arg2T, ResultT> Prepare<Arg1T, Arg2T, ResultT>(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
		{
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Executor.ConstantsCollector constantsCollector = new Executor.ConstantsCollector();
			constantsCollector.Visit(body);
			ConstantExpression[] array = constantsCollector.Constants.ToArray();
			ParameterExpression[] array2 = parameters.ToArray<ParameterExpression>();
			Executor.ExecuteFunc compiledFn = Executor.Expression(body, array, array2);
			object[] constants = Array.ConvertAll<ConstantExpression, object>(array, (ConstantExpression c) => c.Value);
			return delegate(Arg1T arg1, Arg2T arg2)
			{
				object[] array3 = new object[] { null, null, null, arg1, arg2 };
				Executor.Closure closure = new Executor.Closure(constants, array3);
				ResultT resultT = (ResultT)((object)compiledFn(closure));
				Array.Clear(array3, 0, array3.Length);
				return resultT;
			};
		}

		public static Func<Arg1T, Arg2T, Arg3T, ResultT> Prepare<Arg1T, Arg2T, Arg3T, ResultT>(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
		{
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Executor.ConstantsCollector constantsCollector = new Executor.ConstantsCollector();
			constantsCollector.Visit(body);
			ConstantExpression[] array = constantsCollector.Constants.ToArray();
			ParameterExpression[] array2 = parameters.ToArray<ParameterExpression>();
			Executor.ExecuteFunc compiledFn = Executor.Expression(body, array, array2);
			object[] constants = Array.ConvertAll<ConstantExpression, object>(array, (ConstantExpression c) => c.Value);
			return delegate(Arg1T arg1, Arg2T arg2, Arg3T arg3)
			{
				object[] array3 = new object[] { null, null, null, arg1, arg2, arg3 };
				Executor.Closure closure = new Executor.Closure(constants, array3);
				ResultT resultT = (ResultT)((object)compiledFn(closure));
				Array.Clear(array3, 0, array3.Length);
				return resultT;
			};
		}

		public static Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT> Prepare<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
		{
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Executor.ConstantsCollector constantsCollector = new Executor.ConstantsCollector();
			constantsCollector.Visit(body);
			ConstantExpression[] array = constantsCollector.Constants.ToArray();
			ParameterExpression[] array2 = parameters.ToArray<ParameterExpression>();
			Executor.ExecuteFunc compiledFn = Executor.Expression(body, array, array2);
			object[] constants = Array.ConvertAll<ConstantExpression, object>(array, (ConstantExpression c) => c.Value);
			return delegate(Arg1T arg1, Arg2T arg2, Arg3T arg3, Arg4T arg4)
			{
				object[] array3 = new object[] { null, null, null, arg1, arg2, arg3, arg4 };
				Executor.Closure closure = new Executor.Closure(constants, array3);
				ResultT resultT = (ResultT)((object)compiledFn(closure));
				Array.Clear(array3, 0, array3.Length);
				return resultT;
			};
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>()
		{
			Executor.MethodCall.RegisterInstanceMethod<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, Arg2T, ResultT>()
		{
			Executor.MethodCall.RegisterInstanceMethod<InstanceT, Arg1T, Arg2T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, ResultT>()
		{
			Executor.MethodCall.RegisterInstanceMethod<InstanceT, Arg1T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, ResultT>()
		{
			Executor.MethodCall.RegisterInstanceMethod<InstanceT, ResultT>();
		}

		private static Executor.ExecuteFunc Expression(Expression expression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			if (expression == null)
			{
				return (Executor.Closure closure) => null;
			}
			switch (expression.NodeType)
			{
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Coalesce:
			case ExpressionType.Divide:
			case ExpressionType.Equal:
			case ExpressionType.ExclusiveOr:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.LeftShift:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.Modulo:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.NotEqual:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.Power:
			case ExpressionType.RightShift:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
				return Executor.Binary((BinaryExpression)expression, constExprs, paramExprs);
			case ExpressionType.ArrayLength:
			case ExpressionType.Negate:
			case ExpressionType.UnaryPlus:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
				return Executor.Unary((UnaryExpression)expression, constExprs, paramExprs);
			case ExpressionType.ArrayIndex:
				return Executor.ArrayIndex(expression, constExprs, paramExprs);
			case ExpressionType.Call:
				return Executor.Call((MethodCallExpression)expression, constExprs, paramExprs);
			case ExpressionType.Conditional:
				return Executor.Conditional((ConditionalExpression)expression, constExprs, paramExprs);
			case ExpressionType.Constant:
				return Executor.Constant((ConstantExpression)expression, constExprs, paramExprs);
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
				return Executor.Convert((UnaryExpression)expression, constExprs, paramExprs);
			case ExpressionType.Invoke:
				return Executor.Invocation((InvocationExpression)expression, constExprs, paramExprs);
			case ExpressionType.Lambda:
				return Executor.Lambda((LambdaExpression)expression, constExprs, paramExprs);
			case ExpressionType.ListInit:
				return Executor.ListInit((ListInitExpression)expression, constExprs, paramExprs);
			case ExpressionType.MemberAccess:
				return Executor.MemberAccess((MemberExpression)expression, constExprs, paramExprs);
			case ExpressionType.MemberInit:
				return Executor.MemberInit((MemberInitExpression)expression, constExprs, paramExprs);
			case ExpressionType.New:
				return Executor.New((NewExpression)expression, constExprs, paramExprs);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return Executor.NewArray((NewArrayExpression)expression, constExprs, paramExprs);
			case ExpressionType.Parameter:
				return Executor.Parameter((ParameterExpression)expression, constExprs, paramExprs);
			case ExpressionType.Quote:
				return (Executor.Closure closure) => ((UnaryExpression)expression).Operand;
			case ExpressionType.TypeAs:
				return Executor.TypeAs((UnaryExpression)expression, constExprs, paramExprs);
			case ExpressionType.TypeIs:
				return Executor.TypeIs((TypeBinaryExpression)expression, constExprs, paramExprs);
			default:
				throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_UNKNOWNEXPRTYPE, expression.Type));
			}
		}

		private static Executor.ExecuteFunc Conditional(ConditionalExpression conditionalExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc trueFn = Executor.Expression(conditionalExpression.IfTrue, constExprs, paramExprs);
			Executor.ExecuteFunc falseFn = Executor.Expression(conditionalExpression.IfFalse, constExprs, paramExprs);
			Executor.ExecuteFunc testFn = Executor.Expression(conditionalExpression.Test, constExprs, paramExprs);
			return (Executor.Closure closure) => (!closure.Unbox<bool>(testFn(closure))) ? falseFn(closure) : trueFn(closure);
		}

		private static Executor.ExecuteFunc Constant(ConstantExpression constantExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			return (Executor.Closure closure) => closure.Constants[Array.IndexOf<ConstantExpression>(constExprs, constantExpression)];
		}

		private static Executor.ExecuteFunc Invocation(InvocationExpression invocationExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc target = Executor.Expression(invocationExpression.Expression, constExprs, paramExprs);
			Executor.ExecuteFunc[] argsFns = invocationExpression.Arguments.Select((Expression e) => Executor.Expression(e, constExprs, paramExprs)).ToArray<Executor.ExecuteFunc>();
			return delegate(Executor.Closure closure)
			{
				Delegate @delegate = (Delegate)target(closure);
				object[] array = new object[argsFns.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = argsFns[i](closure);
				}
				return @delegate.DynamicInvoke(array);
			};
		}

		private static Executor.ExecuteFunc Lambda(LambdaExpression lambdaExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			if (!lambdaExpression.Type.IsGenericType)
			{
				throw new NotSupportedException(Resources.EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED);
			}
			Type genericTypeDefinition = lambdaExpression.Type.GetGenericTypeDefinition();
			if (genericTypeDefinition != typeof(Func<>) && genericTypeDefinition != typeof(Func<, >) && genericTypeDefinition != typeof(Func<, , >) && genericTypeDefinition != typeof(Func<, , , >) && genericTypeDefinition != typeof(Func<, , , , >))
			{
				throw new NotSupportedException(Resources.EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED);
			}
			Type[] funcArguments = lambdaExpression.Type.GetGenericArguments();
			MethodInfo methodInfo = typeof(Executor).GetMethods(BindingFlags.Static | BindingFlags.Public).Single((MethodInfo m) => m.Name == "Prepare" && m.GetGenericArguments().Length == funcArguments.Length);
			MethodInfo prepareMethod = methodInfo.MakeGenericMethod(funcArguments);
			return delegate(Executor.Closure closure)
			{
				Expression expression = lambdaExpression.Body;
				ReadOnlyCollection<ParameterExpression> parameters = lambdaExpression.Parameters;
				if (paramExprs.Length > 0)
				{
					Dictionary<Expression, Expression> dictionary = new Dictionary<Expression, Expression>(paramExprs.Length);
					foreach (ParameterExpression parameterExpression in paramExprs)
					{
						object obj = closure.Locals[3 + Array.IndexOf<ParameterExpression>(paramExprs, parameterExpression)];
						dictionary.Add(parameterExpression, global::System.Linq.Expressions.Expression.Constant(obj, parameterExpression.Type));
					}
					expression = ExpressionSubstitutor.Visit(expression, dictionary);
				}
				return prepareMethod.Invoke(null, new object[] { expression, parameters });
			};
		}

		private static Executor.ExecuteFunc ListInit(ListInitExpression listInitExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc newFn = Executor.New(listInitExpression.NewExpression, constExprs, paramExprs);
			<>__AnonType1<MethodInfo, Executor.ExecuteFunc[]>[] listInits = (from elemInit in listInitExpression.Initializers
				let argsFns = elemInit.Arguments.Select((Expression e) => Executor.Expression(e, constExprs, paramExprs)).ToArray<Executor.ExecuteFunc>()
				select new
				{
					addMethod = elemInit.AddMethod,
					argsFns = argsFns
				}).ToArray();
			return delegate(Executor.Closure closure)
			{
				object obj = newFn(closure);
				if (listInits.Length == 0)
				{
					return obj;
				}
				var listInits2 = listInits;
				for (int i = 0; i < listInits2.Length; i++)
				{
					var <>__AnonType = listInits2[i];
					MethodInfo addMethod = <>__AnonType.addMethod;
					object[] array = new object[<>__AnonType.argsFns.Length];
					for (int j = 0; j < <>__AnonType.argsFns.Length; j++)
					{
						array[j] = <>__AnonType.argsFns[j](closure);
					}
					addMethod.Invoke(obj, array);
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc MemberAccess(MemberExpression memberExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc valueFn = Executor.Expression(memberExpression.Expression, constExprs, paramExprs);
			return delegate(Executor.Closure closure)
			{
				object obj = valueFn(closure);
				MemberInfo member = memberExpression.Member;
				if (member is FieldInfo)
				{
					return ((FieldInfo)member).GetValue(obj);
				}
				return ((PropertyInfo)member).GetValue(obj, null);
			};
		}

		private static Executor.ExecuteFunc MemberInit(MemberInitExpression memberInitExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc newFn = Executor.New(memberInitExpression.NewExpression, constExprs, paramExprs);
			Executor.ExecuteFunc memberAssignments = Executor.MemberAssignments(memberInitExpression.Bindings, constExprs, paramExprs);
			Executor.ExecuteFunc listBindings = Executor.MemberListBindings(memberInitExpression.Bindings, constExprs, paramExprs);
			Executor.ExecuteFunc memberMemberBindings = Executor.MemberMemberBindings(memberInitExpression.Bindings, constExprs, paramExprs);
			return delegate(Executor.Closure closure)
			{
				object obj = newFn(closure);
				closure.Locals[0] = obj;
				memberAssignments(closure);
				closure.Locals[0] = obj;
				listBindings(closure);
				closure.Locals[0] = obj;
				memberMemberBindings(closure);
				closure.Locals[0] = null;
				return obj;
			};
		}

		private static Executor.ExecuteFunc MemberAssignments(IEnumerable<MemberBinding> bindings, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			<>__AnonType3<MemberInfo, Executor.ExecuteFunc>[] assignFns = (from bind in bindings
				let assign = bind as MemberAssignment
				where assign != null
				select new
				{
					member = bind.Member,
					valueFn = Executor.Expression(assign.Expression, constExprs, paramExprs)
				}).ToArray();
			return delegate(Executor.Closure closure)
			{
				object obj = closure.Locals[0];
				if (assignFns.Length == 0)
				{
					return obj;
				}
				var assignFns2 = assignFns;
				for (int i = 0; i < assignFns2.Length; i++)
				{
					var <>__AnonType = assignFns2[i];
					MemberInfo member = <>__AnonType.member;
					Executor.ExecuteFunc valueFn = <>__AnonType.valueFn;
					if (member is FieldInfo)
					{
						((FieldInfo)member).SetValue(obj, valueFn(closure));
					}
					else
					{
						((PropertyInfo)member).SetValue(obj, valueFn(closure), null);
					}
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc MemberListBindings(IEnumerable<MemberBinding> bindings, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			ILookup<MemberInfo, <>__AnonType7<MemberInfo, MethodInfo, Executor.ExecuteFunc[]>> listBindGroups = (from bind in bindings
				let listBind = bind as MemberListBinding
				where listBind != null
				from elemInit in listBind.Initializers
				let argsFns = elemInit.Arguments.Select((Expression e) => Executor.Expression(e, constExprs, paramExprs)).ToArray<Executor.ExecuteFunc>()
				select new
				{
					member = bind.Member,
					addMethod = elemInit.AddMethod,
					argsFns = argsFns
				}).ToLookup(m => m.member);
			return delegate(Executor.Closure closure)
			{
				object obj = closure.Locals[0];
				if (listBindGroups.Count == 0)
				{
					return obj;
				}
				foreach (var grouping in listBindGroups)
				{
					MemberInfo key = grouping.Key;
					object obj2;
					if (key is FieldInfo)
					{
						obj2 = ((FieldInfo)key).GetValue(obj);
					}
					else
					{
						obj2 = ((PropertyInfo)key).GetValue(obj, null);
					}
					if (obj2 == null)
					{
						throw new NullReferenceException();
					}
					foreach (var <>__AnonType in grouping)
					{
						MethodInfo addMethod = <>__AnonType.addMethod;
						object[] array = new object[<>__AnonType.argsFns.Length];
						for (int i = 0; i < <>__AnonType.argsFns.Length; i++)
						{
							array[i] = <>__AnonType.argsFns[i](closure);
						}
						addMethod.Invoke(obj2, array);
					}
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc MemberMemberBindings(IEnumerable<MemberBinding> bindings, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			ILookup<MemberInfo, <>__AnonType12<MemberInfo, Executor.ExecuteFunc, Executor.ExecuteFunc, Executor.ExecuteFunc>> bindGroups = (from bind in bindings
				let memberBinding = bind as MemberMemberBinding
				where memberBinding != null
				let memberAssignments = Executor.MemberAssignments(memberBinding.Bindings, constExprs, paramExprs)
				let listBindings = Executor.MemberListBindings(memberBinding.Bindings, constExprs, paramExprs)
				let memberMemberBindings = Executor.MemberMemberBindings(memberBinding.Bindings, constExprs, paramExprs)
				select new
				{
					member = memberBinding.Member,
					memberAssignments = memberAssignments,
					listBindings = listBindings,
					memberMemberBindings = memberMemberBindings
				}).ToLookup(m => m.member);
			return delegate(Executor.Closure closure)
			{
				object obj = closure.Locals[0];
				if (bindGroups.Count == 0)
				{
					return obj;
				}
				foreach (var grouping in bindGroups)
				{
					MemberInfo key = grouping.Key;
					object obj2;
					if (key is FieldInfo)
					{
						obj2 = ((FieldInfo)key).GetValue(obj);
					}
					else
					{
						obj2 = ((PropertyInfo)key).GetValue(obj, null);
					}
					if (obj2 == null)
					{
						throw new NullReferenceException();
					}
					foreach (var <>__AnonType in grouping)
					{
						closure.Locals[0] = obj2;
						<>__AnonType.memberAssignments(closure);
						closure.Locals[0] = obj2;
						<>__AnonType.listBindings(closure);
						closure.Locals[0] = obj2;
						<>__AnonType.memberMemberBindings(closure);
					}
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc Call(MethodCallExpression methodCallExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc targetFn = Executor.Expression(methodCallExpression.Object, constExprs, paramExprs);
			Executor.ExecuteFunc[] argsFns = methodCallExpression.Arguments.Select((Expression e) => Executor.Expression(e, constExprs, paramExprs)).ToArray<Executor.ExecuteFunc>();
			Executor.InvokeOperation invokeFn = Executor.MethodCall.TryCreate(methodCallExpression.Method);
			if (invokeFn != null)
			{
				return (Executor.Closure closure) => invokeFn(closure, argsFns);
			}
			return delegate(Executor.Closure closure)
			{
				object obj = targetFn(closure);
				object[] array = new object[argsFns.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = argsFns[i](closure);
				}
				return methodCallExpression.Method.Invoke(obj, array);
			};
		}

		private static Executor.ExecuteFunc New(NewExpression newExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc[] valuesFns = newExpression.Arguments.Select((Expression e) => Executor.Expression(e, constExprs, paramExprs)).ToArray<Executor.ExecuteFunc>();
			return delegate(Executor.Closure closure)
			{
				object[] array = new object[valuesFns.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = valuesFns[i](closure);
				}
				object[] array2 = array.Take(newExpression.Constructor.GetParameters().Length).ToArray<object>();
				bool flag = Executor.IsNullable(newExpression.Type);
				object obj = ((!flag) ? Activator.CreateInstance(newExpression.Type, array2) : null);
				if (newExpression.Members != null)
				{
					for (int j = 0; j < newExpression.Members.Count; j++)
					{
						MemberInfo memberInfo = newExpression.Members[j];
						if (memberInfo is FieldInfo)
						{
							((FieldInfo)memberInfo).SetValue(obj, array[array2.Length + j]);
						}
						else
						{
							((PropertyInfo)memberInfo).SetValue(obj, array[array2.Length + j], null);
						}
					}
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc NewArray(NewArrayExpression newArrayExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.<NewArray>c__AnonStorey12 <NewArray>c__AnonStorey = new Executor.<NewArray>c__AnonStorey12();
			<NewArray>c__AnonStorey.constExprs = constExprs;
			<NewArray>c__AnonStorey.paramExprs = paramExprs;
			<NewArray>c__AnonStorey.newArrayExpression = newArrayExpression;
			if (<NewArray>c__AnonStorey.newArrayExpression.NodeType == ExpressionType.NewArrayBounds)
			{
				Executor.ExecuteFunc[] lengthFns = <NewArray>c__AnonStorey.newArrayExpression.Expressions.Select((Expression e) => Executor.Expression(e, <NewArray>c__AnonStorey.constExprs, <NewArray>c__AnonStorey.paramExprs)).ToArray<Executor.ExecuteFunc>();
				return delegate(Executor.Closure closure)
				{
					int[] array = new int[lengthFns.Length];
					for (int i = 0; i < lengthFns.Length; i++)
					{
						array[i] = closure.Unbox<int>(lengthFns[i](closure));
					}
					return Array.CreateInstance(<NewArray>c__AnonStorey.newArrayExpression.Type.GetElementType(), array);
				};
			}
			Executor.ExecuteFunc[] valuesFns = <NewArray>c__AnonStorey.newArrayExpression.Expressions.Select((Expression e) => Executor.Expression(e, <NewArray>c__AnonStorey.constExprs, <NewArray>c__AnonStorey.paramExprs)).ToArray<Executor.ExecuteFunc>();
			return delegate(Executor.Closure closure)
			{
				Array array2 = Array.CreateInstance(<NewArray>c__AnonStorey.newArrayExpression.Type.GetElementType(), valuesFns.Length);
				for (int j = 0; j < valuesFns.Length; j++)
				{
					array2.SetValue(valuesFns[j](closure), j);
				}
				return array2;
			};
		}

		private static Executor.ExecuteFunc Parameter(ParameterExpression parameterExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			return (Executor.Closure closure) => closure.Locals[3 + Array.IndexOf<ParameterExpression>(paramExprs, parameterExpression)];
		}

		private static Executor.ExecuteFunc TypeIs(TypeBinaryExpression typeBinaryExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc valueFn = Executor.Expression(typeBinaryExpression.Expression, constExprs, paramExprs);
			return delegate(Executor.Closure closure)
			{
				object obj = valueFn(closure);
				if (obj == null)
				{
					return false;
				}
				return typeBinaryExpression.TypeOperand.IsAssignableFrom(obj.GetType());
			};
		}

		private static Executor.ExecuteFunc TypeAs(UnaryExpression typeAsExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			if (typeAsExpression.Type.IsValueType)
			{
				return Executor.Convert(typeAsExpression, constExprs, paramExprs);
			}
			Executor.ExecuteFunc valueFn = Executor.Expression(typeAsExpression.Operand, constExprs, paramExprs);
			return delegate(Executor.Closure closure)
			{
				object obj = valueFn(closure);
				if (obj == null)
				{
					return null;
				}
				if (!typeAsExpression.Type.IsAssignableFrom(obj.GetType()))
				{
					return null;
				}
				return obj;
			};
		}

		private static Executor.ExecuteFunc Convert(UnaryExpression convertExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc valueFn = Executor.Expression(convertExpression.Operand, constExprs, paramExprs);
			Executor.UnaryOperation convertOperator = Executor.WrapUnaryOperation(convertExpression.Method) ?? Executor.WrapUnaryOperation(convertExpression.Type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo m) => (string.Equals(m.Name, "op_Explicit", StringComparison.Ordinal) || string.Equals(m.Name, "op_Implicit", StringComparison.Ordinal)) && m.ReturnType == convertExpression.Type && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == convertExpression.Operand.Type));
			Type toType = Nullable.GetUnderlyingType(convertExpression.Type) ?? convertExpression.Type;
			bool isToTypeNullable = Executor.IsNullable(convertExpression.Type);
			Type fromType = Nullable.GetUnderlyingType(convertExpression.Operand.Type) ?? convertExpression.Operand.Type;
			bool isFromTypeNullable = Executor.IsNullable(convertExpression.Operand);
			return delegate(Executor.Closure closure)
			{
				object obj = closure.Unbox<object>(valueFn(closure));
				if (obj == null && (!convertExpression.Type.IsValueType || isToTypeNullable))
				{
					return null;
				}
				ExpressionType expressionType = convertExpression.NodeType;
				if (expressionType != ExpressionType.Convert)
				{
					expressionType = ExpressionType.ConvertChecked;
				}
				if ((fromType == typeof(object) || fromType == typeof(ValueType) || fromType.IsInterface) && toType.IsValueType)
				{
					if (obj == null)
					{
						throw new NullReferenceException("Attempt to unbox a null value.");
					}
					if (obj.GetType() == toType)
					{
						return obj;
					}
					throw new InvalidCastException();
				}
				else
				{
					if (fromType.IsValueType && (toType == typeof(object) || toType == typeof(ValueType) || toType.IsInterface))
					{
						return (!toType.IsInstanceOfType(obj)) ? null : obj;
					}
					if (toType.IsEnum && (fromType == typeof(byte) || fromType == typeof(sbyte) || fromType == typeof(short) || fromType == typeof(ushort) || fromType == typeof(int) || fromType == typeof(uint) || fromType == typeof(long) || fromType == typeof(ulong)))
					{
						if (obj == null)
						{
							throw new NullReferenceException("Attempt to unbox a null value.");
						}
						obj = Executor.Intrinsic.Convert(closure, obj, Enum.GetUnderlyingType(toType), convertExpression.NodeType, null);
						return Enum.ToObject(toType, closure.Unbox<object>(obj));
					}
					else
					{
						if (!fromType.IsEnum || (toType != typeof(byte) && toType != typeof(sbyte) && toType != typeof(short) && toType != typeof(ushort) && toType != typeof(int) && toType != typeof(uint) && toType != typeof(long) && toType != typeof(ulong)))
						{
							if (toType.IsValueType && isFromTypeNullable)
							{
								if (obj == null)
								{
									throw new NullReferenceException("Attempt to unbox a null value.");
								}
								obj = Executor.Intrinsic.Convert(closure, obj, toType, convertExpression.NodeType, null);
							}
							else if (toType.IsInstanceOfType(obj))
							{
								return obj;
							}
							return Executor.Intrinsic.Convert(closure, obj, toType, expressionType, convertOperator);
						}
						if (obj == null)
						{
							throw new NullReferenceException("Attempt to unbox a null value.");
						}
						obj = global::System.Convert.ChangeType(obj, Enum.GetUnderlyingType(fromType));
						return Executor.Intrinsic.Convert(closure, obj, toType, convertExpression.NodeType, null);
					}
				}
			};
		}

		private static Executor.ExecuteFunc Unary(UnaryExpression unaryExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc valueFn = Executor.Expression(unaryExpression.Operand, constExprs, paramExprs);
			Executor.UnaryOperation opUnaryNegation = Executor.WrapUnaryOperation(unaryExpression.Method) ?? Executor.WrapUnaryOperation(unaryExpression.Operand.Type, "op_UnaryNegation");
			Executor.UnaryOperation opUnaryPlus = Executor.WrapUnaryOperation(unaryExpression.Method) ?? Executor.WrapUnaryOperation(unaryExpression.Operand.Type, "op_UnaryPlus");
			Executor.UnaryOperation opOnesComplement = Executor.WrapUnaryOperation(unaryExpression.Method) ?? Executor.WrapUnaryOperation(unaryExpression.Operand.Type, "op_OnesComplement");
			bool isNullable = Executor.IsNullable(unaryExpression.Operand);
			return delegate(Executor.Closure closure)
			{
				object obj = valueFn(closure);
				if (isNullable && obj == null && unaryExpression.NodeType != ExpressionType.ArrayLength)
				{
					return null;
				}
				ExpressionType nodeType = unaryExpression.NodeType;
				switch (nodeType)
				{
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
					return Executor.Intrinsic.UnaryOperation(closure, obj, unaryExpression.NodeType, opUnaryNegation);
				case ExpressionType.UnaryPlus:
					return Executor.Intrinsic.UnaryOperation(closure, obj, unaryExpression.NodeType, opUnaryPlus);
				default:
					if (nodeType != ExpressionType.ArrayLength)
					{
						throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_UNKNOWNUNARYEXPRTYPE, unaryExpression.Type));
					}
					return closure.Unbox<Array>(obj).Length;
				case ExpressionType.Not:
					return Executor.Intrinsic.UnaryOperation(closure, obj, unaryExpression.NodeType, opOnesComplement);
				}
			};
		}

		private static Executor.ExecuteFunc ArrayIndex(Expression expression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			BinaryExpression binaryExpression = expression as BinaryExpression;
			Executor.ExecuteFunc leftFn = ((binaryExpression == null) ? null : Executor.Expression(binaryExpression.Left, constExprs, paramExprs));
			Executor.ExecuteFunc rightFn = ((binaryExpression == null) ? null : Executor.Expression(binaryExpression.Right, constExprs, paramExprs));
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			if (binaryExpression != null)
			{
				return delegate(Executor.Closure closure)
				{
					object obj = leftFn(closure);
					object obj2 = rightFn(closure);
					return (!closure.Is<int[]>(obj2)) ? closure.Unbox<Array>(obj).GetValue(closure.Unbox<int>(obj2)) : closure.Unbox<Array>(obj).GetValue(closure.Unbox<int[]>(obj2));
				};
			}
			return Executor.Call(methodCallExpression, constExprs, paramExprs);
		}

		private static Executor.ExecuteFunc Binary(BinaryExpression binaryExpression, ConstantExpression[] constExprs, ParameterExpression[] paramExprs)
		{
			Executor.ExecuteFunc leftFn = Executor.Expression(binaryExpression.Left, constExprs, paramExprs);
			Executor.ExecuteFunc rightFn = Executor.Expression(binaryExpression.Right, constExprs, paramExprs);
			Executor.BinaryOperation opAddition = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Addition");
			Executor.BinaryOperation opBitwiseAnd = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_BitwiseAnd");
			Executor.BinaryOperation opDivision = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Division");
			Executor.BinaryOperation opEquality = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Equality");
			Executor.BinaryOperation opInequality = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Inequality");
			Executor.BinaryOperation opExclusiveOr = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_ExclusiveOr");
			Executor.BinaryOperation opGreaterThan = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_GreaterThan");
			Executor.BinaryOperation opGreaterThanOrEqual = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_GreaterThanOrEqual");
			Executor.BinaryOperation opLessThan = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_LessThan");
			Executor.BinaryOperation opLessThanOrEqual = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_LessThanOrEqual");
			Executor.BinaryOperation opModulus = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Modulus");
			Executor.BinaryOperation opMultiply = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Multiply");
			Executor.BinaryOperation opBitwiseOr = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_BitwiseOr");
			Executor.BinaryOperation opSubtraction = Executor.WrapBinaryOperation(binaryExpression.Method) ?? Executor.WrapBinaryOperation(binaryExpression.Left.Type, "op_Subtraction");
			bool isNullable = Executor.IsNullable(binaryExpression.Left) || Executor.IsNullable(binaryExpression.Right);
			return delegate(Executor.Closure closure)
			{
				object obj = leftFn(closure);
				object obj2 = rightFn(closure);
				if (!isNullable || (obj != null && obj2 != null) || binaryExpression.NodeType == ExpressionType.Coalesce || binaryExpression.NodeType == ExpressionType.ArrayIndex)
				{
					ExpressionType nodeType = binaryExpression.NodeType;
					switch (nodeType)
					{
					case ExpressionType.Divide:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opDivision);
					case ExpressionType.Equal:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opEquality);
					case ExpressionType.ExclusiveOr:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opExclusiveOr);
					case ExpressionType.GreaterThan:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opGreaterThan);
					case ExpressionType.GreaterThanOrEqual:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opGreaterThanOrEqual);
					default:
						switch (nodeType)
						{
						case ExpressionType.Add:
						case ExpressionType.AddChecked:
							return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opAddition);
						case ExpressionType.And:
							return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opBitwiseAnd);
						case ExpressionType.AndAlso:
							goto IL_2D8;
						case ExpressionType.Coalesce:
							return obj ?? obj2;
						}
						throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_UNKNOWNBINARYEXPRTYPE, binaryExpression.NodeType));
					case ExpressionType.LeftShift:
					case ExpressionType.OrElse:
					case ExpressionType.Power:
					case ExpressionType.RightShift:
						break;
					case ExpressionType.LessThan:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opLessThan);
					case ExpressionType.LessThanOrEqual:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opLessThanOrEqual);
					case ExpressionType.Modulo:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opModulus);
					case ExpressionType.Multiply:
					case ExpressionType.MultiplyChecked:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opMultiply);
					case ExpressionType.NotEqual:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opInequality);
					case ExpressionType.Or:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opBitwiseOr);
					case ExpressionType.Subtract:
					case ExpressionType.SubtractChecked:
						return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, opSubtraction);
					}
					IL_2D8:
					return Executor.Intrinsic.BinaryOperation(closure, obj, obj2, binaryExpression.NodeType, null);
				}
				ExpressionType nodeType2 = binaryExpression.NodeType;
				switch (nodeType2)
				{
				case ExpressionType.Equal:
					return (!object.ReferenceEquals(obj, obj2)) ? Executor.FalseObject : Executor.TrueObject;
				default:
					if (nodeType2 != ExpressionType.NotEqual)
					{
						if (nodeType2 != ExpressionType.Or)
						{
							if (nodeType2 == ExpressionType.And)
							{
								if (object.Equals(obj, Executor.FalseObject) || object.Equals(obj2, Executor.FalseObject))
								{
									return Executor.FalseObject;
								}
							}
						}
						else if (object.Equals(obj, Executor.TrueObject) || object.Equals(obj2, Executor.TrueObject))
						{
							return Executor.TrueObject;
						}
						return null;
					}
					return (!object.ReferenceEquals(obj, obj2)) ? Executor.TrueObject : Executor.FalseObject;
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
					return Executor.FalseObject;
				}
			};
		}

		private static bool IsNullable(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			ConstantExpression constantExpression = expression as ConstantExpression;
			return (constantExpression != null && constantExpression.Type == typeof(object) && constantExpression.Value == null) || Executor.IsNullable(expression.Type);
		}

		private static bool IsNullable(Type type)
		{
			if (type == null)
			{
				throw new ArgumentException("type");
			}
			return Nullable.GetUnderlyingType(type) != null;
		}

		private static Executor.UnaryOperation CreateUnaryOperationFn(MethodInfo method)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			return (Executor.UnaryOperation)Delegate.CreateDelegate(typeof(Executor.UnaryOperation), method, true);
		}

		private static Executor.BinaryOperation CreateBinaryOperationFn(MethodInfo method)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			return (Executor.BinaryOperation)Delegate.CreateDelegate(typeof(Executor.BinaryOperation), method, true);
		}

		private static Executor.UnaryOperation WrapUnaryOperation(Type type, string methodName)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (methodName == null)
			{
				throw new ArgumentNullException("methodName");
			}
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
			return Executor.WrapUnaryOperation(method);
		}

		private static Executor.UnaryOperation WrapUnaryOperation(MethodInfo method)
		{
			Executor.<WrapUnaryOperation>c__AnonStorey1C <WrapUnaryOperation>c__AnonStorey1C = new Executor.<WrapUnaryOperation>c__AnonStorey1C();
			<WrapUnaryOperation>c__AnonStorey1C.method = method;
			if (<WrapUnaryOperation>c__AnonStorey1C.method == null)
			{
				return null;
			}
			<WrapUnaryOperation>c__AnonStorey1C.invoker = Executor.MethodCall.TryCreate(<WrapUnaryOperation>c__AnonStorey1C.method);
			if (<WrapUnaryOperation>c__AnonStorey1C.invoker != null)
			{
				Executor.<WrapUnaryOperation>c__AnonStorey1D <WrapUnaryOperation>c__AnonStorey1D = new Executor.<WrapUnaryOperation>c__AnonStorey1D();
				<WrapUnaryOperation>c__AnonStorey1D.<>f__ref$28 = <WrapUnaryOperation>c__AnonStorey1C;
				Executor.<WrapUnaryOperation>c__AnonStorey1D <WrapUnaryOperation>c__AnonStorey1D2 = <WrapUnaryOperation>c__AnonStorey1D;
				Executor.ExecuteFunc[] array = new Executor.ExecuteFunc[1];
				array[0] = (Executor.Closure closure) => closure.Locals[0];
				<WrapUnaryOperation>c__AnonStorey1D2.argFns = array;
				return delegate(Executor.Closure closure, object operand)
				{
					closure.Locals[0] = operand;
					object obj = <WrapUnaryOperation>c__AnonStorey1D.<>f__ref$28.invoker(closure, <WrapUnaryOperation>c__AnonStorey1D.argFns);
					closure.Locals[0] = null;
					return obj;
				};
			}
			return (Executor.Closure closure, object operand) => <WrapUnaryOperation>c__AnonStorey1C.method.Invoke(null, new object[] { operand });
		}

		private static Executor.BinaryOperation WrapBinaryOperation(Type type, string methodName)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (methodName == null)
			{
				throw new ArgumentNullException("methodName");
			}
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				return null;
			}
			return Executor.WrapBinaryOperation(method);
		}

		private static Executor.BinaryOperation WrapBinaryOperation(MethodInfo method)
		{
			Executor.<WrapBinaryOperation>c__AnonStorey1E <WrapBinaryOperation>c__AnonStorey1E = new Executor.<WrapBinaryOperation>c__AnonStorey1E();
			<WrapBinaryOperation>c__AnonStorey1E.method = method;
			if (<WrapBinaryOperation>c__AnonStorey1E.method == null)
			{
				return null;
			}
			<WrapBinaryOperation>c__AnonStorey1E.invoker = Executor.MethodCall.TryCreate(<WrapBinaryOperation>c__AnonStorey1E.method);
			if (<WrapBinaryOperation>c__AnonStorey1E.invoker != null)
			{
				Executor.<WrapBinaryOperation>c__AnonStorey1F <WrapBinaryOperation>c__AnonStorey1F = new Executor.<WrapBinaryOperation>c__AnonStorey1F();
				<WrapBinaryOperation>c__AnonStorey1F.<>f__ref$30 = <WrapBinaryOperation>c__AnonStorey1E;
				Executor.<WrapBinaryOperation>c__AnonStorey1F <WrapBinaryOperation>c__AnonStorey1F2 = <WrapBinaryOperation>c__AnonStorey1F;
				Executor.ExecuteFunc[] array = new Executor.ExecuteFunc[2];
				array[0] = (Executor.Closure closure) => closure.Locals[0];
				array[1] = (Executor.Closure closure) => closure.Locals[1];
				<WrapBinaryOperation>c__AnonStorey1F2.argFns = array;
				return delegate(Executor.Closure closure, object left, object right)
				{
					closure.Locals[0] = left;
					closure.Locals[1] = right;
					object obj = <WrapBinaryOperation>c__AnonStorey1F.<>f__ref$30.invoker(closure, <WrapBinaryOperation>c__AnonStorey1F.argFns);
					closure.Locals[0] = null;
					closure.Locals[1] = null;
					return obj;
				};
			}
			return (Executor.Closure closure, object left, object right) => <WrapBinaryOperation>c__AnonStorey1E.method.Invoke(null, new object[] { left, right });
		}

		private static readonly object TrueObject = true;

		private static readonly object FalseObject = false;

		private const int LOCAL_OPERAND1 = 0;

		private const int LOCAL_OPERAND2 = 1;

		private const int LOCAL_SLOT1 = 2;

		private const int LOCAL_FIRST_PARAMETER = 3;

		private sealed class Closure
		{
			public Closure(object[] constants, object[] locals)
			{
				if (constants == null)
				{
					throw new ArgumentNullException("constants");
				}
				if (locals == null)
				{
					throw new ArgumentNullException("locals");
				}
				this.Constants = constants;
				this.Locals = locals;
			}

			public object Box<T>(T value)
			{
				return value;
			}

			public T Unbox<T>(object boxed)
			{
				if (boxed is T)
				{
					return (T)((object)boxed);
				}
				return (T)((object)global::System.Convert.ChangeType(boxed, typeof(T)));
			}

			public bool Is<T>(object boxed)
			{
				return boxed is T;
			}

			public readonly object[] Constants;

			public readonly object[] Locals;
		}

		private sealed class ConstantsCollector : ExpressionVisitor
		{
			protected override Expression VisitConstant(ConstantExpression constantExpression)
			{
				this.Constants.Add(constantExpression);
				return constantExpression;
			}

			public readonly List<ConstantExpression> Constants = new List<ConstantExpression>();
		}

		private delegate object ExecuteFunc(Executor.Closure closure);

		private delegate object BinaryOperation(Executor.Closure closure, object left, object right);

		private delegate object UnaryOperation(Executor.Closure closure, object operand);

		private static class Intrinsic
		{
			static Intrinsic()
			{
				if (typeof(Executor.Intrinsic).Name == string.Empty)
				{
					Executor.op_Boolean.Not(null, null);
					Executor.op_Byte.Negate(null, null);
					Executor.op_SByte.Negate(null, null);
					Executor.op_Int16.Negate(null, null);
					Executor.op_UInt16.Negate(null, null);
					Executor.op_Int32.Negate(null, null);
					Executor.op_UInt32.Negate(null, null);
					Executor.op_Int64.Negate(null, null);
					Executor.op_UInt64.UnaryPlus(null, null);
					Executor.op_Single.Negate(null, null);
					Executor.op_Double.Negate(null, null);
					Executor.op_Decimal.Negate(null, null);
					Executor.op_Object.Equal(null, null, null);
					Executor.Intrinsic.BinaryOperation(null, null, null, ExpressionType.Add, null);
					Executor.Intrinsic.UnaryOperation(null, null, ExpressionType.Add, null);
					Executor.Intrinsic.Convert(null, null, null, ExpressionType.Add, null);
				}
				string[] names = Enum.GetNames(typeof(ExpressionType));
				Array.Sort<string>(names, StringComparer.Ordinal);
				Executor.Intrinsic.Operations = new Dictionary<Type, Dictionary<int, Delegate>>();
				foreach (Type type in typeof(Executor).GetNestedTypes(BindingFlags.NonPublic))
				{
					if (type.Name.StartsWith("op_", StringComparison.Ordinal))
					{
						Type type2 = Type.GetType("System." + type.Name.Substring(3), false);
						if (type2 != null)
						{
							Dictionary<int, Delegate> dictionary = null;
							if (!Executor.Intrinsic.Operations.TryGetValue(type2, out dictionary))
							{
								dictionary = (Executor.Intrinsic.Operations[type2] = new Dictionary<int, Delegate>());
							}
							foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
							{
								if (Array.BinarySearch<string>(names, methodInfo.Name) >= 0)
								{
									ExpressionType expressionType = (ExpressionType)Enum.Parse(typeof(ExpressionType), methodInfo.Name);
									ParameterInfo[] parameters = methodInfo.GetParameters();
									Delegate @delegate = ((parameters.Length != 3) ? ((parameters.Length != 2) ? null : Executor.CreateUnaryOperationFn(methodInfo)) : Executor.CreateBinaryOperationFn(methodInfo));
									dictionary[(int)expressionType] = @delegate;
								}
							}
						}
					}
				}
				Executor.Intrinsic.Convertions = new Dictionary<Type, Dictionary<Type, Delegate>>();
				foreach (Type type3 in typeof(Executor).GetNestedTypes(BindingFlags.NonPublic))
				{
					if (type3.Name.StartsWith("op_", StringComparison.Ordinal))
					{
						Type type4 = Type.GetType("System." + type3.Name.Substring(3), false);
						if (type4 != null)
						{
							Dictionary<Type, Delegate> dictionary2 = null;
							if (!Executor.Intrinsic.Convertions.TryGetValue(type4, out dictionary2))
							{
								dictionary2 = (Executor.Intrinsic.Convertions[type4] = new Dictionary<Type, Delegate>());
							}
							foreach (MethodInfo methodInfo2 in type3.GetMethods(BindingFlags.Static | BindingFlags.Public))
							{
								if (methodInfo2.Name.StartsWith("To", StringComparison.Ordinal))
								{
									Delegate delegate2 = Executor.CreateBinaryOperationFn(methodInfo2);
									Type type5 = Type.GetType("System." + methodInfo2.Name.Substring(2), false);
									if (type5 != null)
									{
										dictionary2[type5] = delegate2;
									}
								}
							}
						}
					}
				}
			}

			public static object BinaryOperation(Executor.Closure closure, object left, object right, ExpressionType binaryOperationType, Executor.BinaryOperation userDefinedBinaryOperation)
			{
				if (closure == null)
				{
					throw new ArgumentNullException("closure");
				}
				Type type = ((left == null) ? ((right == null) ? typeof(object) : right.GetType()) : left.GetType());
				Dictionary<int, Delegate> dictionary = null;
				Delegate @delegate = null;
				if (Executor.Intrinsic.Operations.TryGetValue(type, out dictionary) && dictionary.TryGetValue((int)binaryOperationType, out @delegate))
				{
					return ((Executor.BinaryOperation)@delegate)(closure, left, right);
				}
				if (binaryOperationType == ExpressionType.Equal)
				{
					userDefinedBinaryOperation = (Executor.BinaryOperation)Executor.Intrinsic.Operations[typeof(object)][13];
				}
				else if (binaryOperationType == ExpressionType.NotEqual)
				{
					userDefinedBinaryOperation = (Executor.BinaryOperation)Executor.Intrinsic.Operations[typeof(object)][35];
				}
				if (userDefinedBinaryOperation == null)
				{
					throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_NOBINARYOPONTYPE, binaryOperationType, type));
				}
				return userDefinedBinaryOperation(closure, left, right);
			}

			public static object UnaryOperation(Executor.Closure closure, object operand, ExpressionType unaryOperationType, Executor.UnaryOperation userDefinedUnaryOperation)
			{
				if (closure == null)
				{
					throw new ArgumentNullException("closure");
				}
				Type type = ((operand == null) ? typeof(object) : operand.GetType());
				Dictionary<int, Delegate> dictionary = null;
				Delegate @delegate = null;
				if (Executor.Intrinsic.Operations.TryGetValue(type, out dictionary) && dictionary.TryGetValue((int)unaryOperationType, out @delegate))
				{
					return ((Executor.UnaryOperation)@delegate)(closure, operand);
				}
				if (userDefinedUnaryOperation == null)
				{
					throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_NOUNARYOPONTYPE, unaryOperationType, type));
				}
				return userDefinedUnaryOperation(closure, operand);
			}

			public static object Convert(Executor.Closure closure, object value, Type toType, ExpressionType convertType, Executor.UnaryOperation userDefinedConvertOperation)
			{
				if (closure == null)
				{
					throw new ArgumentNullException("closure");
				}
				if (toType == null)
				{
					throw new ArgumentNullException("toType");
				}
				Type type = ((value == null) ? typeof(object) : value.GetType());
				Dictionary<Type, Delegate> dictionary = null;
				Delegate @delegate = null;
				if (Executor.Intrinsic.Convertions.TryGetValue(type, out dictionary) && dictionary.TryGetValue(toType, out @delegate))
				{
					return ((Executor.BinaryOperation)@delegate)(closure, value, (convertType != ExpressionType.Convert) ? bool.TrueString : bool.FalseString);
				}
				if (userDefinedConvertOperation == null)
				{
					throw new InvalidOperationException(string.Format(Resources.EXCEPTION_COMPIL_NOCONVERTIONBETWEENTYPES, type, toType));
				}
				return userDefinedConvertOperation(closure, value);
			}

			private static readonly Dictionary<Type, Dictionary<int, Delegate>> Operations;

			private static readonly Dictionary<Type, Dictionary<Type, Delegate>> Convertions;
		}

		private static class op_Object
		{
			static op_Object()
			{
				if (typeof(Executor.op_Object).Name == string.Empty)
				{
					Executor.op_Object.Default(null);
					Executor.op_Object.Equal(null, null, null);
					Executor.op_Object.NotEqual(null, null, null);
					Executor.op_Object.ToObject(null, null, null);
					Executor.op_Object.ToBoolean(null, null, null);
					Executor.op_Object.ToSByte(null, null, null);
					Executor.op_Object.ToByte(null, null, null);
					Executor.op_Object.ToInt16(null, null, null);
					Executor.op_Object.ToUInt16(null, null, null);
					Executor.op_Object.ToInt32(null, null, null);
					Executor.op_Object.ToUInt32(null, null, null);
					Executor.op_Object.ToInt64(null, null, null);
					Executor.op_Object.ToUInt64(null, null, null);
					Executor.op_Object.ToSingle(null, null, null);
					Executor.op_Object.ToDouble(null, null, null);
					Executor.op_Object.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<object>(null);
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(object.Equals(closure.Unbox<object>(left), closure.Unbox<object>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(!object.Equals(closure.Unbox<object>(left), closure.Unbox<object>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<object>(left);
			}

			public static object ToBoolean(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left));
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<sbyte>(closure.Unbox<sbyte>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<short>(closure.Unbox<short>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<int>(closure.Unbox<int>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<long>(closure.Unbox<long>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<float>(closure.Unbox<float>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<double>(closure.Unbox<double>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left));
			}
		}

		private static class op_Boolean
		{
			static op_Boolean()
			{
				if (typeof(Executor.op_Boolean).Name == string.Empty)
				{
					Executor.op_Boolean.Default(null);
					Executor.op_Boolean.Not(null, false);
					Executor.op_Boolean.Equal(null, null, null);
					Executor.op_Boolean.NotEqual(null, null, null);
					Executor.op_Boolean.ToObject(null, null, null);
					Executor.op_Boolean.ToBoolean(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<bool>(false);
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<bool>(!closure.Unbox<bool>(operand));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(object.Equals(closure.Unbox<bool>(left), closure.Unbox<bool>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(!object.Equals(closure.Unbox<bool>(left), closure.Unbox<bool>(right)));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left) | closure.Unbox<bool>(right));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left) & closure.Unbox<bool>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left) ^ closure.Unbox<bool>(right));
			}

			public static object AndAlso(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left) && closure.Unbox<bool>(right));
			}

			public static object OrElse(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left) || closure.Unbox<bool>(right));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToBoolean(Executor.Closure closure, object left, object _)
			{
				return closure.Box<bool>(closure.Unbox<bool>(left));
			}
		}

		private static class op_Byte
		{
			static op_Byte()
			{
				if (typeof(Executor.op_Byte).Name == string.Empty)
				{
					Executor.op_Byte.Default(null);
					Executor.op_Byte.Negate(null, 0);
					Executor.op_Byte.NegateChecked(null, null);
					Executor.op_Byte.UnaryPlus(null, null);
					Executor.op_Byte.Not(null, null);
					Executor.op_Byte.Add(null, null, null);
					Executor.op_Byte.AddChecked(null, null, null);
					Executor.op_Byte.And(null, null, null);
					Executor.op_Byte.Divide(null, null, null);
					Executor.op_Byte.Equal(null, null, null);
					Executor.op_Byte.ExclusiveOr(null, null, null);
					Executor.op_Byte.GreaterThan(null, null, null);
					Executor.op_Byte.GreaterThanOrEqual(null, null, null);
					Executor.op_Byte.LeftShift(null, null, null);
					Executor.op_Byte.Power(null, null, null);
					Executor.op_Byte.RightShift(null, null, null);
					Executor.op_Byte.LessThan(null, null, null);
					Executor.op_Byte.LessThanOrEqual(null, null, null);
					Executor.op_Byte.Modulo(null, null, null);
					Executor.op_Byte.Multiply(null, null, null);
					Executor.op_Byte.MultiplyChecked(null, null, null);
					Executor.op_Byte.NotEqual(null, null, null);
					Executor.op_Byte.Or(null, null, null);
					Executor.op_Byte.Subtract(null, null, null);
					Executor.op_Byte.SubtractChecked(null, null, null);
					Executor.op_Byte.ToObject(null, null, null);
					Executor.op_Byte.ToSByte(null, null, null);
					Executor.op_Byte.ToByte(null, null, null);
					Executor.op_Byte.ToInt16(null, null, null);
					Executor.op_Byte.ToUInt16(null, null, null);
					Executor.op_Byte.ToInt32(null, null, null);
					Executor.op_Byte.ToUInt32(null, null, null);
					Executor.op_Byte.ToInt64(null, null, null);
					Executor.op_Byte.ToUInt64(null, null, null);
					Executor.op_Byte.ToSingle(null, null, null);
					Executor.op_Byte.ToDouble(null, null, null);
					Executor.op_Byte.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<byte>(0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<byte>(-closure.Unbox<byte>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<byte>(checked(0 - closure.Unbox<byte>(operand)));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<byte>(closure.Unbox<byte>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<byte>(~closure.Unbox<byte>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) + closure.Unbox<byte>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(checked(closure.Unbox<byte>(left) + closure.Unbox<byte>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) & closure.Unbox<byte>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) / closure.Unbox<byte>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) == closure.Unbox<byte>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) ^ closure.Unbox<byte>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) > closure.Unbox<byte>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) >= closure.Unbox<byte>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>((byte)(closure.Unbox<byte>(left) << closure.Unbox<int>(right)));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>((byte)Math.Pow((double)closure.Unbox<byte>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>((byte)(closure.Unbox<byte>(left) >> closure.Unbox<int>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) < closure.Unbox<byte>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) <= closure.Unbox<byte>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) % closure.Unbox<byte>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) * closure.Unbox<byte>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(checked(closure.Unbox<byte>(left) * closure.Unbox<byte>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<byte>(left) != closure.Unbox<byte>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) | closure.Unbox<byte>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(closure.Unbox<byte>(left) - closure.Unbox<byte>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<byte>(checked(closure.Unbox<byte>(left) - closure.Unbox<byte>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<byte>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<byte>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(closure.Unbox<byte>(left));
				}
				return closure.Box<byte>(closure.Unbox<byte>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>((short)closure.Unbox<byte>(left));
				}
				return closure.Box<short>((short)closure.Unbox<byte>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>((ushort)closure.Unbox<byte>(left));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<byte>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>((int)closure.Unbox<byte>(left));
				}
				return closure.Box<int>((int)closure.Unbox<byte>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>((uint)closure.Unbox<byte>(left));
				}
				return closure.Box<uint>((uint)closure.Unbox<byte>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<byte>(left));
				}
				return closure.Box<long>((long)closure.Unbox<byte>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>((ulong)closure.Unbox<byte>(left));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<byte>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<byte>(left));
				}
				return closure.Box<float>((float)closure.Unbox<byte>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<byte>(left));
				}
				return closure.Box<double>((double)closure.Unbox<byte>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<byte>(left));
				}
				return closure.Box<decimal>(closure.Unbox<byte>(left));
			}
		}

		private static class op_SByte
		{
			static op_SByte()
			{
				if (typeof(Executor.op_SByte).Name == string.Empty)
				{
					Executor.op_SByte.Default(null);
					Executor.op_SByte.Negate(null, 0);
					Executor.op_SByte.NegateChecked(null, null);
					Executor.op_SByte.UnaryPlus(null, null);
					Executor.op_SByte.Not(null, null);
					Executor.op_SByte.Add(null, null, null);
					Executor.op_SByte.AddChecked(null, null, null);
					Executor.op_SByte.And(null, null, null);
					Executor.op_SByte.Divide(null, null, null);
					Executor.op_SByte.Equal(null, null, null);
					Executor.op_SByte.ExclusiveOr(null, null, null);
					Executor.op_SByte.GreaterThan(null, null, null);
					Executor.op_SByte.GreaterThanOrEqual(null, null, null);
					Executor.op_SByte.LeftShift(null, null, null);
					Executor.op_SByte.Power(null, null, null);
					Executor.op_SByte.RightShift(null, null, null);
					Executor.op_SByte.LessThan(null, null, null);
					Executor.op_SByte.LessThanOrEqual(null, null, null);
					Executor.op_SByte.Modulo(null, null, null);
					Executor.op_SByte.Multiply(null, null, null);
					Executor.op_SByte.MultiplyChecked(null, null, null);
					Executor.op_SByte.NotEqual(null, null, null);
					Executor.op_SByte.Or(null, null, null);
					Executor.op_SByte.Subtract(null, null, null);
					Executor.op_SByte.SubtractChecked(null, null, null);
					Executor.op_SByte.ToObject(null, null, null);
					Executor.op_SByte.ToSByte(null, null, null);
					Executor.op_SByte.ToByte(null, null, null);
					Executor.op_SByte.ToInt16(null, null, null);
					Executor.op_SByte.ToUInt16(null, null, null);
					Executor.op_SByte.ToInt32(null, null, null);
					Executor.op_SByte.ToUInt32(null, null, null);
					Executor.op_SByte.ToInt64(null, null, null);
					Executor.op_SByte.ToUInt64(null, null, null);
					Executor.op_SByte.ToSingle(null, null, null);
					Executor.op_SByte.ToDouble(null, null, null);
					Executor.op_SByte.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<sbyte>(0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<sbyte>((sbyte)(-(sbyte)((int)closure.Unbox<sbyte>(operand))));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<sbyte>((sbyte)(checked(0 - unchecked((int)closure.Unbox<sbyte>(operand)))));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(operand)));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<sbyte>((sbyte)(~(sbyte)((int)closure.Unbox<sbyte>(operand))));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) + (int)closure.Unbox<sbyte>(right)));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)(checked(unchecked((int)closure.Unbox<sbyte>(left)) + unchecked((int)closure.Unbox<sbyte>(right)))));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) & (int)closure.Unbox<sbyte>(right)));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) / (int)closure.Unbox<sbyte>(right)));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) == (int)closure.Unbox<sbyte>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) ^ (int)closure.Unbox<sbyte>(right)));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) > (int)closure.Unbox<sbyte>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) >= (int)closure.Unbox<sbyte>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) << closure.Unbox<int>(right)));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)Math.Pow((double)closure.Unbox<sbyte>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) >> closure.Unbox<int>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) < (int)closure.Unbox<sbyte>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) <= (int)closure.Unbox<sbyte>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) % (int)closure.Unbox<sbyte>(right)));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) * (int)closure.Unbox<sbyte>(right)));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)(checked(unchecked((int)closure.Unbox<sbyte>(left)) * unchecked((int)closure.Unbox<sbyte>(right)))));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>((int)closure.Unbox<sbyte>(left) != (int)closure.Unbox<sbyte>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) | (int)closure.Unbox<sbyte>(right)));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)((int)closure.Unbox<sbyte>(left) - (int)closure.Unbox<sbyte>(right)));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<sbyte>((sbyte)(checked(unchecked((int)closure.Unbox<sbyte>(left)) - unchecked((int)closure.Unbox<sbyte>(right)))));
			}

			public static object ToObject(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(closure.Unbox<sbyte>(left));
				}
				return closure.Box<sbyte>(closure.Unbox<sbyte>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<sbyte>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<sbyte>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>((short)closure.Unbox<sbyte>(left));
				}
				return closure.Box<short>((short)closure.Unbox<sbyte>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<sbyte>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<sbyte>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>((int)closure.Unbox<sbyte>(left));
				}
				return closure.Box<int>((int)closure.Unbox<sbyte>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<sbyte>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<sbyte>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<sbyte>(left));
				}
				return closure.Box<long>((long)closure.Unbox<sbyte>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<sbyte>(left)));
				}
				return closure.Box<ulong>((ulong)((long)closure.Unbox<sbyte>(left)));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<sbyte>(left));
				}
				return closure.Box<float>((float)closure.Unbox<sbyte>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<sbyte>(left));
				}
				return closure.Box<double>((double)closure.Unbox<sbyte>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<sbyte>(left));
				}
				return closure.Box<decimal>(closure.Unbox<sbyte>(left));
			}
		}

		private static class op_Int16
		{
			static op_Int16()
			{
				if (typeof(Executor.op_Int16).Name == string.Empty)
				{
					Executor.op_Int16.Default(null);
					Executor.op_Int16.Negate(null, 0);
					Executor.op_Int16.NegateChecked(null, null);
					Executor.op_Int16.UnaryPlus(null, null);
					Executor.op_Int16.Not(null, null);
					Executor.op_Int16.Add(null, null, null);
					Executor.op_Int16.AddChecked(null, null, null);
					Executor.op_Int16.And(null, null, null);
					Executor.op_Int16.Divide(null, null, null);
					Executor.op_Int16.Equal(null, null, null);
					Executor.op_Int16.ExclusiveOr(null, null, null);
					Executor.op_Int16.GreaterThan(null, null, null);
					Executor.op_Int16.GreaterThanOrEqual(null, null, null);
					Executor.op_Int16.LeftShift(null, null, null);
					Executor.op_Int16.Power(null, null, null);
					Executor.op_Int16.RightShift(null, null, null);
					Executor.op_Int16.LessThan(null, null, null);
					Executor.op_Int16.LessThanOrEqual(null, null, null);
					Executor.op_Int16.Modulo(null, null, null);
					Executor.op_Int16.Multiply(null, null, null);
					Executor.op_Int16.MultiplyChecked(null, null, null);
					Executor.op_Int16.NotEqual(null, null, null);
					Executor.op_Int16.Or(null, null, null);
					Executor.op_Int16.Subtract(null, null, null);
					Executor.op_Int16.SubtractChecked(null, null, null);
					Executor.op_Int16.ToObject(null, null, null);
					Executor.op_Int16.ToSByte(null, null, null);
					Executor.op_Int16.ToByte(null, null, null);
					Executor.op_Int16.ToInt16(null, null, null);
					Executor.op_Int16.ToUInt16(null, null, null);
					Executor.op_Int16.ToInt32(null, null, null);
					Executor.op_Int16.ToUInt32(null, null, null);
					Executor.op_Int16.ToInt64(null, null, null);
					Executor.op_Int16.ToUInt64(null, null, null);
					Executor.op_Int16.ToSingle(null, null, null);
					Executor.op_Int16.ToDouble(null, null, null);
					Executor.op_Int16.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<short>(0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<short>(-closure.Unbox<short>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<short>(checked(0 - closure.Unbox<short>(operand)));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<short>(closure.Unbox<short>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<short>(~closure.Unbox<short>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) + closure.Unbox<short>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(checked(closure.Unbox<short>(left) + closure.Unbox<short>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) & closure.Unbox<short>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) / closure.Unbox<short>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) == closure.Unbox<short>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) ^ closure.Unbox<short>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) > closure.Unbox<short>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) >= closure.Unbox<short>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>((short)(closure.Unbox<short>(left) << closure.Unbox<int>(right)));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>((short)Math.Pow((double)closure.Unbox<short>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>((short)(closure.Unbox<short>(left) >> closure.Unbox<int>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) < closure.Unbox<short>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) <= closure.Unbox<short>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) % closure.Unbox<short>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) * closure.Unbox<short>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(checked(closure.Unbox<short>(left) * closure.Unbox<short>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<short>(left) != closure.Unbox<short>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) | closure.Unbox<short>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(closure.Unbox<short>(left) - closure.Unbox<short>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<short>(checked(closure.Unbox<short>(left) - closure.Unbox<short>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<short>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<short>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<short>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<short>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(closure.Unbox<short>(left));
				}
				return closure.Box<short>(closure.Unbox<short>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<short>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<short>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>((int)closure.Unbox<short>(left));
				}
				return closure.Box<int>((int)closure.Unbox<short>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<short>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<short>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<short>(left));
				}
				return closure.Box<long>((long)closure.Unbox<short>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<short>(left)));
				}
				return closure.Box<ulong>((ulong)((long)closure.Unbox<short>(left)));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<short>(left));
				}
				return closure.Box<float>((float)closure.Unbox<short>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<short>(left));
				}
				return closure.Box<double>((double)closure.Unbox<short>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<short>(left));
				}
				return closure.Box<decimal>(closure.Unbox<short>(left));
			}
		}

		private static class op_UInt16
		{
			static op_UInt16()
			{
				if (typeof(Executor.op_UInt16).Name == string.Empty)
				{
					Executor.op_UInt16.Default(null);
					Executor.op_UInt16.Negate(null, 0);
					Executor.op_UInt16.NegateChecked(null, null);
					Executor.op_UInt16.UnaryPlus(null, null);
					Executor.op_UInt16.Not(null, null);
					Executor.op_UInt16.Add(null, null, null);
					Executor.op_UInt16.AddChecked(null, null, null);
					Executor.op_UInt16.And(null, null, null);
					Executor.op_UInt16.Divide(null, null, null);
					Executor.op_UInt16.Equal(null, null, null);
					Executor.op_UInt16.ExclusiveOr(null, null, null);
					Executor.op_UInt16.GreaterThan(null, null, null);
					Executor.op_UInt16.GreaterThanOrEqual(null, null, null);
					Executor.op_UInt16.LeftShift(null, null, null);
					Executor.op_UInt16.Power(null, null, null);
					Executor.op_UInt16.RightShift(null, null, null);
					Executor.op_UInt16.LessThan(null, null, null);
					Executor.op_UInt16.LessThanOrEqual(null, null, null);
					Executor.op_UInt16.Modulo(null, null, null);
					Executor.op_UInt16.Multiply(null, null, null);
					Executor.op_UInt16.MultiplyChecked(null, null, null);
					Executor.op_UInt16.NotEqual(null, null, null);
					Executor.op_UInt16.Or(null, null, null);
					Executor.op_UInt16.Subtract(null, null, null);
					Executor.op_UInt16.SubtractChecked(null, null, null);
					Executor.op_UInt16.ToObject(null, null, null);
					Executor.op_UInt16.ToSByte(null, null, null);
					Executor.op_UInt16.ToByte(null, null, null);
					Executor.op_UInt16.ToInt16(null, null, null);
					Executor.op_UInt16.ToUInt16(null, null, null);
					Executor.op_UInt16.ToInt32(null, null, null);
					Executor.op_UInt16.ToUInt32(null, null, null);
					Executor.op_UInt16.ToInt64(null, null, null);
					Executor.op_UInt16.ToUInt64(null, null, null);
					Executor.op_UInt16.ToSingle(null, null, null);
					Executor.op_UInt16.ToDouble(null, null, null);
					Executor.op_UInt16.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<ushort>(0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<ushort>(-closure.Unbox<ushort>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<ushort>(checked(0 - closure.Unbox<ushort>(operand)));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<ushort>(~closure.Unbox<ushort>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) + closure.Unbox<ushort>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(checked(closure.Unbox<ushort>(left) + closure.Unbox<ushort>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) & closure.Unbox<ushort>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) / closure.Unbox<ushort>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) == closure.Unbox<ushort>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) ^ closure.Unbox<ushort>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) > closure.Unbox<ushort>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) >= closure.Unbox<ushort>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>((ushort)(closure.Unbox<ushort>(left) << closure.Unbox<int>(right)));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>((ushort)Math.Pow((double)closure.Unbox<ushort>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>((ushort)(closure.Unbox<ushort>(left) >> closure.Unbox<int>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) < closure.Unbox<ushort>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) <= closure.Unbox<ushort>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) % closure.Unbox<ushort>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) * closure.Unbox<ushort>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(checked(closure.Unbox<ushort>(left) * closure.Unbox<ushort>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ushort>(left) != closure.Unbox<ushort>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) | closure.Unbox<ushort>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(closure.Unbox<ushort>(left) - closure.Unbox<ushort>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ushort>(checked(closure.Unbox<ushort>(left) - closure.Unbox<ushort>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<ushort>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<ushort>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<ushort>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<ushort>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<ushort>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<ushort>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(closure.Unbox<ushort>(left));
				}
				return closure.Box<ushort>(closure.Unbox<ushort>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>((int)closure.Unbox<ushort>(left));
				}
				return closure.Box<int>((int)closure.Unbox<ushort>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>((uint)closure.Unbox<ushort>(left));
				}
				return closure.Box<uint>((uint)closure.Unbox<ushort>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<ushort>(left));
				}
				return closure.Box<long>((long)closure.Unbox<ushort>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>((ulong)closure.Unbox<ushort>(left));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<ushort>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<ushort>(left));
				}
				return closure.Box<float>((float)closure.Unbox<ushort>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<ushort>(left));
				}
				return closure.Box<double>((double)closure.Unbox<ushort>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<ushort>(left));
				}
				return closure.Box<decimal>(closure.Unbox<ushort>(left));
			}
		}

		private static class op_Int32
		{
			static op_Int32()
			{
				if (typeof(Executor.op_Int32).Name == string.Empty)
				{
					Executor.op_Int32.Default(null);
					Executor.op_Int32.Negate(null, 0);
					Executor.op_Int32.NegateChecked(null, null);
					Executor.op_Int32.UnaryPlus(null, null);
					Executor.op_Int32.Not(null, null);
					Executor.op_Int32.Add(null, null, null);
					Executor.op_Int32.AddChecked(null, null, null);
					Executor.op_Int32.And(null, null, null);
					Executor.op_Int32.Divide(null, null, null);
					Executor.op_Int32.Equal(null, null, null);
					Executor.op_Int32.ExclusiveOr(null, null, null);
					Executor.op_Int32.GreaterThan(null, null, null);
					Executor.op_Int32.GreaterThanOrEqual(null, null, null);
					Executor.op_Int32.LeftShift(null, null, null);
					Executor.op_Int32.Power(null, null, null);
					Executor.op_Int32.RightShift(null, null, null);
					Executor.op_Int32.LessThan(null, null, null);
					Executor.op_Int32.LessThanOrEqual(null, null, null);
					Executor.op_Int32.Modulo(null, null, null);
					Executor.op_Int32.Multiply(null, null, null);
					Executor.op_Int32.MultiplyChecked(null, null, null);
					Executor.op_Int32.NotEqual(null, null, null);
					Executor.op_Int32.Or(null, null, null);
					Executor.op_Int32.Subtract(null, null, null);
					Executor.op_Int32.SubtractChecked(null, null, null);
					Executor.op_Int32.ToObject(null, null, null);
					Executor.op_Int32.ToSByte(null, null, null);
					Executor.op_Int32.ToByte(null, null, null);
					Executor.op_Int32.ToInt16(null, null, null);
					Executor.op_Int32.ToUInt16(null, null, null);
					Executor.op_Int32.ToInt32(null, null, null);
					Executor.op_Int32.ToUInt32(null, null, null);
					Executor.op_Int32.ToInt64(null, null, null);
					Executor.op_Int32.ToUInt64(null, null, null);
					Executor.op_Int32.ToSingle(null, null, null);
					Executor.op_Int32.ToDouble(null, null, null);
					Executor.op_Int32.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<int>(0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<int>(-closure.Unbox<int>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<int>(checked(0 - closure.Unbox<int>(operand)));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<int>(closure.Unbox<int>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<int>(~closure.Unbox<int>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) + closure.Unbox<int>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(checked(closure.Unbox<int>(left) + closure.Unbox<int>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) & closure.Unbox<int>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) / closure.Unbox<int>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) == closure.Unbox<int>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) ^ closure.Unbox<int>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) > closure.Unbox<int>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) >= closure.Unbox<int>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) << closure.Unbox<int>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>((int)Math.Pow((double)closure.Unbox<int>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) >> closure.Unbox<int>(right));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) < closure.Unbox<int>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) <= closure.Unbox<int>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) % closure.Unbox<int>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) * closure.Unbox<int>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(checked(closure.Unbox<int>(left) * closure.Unbox<int>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<int>(left) != closure.Unbox<int>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) | closure.Unbox<int>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(closure.Unbox<int>(left) - closure.Unbox<int>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<int>(checked(closure.Unbox<int>(left) - closure.Unbox<int>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<int>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<int>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<int>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<int>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<int>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<int>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<int>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<int>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(closure.Unbox<int>(left));
				}
				return closure.Box<int>(closure.Unbox<int>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<int>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<int>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<int>(left));
				}
				return closure.Box<long>((long)closure.Unbox<int>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<int>(left)));
				}
				return closure.Box<ulong>((ulong)((long)closure.Unbox<int>(left)));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<int>(left));
				}
				return closure.Box<float>((float)closure.Unbox<int>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<int>(left));
				}
				return closure.Box<double>((double)closure.Unbox<int>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<int>(left));
				}
				return closure.Box<decimal>(closure.Unbox<int>(left));
			}
		}

		private static class op_UInt32
		{
			static op_UInt32()
			{
				if (typeof(Executor.op_UInt32).Name == string.Empty)
				{
					Executor.op_UInt32.Default(null);
					Executor.op_UInt32.Negate(null, 0U);
					Executor.op_UInt32.NegateChecked(null, null);
					Executor.op_UInt32.UnaryPlus(null, null);
					Executor.op_UInt32.Not(null, null);
					Executor.op_UInt32.Add(null, null, null);
					Executor.op_UInt32.AddChecked(null, null, null);
					Executor.op_UInt32.And(null, null, null);
					Executor.op_UInt32.Divide(null, null, null);
					Executor.op_UInt32.Equal(null, null, null);
					Executor.op_UInt32.ExclusiveOr(null, null, null);
					Executor.op_UInt32.GreaterThan(null, null, null);
					Executor.op_UInt32.GreaterThanOrEqual(null, null, null);
					Executor.op_UInt32.LeftShift(null, null, null);
					Executor.op_UInt32.Power(null, null, null);
					Executor.op_UInt32.RightShift(null, null, null);
					Executor.op_UInt32.LessThan(null, null, null);
					Executor.op_UInt32.LessThanOrEqual(null, null, null);
					Executor.op_UInt32.Modulo(null, null, null);
					Executor.op_UInt32.Multiply(null, null, null);
					Executor.op_UInt32.MultiplyChecked(null, null, null);
					Executor.op_UInt32.NotEqual(null, null, null);
					Executor.op_UInt32.Or(null, null, null);
					Executor.op_UInt32.Subtract(null, null, null);
					Executor.op_UInt32.SubtractChecked(null, null, null);
					Executor.op_UInt32.ToObject(null, null, null);
					Executor.op_UInt32.ToSByte(null, null, null);
					Executor.op_UInt32.ToByte(null, null, null);
					Executor.op_UInt32.ToInt16(null, null, null);
					Executor.op_UInt32.ToUInt16(null, null, null);
					Executor.op_UInt32.ToInt32(null, null, null);
					Executor.op_UInt32.ToUInt32(null, null, null);
					Executor.op_UInt32.ToInt64(null, null, null);
					Executor.op_UInt32.ToUInt64(null, null, null);
					Executor.op_UInt32.ToSingle(null, null, null);
					Executor.op_UInt32.ToDouble(null, null, null);
					Executor.op_UInt32.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<uint>(0U);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<uint>((uint)(-(uint)((ulong)closure.Unbox<uint>(operand))));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<uint>((uint)(checked(unchecked((ulong)0) - unchecked((ulong)closure.Unbox<uint>(operand)))));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<uint>(closure.Unbox<uint>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<uint>(~closure.Unbox<uint>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) + closure.Unbox<uint>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(checked(closure.Unbox<uint>(left) + closure.Unbox<uint>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) & closure.Unbox<uint>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) / closure.Unbox<uint>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) == closure.Unbox<uint>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) ^ closure.Unbox<uint>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) > closure.Unbox<uint>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) >= closure.Unbox<uint>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) << closure.Unbox<int>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>((uint)Math.Pow(closure.Unbox<uint>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) >> closure.Unbox<int>(right));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) < closure.Unbox<uint>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) <= closure.Unbox<uint>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) % closure.Unbox<uint>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) * closure.Unbox<uint>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(checked(closure.Unbox<uint>(left) * closure.Unbox<uint>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<uint>(left) != closure.Unbox<uint>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) | closure.Unbox<uint>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(closure.Unbox<uint>(left) - closure.Unbox<uint>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<uint>(checked(closure.Unbox<uint>(left) - closure.Unbox<uint>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<uint>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<uint>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<uint>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<uint>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<uint>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<uint>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<uint>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<uint>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(checked((int)closure.Unbox<uint>(left)));
				}
				return closure.Box<int>((int)closure.Unbox<uint>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(closure.Unbox<uint>(left));
				}
				return closure.Box<uint>(closure.Unbox<uint>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)((ulong)closure.Unbox<uint>(left)));
				}
				return closure.Box<long>((long)((ulong)closure.Unbox<uint>(left)));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>((ulong)closure.Unbox<uint>(left));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<uint>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>(closure.Unbox<uint>(left));
				}
				return closure.Box<float>(closure.Unbox<uint>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>(closure.Unbox<uint>(left));
				}
				return closure.Box<double>(closure.Unbox<uint>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<uint>(left));
				}
				return closure.Box<decimal>(closure.Unbox<uint>(left));
			}
		}

		private static class op_Int64
		{
			static op_Int64()
			{
				if (typeof(Executor.op_Int64).Name == string.Empty)
				{
					Executor.op_Int64.Default(null);
					Executor.op_Int64.Negate(null, 0L);
					Executor.op_Int64.NegateChecked(null, null);
					Executor.op_Int64.UnaryPlus(null, null);
					Executor.op_Int64.Not(null, null);
					Executor.op_Int64.Add(null, null, null);
					Executor.op_Int64.AddChecked(null, null, null);
					Executor.op_Int64.And(null, null, null);
					Executor.op_Int64.Divide(null, null, null);
					Executor.op_Int64.Equal(null, null, null);
					Executor.op_Int64.ExclusiveOr(null, null, null);
					Executor.op_Int64.GreaterThan(null, null, null);
					Executor.op_Int64.GreaterThanOrEqual(null, null, null);
					Executor.op_Int64.LeftShift(null, null, null);
					Executor.op_Int64.Power(null, null, null);
					Executor.op_Int64.RightShift(null, null, null);
					Executor.op_Int64.LessThan(null, null, null);
					Executor.op_Int64.LessThanOrEqual(null, null, null);
					Executor.op_Int64.Modulo(null, null, null);
					Executor.op_Int64.Multiply(null, null, null);
					Executor.op_Int64.MultiplyChecked(null, null, null);
					Executor.op_Int64.NotEqual(null, null, null);
					Executor.op_Int64.Or(null, null, null);
					Executor.op_Int64.Subtract(null, null, null);
					Executor.op_Int64.SubtractChecked(null, null, null);
					Executor.op_Int64.ToObject(null, null, null);
					Executor.op_Int64.ToSByte(null, null, null);
					Executor.op_Int64.ToByte(null, null, null);
					Executor.op_Int64.ToInt16(null, null, null);
					Executor.op_Int64.ToUInt16(null, null, null);
					Executor.op_Int64.ToInt32(null, null, null);
					Executor.op_Int64.ToUInt32(null, null, null);
					Executor.op_Int64.ToInt64(null, null, null);
					Executor.op_Int64.ToUInt64(null, null, null);
					Executor.op_Int64.ToSingle(null, null, null);
					Executor.op_Int64.ToDouble(null, null, null);
					Executor.op_Int64.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<long>(0L);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<long>(-closure.Unbox<long>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				checked
				{
					return closure.Box<long>((long)(unchecked((ulong)0) - (ulong)closure.Unbox<long>(operand)));
				}
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<long>(closure.Unbox<long>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<long>(~closure.Unbox<long>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) + closure.Unbox<long>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(checked(closure.Unbox<long>(left) + closure.Unbox<long>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) & closure.Unbox<long>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) / closure.Unbox<long>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) == closure.Unbox<long>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) ^ closure.Unbox<long>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) > closure.Unbox<long>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) >= closure.Unbox<long>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) << closure.Unbox<int>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>((long)Math.Pow((double)closure.Unbox<long>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) >> closure.Unbox<int>(right));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) < closure.Unbox<long>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) <= closure.Unbox<long>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) % closure.Unbox<long>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) * closure.Unbox<long>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(checked(closure.Unbox<long>(left) * closure.Unbox<long>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<long>(left) != closure.Unbox<long>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) | closure.Unbox<long>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(closure.Unbox<long>(left) - closure.Unbox<long>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<long>(checked(closure.Unbox<long>(left) - closure.Unbox<long>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<long>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<long>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<long>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<long>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<long>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<long>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<long>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<long>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(checked((int)closure.Unbox<long>(left)));
				}
				return closure.Box<int>((int)closure.Unbox<long>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<long>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<long>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>(closure.Unbox<long>(left));
				}
				return closure.Box<long>(closure.Unbox<long>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<long>(left)));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<long>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<long>(left));
				}
				return closure.Box<float>((float)closure.Unbox<long>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<long>(left));
				}
				return closure.Box<double>((double)closure.Unbox<long>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<long>(left));
				}
				return closure.Box<decimal>(closure.Unbox<long>(left));
			}
		}

		private static class op_UInt64
		{
			static op_UInt64()
			{
				if (typeof(Executor.op_UInt64).Name == string.Empty)
				{
					Executor.op_UInt64.Default(null);
					Executor.op_UInt64.UnaryPlus(null, 0UL);
					Executor.op_UInt64.Not(null, null);
					Executor.op_UInt64.Add(null, null, null);
					Executor.op_UInt64.AddChecked(null, null, null);
					Executor.op_UInt64.And(null, null, null);
					Executor.op_UInt64.Divide(null, null, null);
					Executor.op_UInt64.Equal(null, null, null);
					Executor.op_UInt64.ExclusiveOr(null, null, null);
					Executor.op_UInt64.GreaterThan(null, null, null);
					Executor.op_UInt64.GreaterThanOrEqual(null, null, null);
					Executor.op_UInt64.LeftShift(null, null, null);
					Executor.op_UInt64.Power(null, null, null);
					Executor.op_UInt64.RightShift(null, null, null);
					Executor.op_UInt64.LessThan(null, null, null);
					Executor.op_UInt64.LessThanOrEqual(null, null, null);
					Executor.op_UInt64.Modulo(null, null, null);
					Executor.op_UInt64.Multiply(null, null, null);
					Executor.op_UInt64.MultiplyChecked(null, null, null);
					Executor.op_UInt64.NotEqual(null, null, null);
					Executor.op_UInt64.Or(null, null, null);
					Executor.op_UInt64.Subtract(null, null, null);
					Executor.op_UInt64.SubtractChecked(null, null, null);
					Executor.op_UInt64.ToObject(null, null, null);
					Executor.op_UInt64.ToSByte(null, null, null);
					Executor.op_UInt64.ToByte(null, null, null);
					Executor.op_UInt64.ToInt16(null, null, null);
					Executor.op_UInt64.ToUInt16(null, null, null);
					Executor.op_UInt64.ToInt32(null, null, null);
					Executor.op_UInt64.ToUInt32(null, null, null);
					Executor.op_UInt64.ToInt64(null, null, null);
					Executor.op_UInt64.ToUInt64(null, null, null);
					Executor.op_UInt64.ToSingle(null, null, null);
					Executor.op_UInt64.ToDouble(null, null, null);
					Executor.op_UInt64.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<ulong>(0UL);
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(operand));
			}

			public static object Not(Executor.Closure closure, object operand)
			{
				return closure.Box<ulong>(~closure.Unbox<ulong>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) + closure.Unbox<ulong>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(checked(closure.Unbox<ulong>(left) + closure.Unbox<ulong>(right)));
			}

			public static object And(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) & closure.Unbox<ulong>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) / closure.Unbox<ulong>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) == closure.Unbox<ulong>(right));
			}

			public static object ExclusiveOr(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) ^ closure.Unbox<ulong>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) > closure.Unbox<ulong>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) >= closure.Unbox<ulong>(right));
			}

			public static object LeftShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) << closure.Unbox<int>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>((ulong)Math.Pow(closure.Unbox<ulong>(left), closure.Unbox<double>(right)));
			}

			public static object RightShift(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) >> closure.Unbox<int>(right));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) < closure.Unbox<ulong>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) <= closure.Unbox<ulong>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) % closure.Unbox<ulong>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) * closure.Unbox<ulong>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(checked(closure.Unbox<ulong>(left) * closure.Unbox<ulong>(right)));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<ulong>(left) != closure.Unbox<ulong>(right));
			}

			public static object Or(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) | closure.Unbox<ulong>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(closure.Unbox<ulong>(left) - closure.Unbox<ulong>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<ulong>(checked(closure.Unbox<ulong>(left) - closure.Unbox<ulong>(right)));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<ulong>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<ulong>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<ulong>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<ulong>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<ulong>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<ulong>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<ulong>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<ulong>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(checked((int)closure.Unbox<ulong>(left)));
				}
				return closure.Box<int>((int)closure.Unbox<ulong>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<ulong>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<ulong>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>(checked((long)closure.Unbox<ulong>(left)));
				}
				return closure.Box<long>((long)closure.Unbox<ulong>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(closure.Unbox<ulong>(left));
				}
				return closure.Box<ulong>(closure.Unbox<ulong>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>(closure.Unbox<ulong>(left));
				}
				return closure.Box<float>(closure.Unbox<ulong>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>(closure.Unbox<ulong>(left));
				}
				return closure.Box<double>(closure.Unbox<ulong>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<ulong>(left));
				}
				return closure.Box<decimal>(closure.Unbox<ulong>(left));
			}
		}

		private static class op_Single
		{
			static op_Single()
			{
				if (typeof(Executor.op_Single).Name == string.Empty)
				{
					Executor.op_Single.Default(null);
					Executor.op_Single.Negate(null, 0f);
					Executor.op_Single.NegateChecked(null, null);
					Executor.op_Single.UnaryPlus(null, null);
					Executor.op_Single.Add(null, null, null);
					Executor.op_Single.AddChecked(null, null, null);
					Executor.op_Single.Divide(null, null, null);
					Executor.op_Single.Equal(null, null, null);
					Executor.op_Single.GreaterThan(null, null, null);
					Executor.op_Single.GreaterThanOrEqual(null, null, null);
					Executor.op_Single.Power(null, null, null);
					Executor.op_Single.LessThan(null, null, null);
					Executor.op_Single.LessThanOrEqual(null, null, null);
					Executor.op_Single.Modulo(null, null, null);
					Executor.op_Single.Multiply(null, null, null);
					Executor.op_Single.MultiplyChecked(null, null, null);
					Executor.op_Single.NotEqual(null, null, null);
					Executor.op_Single.Subtract(null, null, null);
					Executor.op_Single.SubtractChecked(null, null, null);
					Executor.op_Single.ToObject(null, null, null);
					Executor.op_Single.ToSByte(null, null, null);
					Executor.op_Single.ToByte(null, null, null);
					Executor.op_Single.ToInt16(null, null, null);
					Executor.op_Single.ToUInt16(null, null, null);
					Executor.op_Single.ToInt32(null, null, null);
					Executor.op_Single.ToUInt32(null, null, null);
					Executor.op_Single.ToInt64(null, null, null);
					Executor.op_Single.ToUInt64(null, null, null);
					Executor.op_Single.ToSingle(null, null, null);
					Executor.op_Single.ToDouble(null, null, null);
					Executor.op_Single.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<float>(0f);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<float>(-closure.Unbox<float>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<float>(-closure.Unbox<float>(operand));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<float>(closure.Unbox<float>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) + closure.Unbox<float>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) + closure.Unbox<float>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) / closure.Unbox<float>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) == closure.Unbox<float>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) > closure.Unbox<float>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) >= closure.Unbox<float>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>((float)Math.Pow((double)closure.Unbox<float>(left), closure.Unbox<double>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) < closure.Unbox<float>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) <= closure.Unbox<float>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) % closure.Unbox<float>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) * closure.Unbox<float>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) * closure.Unbox<float>(right));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<float>(left) != closure.Unbox<float>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) - closure.Unbox<float>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<float>(closure.Unbox<float>(left) - closure.Unbox<float>(right));
			}

			public static object ToObject(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<float>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<float>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<float>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<float>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<float>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<float>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<float>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<float>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(checked((int)closure.Unbox<float>(left)));
				}
				return closure.Box<int>((int)closure.Unbox<float>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<float>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<float>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>(checked((long)closure.Unbox<float>(left)));
				}
				return closure.Box<long>((long)closure.Unbox<float>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<float>(left)));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<float>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>(closure.Unbox<float>(left));
				}
				return closure.Box<float>(closure.Unbox<float>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<float>(left));
				}
				return closure.Box<double>((double)closure.Unbox<float>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>((decimal)closure.Unbox<float>(left));
				}
				return closure.Box<decimal>((decimal)closure.Unbox<float>(left));
			}
		}

		private static class op_Double
		{
			static op_Double()
			{
				if (typeof(Executor.op_Double).Name == string.Empty)
				{
					Executor.op_Double.Default(null);
					Executor.op_Double.Negate(null, 0.0);
					Executor.op_Double.NegateChecked(null, null);
					Executor.op_Double.UnaryPlus(null, null);
					Executor.op_Double.Add(null, null, null);
					Executor.op_Double.AddChecked(null, null, null);
					Executor.op_Double.Divide(null, null, null);
					Executor.op_Double.Equal(null, null, null);
					Executor.op_Double.GreaterThan(null, null, null);
					Executor.op_Double.GreaterThanOrEqual(null, null, null);
					Executor.op_Double.Power(null, null, null);
					Executor.op_Double.LessThan(null, null, null);
					Executor.op_Double.LessThanOrEqual(null, null, null);
					Executor.op_Double.Modulo(null, null, null);
					Executor.op_Double.Multiply(null, null, null);
					Executor.op_Double.MultiplyChecked(null, null, null);
					Executor.op_Double.NotEqual(null, null, null);
					Executor.op_Double.Subtract(null, null, null);
					Executor.op_Double.SubtractChecked(null, null, null);
					Executor.op_Double.ToObject(null, null, null);
					Executor.op_Double.ToSByte(null, null, null);
					Executor.op_Double.ToByte(null, null, null);
					Executor.op_Double.ToInt16(null, null, null);
					Executor.op_Double.ToUInt16(null, null, null);
					Executor.op_Double.ToInt32(null, null, null);
					Executor.op_Double.ToUInt32(null, null, null);
					Executor.op_Double.ToInt64(null, null, null);
					Executor.op_Double.ToUInt64(null, null, null);
					Executor.op_Double.ToSingle(null, null, null);
					Executor.op_Double.ToDouble(null, null, null);
					Executor.op_Double.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<double>(0.0);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<double>(-closure.Unbox<double>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<double>(-closure.Unbox<double>(operand));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<double>(closure.Unbox<double>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) + closure.Unbox<double>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) + closure.Unbox<double>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) / closure.Unbox<double>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) == closure.Unbox<double>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) > closure.Unbox<double>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) >= closure.Unbox<double>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(Math.Pow(closure.Unbox<double>(left), closure.Unbox<double>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) < closure.Unbox<double>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) <= closure.Unbox<double>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) % closure.Unbox<double>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) * closure.Unbox<double>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) * closure.Unbox<double>(right));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<double>(left) != closure.Unbox<double>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) - closure.Unbox<double>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<double>(closure.Unbox<double>(left) - closure.Unbox<double>(right));
			}

			public static object ToObject(Executor.Closure closure, object left, object _)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>(checked((sbyte)closure.Unbox<double>(left)));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<double>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>(checked((byte)closure.Unbox<double>(left)));
				}
				return closure.Box<byte>((byte)closure.Unbox<double>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>(checked((short)closure.Unbox<double>(left)));
				}
				return closure.Box<short>((short)closure.Unbox<double>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>(checked((ushort)closure.Unbox<double>(left)));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<double>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>(checked((int)closure.Unbox<double>(left)));
				}
				return closure.Box<int>((int)closure.Unbox<double>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>(checked((uint)closure.Unbox<double>(left)));
				}
				return closure.Box<uint>((uint)closure.Unbox<double>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>(checked((long)closure.Unbox<double>(left)));
				}
				return closure.Box<long>((long)closure.Unbox<double>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>(checked((ulong)closure.Unbox<double>(left)));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<double>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<double>(left));
				}
				return closure.Box<float>((float)closure.Unbox<double>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>(closure.Unbox<double>(left));
				}
				return closure.Box<double>(closure.Unbox<double>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>((decimal)closure.Unbox<double>(left));
				}
				return closure.Box<decimal>((decimal)closure.Unbox<double>(left));
			}
		}

		private static class op_Decimal
		{
			static op_Decimal()
			{
				if (typeof(Executor.op_Decimal).Name == string.Empty)
				{
					Executor.op_Decimal.Default(null);
					Executor.op_Decimal.Negate(null, 0m);
					Executor.op_Decimal.NegateChecked(null, null);
					Executor.op_Decimal.UnaryPlus(null, null);
					Executor.op_Decimal.Add(null, null, null);
					Executor.op_Decimal.AddChecked(null, null, null);
					Executor.op_Decimal.Divide(null, null, null);
					Executor.op_Decimal.Equal(null, null, null);
					Executor.op_Decimal.GreaterThan(null, null, null);
					Executor.op_Decimal.GreaterThanOrEqual(null, null, null);
					Executor.op_Decimal.Power(null, null, null);
					Executor.op_Decimal.LessThan(null, null, null);
					Executor.op_Decimal.LessThanOrEqual(null, null, null);
					Executor.op_Decimal.Modulo(null, null, null);
					Executor.op_Decimal.Multiply(null, null, null);
					Executor.op_Decimal.MultiplyChecked(null, null, null);
					Executor.op_Decimal.NotEqual(null, null, null);
					Executor.op_Decimal.Subtract(null, null, null);
					Executor.op_Decimal.SubtractChecked(null, null, null);
					Executor.op_Decimal.ToObject(null, null, null);
					Executor.op_Decimal.ToSByte(null, null, null);
					Executor.op_Decimal.ToByte(null, null, null);
					Executor.op_Decimal.ToInt16(null, null, null);
					Executor.op_Decimal.ToUInt16(null, null, null);
					Executor.op_Decimal.ToInt32(null, null, null);
					Executor.op_Decimal.ToUInt32(null, null, null);
					Executor.op_Decimal.ToInt64(null, null, null);
					Executor.op_Decimal.ToUInt64(null, null, null);
					Executor.op_Decimal.ToSingle(null, null, null);
					Executor.op_Decimal.ToDouble(null, null, null);
					Executor.op_Decimal.ToDecimal(null, null, null);
				}
			}

			public static object Default(Executor.Closure closure)
			{
				return closure.Box<decimal>(0m);
			}

			public static object Negate(Executor.Closure closure, object operand)
			{
				return closure.Box<decimal>(-closure.Unbox<decimal>(operand));
			}

			public static object NegateChecked(Executor.Closure closure, object operand)
			{
				return closure.Box<decimal>(-closure.Unbox<decimal>(operand));
			}

			public static object UnaryPlus(Executor.Closure closure, object operand)
			{
				return closure.Box<decimal>(+closure.Unbox<decimal>(operand));
			}

			public static object Add(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) + closure.Unbox<decimal>(right));
			}

			public static object AddChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) + closure.Unbox<decimal>(right));
			}

			public static object Divide(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) / closure.Unbox<decimal>(right));
			}

			public static object Equal(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) == closure.Unbox<decimal>(right));
			}

			public static object GreaterThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) > closure.Unbox<decimal>(right));
			}

			public static object GreaterThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) >= closure.Unbox<decimal>(right));
			}

			public static object Power(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>((decimal)Math.Pow((double)closure.Unbox<decimal>(left), closure.Unbox<double>(right)));
			}

			public static object LessThan(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) < closure.Unbox<decimal>(right));
			}

			public static object LessThanOrEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) <= closure.Unbox<decimal>(right));
			}

			public static object Modulo(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) % closure.Unbox<decimal>(right));
			}

			public static object Multiply(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) * closure.Unbox<decimal>(right));
			}

			public static object MultiplyChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) * closure.Unbox<decimal>(right));
			}

			public static object NotEqual(Executor.Closure closure, object left, object right)
			{
				return closure.Box<bool>(closure.Unbox<decimal>(left) != closure.Unbox<decimal>(right));
			}

			public static object Subtract(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) - closure.Unbox<decimal>(right));
			}

			public static object SubtractChecked(Executor.Closure closure, object left, object right)
			{
				return closure.Box<decimal>(closure.Unbox<decimal>(left) - closure.Unbox<decimal>(right));
			}

			public static object ToObject(Executor.Closure closure, object left, object isChecked)
			{
				return closure.Box<object>(left);
			}

			public static object ToSByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<sbyte>((sbyte)closure.Unbox<decimal>(left));
				}
				return closure.Box<sbyte>((sbyte)closure.Unbox<decimal>(left));
			}

			public static object ToByte(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<byte>((byte)closure.Unbox<decimal>(left));
				}
				return closure.Box<byte>((byte)closure.Unbox<decimal>(left));
			}

			public static object ToInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<short>((short)closure.Unbox<decimal>(left));
				}
				return closure.Box<short>((short)closure.Unbox<decimal>(left));
			}

			public static object ToUInt16(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ushort>((ushort)closure.Unbox<decimal>(left));
				}
				return closure.Box<ushort>((ushort)closure.Unbox<decimal>(left));
			}

			public static object ToInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<int>((int)closure.Unbox<decimal>(left));
				}
				return closure.Box<int>((int)closure.Unbox<decimal>(left));
			}

			public static object ToUInt32(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<uint>((uint)closure.Unbox<decimal>(left));
				}
				return closure.Box<uint>((uint)closure.Unbox<decimal>(left));
			}

			public static object ToInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<long>((long)closure.Unbox<decimal>(left));
				}
				return closure.Box<long>((long)closure.Unbox<decimal>(left));
			}

			public static object ToUInt64(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<ulong>((ulong)closure.Unbox<decimal>(left));
				}
				return closure.Box<ulong>((ulong)closure.Unbox<decimal>(left));
			}

			public static object ToSingle(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<float>((float)closure.Unbox<decimal>(left));
				}
				return closure.Box<float>((float)closure.Unbox<decimal>(left));
			}

			public static object ToDouble(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<double>((double)closure.Unbox<decimal>(left));
				}
				return closure.Box<double>((double)closure.Unbox<decimal>(left));
			}

			public static object ToDecimal(Executor.Closure closure, object left, object isChecked)
			{
				if (object.ReferenceEquals(isChecked, bool.TrueString))
				{
					return closure.Box<decimal>(closure.Unbox<decimal>(left));
				}
				return closure.Box<decimal>(closure.Unbox<decimal>(left));
			}
		}

		private delegate object InvokeOperation(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns);

		private delegate Executor.InvokeOperation InvokeOperationCreator(MethodInfo method, ParameterInfo[] parameters);

		private class MethodCall
		{
			private MethodCall(Type delegateType, MethodInfo method)
			{
				if (delegateType == null)
				{
					throw new ArgumentNullException("method");
				}
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				this.fn = Delegate.CreateDelegate(delegateType, method, true);
			}

			private object FuncInvoker<ResultT>(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns)
			{
				return ((Func<ResultT>)this.fn)();
			}

			private object FuncInvoker<Arg1T, ResultT>(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns)
			{
				Arg1T arg1T = closure.Unbox<Arg1T>(argumentFns[0](closure));
				return ((Func<Arg1T, ResultT>)this.fn)(arg1T);
			}

			private object FuncInvoker<Arg1T, Arg2T, ResultT>(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns)
			{
				Arg1T arg1T = closure.Unbox<Arg1T>(argumentFns[0](closure));
				Arg2T arg2T = closure.Unbox<Arg2T>(argumentFns[1](closure));
				return ((Func<Arg1T, Arg2T, ResultT>)this.fn)(arg1T, arg2T);
			}

			private object FuncInvoker<Arg1T, Arg2T, Arg3T, ResultT>(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns)
			{
				Arg1T arg1T = closure.Unbox<Arg1T>(argumentFns[0](closure));
				Arg2T arg2T = closure.Unbox<Arg2T>(argumentFns[1](closure));
				Arg3T arg3T = closure.Unbox<Arg3T>(argumentFns[2](closure));
				return ((Func<Arg1T, Arg2T, Arg3T, ResultT>)this.fn)(arg1T, arg2T, arg3T);
			}

			private object FuncInvoker<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(Executor.Closure closure, Executor.ExecuteFunc[] argumentFns)
			{
				Arg1T arg1T = closure.Unbox<Arg1T>(argumentFns[0](closure));
				Arg2T arg2T = closure.Unbox<Arg2T>(argumentFns[1](closure));
				Arg3T arg3T = closure.Unbox<Arg3T>(argumentFns[2](closure));
				Arg4T arg4T = closure.Unbox<Arg4T>(argumentFns[3](closure));
				return ((Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>)this.fn)(arg1T, arg2T, arg3T, arg4T);
			}

			public static Executor.InvokeOperation TryCreate(MethodInfo method)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (method.IsStatic)
				{
					return Executor.MethodCall.TryCreateStaticMethod(method);
				}
				return Executor.MethodCall.TryCreateInstanceMethod(method);
			}

			private static Executor.InvokeOperation TryCreateStaticMethod(MethodInfo method)
			{
				Executor.InvokeOperation invokeOperation = null;
				object staticMethods = Executor.MethodCall.StaticMethods;
				lock (staticMethods)
				{
					if (Executor.MethodCall.StaticMethods.TryGetValue(method, out invokeOperation))
					{
						return invokeOperation;
					}
				}
				ParameterInfo[] parameters = method.GetParameters();
				switch (parameters.Length)
				{
				case 0:
				{
					Executor.InvokeOperation invokeOperation2;
					if ((invokeOperation2 = Executor.MethodCall.TryCreate<bool>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<byte>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<sbyte>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<short>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<ushort>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<int>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<uint>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<long>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<ulong>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<float>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<double>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<decimal>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<string>(method, parameters)) == null && (invokeOperation2 = Executor.MethodCall.TryCreate<object>(method, parameters)) == null)
					{
						invokeOperation2 = Executor.MethodCall.TryCreate<TimeSpan>(method, parameters) ?? Executor.MethodCall.TryCreate<DateTime>(method, parameters);
					}
					invokeOperation = invokeOperation2;
					break;
				}
				case 1:
				{
					Executor.InvokeOperation invokeOperation3;
					if ((invokeOperation3 = Executor.MethodCall.TryCreate<bool, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<byte, byte>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<sbyte, sbyte>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<short, short>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<ushort, ushort>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<int, int>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<uint, long>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<long, long>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<ulong, ulong>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<float, float>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<double, double>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<decimal, decimal>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<string, string>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<object, object>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<TimeSpan, TimeSpan>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<DateTime, DateTime>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<DateTime, TimeSpan>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<byte, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<sbyte, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<short, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<ushort, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<int, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<uint, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<long, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<ulong, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<float, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<double, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<decimal, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<string, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<object, bool>(method, parameters)) == null && (invokeOperation3 = Executor.MethodCall.TryCreate<object, string>(method, parameters)) == null)
					{
						invokeOperation3 = Executor.MethodCall.TryCreate<TimeSpan, bool>(method, parameters) ?? Executor.MethodCall.TryCreate<DateTime, bool>(method, parameters);
					}
					invokeOperation = invokeOperation3;
					break;
				}
				case 2:
				{
					Executor.InvokeOperation invokeOperation4;
					if ((invokeOperation4 = Executor.MethodCall.TryCreate<bool, bool, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<byte, byte, byte>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<sbyte, sbyte, sbyte>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<short, short, short>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<ushort, ushort, ushort>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<int, int, int>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<uint, uint, long>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<long, long, long>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<ulong, ulong, ulong>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<float, float, float>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<double, double, double>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<decimal, decimal, decimal>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<string, string, string>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<object, object, object>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<TimeSpan, TimeSpan, TimeSpan>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<DateTime, DateTime, DateTime>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<DateTime, DateTime, TimeSpan>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<DateTime, TimeSpan, DateTime>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<byte, byte, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<sbyte, sbyte, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<short, short, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<ushort, ushort, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<int, int, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<uint, uint, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<long, long, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<ulong, ulong, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<float, float, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<double, double, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<decimal, decimal, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<string, string, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<object, object, bool>(method, parameters)) == null && (invokeOperation4 = Executor.MethodCall.TryCreate<object, object, string>(method, parameters)) == null)
					{
						invokeOperation4 = Executor.MethodCall.TryCreate<TimeSpan, TimeSpan, bool>(method, parameters) ?? Executor.MethodCall.TryCreate<DateTime, DateTime, bool>(method, parameters);
					}
					invokeOperation = invokeOperation4;
					break;
				}
				case 3:
				{
					Executor.InvokeOperation invokeOperation5;
					if ((invokeOperation5 = Executor.MethodCall.TryCreate<bool, bool, bool, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<byte, byte, byte, byte>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<sbyte, sbyte, sbyte, sbyte>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<short, short, short, short>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<ushort, ushort, ushort, ushort>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<int, int, int, int>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<uint, uint, uint, long>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<long, long, long, long>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<ulong, ulong, ulong, ulong>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<float, float, float, float>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<double, double, double, double>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<decimal, decimal, decimal, decimal>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<string, string, string, string>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<object, object, object, object>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<byte, byte, byte, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<sbyte, sbyte, sbyte, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<short, short, short, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<ushort, ushort, ushort, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<int, int, int, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<uint, uint, uint, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<long, long, long, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<ulong, ulong, ulong, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<float, float, float, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<double, double, double, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<decimal, decimal, decimal, bool>(method, parameters)) == null && (invokeOperation5 = Executor.MethodCall.TryCreate<string, string, string, bool>(method, parameters)) == null)
					{
						invokeOperation5 = Executor.MethodCall.TryCreate<object, object, object, bool>(method, parameters) ?? Executor.MethodCall.TryCreate<object, object, object, string>(method, parameters);
					}
					invokeOperation = invokeOperation5;
					break;
				}
				case 4:
				{
					Executor.InvokeOperation invokeOperation6;
					if ((invokeOperation6 = Executor.MethodCall.TryCreate<bool, bool, bool, bool, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<byte, byte, byte, byte, byte>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<sbyte, sbyte, sbyte, sbyte, sbyte>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<sbyte, short, short, short, short>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<ushort, ushort, ushort, ushort, ushort>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<int, int, int, int, int>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<uint, uint, uint, uint, long>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<long, long, long, long, long>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<ulong, ulong, ulong, ulong, ulong>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<float, float, float, float, float>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<double, double, double, double, double>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<decimal, decimal, decimal, decimal, decimal>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<string, string, string, string, string>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<object, object, object, object, object>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<byte, byte, byte, byte, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<sbyte, sbyte, sbyte, sbyte, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<short, short, short, short, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<ushort, ushort, ushort, ushort, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<int, int, int, int, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<uint, uint, uint, uint, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<long, long, long, long, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<ulong, ulong, ulong, ulong, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<float, float, float, float, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<double, double, double, double, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<decimal, decimal, decimal, decimal, bool>(method, parameters)) == null && (invokeOperation6 = Executor.MethodCall.TryCreate<string, string, string, string, bool>(method, parameters)) == null)
					{
						invokeOperation6 = Executor.MethodCall.TryCreate<object, object, object, object, bool>(method, parameters) ?? Executor.MethodCall.TryCreate<object, object, object, object, string>(method, parameters);
					}
					invokeOperation = invokeOperation6;
					break;
				}
				}
				object staticMethods2 = Executor.MethodCall.StaticMethods;
				lock (staticMethods2)
				{
					Executor.MethodCall.StaticMethods[method] = invokeOperation;
				}
				return invokeOperation;
			}

			private static Executor.InvokeOperation TryCreateInstanceMethod(MethodInfo method)
			{
				if (method.DeclaringType == null)
				{
					return null;
				}
				Executor.InvokeOperation invokeOperation = null;
				object instanceMethods = Executor.MethodCall.InstanceMethods;
				lock (instanceMethods)
				{
					if (Executor.MethodCall.InstanceMethods.TryGetValue(method, out invokeOperation))
					{
						return invokeOperation;
					}
				}
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[] array = null;
				object instanceMethodCreators = Executor.MethodCall.InstanceMethodCreators;
				lock (instanceMethodCreators)
				{
					if (!Executor.MethodCall.InstanceMethodCreators.TryGetValue(method.DeclaringType, out array) || array == null)
					{
						goto IL_FB;
					}
				}
				MethodCallSignature methodCallSignature = new MethodCallSignature(method, true);
				if (array.Length >= methodCallSignature.Count && array[methodCallSignature.Count] != null)
				{
					Dictionary<MethodCallSignature, Executor.InvokeOperationCreator> dictionary = array[methodCallSignature.Count];
					Executor.InvokeOperationCreator invokeOperationCreator = null;
					object obj = dictionary;
					lock (obj)
					{
						if (!dictionary.TryGetValue(methodCallSignature, out invokeOperationCreator) || invokeOperationCreator == null)
						{
							goto IL_FB;
						}
					}
					invokeOperation = invokeOperationCreator(method, method.GetParameters());
				}
				IL_FB:
				object instanceMethods2 = Executor.MethodCall.InstanceMethods;
				lock (instanceMethods2)
				{
					Executor.MethodCall.InstanceMethods[method] = invokeOperation;
				}
				return invokeOperation;
			}

			public static void RegisterInstanceMethod<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>()
			{
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[] array = null;
				object instanceMethodCreators = Executor.MethodCall.InstanceMethodCreators;
				lock (instanceMethodCreators)
				{
					if (!Executor.MethodCall.InstanceMethodCreators.TryGetValue(typeof(InstanceT), out array))
					{
						array = (Executor.MethodCall.InstanceMethodCreators[typeof(InstanceT)] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[4]);
					}
				}
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator> dictionary = array[3];
				if (dictionary == null)
				{
					dictionary = (array[3] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>());
				}
				MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), string.Empty, typeof(Arg2T), string.Empty, typeof(Arg3T), string.Empty, typeof(ResultT));
				object obj = dictionary;
				lock (obj)
				{
					dictionary[methodCallSignature] = new Executor.InvokeOperationCreator(Executor.MethodCall.TryCreate<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>);
				}
			}

			public static void RegisterInstanceMethod<InstanceT, Arg1T, Arg2T, ResultT>()
			{
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[] array = null;
				object instanceMethodCreators = Executor.MethodCall.InstanceMethodCreators;
				lock (instanceMethodCreators)
				{
					if (!Executor.MethodCall.InstanceMethodCreators.TryGetValue(typeof(InstanceT), out array))
					{
						array = (Executor.MethodCall.InstanceMethodCreators[typeof(InstanceT)] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[4]);
					}
				}
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator> dictionary = array[2];
				if (dictionary == null)
				{
					dictionary = (array[2] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>());
				}
				MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), string.Empty, typeof(Arg2T), string.Empty, typeof(ResultT));
				object obj = dictionary;
				lock (obj)
				{
					dictionary[methodCallSignature] = new Executor.InvokeOperationCreator(Executor.MethodCall.TryCreate<InstanceT, Arg1T, Arg2T, ResultT>);
				}
			}

			public static void RegisterInstanceMethod<InstanceT, Arg1T, ResultT>()
			{
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[] array = null;
				object instanceMethodCreators = Executor.MethodCall.InstanceMethodCreators;
				lock (instanceMethodCreators)
				{
					if (!Executor.MethodCall.InstanceMethodCreators.TryGetValue(typeof(InstanceT), out array))
					{
						array = (Executor.MethodCall.InstanceMethodCreators[typeof(InstanceT)] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[4]);
					}
				}
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator> dictionary = array[1];
				if (dictionary == null)
				{
					dictionary = (array[1] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>());
				}
				MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(Arg1T), string.Empty, typeof(ResultT));
				object obj = dictionary;
				lock (obj)
				{
					dictionary[methodCallSignature] = new Executor.InvokeOperationCreator(Executor.MethodCall.TryCreate<InstanceT, Arg1T, ResultT>);
				}
			}

			public static void RegisterInstanceMethod<InstanceT, ResultT>()
			{
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[] array = null;
				object instanceMethodCreators = Executor.MethodCall.InstanceMethodCreators;
				lock (instanceMethodCreators)
				{
					if (!Executor.MethodCall.InstanceMethodCreators.TryGetValue(typeof(InstanceT), out array))
					{
						array = (Executor.MethodCall.InstanceMethodCreators[typeof(InstanceT)] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[4]);
					}
				}
				Dictionary<MethodCallSignature, Executor.InvokeOperationCreator> dictionary = array[0];
				if (dictionary == null)
				{
					dictionary = (array[0] = new Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>());
				}
				MethodCallSignature methodCallSignature = new MethodCallSignature(typeof(ResultT));
				object obj = dictionary;
				lock (obj)
				{
					dictionary[methodCallSignature] = new Executor.InvokeOperationCreator(Executor.MethodCall.TryCreate<InstanceT, ResultT>);
				}
			}

			private static Executor.InvokeOperation TryCreate<ResultT>(MethodInfo method, ParameterInfo[] parameters)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (parameters == null)
				{
					throw new ArgumentNullException("parameters");
				}
				if (parameters.Length != 0 || method.ReturnType != typeof(ResultT))
				{
					return null;
				}
				Executor.MethodCall methodCall = new Executor.MethodCall(typeof(Func<ResultT>), method);
				if (parameters.Length == 2147483647)
				{
					methodCall.FuncInvoker<ResultT>(null, null);
					((Func<ResultT>)null)();
				}
				return new Executor.InvokeOperation(methodCall.FuncInvoker<ResultT>);
			}

			private static Executor.InvokeOperation TryCreate<Arg1T, ResultT>(MethodInfo method, ParameterInfo[] parameters)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (parameters == null)
				{
					throw new ArgumentNullException("parameters");
				}
				if (parameters.Length != 1 || method.ReturnType != typeof(ResultT) || parameters[0].ParameterType != typeof(Arg1T))
				{
					return null;
				}
				Executor.MethodCall methodCall = new Executor.MethodCall(typeof(Func<Arg1T, ResultT>), method);
				if (parameters.Length == 2147483647)
				{
					methodCall.FuncInvoker<Arg1T, ResultT>(null, null);
					((Func<Arg1T, ResultT>)null)(default(Arg1T));
				}
				return new Executor.InvokeOperation(methodCall.FuncInvoker<Arg1T, ResultT>);
			}

			private static Executor.InvokeOperation TryCreate<Arg1T, Arg2T, ResultT>(MethodInfo method, ParameterInfo[] parameters)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (parameters == null)
				{
					throw new ArgumentNullException("parameters");
				}
				if (parameters.Length != 2 || method.ReturnType != typeof(ResultT) || parameters[0].ParameterType != typeof(Arg1T) || parameters[1].ParameterType != typeof(Arg2T))
				{
					return null;
				}
				Executor.MethodCall methodCall = new Executor.MethodCall(typeof(Func<Arg1T, Arg2T, ResultT>), method);
				if (parameters.Length == 2147483647)
				{
					methodCall.FuncInvoker<Arg1T, Arg2T, ResultT>(null, null);
					((Func<Arg1T, Arg2T, ResultT>)null)(default(Arg1T), default(Arg2T));
				}
				return new Executor.InvokeOperation(methodCall.FuncInvoker<Arg1T, Arg2T, ResultT>);
			}

			private static Executor.InvokeOperation TryCreate<Arg1T, Arg2T, Arg3T, ResultT>(MethodInfo method, ParameterInfo[] parameters)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (parameters == null)
				{
					throw new ArgumentNullException("parameters");
				}
				if (parameters.Length != 3 || method.ReturnType != typeof(ResultT) || parameters[0].ParameterType != typeof(Arg1T) || parameters[1].ParameterType != typeof(Arg2T) || parameters[2].ParameterType != typeof(Arg3T))
				{
					return null;
				}
				Executor.MethodCall methodCall = new Executor.MethodCall(typeof(Func<Arg1T, Arg2T, Arg3T, ResultT>), method);
				if (parameters.Length == 2147483647)
				{
					methodCall.FuncInvoker<Arg1T, Arg2T, Arg3T, ResultT>(null, null);
					((Func<Arg1T, Arg2T, Arg3T, ResultT>)null)(default(Arg1T), default(Arg2T), default(Arg3T));
				}
				return new Executor.InvokeOperation(methodCall.FuncInvoker<Arg1T, Arg2T, Arg3T, ResultT>);
			}

			private static Executor.InvokeOperation TryCreate<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(MethodInfo method, ParameterInfo[] parameters)
			{
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (parameters == null)
				{
					throw new ArgumentNullException("parameters");
				}
				if (parameters.Length != 4 || method.ReturnType != typeof(ResultT) || parameters[0].ParameterType != typeof(Arg1T) || parameters[1].ParameterType != typeof(Arg2T) || parameters[2].ParameterType != typeof(Arg3T) || parameters[3].ParameterType != typeof(Arg4T))
				{
					return null;
				}
				Executor.MethodCall methodCall = new Executor.MethodCall(typeof(Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>), method);
				if (parameters.Length == 2147483647)
				{
					methodCall.FuncInvoker<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(null, null);
					((Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>)null)(default(Arg1T), default(Arg2T), default(Arg3T), default(Arg4T));
				}
				return new Executor.InvokeOperation(methodCall.FuncInvoker<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>);
			}

			private static readonly Dictionary<MethodInfo, Executor.InvokeOperation> StaticMethods = new Dictionary<MethodInfo, Executor.InvokeOperation>();

			private static readonly Dictionary<MethodInfo, Executor.InvokeOperation> InstanceMethods = new Dictionary<MethodInfo, Executor.InvokeOperation>();

			private static readonly Dictionary<Type, Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[]> InstanceMethodCreators = new Dictionary<Type, Dictionary<MethodCallSignature, Executor.InvokeOperationCreator>[]>();

			private readonly Delegate fn;
		}
	}
}
