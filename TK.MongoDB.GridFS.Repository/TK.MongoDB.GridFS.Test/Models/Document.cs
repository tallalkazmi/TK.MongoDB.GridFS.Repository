using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;

namespace TK.MongoDB.GridFS.Test.Models
{
    public class Document : BaseFile<ObjectId>
    {
        public bool isPrivate { get; set; }
    }
}
