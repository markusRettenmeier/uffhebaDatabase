using LinqKit;
using System.Linq.Expressions;

namespace Sammlerplattform.Controllers.GenericClasses
{
    public class GenericLambdas
    {
        public static Expression<Func<T, bool>> CreateLambdaStringContainsJoin<T>(string table, string columnName, ICollection<string> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();
            ParameterExpression eParam = Expression.Parameter(typeof(T), "c");
            System.Reflection.MethodInfo? method = typeof(string).GetMethod("Contains", [typeof(string)]);
            if (method is null)
            {
                return predicateloc;
            }

            foreach (string searchValue in searchValues)
            {
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    MethodCallExpression call = Expression.Call(Expression.Property(Expression.Property(eParam, table), columnName), method, Expression.Constant(searchValue));
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(call, eParam);
                    predicateloc = predicateloc.Or(lambda);
                }
            }

            return predicateloc;
        }

        public static Expression<Func<T, bool>> CreateLambdaStringEqualsJoin<T>(string table, string columnName, ICollection<string> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();
            ParameterExpression pe = Expression.Parameter(typeof(T), "c");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);
            System.Reflection.MethodInfo? method = searchValues.GetType().GetMethod("Equals");
            if (method is null)
            {
                return predicateloc;
            }

            foreach (string searchvalue in searchValues)
            {
                if (!string.IsNullOrWhiteSpace(searchvalue))
                {
                    ConstantExpression constant = Expression.Constant(searchvalue.Trim());
                    MethodCallExpression callPredicate = Expression.Call(constant, method, column);
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(callPredicate, pe);
                    predicateloc = predicateloc.Or(lambda);
                }
            }

            return predicateloc;
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanNullIntJoin<T>(string table, string columnName, ICollection<int> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();
            ParameterExpression pe = Expression.Parameter(typeof(T), "s");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);

            if (searchValues.Count == 1)
            {
                ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                predicateloc = Expression.Lambda<Func<T, bool>>(Equal(column, constant), [pe]);
            }
            else if (searchValues.Count == 2)
            {
                ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                predicateloc = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(
                        GreaterThanOrEqual(column, constant0),
                        LessThanOrEqual(column, constant1)),
                            [pe]);
            }
            else if (searchValues.Count >= 3)
            {
                foreach (int value in searchValues)
                {
                    ConstantExpression constant = Expression.Constant(value);
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(
                        Equal(column, constant), [pe]);
                    predicateloc = predicateloc.Or(lambda);
                }
            }
            return predicateloc;
        }
        public static Expression<Func<T, bool>> CreateLambdaSpanIntJoin<T>(string table, string columnName, ICollection<int> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();
            System.Reflection.MethodInfo? method = searchValues.GetType().GetMethod("Equals");
            if (method == null)
            {
                return predicateloc;
            }

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);

            if (searchValues.Count == 1)
            {
                ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                UnaryExpression converted = Expression.Convert(constant, typeof(object));
                MethodCallExpression callPredicate = Expression.Call(column, method, converted);
                Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(callPredicate, pe);
                predicateloc = predicateloc.Or(lambda);
            }
            else if (searchValues.Count == 2)
            {
                ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                predicateloc = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(
                        GreaterThanOrEqual(column, constant0),
                        LessThanOrEqual(column, constant1)),
                            [pe]);
            }
            else if (searchValues.Count >= 3)
            {
                foreach (int value in searchValues)
                {
                    ConstantExpression constant = Expression.Constant(value);
                    UnaryExpression converted = Expression.Convert(constant, typeof(object));
                    MethodCallExpression callPredicate = Expression.Call(column, method, converted);
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(callPredicate, pe);
                    predicateloc = predicateloc.Or(lambda);
                }
            }
            return predicateloc;
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanDateTimeJoin<T>(string table, string columnName, ICollection<DateTime> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);

            if (searchValues.Count == 1)
            {
                ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                predicateloc = Expression.Lambda<Func<T, bool>>(Equal(column, constant), [pe]);
            }
            else if (searchValues.Count == 2)
            {
                ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                predicateloc = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(
                        GreaterThanOrEqual(column, constant0),
                        LessThanOrEqual(column, constant1)),
                            [pe]);
            }
            return predicateloc;
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanDateTimeYearJoin<T>(string table, string columnName, ICollection<int> searchValues)
        {
            ParameterExpression pe = Expression.Parameter(typeof(T), "s");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);
            string stringDate_Beginning = string.Empty;
            string stringDate_Ending = string.Empty;

            if (searchValues.Count == 1)
            {
                stringDate_Beginning = "01.01." + searchValues.ToArray()[0];
                stringDate_Ending = "31.12." + searchValues.ToArray()[0];
            }
            else if (searchValues.Count == 2)
            {
                stringDate_Beginning = "01.01." + searchValues.ToArray()[0];
                stringDate_Ending = "31.12." + searchValues.ToArray()[1];
            }
            ConstantExpression constant0 = Expression.Constant(Convert.ToDateTime(stringDate_Beginning));
            ConstantExpression constant1 = Expression.Constant(Convert.ToDateTime(stringDate_Ending));
            ExpressionStarter<T> predicateloc = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    GreaterThanOrEqual(column, constant0),
                    LessThanOrEqual(column, constant1)),
                        [pe]);
            return predicateloc;
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanDecimalJoin<T>(string table, string columnName, ICollection<decimal> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");
            MemberExpression column = Expression.Property(Expression.Property(pe, table), columnName);

            if (searchValues.Count == 1)
            {
                ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                predicateloc = Expression.Lambda<Func<T, bool>>(Equal(column, constant), [pe]);
            }
            else if (searchValues.Count == 2)
            {
                ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                predicateloc = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(
                        GreaterThanOrEqual(column, constant0),
                        LessThanOrEqual(column, constant1)),
                            [pe]);
            }
            return predicateloc;
        }

        private static BinaryExpression Equal(Expression memberExpression,
                                   ConstantExpression constantToCompare)
        {
            MemberExpression hasValueExpression = Expression.Property(memberExpression, "HasValue");
            MemberExpression valueExpression = Expression.Property(memberExpression, "Value");
            BinaryExpression notEqual = Expression.Equal(valueExpression, constantToCompare);
            return Expression.AndAlso(hasValueExpression, notEqual);
        }
        private static BinaryExpression GreaterThanOrEqual(Expression memberExpression,
                                   ConstantExpression constantToCompare)
        {
            MemberExpression hasValueExpression = Expression.Property(memberExpression, "HasValue");
            MemberExpression valueExpression = Expression.Property(memberExpression, "Value");
            BinaryExpression notEqual = Expression.GreaterThanOrEqual(valueExpression, constantToCompare);
            return Expression.AndAlso(hasValueExpression, notEqual);
        }
        private static BinaryExpression LessThanOrEqual(Expression memberExpression,
                                   ConstantExpression constantToCompare)
        {
            MemberExpression hasValueExpression = Expression.Property(memberExpression, "HasValue");
            MemberExpression valueExpression = Expression.Property(memberExpression, "Value");
            BinaryExpression notEqual = Expression.LessThanOrEqual(valueExpression, constantToCompare);
            return Expression.AndAlso(hasValueExpression, notEqual);
        }
    }
}
