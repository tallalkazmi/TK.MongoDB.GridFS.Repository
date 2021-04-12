namespace TK.MongoDB.GridFS
{
    /// <summary>
    /// Settings for MongoDB connection string
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i>.
        /// </summary>
        public static string ConnectionStringSettingName { get; set; } = "MongoDocConnection";
    }
}
