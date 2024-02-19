using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ASK.HAL.Mvc.DelimitedQueryString;

public static class ValueProviderFactoriesExtensions
{
    public static void AddDelimitedValueProviderFactory(
        this IList<IValueProviderFactory> valueProviderFactories,
        params char[] delimiters)
    {
        var queryStringValueProviderFactory = valueProviderFactories
            .OfType<QueryStringValueProviderFactory>()
            .FirstOrDefault();
        if (queryStringValueProviderFactory == null)
        {
            valueProviderFactories.Insert(
                0,
                new DelimitedQueryStringValueProviderFactory(delimiters));
        }
        else
        {
            valueProviderFactories.Insert(
                valueProviderFactories.IndexOf(queryStringValueProviderFactory),
                new DelimitedQueryStringValueProviderFactory(delimiters));
            valueProviderFactories.Remove(queryStringValueProviderFactory);
        }
    }
}