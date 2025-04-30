// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

namespace ASK.HAL;

internal class SingleOrList<T>
{
    private readonly List<T> _values = new List<T>();

    internal SingleOrList(IEnumerable<T>? items)
    {
        SingleValued = false;
        if(items is not null)
        {
            _values.AddRange(items.Where(x => x != null));
        }
    }

    internal SingleOrList(T? single)
    {
        SingleValued = true;     
        
        if(single is not null)
        {
            _values.Add(single);
        }
    }

    public int Count => _values.Count;

    public IReadOnlyList<T> Values => _values;

    public T? Value => IsSingleValue();

    public bool SingleValued { get; }

    private T? IsSingleValue()
    {
        if (SingleValued)
        {
            if(_values.Count > 0)
            {
                return _values[0];
            }

            return default;
        }

        throw new ArgumentException("This is multivalued");
    }
}