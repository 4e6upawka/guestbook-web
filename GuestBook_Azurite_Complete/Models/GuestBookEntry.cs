using Azure;
using Azure.Data.Tables;
namespace GuestBook.Web.Models;
public class GuestBookEntry : ITableEntity
{
    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string GuestName { get; set; } = "";
    public string Message { get; set; } = "";
    public string PhotoUrl { get; set; } = "";
}
