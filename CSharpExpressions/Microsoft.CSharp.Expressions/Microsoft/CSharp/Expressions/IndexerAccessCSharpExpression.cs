﻿// Prototyping extended expression trees for C#.
//
// bartde - May 2020

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Linq.Expressions.ExpressionStubs;
using LinqError = System.Linq.Expressions.Error;

namespace Microsoft.CSharp.Expressions
{
    /// <summary>
    /// Represents an array access operation.
    /// </summary>
    public sealed partial class IndexerAccessCSharpExpression : CSharpExpression
    {
        internal IndexerAccessCSharpExpression(Expression @object, Expression argument, PropertyInfo lengthOrCount, MemberInfo indexOrSlice)
        {
            Object = @object;
            Argument = argument;
            LengthOrCount = lengthOrCount;
            IndexOrSlice = indexOrSlice;
        }

        /// <summary>
        /// Returns the node type of this <see cref="CSharpExpression" />.
        /// </summary>
        /// <returns>The <see cref="CSharpExpressionType"/> that represents this expression.</returns>
        public override CSharpExpressionType CSharpNodeType => CSharpExpressionType.IndexerAccess;

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression"/>.)
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public override Type Type => IndexOrSlice is PropertyInfo p ? p.PropertyType : ((MethodInfo)IndexOrSlice).ReturnType;

        /// <summary>
        /// Gets the <see cref="Expression" /> that represents the object getting accessed.
        /// </summary>
        public Expression Object { get; }

        /// <summary>
        /// Gets the <see cref="Expression" /> that represents the argument that will be used to index or slice the object.
        /// </summary>
        public Expression Argument { get; }

        /// <summary>
        /// Gets the property used to retrieve the element count of the object getting accessed.
        /// </summary>
        public PropertyInfo LengthOrCount { get; }

