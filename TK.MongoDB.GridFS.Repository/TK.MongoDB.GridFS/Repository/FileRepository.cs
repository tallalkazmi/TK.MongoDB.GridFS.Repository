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
using System.Linq.Expressions;

namespace TK.MongoDB.GridFS.Repository
{
    public class FileRepository<T> : Settings, IFileRepository<T> where T : BaseFile<ObjectId>
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

            BaseObjectType = typeof(BaseFile<ObjectId>);
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
        /// Gets all files
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching files</returns>
        public IEnumerable<T> Get(Expression<Func<GridFSFileInfo<ObjectId>, bool>> condition)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var files = Bucket.Find(condition, new GridFSFindOptions { Sort = sort }).ToList();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = Bucket.DownloadAsBytes(file.Id);
                returnObject.ContentLength = (long)BsonValueConversion.Convert(meta?.GetElement("ContentLength").Value);
                returnObject.ContentType = (string)BsonValueConversion.Convert(meta?.GetElement("ContentType").Value);
                returnObject.UploadDateTime = file.UploadDateTime;

                foreach (var prop in _props)
                {
                    var data = meta?.GetElement(prop.Name).Value;
                    if (data == null) continue;

                    var value = BsonValueConversion.Convert(data);
                    prop.SetValue(returnObject, value);
                }
                returnList.Add(returnObject);
            }
            return returnList;
        }

        /// <summary>
        /// Gets a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns>Matching file</returns>
        public T Get(ObjectId id)
        {            
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
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            using (var cursor = Bucket.Find(filter))
            {
                //fileInfo either has the matching file information or is null
                GridFSFileInfo fileInfo = cursor.FirstOrDefault();
                if (fileInfo != null)
                {
                    BsonDocument meta = fileInfo.Metadata;
                    T returnObject = (T)Activator.CreateInstance(ObjectType);
                    returnObject.Id = fileInfo.Id;
                    returnObject.Filename = fileInfo.Filename;
                    returnObject.Content = Bucket.DownloadAsBytes(id);
                    returnObject.UploadDateTime = fileInfo.UploadDateTime;

                    bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                    if (elementFound.HasValue && elementFound.Value)
                        returnObject.ContentLength = (long)BsonValueConversion.Convert(element.Value);

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
                    throw new FileNotFoundException($"File Id '{id}' was not found in the store");
                }
            }
        }

        /// <summary>
        /// Gets all file with specified filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Matching files</returns>
        public IEnumerable<T> Get(string filename)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.Eq(x => x.Filename, filename);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var files = Bucket.Find(filter, new GridFSFindOptions { Sort = sort }).ToList();
            
            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = Bucket.DownloadAsBytes(file.Id);
                returnObject.UploadDateTime = file.UploadDateTime;

                bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                if (elementFound.HasValue && elementFound.Value)
                    returnObject.ContentLength = (long)BsonValueConversion.Convert(element.Value);

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
            return returnList;
        }

        /// <summary>
        /// Gets files with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        public IEnumerable<T> In<TField>(Expression<Func<GridFSFileInfo, TField>> field, IEnumerable<TField> values) where TField : class
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.In<TField>(field, values);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var files = Bucket.Find(filter, new GridFSFindOptions { Sort = sort }).ToList();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = Bucket.DownloadAsBytes(file.Id);
                returnObject.UploadDateTime = file.UploadDateTime;

                bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                if (elementFound.HasValue && elementFound.Value)
                    returnObject.ContentLength = (long)BsonValueConversion.Convert(element.Value);
                
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
            return returnList;
        }

        /// <summary>
        /// Gets files with In (ObjectId) filter.
        /// </summary>
        /// <typeparam name="ObjectId">ObjectId to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        public IEnumerable<T> InObjectId<ObjectId>(Expression<Func<GridFSFileInfo, ObjectId>> field, IEnumerable<ObjectId> values)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.In(field, values);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var files = Bucket.Find(filter, new GridFSFindOptions { Sort = sort }).ToList();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = Bucket.DownloadAsBytes(file.Id);
                returnObject.UploadDateTime = file.UploadDateTime;

                bool? elementFound = meta?.TryGetElement("ContentLength", out BsonElement element);
                if (elementFound.HasValue && elementFound.Value)
                    returnObject.ContentLength = (long)BsonValueConversion.Convert(element.Value);

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
            return returnList;
        }

        /// <summary>
        /// Inserts a single file
        /// </summary>
        /// <param name="obj">File object</param>
        /// <returns>Inserted file's ObjectId</returns>
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

        /// <summary>
        /// Inserts a single file with the <c>ObjectId</c> provided
        /// </summary>
        /// <param name="obj">File object</param>
        public void InsertWithId(T obj)
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
            Stream stream = new MemoryStream(obj.Content);
            bucket.UploadFromStream(obj.Id, obj.Filename, stream, options);
        }

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="id">Id of the file to rename</param>
        /// <param name="newFilename">New filename</param>
        public void Rename(ObjectId id, string newFilename)
        {
            bool IsValidFilename = Regex.IsMatch(newFilename, @"^[\w\-. ]+$", RegexOptions.IgnoreCase);
            if (!IsValidFilename)
                throw new ArgumentException("Filename", $"File name '{newFilename}' is not of the correct format.");

            Bucket.Rename(id, newFilename);
        }

        /// <summary>
        /// Deletes a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns></returns>
        public void Delete(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            var cursor = Bucket.Find(filter);
            var fileInfo = cursor.FirstOrDefault();
            if (fileInfo == null)
                throw new FileNotFoundException($"File Id '{id}' was not found in the store");

            Bucket.Delete(id);
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }
}
