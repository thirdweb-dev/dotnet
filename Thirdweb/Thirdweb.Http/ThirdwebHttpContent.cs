using System.Text;

namespace Thirdweb;

/// <summary>
/// Represents HTTP content used in the Thirdweb SDK.
/// </summary>
public class ThirdwebHttpContent
{
    private readonly byte[] _content;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdwebHttpContent"/> class from a string.
    /// </summary>
    /// <param name="content">The content string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the content is null.</exception>
    public ThirdwebHttpContent(string content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        this._content = Encoding.UTF8.GetBytes(content);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdwebHttpContent"/> class from a byte array.
    /// </summary>
    /// <param name="content">The content byte array.</param>
    /// <exception cref="ArgumentNullException">Thrown if the content is null.</exception>
    public ThirdwebHttpContent(byte[] content)
    {
        this._content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdwebHttpContent"/> class from a stream.
    /// </summary>
    /// <param name="content">The content stream.</param>
    /// <exception cref="ArgumentNullException">Thrown if the content is null.</exception>
    public ThirdwebHttpContent(Stream content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);
        this._content = memoryStream.ToArray();
    }

    /// <summary>
    /// Reads the content as a string.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content string.</returns>
    public Task<string> ReadAsStringAsync()
    {
        return Task.FromResult(Encoding.UTF8.GetString(this._content));
    }

    /// <summary>
    /// Reads the content as a byte array.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content byte array.</returns>
    public Task<byte[]> ReadAsByteArrayAsync()
    {
        return Task.FromResult(this._content);
    }

    /// <summary>
    /// Reads the content as a stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the content stream.</returns>
    public Task<Stream> ReadAsStreamAsync()
    {
        var stream = new MemoryStream(this._content);
        return Task.FromResult<Stream>(stream);
    }
}