        /// <summary>
        /// Gets the member used to index or slice the object.
        /// </summary>
        public MemberInfo IndexOrSlice { get; }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Following the visitor pattern from System.Linq.Expressions.")]
        protected internal override Expression Accept(CSharpExpressionVisitor visitor)
        {
            return visitor.VisitIndexerAccess(this);
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.
        /// </summary>
        /// <param name="object">The <see cref="Object" /> property of the result.</param>
        /// <param name="indexes">The <see cref="Argument" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public IndexerAccessCSharpExpression Update(Expression @object, Expression argument)
        {
            if (@object == Object && argument == Argument)
            {
                return this;
            }

            return CSharpExpression.IndexerAccess(@object, argument, LengthOrCount, IndexOrSlice);
        }

        /// <summary>
        /// Reduces the expression node to a simpler expression.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce()
        {
            return Argument.Type == typeof(Index) ? ReduceIndex() : ReduceRange();

            Expression ReduceIndex()
            {
                var temps = new List<ParameterExpression>();
                var stmts = new List<Expression>();

                //
                // NB: If Object has a value type, we need to access it using a ref local, which is not supported in System.Linq.Expressions.
                //     We should call the RuntimeOpsEx.WithByRef helper to achieve this.
                //

                var isByRef = Object.Type.IsValueType;

                Expression obj = isByRef ? Expression.Parameter(Object.Type.MakeByRefType(), "__obj") : GetObjectExpression(temps, stmts);

                // NB: We always need to evaluate Length first to have a consistent evaluation order regardless of optimizations.

                var length = Expression.Parameter(typeof(int), "__len");

                var expr = GetIndexOffset(Argument, length, out var useLength);

                var index = Expression.MakeIndex(obj, (PropertyInfo)IndexOrSlice, new[] { expr });

                if (useLength)
                {
                    var count = Expression.Property(obj, LengthOrCount);

                    temps.Add(length);
                    stmts.Add(Expression.Assign(length, count));
                }

                stmts.Add(index);

                var res = Helpers.Comma(temps, stmts);

                if (isByRef)
                {
                    var method = typeof(RuntimeOpsEx).GetMethod(nameof(RuntimeOpsEx.WithByRef));
                    method = method.MakeGenericMethod(Object.Type, Type);
                    var delegateType = typeof(FuncByRef<,>).MakeGenericType(Object.Type, Type);

                    // NB: The introduction of a lambda to lift the computation to the WithByRef helper method can be
                    //     expensive because of closure creation. This scenario with mutable structs and indexers should
                    //     be quite rare though.

                    res = Expression.Call(method, Object, Expression.Lambda(delegateType, res, (ParameterExpression)obj));
                }

                return res;
            }

            Expression ReduceRange()
            {
                var temps = new List<ParameterExpression>();
                var stmts = new List<Expression>();

                //
                // NB: There's no bug here, even if GetObjectExpression results in a copy of Object being create if it has a value type. The Roslyn
                //     compiler also creates a copy, so mutation by Count or Slice is not visible in the original storage location of Object.
                //

                var obj = GetObjectExpression(temps, stmts);

                Expression sliceStartArg, sliceSizeArg;

                if (Argument is RangeCSharpExpression range)
                {
                    //
                    // Deconstruct a start..end range expression to try to optimize common cases.
                    //

                    var length = Expression.Parameter(typeof(int), "__len");
                    var useLength = false;

                    Expression startExpr;

                    if (range.Left == null)
                    {
                        //  ..*  ->  0

                        startExpr = Expression.Constant(0);
                    }
                    else
                    {
                        //  i..*  (where i is Index)  ->  range.Start.GetOffset(length)
                        //  x..*  (where x is int)    ->  x
                        // ^x..*  (where x is int)    ->  length - x

                        startExpr = GetIndexOffset(range.Left, length, out useLength);
                    }

                    Expression endExpr;

                    if (range.Right == null)
                    {
                        //  *..  ->  length

                        useLength = true;
                        endExpr = length;
                    }
                    else
                    {
                        // *..i   (where i is Index)  ->  range.End.GetOffset(length)
                        // *..x   (where x is int)    ->  x
                        // *..^x  (where x is int)    ->  length - x

                        endExpr = GetIndexOffset(range.Right, length, out var useLengthTmp);
                        useLength |= useLengthTmp;
                    }

                    //
                    // If length is needed, spill it, so it gets evaluated first and only once (e.g. before calculating start).
                    //
                    // NB: The Roslyn compiler always spills, even though in some cases the single use of length could be
                    //     inlined into the Slice argument expression. We follow suit for now.
                    //

                    if (useLength)
                    {
                        temps.Add(length);
                        stmts.Add(Expression.Assign(length, Expression.Property(obj, LengthOrCount)));
                    }

                    //
                    // We may not have to spill start if it's pure or trivial.
                    //

                    var start = Expression.Parameter(typeof(int), "__start");
                    var useStart = false;

                    //
                    // Calculate the size and try to optimize when possible.
                    //

                    Expression sizeExpr;

                    if (startExpr is ConstantExpression { Value: 0 })
                    {
                        //  ..x    ->  x - 0 == x

                        sizeExpr = endExpr;
                    }
                    else if (startExpr is ConstantExpression startConst && endExpr is ConstantExpression endConst)
                    {
                        // a..b    ->  (b - a)

                        sizeExpr = Expression.Constant((int)endConst.Value - (int)startConst.Value);
                    }
                    else if (startExpr is BinaryExpression { NodeType: ExpressionType.Subtract } s && s.Left == length &&
                             endExpr is BinaryExpression { NodeType: ExpressionType.Subtract } e && e.Left == length &&
                             Helpers.IsPure(s.Right, readOnly: true))
                    {
                        //
                        // ^x..^y  ->  (length - y) - (length - x) == x - y
                        //
                        // Because length is a variable, we can cancel out the subtraction.
                        //
                        // NB: We need the right operand `y` of the start subtraction expression to be pure, so we can safely duplicate
                        //     the use of it across startExpr and sizeExpr. As a bonus, we get to reorder `y` and `x` to land `x - y`.
                        //

                        sizeExpr = MakeSubtract(s.Right, e.Right);
                    }
                    else
                    {
                        //
                        // If start is pure, we don't need to spill it. We can simply reuse it multiple times, e.g. if it's a constant.
                        //
                        // NB: Even if it's a variable, it will be used in a read-only fashion and no intermediate writes can occur.
                        //
                        //     E.g. __obj.Slice(x, length - x)
                        //
                        //     In here, the size argument to Slice cannot have an assignment to the variable, so x is stable across.
                        //

                        if (Helpers.IsPure(startExpr, readOnly: true))
                        {
                            sizeExpr = MakeSubtract(endExpr, startExpr);
                        }
                        else
                        {
                            sizeExpr = MakeSubtract(endExpr, start);
                            useStart = true;
                        }
                    }

                    if (useStart)
                    {
                        temps.Add(start);
                        stmts.Add(Expression.Assign(start, startExpr));

                        startExpr = start;
                    }

                    sliceStartArg = startExpr;
                    sliceSizeArg = sizeExpr;
                }
                else
                {
                    // int length = obj.Length;

                    var length = Expression.Parameter(typeof(int), "__len");

                    temps.Add(length);
                    stmts.Add(Expression.Assign(length, Expression.Property(obj, LengthOrCount)));

                    // Range range = argumentExpr;

                    var rng = Expression.Parameter(typeof(Range), "__rng");

                    temps.Add(rng);
                    stmts.Add(Expression.Assign(rng, Argument));

                    // int start = range.Start.GetOffset(length)

                    var start = Expression.Parameter(typeof(int), "__start");
                    var startExpr = Expression.Call(Expression.Property(rng, StartProperty), GetOffsetMethod, length);

                    temps.Add(start);
                    stmts.Add(Expression.Assign(start, startExpr));

                    // receiver.Slice(start, range.End.GetOffset(length) - start)

                    sliceStartArg = start;

                    var endExpr = Expression.Call(Expression.Property(rng, EndProperty), GetOffsetMethod, length);
                    sliceSizeArg = MakeSubtract(endExpr, start);

                    // NB: The Roslyn compiler generates a local for the size argument. It's unclear why that's needed.
                }

                stmts.Add(GetSliceCall(obj, sliceStartArg, sliceSizeArg));

                return Helpers.Comma(temps, stmts);
            }
        }

        internal Expression ReduceAssign(Func<Expression, Expression> assign)
        {
            if (Argument.Type == typeof(Range))
            {
                throw ContractUtils.Unreachable;
            }

            var temps = new List<ParameterExpression>();
            var stmts = new List<Expression>();

            var obj = GetObjectExpression(temps, stmts);

            var length = Expression.Parameter(typeof(int), "__len");

            var expr = GetIndexOffset(Argument, length, out var useLength);

            if (useLength)
            {
                temps.Add(length);
                stmts.Add(Expression.Assign(length, Expression.Property(obj, LengthOrCount)));
            }

            var indexVar = Expression.Parameter(expr.Type, "__idx");

            temps.Add(indexVar);
            stmts.Add(Expression.Assign(indexVar, expr));

            var index = Expression.MakeIndex(obj, (PropertyInfo)IndexOrSlice, new[] { indexVar });

            stmts.Add(assign(index));

            return Helpers.Comma(temps, stmts);
        }

        internal static Expression GetIndexOffset(Expression index, Expression length, out bool useLength)
        {
            Debug.Assert(index.Type == typeof(Index));

            switch (index)
            {
                case ConstantExpression ce:
                    var indexValue = (Index)ce.Value;
                    if (indexValue.IsFromEnd)
                    {
                        useLength = true;
                        return MakeSubtract(length, Expression.Constant(indexValue.Value));
                    }
                    else
                    {
                        useLength = false;
                        return Expression.Constant(indexValue.Value);
                    }
                case DefaultExpression _:
                    useLength = false;
                    return Expression.Constant(0); // NB: This simplifies optimization.
                case UnaryExpression ue when ue.NodeType == ExpressionType.Convert && ue.Operand.Type == typeof(int):
                    useLength = false;
                    return MakeConstantIfDefault(ue.Operand);
                case FromEndIndexCSharpExpression fe when fe.Operand.Type == typeof(int):
                    useLength = true;
                    return MakeSubtract(length, fe.Operand);
                default:
                    useLength = true;
                    return Expression.Call(index, GetOffsetMethod, length);
            }
        }

        private Expression GetObjectExpression(List<ParameterExpression> variables, List<Expression> statements)
        {
            return GetObjectExpression((type, name) =>
            {
                var variable = Expression.Parameter(type, name);
                variables.Add(variable);
                return variable;
            }, statements);
        }

        private Expression GetObjectExpression(Func<Type, string, ParameterExpression> makeVariable, List<Expression> statements)
        {
            var obj = Object;

            if (!Helpers.IsPure(obj))
            {
                var objVariable = makeVariable(Object.Type, "__obj");

                statements.Add(Expression.Assign(objVariable, Object));

                obj = objVariable;
            }

            return obj;
        }

        private Expression GetSliceCall(Expression obj, Expression start, Expression size)
        {
            return Expression.Call(obj, (MethodInfo)IndexOrSlice, start, size);
        }

        private static Expression MakeSubtract(Expression left, Expression right)
        {
            Debug.Assert(left.Type == typeof(int));
            Debug.Assert(right.Type == typeof(int));

            left = MakeConstantIfDefault(left);
            right = MakeConstantIfDefault(right);

            if (left is ConstantExpression l && right is ConstantExpression r)
            {
                return Expression.Constant((int)l.Value - (int)r.Value);
            }

            return Expression.Subtract(left, right);
        }

        private static Expression MakeConstantIfDefault(Expression e) => e is DefaultExpression ? Expression.Constant(0) : e;

        private static MethodInfo s_getOffset;
        private static MethodInfo GetOffsetMethod => s_getOffset ??= typeof(Index).GetNonGenericMethod(nameof(System.Index.GetOffset), BindingFlags.Public | BindingFlags.Instance, new[] { typeof(int) });

        private static PropertyInfo s_start;
        private static PropertyInfo StartProperty => s_start ??= typeof(Range).GetProperty(nameof(System.Range.Start), BindingFlags.Public | BindingFlags.Instance);

        private static PropertyInfo s_end;
        private static PropertyInfo EndProperty => s_end ??= typeof(Range).GetProperty(nameof(System.Range.End), BindingFlags.Public | BindingFlags.Instance);
    }

