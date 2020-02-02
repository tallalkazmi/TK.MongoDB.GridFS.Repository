using MongoDB.Bson;
using TK.MongoDB.GridFS.Models;

namespace TK.MongoDB.GridFS.Test.Models
{
    public class Image : BaseFile<ObjectId>
    {
        public bool isDisplay { get; set; }
    }
}
