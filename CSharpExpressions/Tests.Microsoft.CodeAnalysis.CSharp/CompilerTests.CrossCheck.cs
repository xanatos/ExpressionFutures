﻿// Prototyping extended expression trees for C#.
//
// bartde - December 2015

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;

namespace Tests.Microsoft.CodeAnalysis.CSharp
{
    // NB: The tests cross-check the outcome of evaluating a lambda expression - specified as a string in the
    //     test cases - in two ways. First, by converting the lambda expression to a delegate type and running
    //     IL code produced by the compiler. Second, by converting the lambda expression to an expression tree
    //     using the extended expression tree support in our modified Roslyn build and compiling the expression
    //     at runtime (therefore invoking our Reduce methods).
    //
    //     It is assumed that the outcome has proper equality defined (i.e. EqualityComparer<T>.Default should
    //     return a meaningful equality comparer to assert evaluation outcomes against each other). If the
    //     evaluation results in an exception, its type is cross-checked.
    //
    //     In addition to cross-checking the evaluation outcome, a log is maintained and cross-checked, which
    //     is useful to assert the order of side-effects. The code fragments can write to this log by means of
    //     the Log method and the Return method (to prepend returning a value of type T with a logging side-
    //     effect).

    partial class CompilerTests
    {
        #region C# 3.0

        [TestMethod]
        public void CrossCheck_Arithmetic()
        {
            var f = Compile<Func<int>>("() => Return(1) + Return(2)");
            f();
        }

        #endregion

        #region Conditional access

        [TestMethod]
        public void CrossCheck_NullConditional()
        {
            var f = Compile<Func<string, int?>>("s => s?.Length");
            f("bar");
            f(null);
        }

        #endregion

        #region Named parameters

        [TestMethod]
        public void CrossCheck_NamedParameters()
        {
            var f = Compile<Func<int>>(@"() =>
{
    var b = new StrongBox<int>(1);
    Log(b.Value);
    return System.Threading.Interlocked.Exchange(value: Return(42), location1: ref b.Value);
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_NamedParameters_ByRef1()
        {
            var f = Compile<Func<int[], int>>(@"xs =>
{
    return Utils.NamedParamByRef(y: Return(42), x: ref xs[0]);
}");
            f(new[] { 17 });
            AssertEx.Throws<IndexOutOfRangeException>(() => f(new int[0]));
            AssertEx.Throws<NullReferenceException>(() => f(null));
        }

        [TestMethod]
        public void CrossCheck_NamedParameters_ByRef2()
        {
            var f = Compile<Func<StrongBox<int>, int>>(@"b =>
{
    return Utils.NamedParamByRef(y: Return(42), x: ref b.Value);
}");
            f(new StrongBox<int>(17));
            AssertEx.Throws<NullReferenceException>(() => f(null));
        }

        #endregion

        #region For

        [TestMethod]
        public void CrossCheck_For()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    for (var i = Return(0); Return(i < 10); Return(i++))
    {
        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        #endregion

        #region Foreach

        [TestMethod]
        public void CrossCheck_ForEach1()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    foreach (var i in Enumerable.Range(Return(0), Return(10)))
    {
        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_ForEach2()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    foreach (var c in ""0123456789"")
    {
        var i = int.Parse(Return(c.ToString()));

        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_ForEach3()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    foreach (var i in from x in Enumerable.Range(Return(0), Return(10)) where Return(x) % Return(2) == Return(0) select Return(x))
    {
        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_ForEach4()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    foreach (var i in new[] { Return(0), 1, 2, 3, Return(4), 5, 6, 7, Return(8) })
    {
        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_ForEach5()
        {
            var f = Compile<Action>(@"() =>
{
    Log(""Before"");

    foreach (int i in new object[] { Return(0), 1, 2, 3, Return(4), 5, 6, 7, Return(8) })
    {
        if (i == 2)
        {
            Log(""continue"");
            continue;
        }

        if (i == 5)
        {
            Log(""break"");
            break;
        }

        Log($""body({i})"");
    }

    Log(""After"");
}");
            f();
        }

        #endregion

        #region Async

        [TestMethod]
        public void CrossCheck_Async1()
        {
            var f = Compile<Func<int>>(@"() =>
{
    return Await(async () =>
    {
        Log(""A"");
    
        await Task.Yield();
    
        Log(""B"");
    
        return 42;
    });
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_Async2()
        {
            var f = Compile<Func<int>>(@"() =>
{
    return Await(async () =>
    {
        Log(""A"");
    
        var res = Return(1) + await Task.FromResult(Return(41));
    
        Log(""B"");
    
        return res;
    });
}");
            f();
        }

        [TestMethod]
        public void CrossCheck_Async3()
        {
            var f = Compile<Func<int>>(@"() =>
{
    return Await(async () =>
    {
        Log(""A"");
    
        var res = await Task.FromResult(Return(41)) + Return(1);
    
        Log(""B"");
    
        return res;
    });
}");
            f();
        }

        #endregion

        #region Assignment

        [TestMethod]
        public void CrossCheck_CompoundAssignment()
        {
            var f = Compile<Func<int, int>>(@"i =>
{
    var b = new StrongBox<int>(i);
    Log(b.Value);
    var res = b.Value += Return(1);
    Log(res);
    return b.Value;
}");
            f(0);
            f(41);
        }

        [TestMethod] // See https://github.com/dotnet/corefx/issues/4984 for a relevant discussion
        public void CrossCheck_Issue4984_Binary_Repro1()
        {
            var f = Compile<Func<int, int>>(@"i =>
{
    var b = new WeakBox<int>();
    Log(b.Value);
    var res = b.Value += Return(1);
    Log(res);
    return b.Value;
}");
            f(0);
            f(41);
        }

        [TestMethod] // See https://github.com/dotnet/corefx/issues/4984 for a relevant discussion
        public void CrossCheck_Issue4984_Binary_Repro2()
        {
            var f = Compile<Func<int, int>>(@"i =>
{
    var b = new WeakBox<int>();
    Log(b[0]);
    var res = b[0] += Return(1);
    Log(res);
    return b[0];
}");
            f(0);
            f(41);
        }

        [TestMethod] // See https://github.com/dotnet/corefx/issues/4984 for a relevant discussion
        public void CrossCheck_Issue4984_Unary_Repro1()
        {
            var f = Compile<Func<int, int>>(@"i =>
{
    var b = new WeakBox<int>();
    Log(b.Value);
    var res = b.Value++;
    Log(res);
    return b.Value;
}");
            f(0);
            f(41);
        }

        [TestMethod] // See https://github.com/dotnet/corefx/issues/4984 for a relevant discussion
        public void CrossCheck_Issue4984_Unary_Repro2()
        {
            var f = Compile<Func<int, int>>(@"i =>
{
    var b = new WeakBox<int>();
    Log(b[0]);
    var res = b[0]++;
    Log(res);
    return b[0];
}");
            f(0);
            f(41);
        }

        #endregion
    }
}
