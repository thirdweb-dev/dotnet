using System.Text;

namespace Thirdweb.Tests.Http;

public class HttpTests : BaseTests
{
    public HttpTests(ITestOutputHelper output)
        : base(output) { }

    #region ThirdwebHttpClient

    [Fact(Timeout = 120000)]
    public async Task GetAsync_ShouldReturnSuccessResponse()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var requestUri = "https://jsonplaceholder.typicode.com/posts/1";

        // Act
        var response = await httpClient.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact(Timeout = 120000)]
    public async Task PostAsync_ShouldReturnSuccessResponse()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var requestUri = "https://jsonplaceholder.typicode.com/posts";
        var content = new StringContent( /*lang=json,strict*/
            "{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await httpClient.PostAsync(requestUri, content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(201, response.StatusCode);
    }

    [Fact(Timeout = 120000)]
    public void SetHeaders_ShouldAddHeaders()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };

        // Act
        httpClient.SetHeaders(headers);

        // Assert
        _ = Assert.Single(httpClient.Headers);
        Assert.Equal("Bearer token", httpClient.Headers["Authorization"]);
    }

    [Fact(Timeout = 120000)]
    public void ClearHeaders_ShouldRemoveAllHeaders()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };
        httpClient.SetHeaders(headers);

        // Act
        httpClient.ClearHeaders();

        // Assert
        Assert.Empty(httpClient.Headers);
    }

    [Fact(Timeout = 120000)]
    public void AddHeader_ShouldAddHeader()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();

        // Act
        httpClient.AddHeader("Authorization", "Bearer token");

        // Assert
        _ = Assert.Single(httpClient.Headers);
        Assert.Equal("Bearer token", httpClient.Headers["Authorization"]);
    }

    [Fact(Timeout = 120000)]
    public void RemoveHeader_ShouldRemoveHeader()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        httpClient.AddHeader("Authorization", "Bearer token");

        // Act
        httpClient.RemoveHeader("Authorization");

        // Assert
        Assert.Empty(httpClient.Headers);
    }

    [Fact(Timeout = 120000)]
    public async Task PutAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var requestUri = "https://jsonplaceholder.typicode.com/posts/1";
        var content = new StringContent( /*lang=json,strict*/
            "{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}",
            Encoding.UTF8,
            "application/json"
        );

        // Act & Assert
        _ = await Assert.ThrowsAsync<NotImplementedException>(() => httpClient.PutAsync(requestUri, content));
    }

    [Fact(Timeout = 120000)]
    public async Task DeleteAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();
        var requestUri = "https://jsonplaceholder.typicode.com/posts/1";

        // Act & Assert
        _ = await Assert.ThrowsAsync<NotImplementedException>(() => httpClient.DeleteAsync(requestUri));
    }

    [Fact(Timeout = 120000)]
    public void Dispose_ShouldDisposeHttpClient()
    {
        // Arrange
        var httpClient = new ThirdwebHttpClient();

        // Act
        httpClient.Dispose();

        // Assert
        // Check that disposing twice does not throw an exception
        var exception = Record.Exception(httpClient.Dispose);
        Assert.Null(exception);
    }

    #endregion

    #region ThirdwebHttpContent

    [Fact(Timeout = 120000)]
    public async Task Constructor_WithString_ShouldInitializeContent()
    {
        // Arrange
        var contentString = "Hello, World!";
        var expectedBytes = Encoding.UTF8.GetBytes(contentString);

        // Act
        var content = new ThirdwebHttpContent(contentString);
        var resultBytes = await content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(expectedBytes, resultBytes);
    }

    [Fact(Timeout = 120000)]
    public async Task Constructor_WithByteArray_ShouldInitializeContent()
    {
        // Arrange
        var contentBytes = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var content = new ThirdwebHttpContent(contentBytes);
        var resultBytes = await content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(contentBytes, resultBytes);
    }

    [Fact(Timeout = 120000)]
    public async Task Constructor_WithStream_ShouldInitializeContent()
    {
        // Arrange
        var contentString = "Hello, World!";
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));
        var expectedBytes = Encoding.UTF8.GetBytes(contentString);

        // Act
        var content = new ThirdwebHttpContent(contentStream);
        var resultBytes = await content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(expectedBytes, resultBytes);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadAsStringAsync_ShouldReturnContentAsString()
    {
        // Arrange
        var contentString = "Hello, World!";
        var content = new ThirdwebHttpContent(contentString);

        // Act
        var resultString = await content.ReadAsStringAsync();

        // Assert
        Assert.Equal(contentString, resultString);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadAsByteArrayAsync_ShouldReturnContentAsByteArray()
    {
        // Arrange
        var contentBytes = Encoding.UTF8.GetBytes("Hello, World!");
        var content = new ThirdwebHttpContent(contentBytes);

        // Act
        var resultBytes = await content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(contentBytes, resultBytes);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadAsStreamAsync_ShouldReturnContentAsStream()
    {
        // Arrange
        var contentString = "Hello, World!";
        var content = new ThirdwebHttpContent(contentString);
        var expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));

        // Act
        var resultStream = await content.ReadAsStreamAsync();

        // Assert
        using var reader = new StreamReader(resultStream);
        using var expectedReader = new StreamReader(expectedStream);
        var resultString = await reader.ReadToEndAsync();
        var expectedString = await expectedReader.ReadToEndAsync();
        Assert.Equal(expectedString, resultString);
    }

