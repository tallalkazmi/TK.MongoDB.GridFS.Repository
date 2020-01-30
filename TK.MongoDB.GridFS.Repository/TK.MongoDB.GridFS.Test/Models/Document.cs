using TK.MongoDB.GridFS.Models;
using System;

namespace TK.MongoDB.GridFS.Test.Models
{
    public class Document : IBaseFile
    {
        public string Id { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public string Filename { get; set; }

        public byte[] Content { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
