namespace Thirdweb
{
    /// <summary>
    /// Represents the result of an IPFS upload.
    /// </summary>
    [Serializable]
    public struct IPFSUploadResult
    {
        /// <summary>
        /// Gets or sets the IPFS hash of the uploaded content.
        /// </summary>
        public string IpfsHash;

        /// <summary>
        /// Gets or sets the size of the pinned content.
        /// </summary>
        public string PinSize;

        /// <summary>
        /// Gets or sets the timestamp of the upload.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Gets or sets the preview URL of the uploaded content.
        /// </summary>
        public string PreviewUrl;
    }
}
