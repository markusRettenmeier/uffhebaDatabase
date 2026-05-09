using LinqKit;
using System.Linq.Expressions;
using System.Reflection;

namespace Sammlerplattform.Data
{
    public class SearchPredicateBuilder
    {
        public static Expression<Func<T, bool>> BuildPredicate<T>(object searchModel)
        {
            var predicate = PredicateBuilder.New<T>(true); // TRUE = kein Filter

            PropertyInfo[] modelProperties = searchModel
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in modelProperties)
            {
                object? value = prop.GetValue(searchModel);
                if (value == null)
                    continue;

                string columnName = prop.Name.Replace("_", ".");

                switch (value)
                {
                    case ICollection<int> intValue when intValue.Count > 0:
                        predicate = predicate.And(CreateLambdaSpanIntJoin<T>(columnName, intValue));
                        break;
                    case ICollection<int?> intNullValue when intNullValue.Count > 0:
                        predicate = predicate.And(CreateLambdaSpanIntNullJoin<T>(columnName, intNullValue));
                        break;

                    case ICollection<string> strList when strList.Count > 0:
                        predicate = predicate.And(
                            CreateLambdaStringContainsJoin<T>(columnName, strList));
                        break;
                }
            }

            return predicate;
        }

        private static Expression<Func<T, bool>> CreateLambdaStringContainsJoin<T>(string propertyPath, ICollection<string> values)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            // Prüfen, ob wir auf eine Collection zugreifen
            Type currentType = typeof(T);
            string[] pathParts = propertyPath.Split('.');

            Expression property = parameter;
            bool isCollectionPath = false;
            Type collectionElementType = typeof(object); // Platzhalter, wird später gesetzt
            ParameterExpression? collectionParameter = null;
            Expression? collectionPropertyAccess = null;

            // Pfad analysieren und erkennen, wo eine Collection ist
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                PropertyInfo? propInfo = currentType.GetProperty(part) ?? throw new ArgumentException($"Property '{part}' not found on type '{currentType.Name}'");

