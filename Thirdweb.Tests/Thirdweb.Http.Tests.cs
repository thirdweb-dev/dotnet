using System.Numerics;
using System.Text;

namespace Thirdweb.Tests
{
    public class HttpTests : BaseTests
    {
        public HttpTests(ITestOutputHelper output)
            : base(output) { }

        #region ThirdwebHttpClient

        [Fact]
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

        [Fact]
        public async Task PostAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var httpClient = new ThirdwebHttpClient();
            var requestUri = "https://jsonplaceholder.typicode.com/posts";
            var content = new StringContent("{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync(requestUri, content);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(201, response.StatusCode);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task PutAsync_ShouldThrowNotImplementedException()
        {
            // Arrange
            var httpClient = new ThirdwebHttpClient();
            var requestUri = "https://jsonplaceholder.typicode.com/posts/1";
            var content = new StringContent("{\"title\": \"foo\", \"body\": \"bar\", \"userId\": 1}", System.Text.Encoding.UTF8, "application/json");

            // Act & Assert
            _ = await Assert.ThrowsAsync<NotImplementedException>(() => httpClient.PutAsync(requestUri, content));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotImplementedException()
        {
            // Arrange
            var httpClient = new ThirdwebHttpClient();
            var requestUri = "https://jsonplaceholder.typicode.com/posts/1";

            // Act & Assert
            _ = await Assert.ThrowsAsync<NotImplementedException>(() => httpClient.DeleteAsync(requestUri));
        }

        [Fact]
        public void Dispose_ShouldDisposeHttpClient()
        {
            // Arrange
            var httpClient = new ThirdwebHttpClient();

            // Act
            httpClient.Dispose();

            // Assert
            // Check that disposing twice does not throw an exception
            var exception = Record.Exception(() => httpClient.Dispose());
            Assert.Null(exception);
        }

        #endregion

        #region ThirdwebHttpContent

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task ReadAsStreamAsync_ShouldReturnContentAsStream()
        {
            // Arrange
            var contentString = "Hello, World!";
            var content = new ThirdwebHttpContent(contentString);
            var expectedStream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));

            // Act
            var resultStream = await content.ReadAsStreamAsync();

            // Assert
            using (var reader = new StreamReader(resultStream))
            using (var expectedReader = new StreamReader(expectedStream))
            {
                var resultString = await reader.ReadToEndAsync();
                var expectedString = await expectedReader.ReadToEndAsync();
                Assert.Equal(expectedString, resultString);
            }
        }

#nullable disable

        [Fact]
        public void Constructor_WithNullString_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((string)null));
        }

        [Fact]
        public void Constructor_WithNullByteArray_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((byte[])null));
        }

        [Fact]
        public void Constructor_WithNullStream_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => new ThirdwebHttpContent((Stream)null));
        }

#nullable restore

        #endregion
    }
}
