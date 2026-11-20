using Azure.Data.Tables;
using GuestBook.Web.Models;
namespace GuestBook.Web.Services;
public class TableStorageService
{
    private readonly TableClient _tableClient;
    public TableStorageService(IConfiguration config)
    {
        var connection = config.GetConnectionString("TableStorage") ?? "UseDevelopmentStorage=true";
        _tableClient = new TableClient(connection, "GuestBookEntries");
        _tableClient.CreateIfNotExists();
    }
    public async Task<GuestBookEntry> AddEntryAsync(string guestName, string message, string? photoUrl)
    {
        var entry = new GuestBookEntry
        {
            PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
            RowKey = Guid.NewGuid().ToString("N"),
            GuestName = guestName.Trim(),
            Message = message.Trim(),
            PhotoUrl = photoUrl?.Trim() ?? ""
        };
        await _tableClient.AddEntityAsync(entry);
        return entry;
    }
    public async Task<List<GuestBookEntry>> GetEntriesAsync(string? guestName = null)
    {
        var entries = new List<GuestBookEntry>();
        // PartitionKey и RowKey индексируются; GuestName фильтруем локально для учебного примера.
        await foreach (var entry in _tableClient.QueryAsync<GuestBookEntry>())
            if (string.IsNullOrWhiteSpace(guestName) || entry.GuestName.Equals(guestName.Trim(), StringComparison.OrdinalIgnoreCase))
                entries.Add(entry);
        return entries.OrderByDescending(e => e.PartitionKey).ThenByDescending(e => e.Timestamp).ToList();
    }
    public async Task DeleteEntryAsync(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }
}
