// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

namespace ASK.HAL;

public interface IResourceFactory
{
    Resource Create();
    Resource Create(string self);
    Resource Create(Uri self);
}