namespace ASK.HAL.Mvc;

public interface IResourceUriFactory
{
    Uri GetUriByName(string name, object? parameters = null);
}