using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Common.Collections
{
    public static class Extensions
    {
        public
        static
        bool
        Contains<T>(
            this
            IEnumerable<T> superset,
            IEnumerable<T> subset)
        {
            return new HashSet<T>(superset)
                .IsSupersetOf(subset);
        }

        public
        static
        T
        Second<T>(
            this 
            IEnumerable<T> sequence)
        {
            return sequence
                .Skip(1)
                .Single();
        }

        public
        static
        IEnumerable<Func<T, T2>>
        CompileAll<T, T2>(
            this Expression<Func<T, T2>>[] expressions)
        {
            return expressions.Select(expression => expression.Compile());
        }

        public
        static
        string
        GetMemberName<T, T2>(
            this Expression<Func<T, T2>> expression)
        {
            return expression
                .GetMemberInfo()
                .Member
                .Name;
        }

        public
        static
        MemberExpression
        GetMemberInfo(
            this
            Expression method)
        {
            var lambda = method as LambdaExpression;

            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }
    }
}
