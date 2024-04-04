// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

namespace ASK.HAL.Mvc;

public interface IResourceUriFactory
{
    Uri GetUriByName(string name, object? parameters = null);
}