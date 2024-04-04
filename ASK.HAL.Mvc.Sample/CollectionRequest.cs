// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

namespace HAL.Mvc.Sample;

public class CollectionRequest
{
    public int Max { get; set; } = 10;
    public int Index { get; set; } = 0;
}