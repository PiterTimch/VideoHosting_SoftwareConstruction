using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Videos.Queries.GetVideoStream;

public class RangeFileStreamResult : ActionResult
{
    private readonly Stream _stream;
    private readonly string _contentType;
    private readonly long _start;
    private readonly long _end;
    private readonly long _totalLength;

    public RangeFileStreamResult(
        Stream stream,
        string contentType,
        long start,
        long end,
        long totalLength)
    {
        _stream = stream;
        _contentType = contentType;
        _start = start;
        _end = end;
        _totalLength = totalLength;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status206PartialContent;
        response.ContentType = _contentType;
        response.Headers.ContentRange = $"bytes {_start}-{_end}/{_totalLength}";
        response.Headers.AcceptRanges = "bytes";
        response.ContentLength = _end - _start + 1;

        const int bufferSize = 65536;
        var buffer = new byte[bufferSize];
        long remaining = _end - _start + 1;

        try
        {
            _stream.Seek(_start, SeekOrigin.Begin);
            while (remaining > 0)
            {
                var toRead = (int)Math.Min(remaining, bufferSize);
                var read = await _stream.ReadAsync(buffer.AsMemory(0, toRead), context.HttpContext.RequestAborted);
                if (read == 0) break;
                await response.Body.WriteAsync(buffer.AsMemory(0, read), context.HttpContext.RequestAborted);
                remaining -= read;
            }
        }
        finally
        {
            await _stream.DisposeAsync();
        }
    }
}
