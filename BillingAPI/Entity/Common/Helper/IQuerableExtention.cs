using Entity.Base;
using Entity.Common.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Entity.Common.Helper
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> AnySortBy<T>(this IQueryable<T> source, string ordering, bool Ascending, string Culture = "en")
        {
            return ManageSorting(source, ordering, Ascending, Culture);
        }

        public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, string ordering, bool Ascending, string Culture = "en")
        where T : BaseEntity
        {
            return ManageSorting(source, ordering, Ascending, Culture);
        }

        public static IOrderedQueryable<T> LookupOrderBy<T>(this IQueryable<T> source, string ordering, bool Ascending, string Culture = "en")
            where T : BaseLookupEntity
        {
            return ManageSorting(source, ordering, Ascending, Culture);
        }

        private static IOrderedQueryable<T> ManageSorting<T>(IQueryable<T> source, string ordering, bool Ascending, string Culture)
        {
            var type = typeof(T);
            PropertyInfo property = type.GetProperty(ordering == null ? string.Empty : ordering);
            if (property == null)
            {
                ordering = "CreatedOn";
            }
            else
            {
                ParentTableMapping tableMapping = property.GetCustomAttributes<ParentTableMapping>().FirstOrDefault();
                if (tableMapping != null)
                {
                    PropertyInfo ParentPro = typeof(T).GetProperties().Where(p => p.PropertyType == tableMapping.SourceType).FirstOrDefault();
                    if (ParentPro != null)
                    {
                        string name = "Name";
                        ordering = $"{ParentPro.Name}.{name}";
                    }
                    else
                    {
                        ordering = "CreatedOn";
                    }
                }
            }
            var query = CallOrderedQueryable(source, Ascending ? "OrderBy" : "OrderByDescending", ordering, null);
            return query;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderByDescending", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenByDescending", propertyName, comparer);
        }

        public static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> query, string methodName, string propertyName,
                IComparer<object> comparer = null)
        {
            var param = Expression.Parameter(typeof(T), "x");

            var body = propertyName.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);

            return comparer != null
                ? (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, param),
                        Expression.Constant(comparer)
                    )
                )
                : (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, param)
                    )
                );
        }




    }
}
