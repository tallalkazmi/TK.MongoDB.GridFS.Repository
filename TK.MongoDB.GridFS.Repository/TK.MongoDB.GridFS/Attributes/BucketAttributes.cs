using System;
using System.Text.RegularExpressions;

namespace TK.MongoDB.GridFS.Attributes
{
    /// <summary>
    /// Defines GridFS bucket attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BucketAttribute : Attribute
    {
        /// <summary>
        /// Pluralize bucket's mame.  Default value is set to <i>True</i>.
        /// </summary>
        public bool PluralizeBucketName { get; set; } = true;

        /// <summary>
        /// Validate file name on insert and update from <i>FileNameRegex</i> field. Default value is set to <i>True</i>.
        /// </summary>
        public bool ValidateFileName { get; set; } = true;

        /// <summary>
        /// Validate file size on insert from <i>MaximumFileSizeInMBs</i> field. Default value is set to <i>True</i>.
        /// </summary>
        public bool ValidateFileSize { get; set; } = true;

        /// <summary>
        /// File name Regex to validate. Default value is set to <i>Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase)</i>.
        /// </summary>
        public Regex FileNameRegex { get; set; } = new Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Maximum file size in MBs. Default value is set to <i>5</i>.
        /// </summary>
        public int MaximumFileSizeInMBs { get; set; } = 5;

        /// <summary>
        /// GridFS bucket chunk size in MBs. Default value is set to <i>2</i>.
        /// </summary>
        public int BucketChunkSizeInMBs { get; set; } = 2; //2097152 B
    }
}
