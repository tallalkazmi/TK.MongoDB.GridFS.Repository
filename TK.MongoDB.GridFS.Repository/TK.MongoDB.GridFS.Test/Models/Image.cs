using TK.MongoDB.GridFS.Models;

namespace TK.MongoDB.GridFS.Test.Models
{
    public class Image : IBaseFile
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string Dimensions { get; set; }
        public long ContentLength { get; set; }
    }
}
