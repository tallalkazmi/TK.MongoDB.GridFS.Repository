using System;

namespace TK.MongoDB.GridFS.Models
{
    public abstract class BaseFile<T>
    {
        /// <summary>
        /// Primary Key. Generates new <c>ObjectId</c> on insert.
        /// </summary>
        public T Id { get; set; }
        
        /// <summary>
        /// Filename
        /// </summary>
        public string Filename { get; set; }
        
        /// <summary>
        /// File content in bytes
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Content-Type of the file inserted via <c>MimeMappingStealer</c> and retrived via <c>BsonValueConversion</c>
        /// </summary>
        public string ContentType { get; internal set; }
        
        /// <summary>
        /// Content-Length in number of bytes
        /// </summary>
        public long ContentLength { get; internal set; }

        /// <summary>
        /// File created on date. Automatically sets <c>DateTime.UtcNow</c> on insert.
        /// </summary>
        public DateTime UploadDateTime { get; internal set; }
    }
}
