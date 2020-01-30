namespace TK.MongoDB.GridFS.Models
{
    public interface IBaseFile
    {
        string Id { get; set; }
        string Filename { get; set; }
        byte[] Content { get; set; }
        string ContentType { get; set; }
        long ContentLength { get; set; }
    }
}