    partial class CSharpExpression
    {
        /// <summary>
        /// Creates an expression representing an indexer access operation.
        /// </summary>
        /// <param name="object">The object to access.</param>
        /// <param name="argument">The argument that will be used to index or slice the object.</param>
        /// <param name="lengthOrCount">The property used to retrieve the element count of the object getting accessed.</param>
        /// <param name="indexOrSlice">The member used to index or slice the object.</param>
        /// <returns>A new <see cref="IndexerAccessCSharpExpression"/> instance representing the array access operation.</returns>
        public static IndexerAccessCSharpExpression IndexerAccess(Expression @object, Expression argument, MethodInfo lengthOrCount, MemberInfo indexOrSlice)
        {
            var property = lengthOrCount != null ? GetProperty(lengthOrCount) : null;

            return IndexerAccess(@object, argument, property, indexOrSlice);
        }

        /// <summary>
        /// Creates an expression representing an indexer access operation.
        /// </summary>
        /// <param name="object">The object to access.</param>
        /// <param name="argument">The argument that will be used to index or slice the object.</param>
        /// <param name="lengthOrCount">The property used to retrieve the element count of the object getting accessed.</param>
        /// <param name="indexOrSlice">The member used to index or slice the object.</param>
        /// <returns>A new <see cref="IndexerAccessCSharpExpression"/> instance representing the array access operation.</returns>
        public static IndexerAccessCSharpExpression IndexerAccess(Expression @object, Expression argument, PropertyInfo lengthOrCount, MemberInfo indexOrSlice)
        {
            RequiresCanRead(@object, nameof(@object));

            //
            // The argument can be of type Index or Range. We'll check indexOrSlice accordingly below.
            //

            RequiresCanRead(argument, nameof(argument));

            if (argument.Type != typeof(Index) && argument.Type != typeof(Range))
            {
                throw Error.InvalidIndexerAccessArgumentType(argument.Type);
            }

            //
            // A type is Countable if it has a property named Length or Count with an accessible getter and a return type of int.
            //

            lengthOrCount ??= FindCountProperty("Length") ?? FindCountProperty("Count");

            PropertyInfo FindCountProperty(string name) => @object.Type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance, binder: null, typeof(int), Type.EmptyTypes, modifiers: null);

