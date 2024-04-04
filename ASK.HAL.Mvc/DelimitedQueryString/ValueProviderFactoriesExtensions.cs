// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

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