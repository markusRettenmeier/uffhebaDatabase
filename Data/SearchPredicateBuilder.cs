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
                        predicate = predicate.And(s => EF.Property<bool>(nameof(T), columnName));
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
        //public static Expression<Func<T, bool>> CreateLambdaAnyDeepContains<T>(string path, ICollection<string> searchValues)
        //{
        //    ExpressionStarter<T> predicate = PredicateBuilder.New<T>();
        //    if (searchValues == null || searchValues.Count == 0)
        //    {
        //        return predicate;
        //    }

        //    // Split Property-Pfad
        //    string[] parts = path.Split('.');

        //    // Outer Param (z.B. City)
        //    ParameterExpression outerParam = Expression.Parameter(typeof(T), "x");
        //    Expression current = outerParam;
        //    Type currentType = typeof(T);

        //    // Aufteilen in Pfad vor und nach der Collection
        //    List<string> beforeCollection = [];
        //    List<string> afterCollection = [];
        //    bool foundCollection = false;

        //    for (int i = 0; i < parts.Length; i++)
        //    {
        //        PropertyInfo prop = currentType.GetProperty(parts[i]) ?? throw new ArgumentException($"'{parts[i]}' is not a member of type '{currentType.Name}'");
        //        currentType = prop.PropertyType;

        //        // Collection erkennen
        //        if (!foundCollection &&
        //            typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) &&
        //            currentType != typeof(string))
        //        {
        //            foundCollection = true;
        //            beforeCollection.Add(parts[i]);
        //            afterCollection.AddRange(parts.Skip(i + 1));
        //            break;
        //        }

        //        beforeCollection.Add(parts[i]);
        //    }

        //    if (!foundCollection)
        //    {
        //        throw new ArgumentException($"No collection found in path: {path}");
        //    }

        //    // Navigiere zur Collection
        //    Expression collectionExpr = GetNestedPropertyExpression(outerParam, [.. beforeCollection]);
        //    Type collectionElementType = collectionExpr.Type.GetGenericArguments().First();

        //    // Inner Lambda: e =>
        //    ParameterExpression innerParam = Expression.Parameter(collectionElementType, "e");
        //    Expression innerExpr = GetNestedPropertyExpression(innerParam, [.. afterCollection]);

        //    // Contains-Methode vorbereiten
        //    MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

        //    foreach (string? val in searchValues.Where(v => !string.IsNullOrWhiteSpace(v)))
        //    {
        //        ConstantExpression constant = Expression.Constant(val);
        //        MethodCallExpression containsCall = Expression.Call(innerExpr, containsMethod, constant);
        //        LambdaExpression innerLambda = Expression.Lambda(containsCall, innerParam);

        //        // Any(e => e.Inner.Contains(...))
        //        MethodInfo anyMethod = typeof(Enumerable).GetMethods()
        //            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
        //            .MakeGenericMethod(collectionElementType);

        //        MethodCallExpression anyCall = Expression.Call(anyMethod, collectionExpr, innerLambda);

        //        // Ganze Lambda: x => x.Collection.Any(e => e.Inner.Contains(...))
        //        Expression<Func<T, bool>> finalLambda = Expression.Lambda<Func<T, bool>>(anyCall, outerParam);
        //        predicate = predicate.Or(finalLambda);
        //    }

        //    return predicate;
        //}

        public static Expression<Func<T, bool>> CreateLambdaAnyDeepContains<T>(
    string path,
    ICollection<string> searchValues)
        {
            if (searchValues == null || !searchValues.Any(v => !string.IsNullOrWhiteSpace(v)))
                return PredicateBuilder.New<T>();

            ParameterExpression outerParam = Expression.Parameter(typeof(T), "x");

            Expression? body = null;
            foreach (var val in searchValues.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                Expression partExpr = BuildAnyLambda(outerParam, path.Split('.'), val);
                body = body == null ? partExpr : Expression.OrElse(body, partExpr);
            }

            if (body == null)
                return PredicateBuilder.New<T>();

            return Expression.Lambda<Func<T, bool>>(body, outerParam);
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

                var anyLambda = Expression.Lambda(innerBody, innerParam);

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


        //public static Expression<Func<T, bool>> CreateLambdaAnyStringContains<T>(string path, ICollection<string> searchValues)
        //{
        //    ExpressionStarter<T> predicate = PredicateBuilder.New<T>();
        //    if (searchValues == null || searchValues.Count == 0)
        //    {
        //        return predicate;
        //    }

        //    ParameterExpression param = Expression.Parameter(typeof(T), "x");
        //    string[] parts = path.Split('.');

        //    // Build navigation to collection
        //    Expression? current = param;
        //    Type? currentType = typeof(T);
        //    for (int i = 0; i < parts.Length - 2; i++) // to collection
        //    {
        //        PropertyInfo? prop = (currentType?.GetProperty(parts[i])) ?? throw new InvalidOperationException($"Property '{parts[i]}' not found on {currentType}");
        //        current = Expression.Property(current, prop);
        //        currentType = prop.PropertyType;
        //    }

        //    // Collection name
        //    string collectionName = parts[^2];
        //    string propertyName = parts[^1];

        //    // Navigate to collection
        //    PropertyInfo collectionProp = (currentType?.GetProperty(collectionName)) ?? throw new InvalidOperationException($"Collection '{collectionName}' not found on {currentType}");
        //    MemberExpression collectionExpr = Expression.Property(current!, collectionProp);
        //    Type elementType = collectionProp.PropertyType.GetGenericArguments()[0]; // assumes ICollection<T>

        //    // inner lambda: y => y.Property.Contains(searchValue)
        //    ParameterExpression innerParam = Expression.Parameter(elementType, "y");
        //    MemberExpression innerProp = Expression.Property(innerParam, propertyName);
        //    MethodInfo? containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);

        //    foreach (string? val in searchValues.Where(v => !string.IsNullOrWhiteSpace(v)))
        //    {
        //        ConstantExpression constant = Expression.Constant(val);
        //        MethodCallExpression containsExpr = Expression.Call(innerProp, containsMethod!, constant);
        //        LambdaExpression innerLambda = Expression.Lambda(containsExpr, innerParam);

        //        MethodInfo anyMethod = typeof(Enumerable).GetMethods()
        //            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
        //            .MakeGenericMethod(elementType);

        //        MethodCallExpression anyExpr = Expression.Call(anyMethod, collectionExpr, innerLambda);

        //        Expression<Func<T, bool>> finalLambda = Expression.Lambda<Func<T, bool>>(anyExpr, param);
        //        predicate = predicate.Or(finalLambda);
        //    }

        //    return predicate;
        //}

        //public static Expression<Func<T, bool>> CreateLambdaStringEqualsJoin<T>(string columnName, ICollection<string> searchValues)
        //{
        //    ExpressionStarter<T> predicate = PredicateBuilder.New<T>();

        //    if (IsICollectionStringValid(searchValues))
        //    {
        //        ParameterExpression pe = Expression.Parameter(typeof(T), "c");
        //        MemberExpression property = Expression.Property(pe, columnName);
        //        MethodInfo? method = searchValues.GetType().GetMethod("Equals");
        //        if (method is null)
        //        {
        //            return predicate;
        //        }

        //        foreach (string value in searchValues.Where(v => !string.IsNullOrWhiteSpace(v)))
        //        {
        //            ConstantExpression constant = Expression.Constant(value.Trim());
        //            MethodCallExpression call = Expression.Call(property, method, constant);

        //            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(call, pe);
        //            predicate = predicate.Or(lambda);
        //        }
        //    }

        //    return predicate;
        //}

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

        //public static Expression<Func<T, bool>> CreateLambdaSpanIntYearJoin<T>(string columnName, ICollection<int> searchValues)
        //{
        //    ExpressionStarter<T> predicateloc = PredicateBuilder.New<T>();

        //    if (IsICollectionIntValid(searchValues))
        //    {
        //        ParameterExpression pe = Expression.Parameter(typeof(T), "s");
        //        MemberExpression property = Expression.Property(pe, columnName);
        //        string stringDate_Beginning = string.Empty;
        //        string stringDate_Ending = string.Empty;

        //        if (searchValues.Count == 1)
        //        {
        //            stringDate_Beginning = "01.01." + searchValues.ToArray()[0];
        //            stringDate_Ending = "31.12." + searchValues.ToArray()[0];
        //        }
        //        else if (searchValues.Count == 2)
        //        {
        //            stringDate_Beginning = "01.01." + searchValues.ToArray()[0];
        //            stringDate_Ending = "31.12." + searchValues.ToArray()[1];
        //        }
        //        ConstantExpression constant0 = Expression.Constant(Convert.ToDateTime(stringDate_Beginning));
        //        ConstantExpression constant1 = Expression.Constant(Convert.ToDateTime(stringDate_Ending));
        //        predicateloc = Expression.Lambda<Func<T, bool>>(
        //            Expression.AndAlso(
        //                GreaterThanOrEqual(property, constant0),
        //                LessThanOrEqual(property, constant1)),
        //                    [pe]);
        //    }
        //    return predicateloc;
        //}

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
        //public static ExpressionStarter<T> QueryManufacturingDate<T>(ICollection<int> SearchYear)
        //{
        //    ExpressionStarter<T> expressionstarter = PredicateBuilder.New<T>();

        //    if (IsICollectionIntValid(SearchYear))
        //    {
        //        ParameterExpression pe = Expression.Parameter(typeof(T), "s");
        //        MemberExpression exactYear = Expression.Property(Expression.Property(pe, "ManufacturingDate"), "ExactYear");
        //        MemberExpression startYear = Expression.Property(Expression.Property(pe, "ManufacturingDate"), "StartYear");
        //        MemberExpression endYear = Expression.Property(Expression.Property(pe, "ManufacturingDate"), "EndYear");

        //        if (SearchYear.Count == 1)
        //        {
        //            ConstantExpression constant = Expression.Constant(SearchYear.ToArray()[0]);
        //            expressionstarter = Expression.Lambda<Func<T, bool>>(Expression
        //                .Or(Equal(exactYear, constant),
        //                Expression.AndAlso(GreaterThanOrEqual(startYear, constant),
        //                    LessThanOrEqual(endYear, constant))), [pe]);
        //        }
        //        else if (SearchYear.Count == 2)
        //        {
        //            ConstantExpression constant0 = Expression.Constant(SearchYear.ToArray()[0]);
        //            ConstantExpression constant1 = Expression.Constant(SearchYear.ToArray()[1]);
        //            expressionstarter = Expression.Lambda<Func<T, bool>>(Expression
        //                .Or(Expression.AndAlso(GreaterThanOrEqual(exactYear, constant0),
        //                    LessThanOrEqual(exactYear, constant1)),
        //                    Expression.AndAlso(GreaterThanOrEqual(startYear, constant0),
        //                    LessThanOrEqual(endYear, constant1))), [pe]);
        //        }
        //    }

        //    return expressionstarter;
        //}


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
            foreach (var member in propertyPath.Split('.'))
            {
                current = Expression.Property(current, member);
            }
            return (MemberExpression)current;
        }
    }
}
