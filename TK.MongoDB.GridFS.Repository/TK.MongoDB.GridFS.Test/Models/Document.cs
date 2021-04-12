using TK.MongoDB.GridFS.Attributes;
using TK.MongoDB.GridFS.Models;

namespace TK.MongoDB.GridFS.Test.Models
{
    [Bucket(PluralizeBucketName = false, MaximumFileSizeInMBs = 1, BucketChunkSizeInMBs = 1)]
    public class Document : BaseFile
    {
        public bool IsPrivate { get; set; }
    }
}
