namespace TK.MongoDB.GridFS
{
    public class Settings
    {
        protected static string ConnectionStringSettingName = "MongoDocConnection";
        protected static int BucketChunkSizeBytes = 2097152; //2MB
        
        /// <summary>
        /// Configure connection string and bucket chunk size
        /// </summary>
        /// <param name="bucketChunkSizeBytes">GridFs Bucket chunk size. Default is set to 2097152</param>
        /// <param name="connectionStringSettingName">Connection String name from *.config file</param>
        public static void Configure(int bucketChunkSizeBytes = 2097152, string connectionStringSettingName = null)
        {
            if (!string.IsNullOrWhiteSpace(connectionStringSettingName)) ConnectionStringSettingName = connectionStringSettingName;
            BucketChunkSizeBytes = bucketChunkSizeBytes;
        }
    }
}
