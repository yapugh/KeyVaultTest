using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace proj2;

public class http1
{
    private readonly ILogger<http1> _logger;
    private readonly AzureStorageService _storageService;

    public http1(ILogger<http1> logger, AzureStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [Function("http1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var files = await _storageService.GetFileSharesAndFilesAsync();
        var queues = await _storageService.GetQueueMessagesAsync();

        var result = new
        {
            FileShares = files,
            Queues = queues
        };

        return new OkObjectResult(result);
    }
}
