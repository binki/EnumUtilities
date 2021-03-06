﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EnumUtilities
{
    internal static class EnumCompiledCache<T>
        where T : struct, IComparable, IFormattable, IConvertible
    {
        #region Generate Functions

        private static Func<T, T, bool> GenerateHasFlag()
        {
            var value = Expression.Parameter(typeof(T));
            var flag = Expression.Parameter(typeof(T));

            // Convert from Enum to underlying type (byte, int, long, ...)
            // to allow bitwise functions to work
            UnaryExpression valueConverted = Expression.Convert(value, Enum.GetUnderlyingType(typeof(T)));
            UnaryExpression flagConverted = Expression.Convert(flag, Enum.GetUnderlyingType(typeof(T)));

            // (Value & Flag)
            BinaryExpression bitwiseAnd =
                Expression.MakeBinary(
                    ExpressionType.And,
                    valueConverted,
                    flagConverted);

            // (Value & Flag) == Flag
            BinaryExpression hasFlagExpression =
                Expression.MakeBinary(ExpressionType.Equal, bitwiseAnd, flagConverted);

            return Expression.Lambda<Func<T, T, bool>>(hasFlagExpression, value, flag)
                .Compile();
        }

        private static Func<T, T, T> BitwiseOperator(ExpressionType expressionType)
        {
            ParameterExpression leftVal = Expression.Parameter(typeof(T));
            ParameterExpression rightVal = Expression.Parameter(typeof(T));

            // Convert from Enum to Enum's underlying type (byte, int, long, ...)
            // to allow bitwise functions to work
            UnaryExpression leftValConverted = Expression.Convert(leftVal, Enum.GetUnderlyingType(typeof(T)));
            UnaryExpression rightValConverted = Expression.Convert(rightVal, Enum.GetUnderlyingType(typeof(T)));

            // left [expressionType] right
            BinaryExpression binaryExpression =
                Expression.MakeBinary(
                    expressionType,
                    leftValConverted,
                    rightValConverted);

            // Convert back to Enum
            UnaryExpression backToEnumType = Expression.Convert(binaryExpression, typeof(T));
            return Expression.Lambda<Func<T, T, T>>(backToEnumType, leftVal, rightVal)
                .Compile();
        }

        private static Func<T, T, T> GenerateBitwiseOr()
        {
            return BitwiseOperator(ExpressionType.Or);
        }

        private static Func<T, T, T> GenerateBitwiseAnd()
        {
            return BitwiseOperator(ExpressionType.And);
        }

        private static Func<T, T, T> GenerateBitwiseExclusiveOr()
        {
            return BitwiseOperator(ExpressionType.ExclusiveOr);
        }

        #endregion

        public static readonly Func<T, T, T> BitwiseOr = GenerateBitwiseOr();

        public static readonly Func<T, T, T> BitwiseAnd = GenerateBitwiseAnd();

        public static readonly Func<T, T, T> BitwiseExclusiveOr = GenerateBitwiseExclusiveOr();

        public static readonly Func<T, T, bool> HasFlag = GenerateHasFlag();
    }

}
