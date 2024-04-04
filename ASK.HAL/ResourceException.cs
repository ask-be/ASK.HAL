// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

namespace ASK.HAL;

public class ResourceException : Exception
{
    public ResourceException()
    {
    }

    public ResourceException(string message) : base(message)
    {
    }

    public ResourceException(string message, Exception inner) : base(message, inner)
    {
    }
}