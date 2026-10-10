# SpectraUtils.AspNetCore

ASP.NET Core add-ons for [SpectraUtils](https://www.nuget.org/packages/SpectraUtils/).

```bash
dotnet add package SpectraUtils.AspNetCore
```

## `AllowedExtensionsAttribute`

Validates uploaded files by extension and maximum size. Works with `IFormFile`, `IFormFileCollection` /
`IEnumerable<IFormFile>` and file names given as `string` (extension only).

```csharp
using SpectraUtils.Attributes.ValidationAttributes;

public class UploadModel
{
    // Max 2 MB, .jpg / .png only
    [Required]
    [AllowedExtensions(2 * 1024 * 1024, ".jpg", ".png")]
    public IFormFile? Avatar { get; set; }

    // Every file in the collection is checked
    [AllowedExtensions(10 * 1024 * 1024, ".pdf", ".docx",
        FileSizeErrorMessage = "{0} must be smaller than {2}.")]
    public IFormFileCollection? Documents { get; set; }
}
```

- `null` is valid — add `[Required]` when a file is mandatory.
- Messages support `{0}` (display name), `{1}` (allowed extensions) and `{2}` (maximum size, e.g. `2 MB`).
- `ErrorMessage` is used for extension errors, `FileSizeErrorMessage` for size errors.
- Only the file **name** is checked. If you store or serve uploads, also verify the content (magic bytes).

Full documentation: <https://github.com/Oguzhankaraguzel/SpectraUtils>
