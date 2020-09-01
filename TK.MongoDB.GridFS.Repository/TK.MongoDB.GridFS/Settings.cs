using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TK.MongoDB.GridFS.Models;

namespace TK.MongoDB.GridFS
{
    /// <summary>
    /// Settings for: <br/>
    /// 1. GridFs Bucket <br/>
    /// 2. File name and size validations <br/>
    /// 3. MongoDb connection string
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i>.
        /// </summary>
        public static string ConnectionStringSettingName { get; set; } = "MongoDocConnection";

        /// <summary>
        /// Validate file name on insert and update from <i>FileNameRegex</i> field. Default value is set to <i>True</i>.
        /// </summary>
        public static bool ValidateFileName { get; set; } = true;

        /// <summary>
        /// Validate file size on insert from <i>MaximumFileSizeInMBs</i> field. Default value is set to <i>True</i>.
        /// </summary>
        public static bool ValidateFileSize { get; set; } = true;

        /// <summary>
        /// File name Regex to validate. Default value is set to <i>Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase)</i>.
        /// </summary>
        public static Regex FileNameRegex { get; set; } = new Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Maximum file size in MBs. Default value is set to <i>5</i>.
        /// </summary>
        public static int MaximumFileSizeInMBs { get; set; } = 5;

        /* Bucket settings */
        protected internal static int _BucketChunkSizeInMBs = 2; //2097152 B
        protected internal static Dictionary<string, IGridFSBucket> _Buckets = new Dictionary<string, IGridFSBucket>();

        /// <summary>
        /// Configure GridFs bucket chunk size in MBs. Default value is set to <i>2 MB</i>.
        /// <typeparam name="T">Model of type <b>BaseFile</b>.</typeparam>
        /// <param name="chunkSize">Chunk size in MBs.</param>
        /// </summary>
        public static void Configure<T>(int chunkSize = 2) where T : BaseFile
        {
            MongoDbContext Context = new MongoDbContext(ConnectionStringSettingName);

            string BucketName = typeof(T).Name.ToLower();
            GridFSBucket Bucket = new GridFSBucket(Context.Database, new GridFSBucketOptions
            {
                BucketName = BucketName,
                ChunkSizeBytes = (int)Math.Pow(1024, 2) * chunkSize,
                WriteConcern = WriteConcern.WMajority,
                ReadPreference = ReadPreference.Secondary
            });

            if (!_Buckets.ContainsKey(BucketName)) _Buckets.Add(BucketName, Bucket);
        }
    }
}
