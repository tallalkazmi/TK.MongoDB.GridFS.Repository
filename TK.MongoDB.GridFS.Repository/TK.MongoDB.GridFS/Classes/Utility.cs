using MongoDB.Bson;
using System;
using System.Reflection;
using System.Web;

namespace TK.MongoDB.GridFS.Classes
{
    /// <summary>
    /// Exposes the Mime Mapping method that Microsoft hid from us.
    /// </summary>
    public static class MimeMappingStealer
    {
        // The get mime mapping method info
        private static readonly MethodInfo _getMimeMappingMethod = null;
        /// <summary>
        /// Static constructor sets up reflection.
        /// </summary>
        static MimeMappingStealer()
        {
            // Load hidden mime mapping class and method from System.Web
            var assembly = Assembly.GetAssembly(typeof(HttpApplication));
            Type mimeMappingType = assembly.GetType("System.Web.MimeMapping");
            _getMimeMappingMethod = mimeMappingType.GetMethod("GetMimeMapping",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }
        /// <summary>
        /// Exposes the hidden Mime mapping method.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The mime mapping.</returns>
        public static string GetMimeMapping(string fileName)
        {
            return (string)_getMimeMappingMethod.Invoke(null /*static method*/, new[] { fileName });
        }
    }

    /// <summary>
    /// BsonValue conversion
    /// </summary>
    public static class BsonValueConversion
    {
        /// <summary>
        /// Converts BsonValue to C# equivalent type
        /// </summary>
        /// <param name="bsonValue">BsonValue</param>
        /// <returns>C# type</returns>
        public static object Convert(BsonValue bsonValue)
        {
            if (bsonValue.IsString)
                return System.Convert.ToString(bsonValue);
            //if (bsonValue.IsInt32)
            //    return System.Convert.ToInt32(bsonValue);
            if (bsonValue.IsInt32 || bsonValue.IsInt64 || bsonValue.IsDouble)
                return System.Convert.ToInt64(bsonValue);
            if (bsonValue.IsBoolean)
                return System.Convert.ToBoolean(bsonValue);
            if (bsonValue.IsValidDateTime)
                return System.Convert.ToDateTime(bsonValue);
            if (bsonValue.IsDecimal128)
                return System.Convert.ToDecimal(bsonValue);
            if (bsonValue.IsObjectId)
                return System.Convert.ToString(bsonValue);
            else
                return null;
        }
    }
}
