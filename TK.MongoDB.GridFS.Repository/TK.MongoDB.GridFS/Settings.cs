using System.Text.RegularExpressions;

namespace TK.MongoDB.GridFS
{
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i>.
        /// </summary>
        public static string ConnectionStringSettingName { get; set; } = "MongoDocConnection";

        /// <summary>
        /// GridFs bucket chunk size in MBs. Default value is set to <i>2 MB</i>.
        /// </summary>
        public static int BucketChunkSizeInMBs { get; set; } = 2; //2097152 B

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
    }
}
