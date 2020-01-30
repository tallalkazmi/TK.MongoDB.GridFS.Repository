namespace TK.MongoDB.GridFS
{
    public class Settings
    {
        protected static string ConnectionStringSettingName;
        protected static int BucketChunkSizeBytes;
        /// <summary>
        /// Default settings
        /// </summary>
        public Settings()
        {
            ConnectionStringSettingName = "MongoDocConnection";
            BucketChunkSizeBytes = 2097152; //2MB
        }

        /// <summary>
        /// Configure connection string and bucket chunk size
        /// </summary>
        /// <param name="connectionStringSettingName">Connection String name from *.config file</param>
        /// <param name="bucketChunkSizeBytes">GridFs Bucket chunk size</param>
        public static void Configure(string connectionStringSettingName, int bucketChunkSizeBytes)
        {
            ConnectionStringSettingName = connectionStringSettingName;
            BucketChunkSizeBytes = bucketChunkSizeBytes;
        }
    }
}
