﻿// Prototyping extended expression trees for C#.
//
// bartde - November 2015

// NB: Running these tests can take a *VERY LONG* time because it invokes the C# compiler for every test
//     case in order to obtain an expression tree object. Be patient when running these tests.

// NB: These tests are generated from a list of expressions in the .tt file by invoking the C# compiler at
//     text template processing time by the T4 engine. See TestUtilities for the helper functions that call
//     into the compiler, load the generated assembly, extract the Expression objects through reflection on
//     the generated type, and call DebugView() on those. The resulting DebugView string is emitted in this
//     file as `expected` variables. The original expression is escaped and gets passed ot the GetDebugView
//     helper method to obtain `actual`, which causes the C# compiler to run at test execution time, using
//     the same helper library, thus obtaining the DebugView string again. This serves a number of goals:
//
//       1. At test generation time, a custom Roslyn build can be invoked to test the implicit conversion
//          of a lambda expression to an expression tree, which involves the changes made to support the
//          C# expression library in this solution. Any fatal compiler errors will come out at that time.
//
//       2. Reflection on the properties in the emitted class causes a deferred execution of the factory
//          method calls generated by the Roslyn compiler for the implicit conversion of the lambda to an
//          expression tree. Any exceptions thrown by the factory methods will show up as well during test
//          generation time, allowing issues to be uncovered.
//
//       3. The string literals in the `expected` variables are inspectable by a human to assert that the
//          compiler has indeed generated an expression representation that's homo-iconic to the original
//          expression that was provided in the test.
//
//       4. Any changes to the compiler or the runtime library could cause regressions. Because template
//          processing of the T4 only takes place upon editing the .tt file, the generated test code won't
//          change. As such, any regression can cause test failures which allows to detect any changes to
//          compiler or runtime library behavior.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.Microsoft.CodeAnalysis.CSharp.TestUtilities;

