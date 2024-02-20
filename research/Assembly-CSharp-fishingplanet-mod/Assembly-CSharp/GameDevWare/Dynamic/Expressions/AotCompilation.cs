using System;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions
{
	public static class AotCompilation
	{
		static AotCompilation()
		{
			try
			{
				(() => true).Compile();
			}
			catch (Exception)
			{
				AotCompilation.IsAotCompiled = true;
			}
			if (typeof(Expression).Name == string.Empty)
			{
				Expression.TypeIs(null, null);
				Expression.MakeUnary(ExpressionType.Add, null, null);
				Expression.MakeUnary(ExpressionType.Add, null, null, null);
				Expression.Negate(null);
				Expression.Negate(null, null);
				Expression.UnaryPlus(null);
				Expression.UnaryPlus(null, null);
				Expression.NegateChecked(null);
				Expression.NegateChecked(null, null);
				Expression.Not(null);
				Expression.Not(null, null);
				Expression.TypeAs(null, null);
				Expression.Convert(null, null);
				Expression.Convert(null, null, null);
				Expression.ConvertChecked(null, null);
				Expression.ConvertChecked(null, null, null);
				Expression.ArrayLength(null);
				Expression.Quote(null);
				Expression.ListInit(null, null);
				Expression.ListInit(null, null);
				Expression.Bind(null, null);
				Expression.Bind(null, null);
				Expression.Field(null, null);
				Expression.Field(null, null);
				Expression.Property(null, null);
				Expression.Property(null, null);
				Expression.Property(null, null);
				Expression.PropertyOrField(null, null);
				Expression.MakeMemberAccess(null, null);
				Expression.MemberInit(null, null);
				Expression.MemberInit(null, null);
				Expression.ListBind(null, null);
				Expression.ListBind(null, null);
				Expression.ListBind(null, null);
				Expression.ListBind(null, null);
				Expression.MemberBind(null, null);
				Expression.MemberBind(null, null);
				Expression.MemberBind(null, null);
				Expression.MemberBind(null, null);
				Expression.Call(null, null);
				Expression.Call(null, null);
				Expression.Call(null, null, null);
				Expression.Call(null, null, null);
				Expression.Call(null, null, null, null);
				Expression.Call(null, null, null, null);
				Expression.ArrayIndex(null, null);
				Expression.ArrayIndex(null, null);
				Expression.NewArrayInit(null, null);
				Expression.NewArrayInit(null, null);
				Expression.NewArrayBounds(null, null);
				Expression.NewArrayBounds(null, null);
				Expression.New(null);
				Expression.New(null, null);
				Expression.New(null, null);
				Expression.New(null, null, null);
				Expression.New(null, null, null);
				Expression.New(null);
				Expression.Parameter(null, null);
				Expression.Invoke(null, null);
				Expression.Invoke(null, null);
				Expression.Lambda(null, null);
				Expression.Lambda(null, null, null);
				Expression.Lambda(null, null, null);
				Expression.GetFuncType(null);
				Expression.GetActionType(null);
				Expression.ListInit(null, null);
				Expression.ListInit(null, null);
				Expression.ListInit(null, null, null);
				Expression.ListInit(null, null, null);
				Expression.LeftShift(null, null);
				Expression.LeftShift(null, null, null);
				Expression.RightShift(null, null);
				Expression.RightShift(null, null, null);
				Expression.And(null, null);
				Expression.And(null, null, null);
				Expression.Or(null, null);
				Expression.Or(null, null, null);
				Expression.ExclusiveOr(null, null);
				Expression.ExclusiveOr(null, null, null);
				Expression.Power(null, null);
				Expression.Power(null, null, null);
				Expression.ArrayIndex(null, null);
				Expression.Condition(null, null, null);
				Expression.Constant(null);
				Expression.Constant(null, null);
				Expression.ElementInit(null, null);
				Expression.ElementInit(null, null);
				Expression.MakeBinary(ExpressionType.Add, null, null);
				Expression.MakeBinary(ExpressionType.Add, null, null, false, null);
				Expression.MakeBinary(ExpressionType.Add, null, null, false, null, null);
				Expression.Equal(null, null);
				Expression.Equal(null, null, false, null);
				Expression.NotEqual(null, null);
				Expression.NotEqual(null, null, false, null);
				Expression.GreaterThan(null, null);
				Expression.GreaterThan(null, null, false, null);
				Expression.LessThan(null, null);
				Expression.LessThan(null, null, false, null);
				Expression.GreaterThanOrEqual(null, null);
				Expression.GreaterThanOrEqual(null, null, false, null);
				Expression.LessThanOrEqual(null, null);
				Expression.LessThanOrEqual(null, null, false, null);
				Expression.AndAlso(null, null);
				Expression.AndAlso(null, null, null);
				Expression.OrElse(null, null);
				Expression.OrElse(null, null, null);
				Expression.Coalesce(null, null);
				Expression.Coalesce(null, null, null);
				Expression.Add(null, null);
				Expression.Add(null, null, null);
				Expression.AddChecked(null, null);
				Expression.AddChecked(null, null, null);
				Expression.Subtract(null, null);
				Expression.Subtract(null, null, null);
				Expression.SubtractChecked(null, null);
				Expression.SubtractChecked(null, null, null);
				Expression.Divide(null, null);
				Expression.Divide(null, null, null);
				Expression.Modulo(null, null);
				Expression.Modulo(null, null, null);
				Expression.Multiply(null, null);
				Expression.Multiply(null, null, null);
				Expression.MultiplyChecked(null, null);
				Expression.MultiplyChecked(null, null, null);
			}
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>()
		{
			Executor.RegisterForFastCall<InstanceT, Arg1T, Arg2T, Arg3T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, Arg2T, ResultT>()
		{
			Executor.RegisterForFastCall<InstanceT, Arg1T, Arg2T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, Arg1T, ResultT>()
		{
			Executor.RegisterForFastCall<InstanceT, Arg1T, ResultT>();
		}

		public static void RegisterForFastCall<InstanceT, ResultT>()
		{
			Executor.RegisterForFastCall<InstanceT, ResultT>();
		}

		public static void RegisterFunc<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>()
		{
			if (typeof(AotCompilation).Name == string.Empty)
			{
				Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT> func = Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>>(null, null).CompileAot(false);
				func(default(Arg1T), default(Arg2T), default(Arg3T), default(Arg4T));
				func.DynamicInvoke(new object[]
				{
					default(Arg1T),
					default(Arg2T),
					default(Arg3T),
					default(Arg4T)
				});
				Executor.Prepare<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(null, null);
			}
		}

		public static void RegisterFunc<Arg1T, Arg2T, Arg3T, ResultT>()
		{
			if (typeof(AotCompilation).Name == string.Empty)
			{
				Func<Arg1T, Arg2T, Arg3T, ResultT> func = Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, ResultT>>(null, null).CompileAot(false);
				func(default(Arg1T), default(Arg2T), default(Arg3T));
				func.DynamicInvoke(new object[]
				{
					default(Arg1T),
					default(Arg2T),
					default(Arg3T)
				});
				Executor.Prepare<Arg1T, Arg2T, Arg3T, ResultT>(null, null);
			}
		}

		public static void RegisterFunc<Arg1T, Arg2T, ResultT>()
		{
			if (typeof(AotCompilation).Name == string.Empty)
			{
				Func<Arg1T, Arg2T, ResultT> func = Expression.Lambda<Func<Arg1T, Arg2T, ResultT>>(null, null).CompileAot(false);
				func(default(Arg1T), default(Arg2T));
				func.DynamicInvoke(new object[]
				{
					default(Arg1T),
					default(Arg2T)
				});
				Executor.Prepare<Arg1T, Arg2T, ResultT>(null, null);
			}
		}

		public static void RegisterFunc<Arg1T, ResultT>()
		{
			if (typeof(AotCompilation).Name == string.Empty)
			{
				Func<Arg1T, ResultT> func = Expression.Lambda<Func<Arg1T, ResultT>>(null, null).CompileAot(false);
				func(default(Arg1T));
				func.DynamicInvoke(new object[] { default(Arg1T) });
				Executor.Prepare<Arg1T, ResultT>(null, null);
			}
		}

		public static void RegisterFunc<ResultT>()
		{
			if (typeof(AotCompilation).Name == string.Empty)
			{
				Func<ResultT> func = Expression.Lambda<Func<ResultT>>(null, null).CompileAot(false);
				func();
				func.DynamicInvoke(new object[0]);
				Executor.Prepare<ResultT>(null, null);
			}
		}

		public static readonly bool IsAotCompiled;
	}
}