            ContractUtils.RequiresNotNull(lengthOrCount, nameof(lengthOrCount));

            var lengthOrCountGetMethod = lengthOrCount.GetGetMethod(nonPublic: true); // NB: System.Linq.Expressions allows non-public properties.

            if (lengthOrCountGetMethod == null)
            {
                throw LinqError.PropertyDoesNotHaveAccessor(lengthOrCount);
            }

            if (lengthOrCountGetMethod.IsStatic)
            {
                throw Error.AccessorCannotBeStatic(lengthOrCountGetMethod);
            }

            if (lengthOrCountGetMethod.GetParametersCached().Length != 0)
            {
                throw LinqError.IncorrectNumberOfMethodCallArguments(lengthOrCountGetMethod);
            }

            if (!TypeUtils.IsValidInstanceType(lengthOrCount, @object.Type))
            {
                throw LinqError.PropertyNotDefinedForType(lengthOrCount, @object.Type);
            }

            if (lengthOrCount.PropertyType != typeof(int))
            {
                throw Error.InvalidLengthOrCountPropertyType(lengthOrCount);
            }

            ValidateMethodInfo(lengthOrCountGetMethod);

            if (argument.Type == typeof(Index))
            {
                //
                // The type has an accessible instance indexer which takes a single int as the argument.
                //

                indexOrSlice ??= FindIndexer();

                PropertyInfo FindIndexer()
                {
                    var indexers = (from p in @object.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    let i = p.GetIndexParameters()
                                    where i.Length == 1 && i[0].ParameterType == typeof(int)
                                    select p)
                                   .ToArray();

                    return indexers.Length == 1 ? indexers[0] : null;
                }

                ContractUtils.RequiresNotNull(indexOrSlice, nameof(indexOrSlice));

                var index = indexOrSlice as PropertyInfo ?? GetProperty(indexOrSlice as MethodInfo ?? throw Error.InvalidIndexMember(indexOrSlice));

                indexOrSlice = index; // NB: Store the property rather than a method.

                var indexAccessor = index.GetGetMethod(nonPublic: true); // NB: System.Linq.Expressions allows non-public properties.

                if (indexAccessor == null)
                {
                    indexAccessor = index.GetSetMethod(nonPublic: true) ?? throw LinqError.PropertyDoesNotHaveAccessor(indexOrSlice);

                    if (indexAccessor.GetParametersCached().Length != 2)
                    {
                        throw LinqError.IncorrectNumberOfMethodCallArguments(indexAccessor);
                    }
                }
                else if (indexAccessor.GetParametersCached().Length != 1)
                {
                    throw LinqError.IncorrectNumberOfMethodCallArguments(indexAccessor);
                }

                if (indexAccessor.IsStatic)
                {
                    throw Error.AccessorCannotBeStatic(indexAccessor);
                }

                if (!TypeUtils.IsValidInstanceType(indexAccessor, @object.Type))
                {
                    throw LinqError.PropertyNotDefinedForType(indexAccessor, @object.Type);
                }

                if (indexAccessor.GetParametersCached()[0].ParameterType != typeof(int))
                {
                    throw Error.InvalidIndexerParameterType(indexOrSlice);
                }

                ValidateMethodInfo(indexAccessor);
            }
            else
            {
                //
                // The type has an accessible member named Slice which has two parameters of type int.
                //

                Debug.Assert(argument.Type == typeof(Range));

                indexOrSlice ??= FindSliceMethod();

                MethodInfo FindSliceMethod() => @object.Type.GetMethod(@object.Type == typeof(string) ? "Substring" : "Slice", BindingFlags.Public | BindingFlags.Instance, binder: null, new[] { typeof(int), typeof(int) }, modifiers: null);

                ContractUtils.RequiresNotNull(indexOrSlice, nameof(indexOrSlice));

                var slice = indexOrSlice as MethodInfo ?? throw Error.InvalidSliceMember(indexOrSlice);

                ValidateMethodInfo(slice);

                if (slice.IsStatic)
                {
                    throw Error.SliceMethodMustNotBeStatic(slice);
                }

                ValidateCallInstanceType(@object.Type, slice);

                var sliceParams = slice.GetParametersCached();

                if (sliceParams.Length != 2 || sliceParams[0].ParameterType != typeof(int) || sliceParams[1].ParameterType != typeof(int))
                {
                    throw Error.InvalidSliceParameters(slice);
                }
            }

            return new IndexerAccessCSharpExpression(@object, argument, lengthOrCount, indexOrSlice);
        }
    }

    partial class CSharpExpressionVisitor
    {
        /// <summary>
        /// Visits the children of the <see cref="IndexerAccessCSharpExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Following the visitor pattern from System.Linq.Expressions.")]
        protected internal virtual Expression VisitIndexerAccess(IndexerAccessCSharpExpression node)
        {
            return node.Update(Visit(node.Object), Visit(node.Argument));
        }
    }
}
