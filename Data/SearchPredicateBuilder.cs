using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace Sammlerplattform.Data
{
    public class SearchPredicateBuilder
    {
        public static ExpressionStarter<T>? BuildPredicate<T>(object searchModel)
        {
            ExpressionStarter<T> predicate = PredicateBuilder.New<T>();
            PropertyInfo[] modelProperties = searchModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in modelProperties)
            {
                object? value = prop.GetValue(searchModel);
                if (value == null)
                {
                    continue;
                }
                string columnName = prop.Name.Replace("_", ".");

                switch (value)
                {
                    case ICollection<int> intValue when intValue.Count != 0:
                        predicate = predicate.And(CreateLambdaSpanIntJoin<T>(columnName, intValue));
                        break;
                    case ICollection<string> strList when strList.Count != 0:
                        predicate = columnName.Contains("List") || columnName.Contains("List")
                            ? (ExpressionStarter<T>)predicate.And(CreateLambdaAnyDeepContains<T>(columnName, strList))
                            : (ExpressionStarter<T>)predicate.And(CreateLambdaStringContainsJoin<T>(columnName, strList));
                        break;
                    case "on":
                    case true:
                        predicate = predicate.Or(CreateLambdaBool<T>(columnName));
                        break;
                    case ICollection<DateTime> dateValue when dateValue.Count != 0:
                        predicate = predicate.And(CreateLambdaSpanDateTimeJoin<T>(columnName, dateValue));
                        break;
                    case ICollection<decimal> decimalValue when decimalValue.Count != 0:
                        predicate = predicate.And(CreateLambdaSpanDecimalJoin<T>(columnName, decimalValue));
                        break;
                }
            }

            return predicate.IsStarted ? predicate : null;
        }

        private static Expression<Func<T, bool>> CreateLambdaBool<T>(string propertyName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            MemberExpression property = Expression.Property(parameter, propertyName);
            ConstantExpression constant = Expression.Constant(true, typeof(bool));
            BinaryExpression equal = Expression.Equal(property, constant);
            return Expression.Lambda<Func<T, bool>>(equal, parameter);
        }

        private static Expression<Func<T, bool>> CreateLambdaStringContainsJoin<T>(string propertyPath, ICollection<string> values)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            Expression? property = parameter;
            foreach (string part in propertyPath.Split('.'))
            {
                property = Expression.PropertyOrField(property!, part);
            }

            MethodInfo containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;

            Expression? combined = null;
            foreach (string value in values)
            {
                ConstantExpression constant = Expression.Constant(value, typeof(string));
                MethodCallExpression contains = Expression.Call(property!, containsMethod, constant);

                combined = combined == null ? contains : Expression.OrElse(combined, contains);
            }

            return Expression.Lambda<Func<T, bool>>(combined!, parameter);
        }

        public static Expression<Func<T, bool>> CreateLambdaAnyDeepContains<T>(string path, ICollection<string> searchValues)
        {
            if (searchValues == null || !searchValues.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                return PredicateBuilder.New<T>();
            }

            ParameterExpression outerParam = Expression.Parameter(typeof(T), "x");

            Expression? body = null;
            foreach (string? val in searchValues.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                Expression partExpr = BuildAnyLambda(outerParam, path.Split('.'), val);
                body = body == null ? partExpr : Expression.OrElse(body, partExpr);
            }

            return body == null ? (Expression<Func<T, bool>>)PredicateBuilder.New<T>() : Expression.Lambda<Func<T, bool>>(body, outerParam);
        }

        private static Expression BuildAnyLambda(Expression source, string[] pathParts, string searchValue)
        {
            Type currentType = source.Type;

            PropertyInfo prop = currentType.GetProperty(pathParts[0])
                ?? throw new ArgumentException($"'{pathParts[0]}' is not a member of type '{currentType.Name}'");

            Expression propExpr = Expression.Property(source, prop);

            // Collection?
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                prop.PropertyType != typeof(string))
            {
                Type elementType = prop.PropertyType.GetGenericArguments().First();

                ParameterExpression innerParam = Expression.Parameter(elementType, "e");
                Expression innerBody = BuildAnyLambda(innerParam, [.. pathParts.Skip(1)], searchValue);

                LambdaExpression anyLambda = Expression.Lambda(innerBody, innerParam);

                MethodInfo anyMethod = typeof(Enumerable)
                    .GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(elementType);

                return Expression.Call(anyMethod, propExpr, anyLambda);
            }
            else
            {
                if (pathParts.Length == 1)
                {
                    // Fallback: ToString().Contains(searchValue)
                    MethodInfo toStringMethod = typeof(object).GetMethod("ToString")!;
                    Expression toStringCall = Expression.Call(propExpr, toStringMethod);

                    MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
                    return Expression.Call(toStringCall, containsMethod, Expression.Constant(searchValue));
                }
                else
                {
                    // Weiter runter
                    return BuildAnyLambda(propExpr, [.. pathParts.Skip(1)], searchValue);
                }
            }
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanIntJoin<T>(string columnName, ICollection<int> searchValues)
        {
            ExpressionStarter<T> predicate = PredicateBuilder.New<T>();

            if (IsICollectionIntValid(searchValues))
            {
                ParameterExpression pe = Expression.Parameter(typeof(T), "s");

                MemberExpression property = GetNestedProperty(pe, columnName);
                MethodInfo? method = searchValues.GetType().GetMethod("Equals");
                if (method == null)
                {
                    return predicate;
                }

                if (searchValues.Count == 1)
                {
                    ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                    UnaryExpression converted = Expression.Convert(constant, typeof(object));
                    MethodCallExpression call = Expression.Call(property, method, converted);
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(call, pe);
                    predicate = predicate.Or(lambda);
                }
                else if (searchValues.Count == 2)
                {
                    ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                    ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                    predicate = Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(
                            GreaterThanOrEqual(property, constant0),
                            LessThanOrEqual(property, constant1)),
                                [pe]);
                }
                else if (searchValues.Count >= 3)
                {
                    foreach (int value in searchValues)
                    {
                        ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                        UnaryExpression converted = Expression.Convert(constant, typeof(object));
                        MethodCallExpression call = Expression.Call(property, method, converted);
                        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(call, pe);
                        predicate = predicate.Or(lambda);
                    }
                }
            }

            return predicate;
        }
        public static Expression<Func<T, bool>> CreateLambdaSpanDateTimeJoin<T>(string columnName, ICollection<DateTime> searchValues)
        {
            ExpressionStarter<T> predicate = PredicateBuilder.New<T>();

            if (IsICollectionDateTimeValid(searchValues))
            {
                ParameterExpression pe = Expression.Parameter(typeof(T), "s");
                MemberExpression property = Expression.Property(pe, columnName);

                if (searchValues.Count == 1)
                {
                    ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                    predicate = Expression.Lambda<Func<T, bool>>(Equal(property, constant), [pe]);
                }
                else if (searchValues.Count == 2)
                {
                    ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                    ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                    predicate = Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(
                            GreaterThanOrEqual(property, constant0),
                            LessThanOrEqual(property, constant1)),
                                [pe]);
                }
            }
            return predicate;
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanDecimalJoin<T>(string columnName, ICollection<decimal> searchValues)
        {
            ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();

            if (IsICollectionDecimalValid(searchValues))
            {
                ParameterExpression pe = Expression.Parameter(typeof(T), "s");
                MemberExpression property = Expression.Property(pe, columnName);

                if (searchValues.Count == 1)
                {
                    ConstantExpression constant = Expression.Constant(searchValues.ToArray()[0]);
                    predicateloc = Expression.Lambda<Func<T, bool>>(Equal(property, constant), [pe]);
                }
                else if (searchValues.Count == 2)
                {
                    ConstantExpression constant0 = Expression.Constant(searchValues.ToArray()[0]);
                    ConstantExpression constant1 = Expression.Constant(searchValues.ToArray()[1]);
                    predicateloc = Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(
                            GreaterThanOrEqual(property, constant0),
                            LessThanOrEqual(property, constant1)),
                                [pe]);
                }
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

        private static bool IsICollectionIntValid(ICollection<int> collection)
        {
            if (collection == null)
            {
                return false;
            }
            else if (collection.Count == 0)
            {
                return false;
            }

            return true;
        }
        private static bool IsICollectionStringValid(ICollection<string> collection)
        {
            if (collection == null)
            {
                return false;
            }
            else if (collection.Count == 0)
            {
                return false;
            }

            return true;
        }
        private static bool IsICollectionDateTimeValid(ICollection<DateTime> collection)
        {
            if (collection == null)
            {
                return false;
            }
            else if (collection.Count == 0)
            {
                return false;
            }

            return true;
        }
        private static bool IsICollectionDecimalValid(ICollection<decimal> collection)
        {
            if (collection == null)
            {
                return false;
            }
            else if (collection.Count == 0)
            {
                return false;
            }

            return true;
        }
        private static Expression GetNestedPropertyExpression(Expression parameter, string[] pathParts)
        {
            Expression current = parameter;
            foreach (string part in pathParts)
            {
                PropertyInfo property = current.Type.GetProperty(part) ?? throw new ArgumentException($"'{part}' is not a member of type '{current.Type.Name}'");
                current = Expression.Property(current, property);
            }

            return current;
        }
        public static MemberExpression GetNestedProperty(Expression parameter, string propertyPath)
        {
            Expression current = parameter;
            foreach (string member in propertyPath.Split('.'))
            {
                current = Expression.Property(current, member);
            }
            return (MemberExpression)current;
        }
    }
}
