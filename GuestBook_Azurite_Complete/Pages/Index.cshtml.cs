using GuestBook.Web.Models;
using GuestBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
namespace GuestBook.Web.Pages;
public class IndexModel : PageModel
{
    private readonly TableStorageService _table;
    private readonly BlobStorageService _blob;
    private readonly QueueStorageService _queue;
    public IndexModel(TableStorageService table, BlobStorageService blob, QueueStorageService queue)
    { _table = table; _blob = blob; _queue = queue; }
    public List<GuestBookEntry> Entries { get; private set; } = new();
    [BindProperty, Required, StringLength(100)] public string GuestName { get; set; } = "";
    [BindProperty, Required, StringLength(1000)] public string Message { get; set; } = "";
    [BindProperty] public IFormFile? Photo { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchName { get; set; }
    public async Task OnGetAsync() => Entries = await _table.GetEntriesAsync(SearchName);
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { Entries = await _table.GetEntriesAsync(); return Page(); }
        string photoName = "";
        try { if (Photo is { Length: > 0 }) photoName = await _blob.UploadPhotoAsync(Photo); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("Photo", ex.Message); Entries = await _table.GetEntriesAsync(); return Page(); }
        var entry = await _table.AddEntryAsync(GuestName, Message, photoName);
        await _queue.SendMessageAsync($"{entry.PartitionKey}|{entry.RowKey}");
        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostDeleteAsync(string partitionKey, string rowKey)
    {
        if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey)) return BadRequest();
        await _table.DeleteEntryAsync(partitionKey, rowKey);
        return RedirectToPage();
    }
    public async Task<IActionResult> OnGetPhotoAsync(string name)
    {
        var result = await _blob.OpenAsync(name);
        if (result is null) return NotFound();
        return File(result.Value.Stream, result.Value.ContentType);
    }
}
