using GuestBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace GuestBook.Web.Pages;
public class GalleryModel : PageModel
{
    private readonly BlobStorageService _blob;
    public GalleryModel(BlobStorageService blob) => _blob = blob;
    public List<string> BlobNames { get; private set; } = new();
    public async Task OnGetAsync() => BlobNames = await _blob.GetAllBlobNamesAsync();
    public async Task<IActionResult> OnPostDeleteAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('\\')) return BadRequest();
        await _blob.DeleteBlobAsync(name);
        return RedirectToPage();
    }
}
