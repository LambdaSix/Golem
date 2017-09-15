using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Golem.OptionsParse {
    public static class OptionSetExtras
    {
        public static string GetHelpText<T>(Expression<Func<T>> propertyExpr) {
            if (propertyExpr == null)
                throw new ArgumentNullException(nameof(propertyExpr));

            var body = propertyExpr.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException("Invalid expression body", nameof(propertyExpr));

            var property = body.Member as PropertyInfo;

            if (property == null)
                throw new ArgumentException("Argument body does not resolve to propery", nameof(propertyExpr));

            var attrib = property.GetCustomAttribute<HelpTextAttribute>();

            if (attrib == null)
                throw new InvalidDataException("Resolved property body has no attached HelpText");

            return String.IsNullOrWhiteSpace(LookupLocalisation(attrib.LocalisedId))
                ? attrib.DefaultHelpText
                : LookupLocalisation(attrib.LocalisedId);
        }

        private static string LookupLocalisation(int localisationId)
        {
            // TODO: Implement for framework stuff.
            return "";
        }
    }
}