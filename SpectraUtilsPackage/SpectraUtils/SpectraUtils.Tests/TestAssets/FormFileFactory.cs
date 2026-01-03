using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SpectraUtils.Tests.TestAssets;

internal static class FormFileFactory
{
    public static IFormFile Create(string fileName, long length)
    {
        return new SimpleFormFile(fileName, length);
    }

    private sealed class SimpleFormFile : IFormFile
    {
        public SimpleFormFile(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
        }

        public string ContentType { get; set; } = "application/octet-stream";
        public string ContentDisposition { get; set; } = string.Empty;
        public IHeaderDictionary Headers { get; } = new SimpleHeaderDictionary();
        public long Length { get; }
        public string Name { get; set; } = "file";
        public string FileName { get; set; }

        public void CopyTo(Stream target) => throw new NotSupportedException();

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Stream OpenReadStream() => Stream.Null;
    }

    private sealed class SimpleHeaderDictionary : Dictionary<string, StringValues>, IHeaderDictionary
    {
        public long? ContentLength { get; set; }
    }
}
