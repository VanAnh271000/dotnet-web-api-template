using Shared.QueryParameter;
using Shared.Results;
using System.Linq.Expressions;

namespace Shared.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterParameter> filters)
        {
            if (!filters.Any()) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            foreach (var filter in filters)
            {
                var propertyExpression = GetPropertyExpression(parameter, filter.Field);
                if (propertyExpression == null) continue;

                var filterExpression = BuildFilterExpression(propertyExpression, filter);
                if (filterExpression == null) continue;

                if (combinedExpression == null)
                {
                    combinedExpression = filterExpression;
                }
                else
                {
                    combinedExpression = filter.LogicalOperator.ToLower() == "or"
                        ? Expression.OrElse(combinedExpression, filterExpression)
                        : Expression.AndAlso(combinedExpression, filterExpression);
                }
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchTerm, params string[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? searchExpression = null;
            var searchValue = Expression.Constant(searchTerm, typeof(string));
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            foreach (var propertyName in searchProperties)
            {
                var propertyExpression = GetPropertyExpression(parameter, propertyName);
                if (propertyExpression == null)
                    continue;

                Expression? stringExpression = null;

                if (propertyExpression.Type == typeof(string))
                {
                    stringExpression = propertyExpression;
                }
                else if (propertyExpression.Type == typeof(decimal) || propertyExpression.Type == typeof(decimal?))
                {
                    stringExpression = Expression.Call(
                        propertyExpression,
                        typeof(object).GetMethod("ToString", Type.EmptyTypes)!
                    );
                }

                if (stringExpression == null) continue;

                var containsCall = Expression.Call(stringExpression, containsMethod!, searchValue);
                searchExpression = searchExpression == null
                    ? containsCall
                    : Expression.OrElse(searchExpression, containsCall);
            }

            if (searchExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }


        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string sortDirection = "asc")
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyExpression = GetPropertyExpression(parameter, sortBy);

            if (propertyExpression == null) return query;

            var lambda = Expression.Lambda(propertyExpression, parameter);
            var methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), propertyExpression.Type },
                query.Expression,
                Expression.Quote(lambda)
            );

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> query, int page, int pageSize)
        {
            var totalCount = query.Count();
            var items = query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Index = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private static Expression? GetPropertyExpression(ParameterExpression parameter, string propertyName)
        {
            try
            {
                return propertyName.Split('.').Aggregate<string, Expression>(parameter, Expression.PropertyOrField);
            }
            catch
            {
                return null;
            }
        }

        private static Expression? BuildFilterExpression(Expression propertyExpression, FilterParameter filter)
        {
            if (filter.Value == null) return null;
            var targetType = Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type;
            if (filter.Operator.Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                if (filter.Value is System.Collections.IEnumerable enumerable && !(filter.Value is string))
                {
                    var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType))!;
                    var addMethod = list.GetType().GetMethod("Add")!;

                    foreach (var item in enumerable)
                    {
                        var convertedItem = Convert.ChangeType(item, targetType);
                        addMethod.Invoke(list, new[] { convertedItem });
                    }

                    var listConstant = Expression.Constant(list);

                    var containsMethod = typeof(Enumerable)
                    .GetMethods()
                    .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(targetType);

                    Expression memberToCompare = propertyExpression;
                    if (Nullable.GetUnderlyingType(propertyExpression.Type) != null)
                    {
                        memberToCompare = Expression.Property(propertyExpression, "Value");
                    }

                    return Expression.Call(containsMethod, listConstant, memberToCompare);
                }

                throw new InvalidOperationException("IN operator requires a list of values");
            }
            object? convertedValue;

            try
            {

                if (targetType == typeof(Guid))
                {
                    convertedValue = Guid.Parse(filter.Value.ToString());
                }
                else if (targetType.IsEnum)
                {
                    convertedValue = Enum.Parse(targetType, filter.Value.ToString()!);
                }
                else
                {
                    convertedValue = Convert.ChangeType(filter.Value, targetType);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot convert '{filter.Value}' to {propertyExpression.Type}", ex);
            }

            var constantExpression = Expression.Constant(convertedValue, propertyExpression.Type);

            return filter.Operator.ToLower() switch
            {
                "==" => Expression.Equal(propertyExpression, constantExpression),
                "!=" => Expression.NotEqual(propertyExpression, constantExpression),
                ">" => Expression.GreaterThan(propertyExpression, constantExpression),
                "<" => Expression.LessThan(propertyExpression, constantExpression),
                ">=" => Expression.GreaterThanOrEqual(propertyExpression, constantExpression),
                "<=" => Expression.LessThanOrEqual(propertyExpression, constantExpression),
                "contains" when propertyExpression.Type == typeof(string) =>
                    Expression.Call(propertyExpression, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constantExpression),
                "startswith" when propertyExpression.Type == typeof(string) =>
                    Expression.Call(propertyExpression, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, constantExpression),
                "endswith" when propertyExpression.Type == typeof(string) =>
                    Expression.Call(propertyExpression, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, constantExpression),
                _ => null
            };
        }
    }
}
