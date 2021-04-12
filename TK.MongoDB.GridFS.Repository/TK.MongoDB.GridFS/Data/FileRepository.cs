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
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.GridFS.Attributes;
using System.Text.RegularExpressions;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace TK.MongoDB.GridFS.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class FileRepository<T> : Settings, IFileRepository<T> where T : BaseFile
    {
        private readonly MongoDbContext Context;
        private readonly Type ObjectType;
        private readonly Type BaseObjectType;
        private readonly PropertyInfo[] ObjectProps;
        private readonly PropertyInfo[] BaseObjectProps;

        private bool PluralizeBucketName;
        private bool ValidateFileName;
        private bool ValidateFileSize;
        private Regex FileNameRegex;
        private int MaximumFileSizeInMBs;
        private int BucketChunkSizeInMBs;

        protected IGridFSBucket Bucket { get; private set; }

        public FileRepository()
        {
            BaseObjectType = typeof(BaseFile);
            ObjectType = typeof(T);
            SetBucketAttributes(ObjectType);

            if (Context == null) Context = new MongoDbContext(ConnectionStringSettingName);
            if (Bucket == null)
            {
                string BucketName;
                if (PluralizeBucketName)
                {
                    string _bucketName = ObjectType.Name.ToLower();
                    PluralizationService ps = PluralizationService.CreateService(new CultureInfo("en-us"));
                    BucketName = ps.IsSingular(_bucketName) ? ps.Pluralize(_bucketName) : _bucketName;
                }
                else BucketName = ObjectType.Name.ToLower();

                Bucket = new GridFSBucket(Context.Database, new GridFSBucketOptions
                {
                    BucketName = BucketName,
                    ChunkSizeBytes = (int)Math.Pow(1024, 2) * BucketChunkSizeInMBs,
                    WriteConcern = WriteConcern.WMajority,
                    ReadPreference = ReadPreference.Secondary
                });
            }

            if (ObjectType.IsGenericType && ObjectType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ObjectType = ObjectType.GenericTypeArguments[0];

            BaseObjectProps = BaseObjectType.GetProperties();
            ObjectProps = ObjectType.GetProperties();
        }

        public void DropBucket()
        {
            Bucket.Drop();
        }
        public async void DropBucketAsync()
        {
            await Bucket.DropAsync();
        }

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
        public async Task<T> GetAsync(ObjectId id)
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
            using (var cursor = await Bucket.FindAsync(filter))
            {
                //fileInfo either has the matching file information or is null
                GridFSFileInfo fileInfo = await cursor.FirstOrDefaultAsync();
                if (fileInfo != null)
                {
                    BsonDocument meta = fileInfo.Metadata;
                    T returnObject = (T)Activator.CreateInstance(ObjectType);
                    returnObject.Id = fileInfo.Id;
                    returnObject.Filename = fileInfo.Filename;
                    returnObject.Content = await Bucket.DownloadAsBytesAsync(id);
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
        public async Task<IEnumerable<T>> GetAsync(string filename)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.Eq(x => x.Filename, filename);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var _files = await Bucket.FindAsync(filter, new GridFSFindOptions { Sort = sort });
            var files = await _files.ToListAsync();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = await Bucket.DownloadAsBytesAsync(file.Id);
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
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<GridFSFileInfo<ObjectId>, bool>> condition)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var _files = await Bucket.FindAsync(condition, new GridFSFindOptions { Sort = sort });
            var files = await _files.ToListAsync();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = await Bucket.DownloadAsBytesAsync(file.Id);
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

        public IEnumerable<T> Get(FilterDefinition<GridFSFileInfo> filter, SortDefinition<GridFSFileInfo> sort)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var files = Bucket.Find(filter, new GridFSFindOptions { Sort = sort }).ToList();

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
        public async Task<IEnumerable<T>> GetAsync(FilterDefinition<GridFSFileInfo> filter, SortDefinition<GridFSFileInfo> sort)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var _files = await Bucket.FindAsync(filter, new GridFSFindOptions { Sort = sort });
            var files = await _files.ToListAsync();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = await Bucket.DownloadAsBytesAsync(file.Id);
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

        public virtual string Insert(T obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Filename)) throw new ArgumentNullException("Filename", "File name cannot be null.");
            if (obj.Content == null || obj.Content.Length == 0) throw new ArgumentNullException("Content", "File content cannot by null or empty.");

            if (ValidateFileSize)
            {
                double size_limit = Math.Pow(1024, 2) * MaximumFileSizeInMBs;
                bool isExceeding = obj.Content.Length > size_limit;
                if (isExceeding) throw new FileSizeException(MaximumFileSizeInMBs);
            }

            if (ValidateFileName)
            {
                bool IsValidFilename = FileNameRegex.IsMatch(obj.Filename);
                if (!IsValidFilename) throw new FileNameFormatException(obj.Filename);
            }

            string FileContentType = MimeMappingStealer.GetMimeMapping(obj.Filename);
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));

            List<KeyValuePair<string, object>> dictionary = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ContentType", FileContentType),
                new KeyValuePair<string, object>("ContentLength", obj.Content.Length)
            };

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

            if (obj.Id == ObjectId.Empty)
            {
                var FileId = bucket.UploadFromBytes(obj.Filename, obj.Content, options);
                return FileId.ToString();
            }
            else
            {
                Stream stream = new MemoryStream(obj.Content);
                bucket.UploadFromStream(obj.Id, obj.Filename, stream, options);
                return obj.Id.ToString();
            }
        }
        public virtual async Task<string> InsertAsync(T obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Filename)) throw new ArgumentNullException("Filename", "File name cannot be null.");
            if (obj.Content == null || obj.Content.Length == 0) throw new ArgumentNullException("Content", "File content cannot by null or empty.");

            if (ValidateFileSize)
            {
                double size_limit = Math.Pow(1024, 2) * MaximumFileSizeInMBs;
                bool isExceeding = obj.Content.Length > size_limit;
                if (isExceeding) throw new FileSizeException(MaximumFileSizeInMBs);
            }

            if (ValidateFileName)
            {
                bool IsValidFilename = FileNameRegex.IsMatch(obj.Filename);
                if (!IsValidFilename) throw new FileNameFormatException(obj.Filename);
            }

            string FileContentType = MimeMappingStealer.GetMimeMapping(obj.Filename);
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));

            List<KeyValuePair<string, object>> dictionary = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ContentType", FileContentType),
                new KeyValuePair<string, object>("ContentLength", obj.Content.Length)
            };

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

            if (obj.Id == ObjectId.Empty)
            {
                var FileId = await bucket.UploadFromBytesAsync(obj.Filename, obj.Content, options);
                return FileId.ToString();
            }
            else
            {
                Stream stream = new MemoryStream(obj.Content);
                await bucket.UploadFromStreamAsync(obj.Id, obj.Filename, stream, options);
                return obj.Id.ToString();
            }
        }

        public virtual void Rename(ObjectId id, string newFilename)
        {
            if (ValidateFileName)
            {
                bool IsValidFilename = FileNameRegex.IsMatch(newFilename);
                if (!IsValidFilename) throw new FileNameFormatException(newFilename);
            }

            try
            {
                Bucket.Rename(id, newFilename);
            }
            catch (GridFSFileNotFoundException fnfex)
            {
                throw new FileNotFoundException(fnfex.Message, fnfex);
            }
        }
        public virtual async Task RenameAsync(ObjectId id, string newFilename)
        {
            if (ValidateFileName)
            {
                bool IsValidFilename = FileNameRegex.IsMatch(newFilename);
                if (!IsValidFilename) throw new FileNameFormatException(newFilename);
            }

            try
            {
                await Bucket.RenameAsync(id, newFilename);
            }
            catch (GridFSFileNotFoundException fnfex)
            {
                throw new FileNotFoundException(fnfex.Message, fnfex);
            }
        }

        public virtual void Delete(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            var cursor = Bucket.Find(filter);
            var fileInfo = cursor.FirstOrDefault();
            if (fileInfo == null)
                throw new FileNotFoundException($"File Id '{id}' was not found in the store");

            Bucket.Delete(id);
        }
        public virtual async Task DeleteAsync(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            var cursor = await Bucket.FindAsync(filter);
            var fileInfo = await cursor.FirstOrDefaultAsync();
            if (fileInfo == null)
                throw new FileNotFoundException($"File Id '{id}' was not found in the store");

            await Bucket.DeleteAsync(id);
        }

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
        public async Task<IEnumerable<T>> InAsync<TField>(Expression<Func<GridFSFileInfo, TField>> field, IEnumerable<TField> values) where TField : class
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.In<TField>(field, values);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var _files = await Bucket.FindAsync(filter, new GridFSFindOptions { Sort = sort });
            var files = await _files.ToListAsync();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = await Bucket.DownloadAsBytesAsync(file.Id);
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
        public async Task<IEnumerable<T>> InObjectIdAsync<ObjectId>(Expression<Func<GridFSFileInfo, ObjectId>> field, IEnumerable<ObjectId> values)
        {
            var _props = ObjectProps.Where(p => !BaseObjectProps.Any(bp => bp.Name == p.Name));
            List<T> returnList = new List<T>();

            var builder = Builders<GridFSFileInfo>.Filter;
            var filter = builder.In(field, values);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var _files = await Bucket.FindAsync(filter, new GridFSFindOptions { Sort = sort });
            var files = await _files.ToListAsync();

            foreach (var file in files)
            {
                BsonDocument meta = file.Metadata;
                T returnObject = (T)Activator.CreateInstance(ObjectType);
                returnObject.Id = file.Id;
                returnObject.Filename = file.Filename;
                returnObject.Content = await Bucket.DownloadAsBytesAsync(file.Id);
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

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }

        private void SetBucketAttributes(Type t)
        {
            BucketAttribute bucketAttribute = (BucketAttribute)Attribute.GetCustomAttribute(t, typeof(BucketAttribute));
            if (bucketAttribute != null)
            {
                PluralizeBucketName = bucketAttribute.PluralizeBucketName;
                ValidateFileName = bucketAttribute.ValidateFileName;
                ValidateFileSize = bucketAttribute.ValidateFileSize;
                FileNameRegex = bucketAttribute.FileNameRegex;
                MaximumFileSizeInMBs = bucketAttribute.MaximumFileSizeInMBs;
                BucketChunkSizeInMBs = bucketAttribute.BucketChunkSizeInMBs;
            }
            else
            {
                bucketAttribute = new BucketAttribute();
                PluralizeBucketName = bucketAttribute.PluralizeBucketName;
                ValidateFileName = bucketAttribute.ValidateFileName;
                ValidateFileSize = bucketAttribute.ValidateFileSize;
                FileNameRegex = bucketAttribute.FileNameRegex;
                MaximumFileSizeInMBs = bucketAttribute.MaximumFileSizeInMBs;
                BucketChunkSizeInMBs = bucketAttribute.BucketChunkSizeInMBs;
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
