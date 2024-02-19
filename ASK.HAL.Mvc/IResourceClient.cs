namespace ASK.HAL.Mvc;

public interface IResourceClient
{
    /// <summary>
    /// Retrieve a Resource at the given Uri.
    /// The current Cookies and Authentication Header will be transferred to the request 
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <param name="uri">Uri of the resource to retrieve</param>
    /// <param name="requestTimeout"></param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The Stream with the resource</returns>
    Task<Resource?> GetResource(
        Uri uri, 
        CancellationToken cancellationToken = new CancellationToken());
}