#nullable disable

    [Fact(Timeout = 120000)]
    public void Constructor_WithNullString_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((string)null));
    }

    [Fact(Timeout = 120000)]
    public void Constructor_WithNullByteArray_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((byte[])null));
    }

    [Fact(Timeout = 120000)]
    public void Constructor_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((Stream)null));
    }

#nullable restore

    #endregion

    #region ThirdwebHttpResponseMessage

    [Fact(Timeout = 120000)]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var statusCode = 200;
        var content = new ThirdwebHttpContent("Test Content");
        var isSuccessStatusCode = true;

        // Act
        var responseMessage = new ThirdwebHttpResponseMessage(statusCode, content, isSuccessStatusCode);

        // Assert
        Assert.Equal(statusCode, responseMessage.StatusCode);
        Assert.Equal(content, responseMessage.Content);
        Assert.Equal(isSuccessStatusCode, responseMessage.IsSuccessStatusCode);
    }

    [Fact(Timeout = 120000)]
    public void EnsureSuccessStatusCode_ShouldReturnSelfOnSuccess()
    {
        // Arrange
        var statusCode = 200;
        var content = new ThirdwebHttpContent("Test Content");
        var isSuccessStatusCode = true;
        var responseMessage = new ThirdwebHttpResponseMessage(statusCode, content, isSuccessStatusCode);

        // Act
        var result = responseMessage.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(responseMessage, result);
    }

    [Fact(Timeout = 120000)]
    public async Task EnsureSuccessStatusCode_ShouldThrowExceptionOnFailure()
    {
        // Arrange
        var statusCode = 400;
        var content = new ThirdwebHttpContent("Error Content");
        var isSuccessStatusCode = false;
        var responseMessage = new ThirdwebHttpResponseMessage(statusCode, content, isSuccessStatusCode);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(responseMessage.EnsureSuccessStatusCode()));
        var contentString = await content.ReadAsStringAsync();
        Assert.Equal($"Request failed with status code {statusCode} and content: {contentString}", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public void StatusCode_ShouldSetAndGet()
    {
        // Arrange
        var responseMessage = new ThirdwebHttpResponseMessage(200, new ThirdwebHttpContent("Test Content"), true)
        {
            // Act
            StatusCode = 404
        };

        // Assert
        Assert.Equal(404, responseMessage.StatusCode);
    }

    [Fact(Timeout = 120000)]
    public void Content_ShouldSetAndGet()
    {
        // Arrange
        var initialContent = new ThirdwebHttpContent("Initial Content");
        var newContent = new ThirdwebHttpContent("New Content");
        var responseMessage = new ThirdwebHttpResponseMessage(200, initialContent, true)
        {
            // Act
            Content = newContent
        };

        // Assert
        Assert.Equal(newContent, responseMessage.Content);
    }

    [Fact(Timeout = 120000)]
    public void IsSuccessStatusCode_ShouldSetAndGet()
    {
        // Arrange
        var responseMessage = new ThirdwebHttpResponseMessage(200, new ThirdwebHttpContent("Test Content"), true)
        {
            // Act
            IsSuccessStatusCode = false
        };

        // Assert
        Assert.False(responseMessage.IsSuccessStatusCode);
    }

    #endregion
}
