using System.Numerics;

namespace Thirdweb.Tests
{
    public class HttpTests : BaseTests
    {
        public HttpTests(ITestOutputHelper output)
            : base(output) { }

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
            Assert.Single(httpClient.Headers);
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
            Assert.Single(httpClient.Headers);
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
    }
}
