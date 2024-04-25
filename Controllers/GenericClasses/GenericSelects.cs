using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Sammlerplattform.Controllers.GenericClasses
{
    public static class GenericSelects
    {
        public static MemberAssignment DynamicSelectGenerator<T>(string table, string Fields = "")
        {
            string[] EntityFields;
            if (Fields == string.Empty)
            {
                // get Properties of the T
                EntityFields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray();
            }
            else
            {
                EntityFields = Fields.Split(',');
            }

            // input parameter "o"
            ParameterExpression xParameter = Expression.Parameter(typeof(T), "o");
            System.Reflection.PropertyInfo EntityTables = typeof(T).GetProperty(table) ?? throw new NullReferenceException();

            // new statement "new Data()"
            NewExpression xNew = Expression.New(EntityTables.PropertyType);

            // create initializers
            IEnumerable<MemberAssignment> bindings = EntityFields.Select(o => o.Trim())
                .Select(o =>
                {
                    // property "Field1"
                    System.Reflection.PropertyInfo mi = EntityTables.PropertyType.GetProperty(o) ?? throw new NullReferenceException();

                    // original value "o.table.Field1"
                    MemberExpression xOriginal = Expression.Property(Expression.Property(xParameter, table), o);

                    // set value "Field1 = o.table.Field1"
                    return Expression.Bind(mi, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.table.Field1, Field2 = o.table.Field2 }"
            MemberInitExpression xInit = Expression.MemberInit(xNew, bindings);

            // expression "Data = new Data { Field1 = o.table.Field1, Field2 = o.table.Field2 }"
            MemberAssignment binding = Expression.Bind(EntityTables, xInit);

            // compile to Func<Data, Data>
            return binding;
        }

        public static Func<T, T> DynamicSelectGeneratorJoined<T>(Dictionary<string, string> TablesNColumns)
        {
            // input parameter "o"
            ParameterExpression xParameter = Expression.Parameter(typeof(T), "m");

            // new statement "new Data()"
            NewExpression xNew = Expression.New(typeof(T));

            // initialization "new Data { Field1 = o.table.Field1, Field2 = o.table.Field2 }"            
            IEnumerable<MemberBinding> bindings = [];
            List<MemberBinding> bindingList = bindings.ToList();
            int index = 0;
            foreach (KeyValuePair<string, string> pair in TablesNColumns)
            {
                index++;
                bindingList.Add(DynamicSelectGenerator<T>(pair.Key, pair.Value));
            }
            bindings = bindingList;

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            MemberInitExpression xInit = Expression.MemberInit(xNew, bindings);
            xInit = (MemberInitExpression)new ParameterReplacer(xParameter).Visit(xInit);

            // expression "o => new Data{ Table1 = new Table1 { Field1 = o.table.Field1, Field2 = o.table.Field2 }}"
            Expression<Func<T, T>> lambda = Expression.Lambda<Func<T, T>>(xInit, xParameter);

            // compile to Func<Data, Data>
            return lambda.Compile();
        }

        internal class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return base.VisitParameter(_parameter);
            }

            internal ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }
        }
    }
}
