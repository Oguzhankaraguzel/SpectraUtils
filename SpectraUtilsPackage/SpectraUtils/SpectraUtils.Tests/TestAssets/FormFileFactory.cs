using Microsoft.AspNetCore.Http;

namespace SpectraUtils.Tests.TestAssets;

internal static class FormFileFactory
{
    public static IFormFile Create(string fileName, long length)
        => new FormFile(Stream.Null, 0, length, "file", fileName);

    public static IFormFileCollection CreateCollection(params IFormFile[] files)
    {
        var collection = new FormFileCollection();
        collection.AddRange(files);
        return collection;
    }
}
