// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using System.Text.Json.Nodes;

namespace ASK.HAL;

public static class Constants
{
    public const string Links = "_links";
    public const string Embedded = "_embedded";
    public const string Self = "self";
    public const string Curies = "curies";
    public const string HrefPropertyName = "href";
    public const string HypertextApplicationLanguageJsonMediaType = "application/hal+json";
    public const string DeprecationPropertyName = "deprecation";
    public const string LangPropertyName = "lang";
    public const string NamePropertyName = "name";
    public const string ProfilePropertyName = "profile";
    public const string TemplatedPropertyName = "template";
    public const string TitlePropertyName = "title";
    public const string TypePropertyName = "type";

    public static readonly JsonNodeOptions DefaultJsonNodeOptions = new JsonNodeOptions
    {
        PropertyNameCaseInsensitive = true
    };
}