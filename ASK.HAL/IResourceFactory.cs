namespace ASK.HAL;

public interface IResourceFactory
{
    Resource Create();
    Resource Create(string self);
    Resource Create(Uri self);
}