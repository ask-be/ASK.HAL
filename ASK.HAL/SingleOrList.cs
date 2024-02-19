namespace ASK.HAL;

public class SingleOrList<T>
{
    private readonly List<T> _values = new List<T>();

    internal SingleOrList(IEnumerable<T> items)
    {
        SingleValued = false;
        _values.AddRange(items.Where(x => x != null));
        if (_values.Count == 0)
            throw new ArgumentException("SingleOrList must contains at least one element");
    }

    internal SingleOrList(T single)
    {
        SingleValued = true;
        _values.Add(single);
    }
    
    public int Count => _values.Count;

    public IEnumerable<T> Values => _values;

    public T Value => _values[0];

    public bool SingleValued { get; }
}