                // Prüfen, ob diese Property eine Collection ist
                if (IsCollectionType(propInfo.PropertyType))
                {
                    isCollectionPath = true;
                    collectionElementType = GetCollectionElementType(propInfo.PropertyType);

                    // Property bis zur Collection zugreifen
                    property = Expression.Property(property, part);

                    // Parameter für das Element der Collection erstellen
                    collectionParameter = Expression.Parameter(collectionElementType, "c");

                    // Restlichen Pfad für das Collection-Element erstellen
                    collectionPropertyAccess = collectionParameter;
                    for (int j = i + 1; j < pathParts.Length; j++)
                    {
                        collectionPropertyAccess = Expression.Property(collectionPropertyAccess, pathParts[j]);
                    }

                    break;
                }
                else
                {
                    property = Expression.Property(property, part);
                    currentType = propInfo.PropertyType;
                }
            }

            // Wenn es eine Collection ist, verwenden wir Any()
            if (isCollectionPath && collectionParameter != null && collectionPropertyAccess != null)
            {
                // Contains-Bedingungen für das Collection-Element erstellen
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;

                Expression? combinedCondition = null;
                foreach (string value in values)
                {
                    ConstantExpression constant = Expression.Constant(value, typeof(string));
                    MethodCallExpression contains = Expression.Call(collectionPropertyAccess, containsMethod, constant);

                    combinedCondition = combinedCondition == null ? contains : Expression.OrElse(combinedCondition, contains);
                }

                // Lambda für das Collection-Element
                var elementLambda = Expression.Lambda(combinedCondition!, collectionParameter);

                // Any-Methode holen und aufrufen
                var anyMethod = typeof(Enumerable)
                    .GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(collectionElementType);

                var anyCall = Expression.Call(anyMethod, property, elementLambda);

                return Expression.Lambda<Func<T, bool>>(anyCall, parameter);
            }
            else
            {
                // Normale Property (keine Collection)
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;

                Expression? combined = null;
                foreach (string value in values)
                {
                    ConstantExpression constant = Expression.Constant(value, typeof(string));
                    MethodCallExpression contains = Expression.Call(property, containsMethod, constant);

                    combined = combined == null ? contains : Expression.OrElse(combined, contains);
                }

                return Expression.Lambda<Func<T, bool>>(combined!, parameter);
            }
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanIntNullJoin<T>(string columnName, ICollection<int?> searchValues)
        {
            if (!IsICollectionIntNullValid(searchValues))
                return PredicateBuilder.New<T>();

            return CreateLambdaSpanIntJoinInternal<T, int?>(columnName, searchValues.Cast<int?>());
        }

        public static Expression<Func<T, bool>> CreateLambdaSpanIntJoin<T>(string columnName, ICollection<int> searchValues)
        {
            if (!IsICollectionIntValid(searchValues))
                return PredicateBuilder.New<T>();

            return CreateLambdaSpanIntJoinInternal<T, int>(columnName, searchValues.Cast<int>());
        }

        private static Expression<Func<T, bool>> CreateLambdaSpanIntJoinInternal<T, TValue>(string columnName, IEnumerable<TValue> searchValues)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "s");

            // Prüfen, ob wir auf eine Collection zugreifen
            Type currentType = typeof(T);
            string[] pathParts = columnName.Split('.');

            Expression property = parameter;
            bool isCollectionPath = false;
            Type? collectionElementType = typeof(object);
            ParameterExpression? collectionParameter = null;
            Expression? collectionPropertyAccess = null;

            // Pfad analysieren
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                PropertyInfo? propInfo = currentType.GetProperty(part) ?? throw new ArgumentException($"Property '{part}' not found on type '{currentType.Name}'");
                if (IsCollectionType(propInfo.PropertyType))
                {
                    isCollectionPath = true;
                    collectionElementType = GetCollectionElementType(propInfo.PropertyType);

                    property = Expression.Property(property, part);
                    collectionParameter = Expression.Parameter(collectionElementType, "c");

                    collectionPropertyAccess = collectionParameter;
                    for (int j = i + 1; j < pathParts.Length; j++)
                    {
                        collectionPropertyAccess = Expression.Property(collectionPropertyAccess, pathParts[j]);
                    }

                    break;
                }
                else
                {
                    property = Expression.Property(property, part);
                    currentType = propInfo.PropertyType;
                }
            }

            // Bedingung für Gleichheit erstellen
            Expression? combinedCondition = null;

            if (isCollectionPath && collectionParameter != null && collectionPropertyAccess != null)
            {
                // Für Collection: Any mit Gleichheitsbedingungen
                foreach (var searchValue in searchValues)
                {
                    ConstantExpression constant = Expression.Constant(searchValue, typeof(TValue));
                    BinaryExpression equalsExpression = Expression.Equal(collectionPropertyAccess, constant);

                    combinedCondition = combinedCondition == null
                        ? equalsExpression
                        : Expression.OrElse(combinedCondition, equalsExpression);
                }

                var elementLambda = Expression.Lambda(combinedCondition!, collectionParameter);

                var anyMethod = typeof(Enumerable)
                    .GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(collectionElementType);

                var anyCall = Expression.Call(anyMethod, property, elementLambda);

                return Expression.Lambda<Func<T, bool>>(anyCall, parameter);
            }
            else
            {
                // Normale Property
                foreach (var searchValue in searchValues)
                {
                    ConstantExpression constant = Expression.Constant(searchValue, typeof(TValue));
                    BinaryExpression equalsExpression = Expression.Equal(property, constant);

                    combinedCondition = combinedCondition == null
                        ? equalsExpression
                        : Expression.OrElse(combinedCondition, equalsExpression);
                }

                return Expression.Lambda<Func<T, bool>>(combinedCondition!, parameter);
            }
        }

        // Hilfsmethoden
        private static bool IsCollectionType(Type type)
        {
            return type.IsGenericType &&
                   (type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(List<>));
        }

        private static Type GetCollectionElementType(Type collectionType)
        {
            return collectionType.GetGenericArguments()[0];
        }

        private static bool IsICollectionIntValid(ICollection<int> collection)
        {
            return collection != null && collection.Count > 0;
        }

        private static bool IsICollectionIntNullValid(ICollection<int?> collection)
        {
            return collection != null && collection.Count > 0;
        }
    }
}
