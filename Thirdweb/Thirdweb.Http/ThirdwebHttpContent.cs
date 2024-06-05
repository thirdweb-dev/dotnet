using System.Text;

namespace Thirdweb
{
    public class ThirdwebHttpContent
    {
        private readonly byte[] content;

        // Constructor to initialize from a string
        public ThirdwebHttpContent(string content)
        {
            this.content = Encoding.UTF8.GetBytes(content);
        }

        // Constructor to initialize from a byte array
        public ThirdwebHttpContent(byte[] content)
        {
            this.content = content;
        }

        // Constructor to initialize from a stream
        public ThirdwebHttpContent(Stream content)
        {
            using (var memoryStream = new MemoryStream())
            {
                content.CopyTo(memoryStream);
                this.content = memoryStream.ToArray();
            }
        }

        // Read the content as a string
        public Task<string> ReadAsStringAsync()
        {
            return Task.FromResult(Encoding.UTF8.GetString(content));
        }

        // Read the content as a byte array
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            return Task.FromResult(content);
        }

        // Read the content as a stream
        public Task<Stream> ReadAsStreamAsync()
        {
            var stream = new MemoryStream(content);
            return Task.FromResult<Stream>(stream);
        }
    }
}
