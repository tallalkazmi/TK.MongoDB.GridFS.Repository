using System;

namespace TK.MongoDB.GridFS.Classes
{
    /// <summary>
    /// The exception that is thrown when an input file name is not of the specified format.
    /// </summary>
    [Serializable]
    public class FileNameFormatException : Exception
    {
        /// <summary>
        /// The exception that is thrown when an input file name is not of the specified format.
        /// </summary>
        public FileNameFormatException()
        {
        }

        /// <summary>
        /// The exception that is thrown when an input file name is not of the specified format.
        /// </summary>
        /// <param name="filename">File name</param>
        public FileNameFormatException(string filename)
            : base($"File name '{filename}' is not of the desired format.")
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when an input file size is greater than the specified limit.
    /// </summary>
    [Serializable]
    public class FileSizeException : Exception
    {
        /// <summary>
        /// The exception that is thrown when an input file size is greater than the specified limit.
        /// </summary>
        public FileSizeException(int size)
            : base($"File size is too large, maximum allowed is {size} MB.")
        {
        }

        /// <summary>
        /// The exception that is thrown when an input file size is greater than the specified limit.
        /// </summary>
        /// <param name="message">Message</param>
        public FileSizeException(string message)
            : base(message)
        {
        }
    }
}
