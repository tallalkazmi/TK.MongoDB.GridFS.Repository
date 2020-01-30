using TK.MongoDB.GridFS.Classes;
using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TK.MongoDB.GridFS.Repository
{
    public class FileRepository<T> : Settings, IFileRepository<T>
        where T : class, IBaseFile
    {
        protected MongoDbContext Context;
        protected string BucketName { get; private set; }
        protected IGridFSBucket Bucket { get; set; }

        private readonly Type ObjectType;
        private readonly Type BaseObjectType;
        private readonly PropertyInfo[] ObjectProps;
        private readonly PropertyInfo[] BaseObjectProps;

        public FileRepository()
        {
            if (Context == null)
                Context = new MongoDbContext(ConnectionStringSettingName);

            BucketName = typeof(T).Name.ToLower();
            if (Bucket == null)
            {
                Bucket = new GridFSBucket(Context.Database, new GridFSBucketOptions
                {
                    BucketName = BucketName,
                    ChunkSizeBytes = BucketChunkSizeBytes,
                    WriteConcern = WriteConcern.WMajority,
                    ReadPreference = ReadPreference.Secondary
                });
            }

            BaseObjectType = typeof(IBaseFile);
            ObjectType = typeof(T);
            if (ObjectType.IsGenericType && ObjectType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ObjectType = ObjectType.GenericTypeArguments[0];

            BaseObjectProps = BaseObjectType.GetProperties();
            ObjectProps = ObjectType.GetProperties();
        }

        /// <summary>
        /// Resets/Initializes Bucket
        /// </summary>
        /// <returns></returns>
        public async void InitBucket()
        {
            await Bucket.DropAsync();
        }

        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <param name="condition">Search filter</param>
        /// <param name="options">Search options</param>
        /// <returns>Matching documents</returns>
        public IEnumerable<T> Get(FilterDefinition<GridFSFileInfo<ObjectId>> condition, GridFSFindOptions options = null)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();
            using (var cursor = Bucket.Find(condition, options))
            {
                //fileInfo either has the matching file information or is null
                var files = cursor.ToList();
                foreach (var file in files)
                {
                    BsonDocument meta = file.Metadata;
                    T returnObject = (T)Activator.CreateInstance(ObjectType);
                    returnObject.Id = file.Id.ToString();
                    returnObject.Filename = file.Filename;
                    returnObject.Content = Bucket.DownloadAsBytes(file.Id);
                    returnObject.ContentLength = (long)BsonValueConversion.Convert(meta?.GetElement("ContentLength").Value);
                    returnObject.ContentType = (string)BsonValueConversion.Convert(meta?.GetElement("ContentType").Value);

                    foreach (var prop in _props)
                    {
                        var data = meta?.GetElement(prop.Name).Value;
                        if (data == null) continue;

                        var value = BsonValueConversion.Convert(data);
                        prop.SetValue(returnObject, value);
                    }
                    returnList.Add(returnObject);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Gets a single document by id
        /// </summary>
        /// <param name="id">ObjectId as string</param>
        /// <returns>Matching document</returns>
        public T GetById(string id)
        {
            ObjectId id_ = new ObjectId(id);
            
            //Search filters
            //IGridFSBucket bucket;
            //var filter = Builders<GridFSFileInfo>.Filter.And(
            //    Builders<GridFSFileInfo>.Filter.EQ(x => x.Filename, "securityvideo"),
            //    Builders<GridFSFileInfo>.Filter.GTE(x => x.UploadDateTime, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            //    Builders<GridFSFileInfo>.Filter.LT(x => x.UploadDateTime, new DateTime(2015, 2, 1, 0, 0, 0, DateTimeKind.Utc)));
            //var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            //var options = new GridFSFindOptions
            //{
            //    Limit = 1,
            //    Sort = sort
            //};

            //Get file information
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id_);
            using (var cursor = Bucket.Find(filter))
            {
                //fileInfo either has the matching file information or is null
                GridFSFileInfo fileInfo = cursor.FirstOrDefault();
                if (fileInfo != null)
                {
                    BsonDocument meta = fileInfo.Metadata;
                    T returnObject = (T)Activator.CreateInstance(ObjectType);
                    returnObject.Id = fileInfo.Id.ToString();
                    returnObject.Filename = fileInfo.Filename;
                    returnObject.Content = Bucket.DownloadAsBytes(id_);


                    bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                    if (elementFound.HasValue && elementFound.Value)
                    {
                        var _ConvertedCL = (int)BsonValueConversion.Convert(element.Value);
                        returnObject.ContentLength = _ConvertedCL;
                    }
                    returnObject.ContentType = (string)BsonValueConversion.Convert(meta?.GetElement("ContentType").Value);

                    var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
                    foreach (var prop in _props)
                    {
                        var data = meta?.GetElement(prop.Name).Value;
                        if (data == null) continue;

                        var value = BsonValueConversion.Convert(data);
                        prop.SetValue(returnObject, value);
                    }
                    return returnObject;
                }
                else
                {
                    throw new FileNotFoundException($"File Id '{id_}' was not found in the store");
                }
            }
        }

        /// <summary>
        /// Gets all documents with specified filename
        /// </summary>
        /// <param name="filename">Document filename</param>
        /// <returns>Matching documents</returns>
        public IEnumerable<T> GetByFileName(string filename)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, filename);
            using (var cursor = Bucket.Find(filter))
            {
                //fileInfo either has the matching file information or is null
                var files = cursor.ToList();
                foreach (var file in files)
                {
                    BsonDocument meta = file.Metadata;
                    T returnObject = (T)Activator.CreateInstance(ObjectType);
                    returnObject.Id = file.Id.ToString();
                    returnObject.Filename = file.Filename;
                    returnObject.Content = Bucket.DownloadAsBytes(file.Id);

                    bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                    if (elementFound.HasValue && elementFound.Value)
                    {
                        var _ConvertedCL = (int)BsonValueConversion.Convert(element.Value);
                        returnObject.ContentLength = _ConvertedCL;
                    }
                    returnObject.ContentType = (string)BsonValueConversion.Convert(meta?.GetElement("ContentType").Value);

                    foreach (var prop in _props)
                    {
                        var data = meta?.GetElement(prop.Name).Value;
                        if (data == null) continue;

                        var value = BsonValueConversion.Convert(data);
                        prop.SetValue(returnObject, value);
                    }
                    returnList.Add(returnObject);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Deletes a single document by id
        /// </summary>
        /// <param name="id">ObjectId as string</param>
        /// <returns></returns>
        public void Delete(string id)
        {
            ObjectId id_ = new ObjectId(id);
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id_);
            var cursor = Bucket.Find(filter);
            var fileInfo = cursor.FirstOrDefault();
            if (fileInfo == null)
                throw new FileNotFoundException($"File Id '{id_}' was not found in the store");

            Bucket.Delete(id_);
        }

        /// <summary>
        /// Inserts a single document
        /// </summary>
        /// <param name="obj">Document object</param>
        /// <returns>Inserted document's ObjectId as string</returns>
        public string Insert(T obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Filename))
                throw new ArgumentNullException("Filename", "File name cannot be null.");
            if (obj.Content == null || obj.Content.Length == 0)
                throw new ArgumentNullException("Content", "File content cannot by null or empty.");

            //bool IsValidFilename = Regex.IsMatch(obj.Filename, @"^[\w,\s-]+\.[A-Za-z0-9]+$", RegexOptions.IgnoreCase);
            bool IsValidFilename = Regex.IsMatch(obj.Filename, @"^[\w\-. ]+$", RegexOptions.IgnoreCase);
            if (!IsValidFilename)
                throw new ArgumentException("Filename", $"File name '{obj.Filename}' is not of the correct format.");

            string FileContentType = MimeMappingStealer.GetMimeMapping(obj.Filename);
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));

            List<KeyValuePair<string, object>> dictionary = new List<KeyValuePair<string, object>>();
            dictionary.Add(new KeyValuePair<string, object>("ContentType", FileContentType));
            dictionary.Add(new KeyValuePair<string, object>("ContentLength", obj.Content.Length));
            foreach (var prop in _props)
            {
                if (prop.Name != "ContentType" && prop.Name != "ContentLength")
                {
                    object value = prop.GetValue(obj);
                    dictionary.Add(new KeyValuePair<string, object>(prop.Name, value));
                }
            }

            BsonDocument Metadata = new BsonDocument(dictionary);

            //Set upload options
            var options = new GridFSUploadOptions
            {
                Metadata = Metadata,
#pragma warning disable 618 //Obsolete warning removed
                //Adding content type here for database viewer. Do not remove.
                ContentType = FileContentType
#pragma warning restore 618
            };

            //Upload file
            IGridFSBucket bucket = Bucket;
            var FileId = bucket.UploadFromBytes(obj.Filename, obj.Content, options);
            return FileId.ToString();
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }
}
