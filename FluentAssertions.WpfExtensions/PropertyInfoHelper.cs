using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAssertions
{
    static class PropertyInfoHelper
    {
        internal static PropertyInfo GetPropertyInfoOrThrow<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression, string parameterName)
        {
            if (!(propertyExpression.Body is MemberExpression))
            {
                throw new ArgumentException($"{parameterName} must be a simple property expression of the form bindable => bindable.Value", parameterName);
            }

            string propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);

            if (propertyInfo is null)
            {
                throw new ArgumentException($"could not find property with name {propertyName} on instance of type {typeof(T).FullName}", parameterName);
            }

            return propertyInfo;
        }
    }
}
