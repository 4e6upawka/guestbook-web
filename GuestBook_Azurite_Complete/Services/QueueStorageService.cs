using Azure.Storage.Queues;
namespace GuestBook.Web.Services;
public class QueueStorageService
{
    private readonly QueueClient _queue;
    public QueueStorageService(IConfiguration config)
    {
        var connection = config.GetConnectionString("TableStorage") ?? "UseDevelopmentStorage=true";
        _queue = new QueueClient(connection, "guestthumbs");
        _queue.CreateIfNotExists();
    }
    public async Task SendMessageAsync(string message) => await _queue.SendMessageAsync(message);
}