namespace Tests.Microsoft.CodeAnalysis.CSharp
{
    [TestClass]
    public partial class CompilerTests_CSharp80_IndexRange
    {
        [TestMethod]
        public void CompilerTest_102D_8041()
        {
            // (Expression<Func<Index>>)(() =>  1)
            var actual = GetDebugView(@"(Expression<Func<Index>>)(() =>  1)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Index]"">
  <Parameters />
  <Body>
    <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
      <Operand>
        <Constant Type=""System.Int32"" Value=""1"" />
      </Operand>
    </Convert>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_102D_8041();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_102D_8041() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_F139_15B7()
        {
            // (Expression<Func<Index>>)(() => ^1)
            var actual = GetDebugView(@"(Expression<Func<Index>>)(() => ^1)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Index]"">
  <Parameters />
  <Body>
    <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
      <Operand>
        <Constant Type=""System.Int32"" Value=""1"" />
      </Operand>
    </CSharpFromEndIndex>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_F139_15B7();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_F139_15B7() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_A48D_CEC8()
        {
            // (Expression<Func<int , Index>>) (i =>  i)
            var actual = GetDebugView(@"(Expression<Func<int , Index>>) (i =>  i)");
            var expected = @"
<Lambda Type=""System.Func`2[System.Int32,System.Index]"">
  <Parameters>
    <Parameter Type=""System.Int32"" Id=""0"" Name=""i"" />
  </Parameters>
  <Body>
    <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
      <Operand>
        <Parameter Type=""System.Int32"" Id=""0"" Name=""i"" />
      </Operand>
    </Convert>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_A48D_CEC8();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_A48D_CEC8() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_1228_4EE4()
        {
            // (Expression<Func<int,  Index>>) (i => ^i)
            var actual = GetDebugView(@"(Expression<Func<int,  Index>>) (i => ^i)");
            var expected = @"
<Lambda Type=""System.Func`2[System.Int32,System.Index]"">
  <Parameters>
    <Parameter Type=""System.Int32"" Id=""0"" Name=""i"" />
  </Parameters>
  <Body>
    <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
      <Operand>
        <Parameter Type=""System.Int32"" Id=""0"" Name=""i"" />
      </Operand>
    </CSharpFromEndIndex>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_1228_4EE4();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_1228_4EE4() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_7287_5EFC()
        {
            // (Expression<Func<int?, Index?>>)(i =>  i)
            var actual = GetDebugView(@"(Expression<Func<int?, Index?>>)(i =>  i)");
            var expected = @"
<Lambda Type=""System.Func`2[System.Nullable`1[System.Int32],System.Nullable`1[System.Index]]"">
  <Parameters>
    <Parameter Type=""System.Nullable`1[System.Int32]"" Id=""0"" Name=""i"" />
  </Parameters>
  <Body>
    <Convert Type=""System.Nullable`1[System.Index]"" Method=""System.Index op_Implicit(Int32)"" IsLifted=""true"" IsLiftedToNull=""true"">
      <Operand>
        <Parameter Type=""System.Nullable`1[System.Int32]"" Id=""0"" Name=""i"" />
      </Operand>
    </Convert>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_7287_5EFC();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_7287_5EFC() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_F0CB_598D()
        {
            // (Expression<Func<int?, Index?>>)(i => ^i)
            var actual = GetDebugView(@"(Expression<Func<int?, Index?>>)(i => ^i)");
            var expected = @"
<Lambda Type=""System.Func`2[System.Nullable`1[System.Int32],System.Nullable`1[System.Index]]"">
  <Parameters>
    <Parameter Type=""System.Nullable`1[System.Int32]"" Id=""0"" Name=""i"" />
  </Parameters>
  <Body>
    <CSharpFromEndIndex Type=""System.Nullable`1[System.Index]"" Method=""Void .ctor(Int32, Boolean)"">
      <Operand>
        <Parameter Type=""System.Nullable`1[System.Int32]"" Id=""0"" Name=""i"" />
      </Operand>
    </CSharpFromEndIndex>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_F0CB_598D();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_F0CB_598D() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_16C2_7E40()
        {
            // (Expression<Func<Range>>)(() =>   ..  )
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>   ..  )");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range get_All()"" />
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_16C2_7E40();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_16C2_7E40() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_9DAB_53E1()
        {
            // (Expression<Func<Range>>)(() =>  1..  )
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>  1..  )");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range StartAt(System.Index)"">
      <Left>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </Convert>
      </Left>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_9DAB_53E1();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_9DAB_53E1() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_311C_4ADD()
        {
            // (Expression<Func<Range>>)(() => ^1..  )
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() => ^1..  )");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range StartAt(System.Index)"">
      <Left>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Left>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_311C_4ADD();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_311C_4ADD() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_0C19_6AD1()
        {
            // (Expression<Func<Range>>)(() =>   .. 2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>   .. 2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range EndAt(System.Index)"">
      <Right>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </Convert>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_0C19_6AD1();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_0C19_6AD1() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_1972_BC5D()
        {
            // (Expression<Func<Range>>)(() =>   ..^2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>   ..^2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range EndAt(System.Index)"">
      <Right>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </CSharpFromEndIndex>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_1972_BC5D();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_1972_BC5D() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_6D28_52A3()
        {
            // (Expression<Func<Range>>)(() =>  1.. 2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>  1.. 2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </Convert>
      </Left>
      <Right>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </Convert>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_6D28_52A3();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_6D28_52A3() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_2322_4FD7()
        {
            // (Expression<Func<Range>>)(() => ^1.. 2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() => ^1.. 2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Left>
      <Right>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </Convert>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_2322_4FD7();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_2322_4FD7() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_96F6_B646()
        {
            // (Expression<Func<Range>>)(() =>  1..^2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() =>  1..^2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </Convert>
      </Left>
      <Right>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </CSharpFromEndIndex>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_96F6_B646();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_96F6_B646() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_4A83_E78F()
        {
            // (Expression<Func<Range>>)(() => ^1..^2)
            var actual = GetDebugView(@"(Expression<Func<Range>>)(() => ^1..^2)");
            var expected = @"
<Lambda Type=""System.Func`1[System.Range]"">
  <Parameters />
  <Body>
    <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Left>
      <Right>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""2"" />
          </Operand>
        </CSharpFromEndIndex>
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_4A83_E78F();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_4A83_E78F() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_2C8C_A2E0()
        {
            // (Expression<Func<Index, Index, Range>>)((l, r) => l..)
            var actual = GetDebugView(@"(Expression<Func<Index, Index, Range>>)((l, r) => l..)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Index,System.Index,System.Range]"">
  <Parameters>
    <Parameter Type=""System.Index"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Index"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range StartAt(System.Index)"">
      <Left>
        <Parameter Type=""System.Index"" Id=""0"" Name=""l"" />
      </Left>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_2C8C_A2E0();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_2C8C_A2E0() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_F408_CCEE()
        {
            // (Expression<Func<Index, Index, Range>>)((l, r) =>  ..r)
            var actual = GetDebugView(@"(Expression<Func<Index, Index, Range>>)((l, r) =>  ..r)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Index,System.Index,System.Range]"">
  <Parameters>
    <Parameter Type=""System.Index"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Index"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Range"" Method=""System.Range EndAt(System.Index)"">
      <Right>
        <Parameter Type=""System.Index"" Id=""1"" Name=""r"" />
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_F408_CCEE();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_F408_CCEE() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_903C_C65A()
        {
            // (Expression<Func<Index, Index, Range>>)((l, r) => l..r)
            var actual = GetDebugView(@"(Expression<Func<Index, Index, Range>>)((l, r) => l..r)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Index,System.Index,System.Range]"">
  <Parameters>
    <Parameter Type=""System.Index"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Index"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <Parameter Type=""System.Index"" Id=""0"" Name=""l"" />
      </Left>
      <Right>
        <Parameter Type=""System.Index"" Id=""1"" Name=""r"" />
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_903C_C65A();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_903C_C65A() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_E7FC_AC00()
        {
            // (Expression<Func<Index?, Index?, Range?>>)((l, r) => l..)
            var actual = GetDebugView(@"(Expression<Func<Index?, Index?, Range?>>)((l, r) => l..)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Nullable`1[System.Index],System.Nullable`1[System.Index],System.Nullable`1[System.Range]]"">
  <Parameters>
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Nullable`1[System.Range]"" Method=""System.Range StartAt(System.Index)"">
      <Left>
        <Parameter Type=""System.Nullable`1[System.Index]"" Id=""0"" Name=""l"" />
      </Left>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_E7FC_AC00();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_E7FC_AC00() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_3718_C5C5()
        {
            // (Expression<Func<Index?, Index?, Range?>>)((l, r) =>  ..r)
            var actual = GetDebugView(@"(Expression<Func<Index?, Index?, Range?>>)((l, r) =>  ..r)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Nullable`1[System.Index],System.Nullable`1[System.Index],System.Nullable`1[System.Range]]"">
  <Parameters>
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Nullable`1[System.Range]"" Method=""System.Range EndAt(System.Index)"">
      <Right>
        <Parameter Type=""System.Nullable`1[System.Index]"" Id=""1"" Name=""r"" />
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_3718_C5C5();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_3718_C5C5() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_126E_F2C3()
        {
            // (Expression<Func<Index?, Index?, Range?>>)((l, r) => l..r)
            var actual = GetDebugView(@"(Expression<Func<Index?, Index?, Range?>>)((l, r) => l..r)");
            var expected = @"
<Lambda Type=""System.Func`3[System.Nullable`1[System.Index],System.Nullable`1[System.Index],System.Nullable`1[System.Range]]"">
  <Parameters>
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""0"" Name=""l"" />
    <Parameter Type=""System.Nullable`1[System.Index]"" Id=""1"" Name=""r"" />
  </Parameters>
  <Body>
    <CSharpRange Type=""System.Nullable`1[System.Range]"" Method=""Void .ctor(System.Index, System.Index)"">
      <Left>
        <Parameter Type=""System.Nullable`1[System.Index]"" Id=""0"" Name=""l"" />
      </Left>
      <Right>
        <Parameter Type=""System.Nullable`1[System.Index]"" Id=""1"" Name=""r"" />
      </Right>
    </CSharpRange>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_126E_F2C3();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_126E_F2C3() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_838B_FA89()
        {
            // (Expression<Func<int[], int>>)    (xs => xs[^1])
            var actual = GetDebugView(@"(Expression<Func<int[], int>>)    (xs => xs[^1])");
            var expected = @"
<Lambda Type=""System.Func`2[System.Int32[],System.Int32]"">
  <Parameters>
    <Parameter Type=""System.Int32[]"" Id=""0"" Name=""xs"" />
  </Parameters>
  <Body>
    <CSharpArrayAccess Type=""System.Int32"">
      <Array>
        <Parameter Type=""System.Int32[]"" Id=""0"" Name=""xs"" />
      </Array>
      <Indexes>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Indexes>
    </CSharpArrayAccess>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_838B_FA89();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_838B_FA89() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_E7F0_C224()
        {
            // (Expression<Func<List<int>, int>>)(xs => xs[^1])
            var actual = GetDebugView(@"(Expression<Func<List<int>, int>>)(xs => xs[^1])");
            var expected = @"
<Lambda Type=""System.Func`2[System.Collections.Generic.List`1[System.Int32],System.Int32]"">
  <Parameters>
    <Parameter Type=""System.Collections.Generic.List`1[System.Int32]"" Id=""0"" Name=""xs"" />
  </Parameters>
  <Body>
    <CSharpIndexerAccess Type=""System.Int32"" LengthOrCount=""Int32 Count"" IndexOrSlice=""Int32 Item [Int32]"">
      <Object>
        <Parameter Type=""System.Collections.Generic.List`1[System.Int32]"" Id=""0"" Name=""xs"" />
      </Object>
      <Argument>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Argument>
    </CSharpIndexerAccess>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_E7F0_C224();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_E7F0_C224() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_CF23_3FC2()
        {
            // (Expression<Func<string, char>>)  ( s =>  s[^1])
            var actual = GetDebugView(@"(Expression<Func<string, char>>)  ( s =>  s[^1])");
            var expected = @"
<Lambda Type=""System.Func`2[System.String,System.Char]"">
  <Parameters>
    <Parameter Type=""System.String"" Id=""0"" Name=""s"" />
  </Parameters>
  <Body>
    <CSharpIndexerAccess Type=""System.Char"" LengthOrCount=""Int32 Length"" IndexOrSlice=""Char Chars [Int32]"">
      <Object>
        <Parameter Type=""System.String"" Id=""0"" Name=""s"" />
      </Object>
      <Argument>
        <CSharpFromEndIndex Type=""System.Index"" Method=""Void .ctor(Int32, Boolean)"">
          <Operand>
            <Constant Type=""System.Int32"" Value=""1"" />
          </Operand>
        </CSharpFromEndIndex>
      </Argument>
    </CSharpIndexerAccess>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_CF23_3FC2();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_CF23_3FC2() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_3669_DF79()
        {
            // (Expression<Func<int[], int[]>>)  (xs => xs[1..2])
            var actual = GetDebugView(@"(Expression<Func<int[], int[]>>)  (xs => xs[1..2])");
            var expected = @"
<Lambda Type=""System.Func`2[System.Int32[],System.Int32[]]"">
  <Parameters>
    <Parameter Type=""System.Int32[]"" Id=""0"" Name=""xs"" />
  </Parameters>
  <Body>
    <CSharpArrayAccess Type=""System.Int32[]"">
      <Array>
        <Parameter Type=""System.Int32[]"" Id=""0"" Name=""xs"" />
      </Array>
      <Indexes>
        <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
          <Left>
            <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
              <Operand>
                <Constant Type=""System.Int32"" Value=""1"" />
              </Operand>
            </Convert>
          </Left>
          <Right>
            <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
              <Operand>
                <Constant Type=""System.Int32"" Value=""2"" />
              </Operand>
            </Convert>
          </Right>
        </CSharpRange>
      </Indexes>
    </CSharpArrayAccess>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_3669_DF79();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_3669_DF79() => INCONCLUSIVE(); }

        [TestMethod]
        public void CompilerTest_3AF4_3ADE()
        {
            // (Expression<Func<string, string>>)( s =>  s[1..2])
            var actual = GetDebugView(@"(Expression<Func<string, string>>)( s =>  s[1..2])");
            var expected = @"
<Lambda Type=""System.Func`2[System.String,System.String]"">
  <Parameters>
    <Parameter Type=""System.String"" Id=""0"" Name=""s"" />
  </Parameters>
  <Body>
    <CSharpIndexerAccess Type=""System.String"" LengthOrCount=""Int32 Length"" IndexOrSlice=""System.String Substring(Int32, Int32)"">
      <Object>
        <Parameter Type=""System.String"" Id=""0"" Name=""s"" />
      </Object>
      <Argument>
        <CSharpRange Type=""System.Range"" Method=""Void .ctor(System.Index, System.Index)"">
          <Left>
            <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
              <Operand>
                <Constant Type=""System.Int32"" Value=""1"" />
              </Operand>
            </Convert>
          </Left>
          <Right>
            <Convert Type=""System.Index"" Method=""System.Index op_Implicit(Int32)"">
              <Operand>
                <Constant Type=""System.Int32"" Value=""2"" />
              </Operand>
            </Convert>
          </Right>
        </CSharpRange>
      </Argument>
    </CSharpIndexerAccess>
  </Body>
</Lambda>";
            Assert.AreEqual(expected.TrimStart('\r', '\n'), actual);
            Verify.CompilerTest_3AF4_3ADE();
        }

        partial class Review { /* override in .Verify.cs */ public virtual void CompilerTest_3AF4_3ADE() => INCONCLUSIVE(); }

        partial class Review
        {
            protected void INCONCLUSIVE() { Assert.Inconclusive(); }
        }

        partial class Reviewed : Review
        {
            private void OK() { }
            private void FAIL(string message = "") { Assert.Fail(message); }
        }

        private readonly Reviewed Verify = new Reviewed();
    }

/*
// NB: The code generated below accepts all tests. *DON'T* just copy/paste this to the .Verify.cs file
//     but review the tests one by one. This output is included in case a minor change is made to debug
//     output produced by DebugView() and all hashes are invalidated. In that case, this output can be
//     copied and pasted into .Verify.cs.

namespace Tests.Microsoft.CodeAnalysis.CSharp
{
    partial class CompilerTests_CSharp80_IndexRange
    {
        partial class Reviewed
        {
            public override void CompilerTest_102D_8041() => OK();
            public override void CompilerTest_F139_15B7() => OK();
            public override void CompilerTest_A48D_CEC8() => OK();
            public override void CompilerTest_1228_4EE4() => OK();
            public override void CompilerTest_7287_5EFC() => OK();
            public override void CompilerTest_F0CB_598D() => OK();
            public override void CompilerTest_16C2_7E40() => OK();
            public override void CompilerTest_9DAB_53E1() => OK();
            public override void CompilerTest_311C_4ADD() => OK();
            public override void CompilerTest_0C19_6AD1() => OK();
            public override void CompilerTest_1972_BC5D() => OK();
            public override void CompilerTest_6D28_52A3() => OK();
            public override void CompilerTest_2322_4FD7() => OK();
            public override void CompilerTest_96F6_B646() => OK();
            public override void CompilerTest_4A83_E78F() => OK();
            public override void CompilerTest_2C8C_A2E0() => OK();
            public override void CompilerTest_F408_CCEE() => OK();
            public override void CompilerTest_903C_C65A() => OK();
            public override void CompilerTest_E7FC_AC00() => OK();
            public override void CompilerTest_3718_C5C5() => OK();
            public override void CompilerTest_126E_F2C3() => OK();
            public override void CompilerTest_838B_FA89() => OK();
            public override void CompilerTest_E7F0_C224() => OK();
            public override void CompilerTest_CF23_3FC2() => OK();
            public override void CompilerTest_3669_DF79() => OK();
            public override void CompilerTest_3AF4_3ADE() => OK();
        }
    }
}
*/
}