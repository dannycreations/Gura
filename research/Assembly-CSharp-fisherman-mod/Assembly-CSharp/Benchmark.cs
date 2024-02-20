using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions;
using GameDevWare.Dynamic.Expressions.CSharp;
using UnityEngine;

public class Benchmark : MonoBehaviour
{
	internal void Awake()
	{
		Debug.Log(SystemInfo.processorType);
	}

	internal void OnGUI()
	{
		float num = 500f;
		float num2 = (float)Screen.height - 40f;
		GUILayout.BeginArea(new Rect(((float)Screen.width - num) / 2f, ((float)Screen.height - num2) / 2f, num, num2));
		GUILayout.BeginVertical(new GUILayoutOption[0]);
		if (GUILayout.Button("Measure Expression Performance", new GUILayoutOption[0]))
		{
			new Action(this.MeasureExpressionPerformance).BeginInvoke(null, null);
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void MeasureExpressionPerformance()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			CSharpExpression.Evaluate<double>("(2 * (2 + 3) << 1 - 1 & 7 | 25 ^ 10) + System.Int32.Parse(\"10\")", null);
			IEnumerable<Token> enumerable = Tokenizer.Tokenize("(2 * (2 + 3) << 1 - 1 & 7 | 25 ^ 10) + System.Int32.Parse(\"10\")");
			stopwatch.Reset();
			stopwatch.Start();
			for (int i = 0; i < 100000; i++)
			{
				Tokenizer.Tokenize("(2 * (2 + 3) << 1 - 1 & 7 | 25 ^ 10) + System.Int32.Parse(\"10\")");
			}
			stopwatch.Stop();
			TimeSpan elapsed = stopwatch.Elapsed;
			ParseTreeNode parseTreeNode = Parser.Parse(enumerable);
			stopwatch.Reset();
			stopwatch.Start();
			for (int j = 0; j < 100000; j++)
			{
				Parser.Parse(enumerable);
			}
			stopwatch.Stop();
			TimeSpan elapsed2 = stopwatch.Elapsed;
			Binder binder = new Binder(new ParameterExpression[0], typeof(double), null);
			LambdaExpression lambdaExpression = binder.Bind(parseTreeNode.ToSyntaxTree(true), null);
			stopwatch.Reset();
			stopwatch.Start();
			for (int k = 0; k < 100000; k++)
			{
				binder.Bind(parseTreeNode.ToSyntaxTree(true), null);
			}
			stopwatch.Stop();
			TimeSpan elapsed3 = stopwatch.Elapsed;
			Expression<Func<double>> expression = (Expression<Func<double>>)lambdaExpression;
			Func<double> func = expression.Compile();
			stopwatch.Reset();
			stopwatch.Start();
			for (int l = 0; l < 100000; l++)
			{
				expression = (Expression<Func<double>>)lambdaExpression;
				expression.Compile();
			}
			stopwatch.Stop();
			TimeSpan elapsed4 = stopwatch.Elapsed;
			expression = (Expression<Func<double>>)lambdaExpression;
			Func<double> func2 = expression.CompileAot(true);
			stopwatch.Reset();
			stopwatch.Start();
			for (int m = 0; m < 100000; m++)
			{
				expression = (Expression<Func<double>>)lambdaExpression;
				expression.CompileAot(true);
			}
			stopwatch.Stop();
			TimeSpan elapsed5 = stopwatch.Elapsed;
			func2();
			stopwatch.Reset();
			stopwatch.Start();
			for (int n = 0; n < 100000; n++)
			{
				func2();
			}
			stopwatch.Stop();
			TimeSpan elapsed6 = stopwatch.Elapsed;
			TimeSpan timeSpan = elapsed + elapsed2 + elapsed3 + elapsed5 + elapsed6;
			func();
			stopwatch.Reset();
			stopwatch.Start();
			for (int num = 0; num < 100000; num++)
			{
				func();
			}
			stopwatch.Stop();
			TimeSpan elapsed7 = stopwatch.Elapsed;
			TimeSpan timeSpan2 = elapsed + elapsed2 + elapsed3 + elapsed4 + elapsed7;
			Debug.Log(string.Format("Tokenization: {0:F2} | {1:F5} | {2:F1}%", elapsed.TotalMilliseconds, elapsed.TotalMilliseconds / 100000.0, elapsed.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Parsing: {0:F2} | {1:F5} {2:F1}%", elapsed2.TotalMilliseconds, elapsed2.TotalMilliseconds / 100000.0, elapsed2.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Binding: {0:F2} | {1:F5} {2:F1}%", elapsed3.TotalMilliseconds, elapsed3.TotalMilliseconds / 100000.0, elapsed3.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Compilation (JIT): {0:F2} | {1:F5} {2:F1}%", elapsed4.TotalMilliseconds, elapsed4.TotalMilliseconds / 100000.0, elapsed4.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Compilation (AOT): {0:F2} | {1:F5} {2:F1}%", elapsed5.TotalMilliseconds, elapsed5.TotalMilliseconds / 100000.0, elapsed5.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Evaluation (AOT): {0:F2} | {1:F5} {2:F1}%", elapsed6.TotalMilliseconds, elapsed6.TotalMilliseconds / 100000.0, elapsed6.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Total (AOT): {0:F2} | {1:F5} {2:F1}%", timeSpan.TotalMilliseconds, timeSpan.TotalMilliseconds / 100000.0, timeSpan.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Evaluation (JIT): {0:F2} | {1:F5} {2:F1}%", elapsed7.TotalMilliseconds, elapsed7.TotalMilliseconds / 100000.0, elapsed7.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
			Debug.Log(string.Format("Total (JIT): {0:F2} | {1:F5} {2:F1}%", timeSpan2.TotalMilliseconds, timeSpan2.TotalMilliseconds / 100000.0, timeSpan2.TotalMilliseconds / timeSpan2.TotalMilliseconds * 100.0));
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
	}

	private const int Iterations = 100000;
}
