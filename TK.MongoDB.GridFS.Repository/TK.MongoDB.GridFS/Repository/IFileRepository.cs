using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;

namespace TK.MongoDB.GridFS.Repository
{
    public interface IFileRepository<T> : IDisposable where T : BaseFile<ObjectId>
    {
        /// <summary>
        /// Resets/Initializes Bucket
        /// </summary>
        /// <returns></returns>
        void InitBucket();

        /// <summary>
        /// Gets all files
        /// </summary>
        /// <param name="condition">Search filter</param>
        /// <param name="options">Search options</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(FilterDefinition<GridFSFileInfo<ObjectId>> condition, GridFSFindOptions options = null);

        /// <summary>
        /// Gets a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns>Matching file</returns>
        T Get(ObjectId id);

        /// <summary>
        /// Gets all file with specified filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(string filename);

        /// <summary>
        /// Inserts a single file
        /// </summary>
        /// <param name="obj">File object</param>
        /// <returns>Inserted file's ObjectId</returns>
        string Insert(T obj);

        /// <summary>
        /// Deletes a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns></returns>
        void Delete(ObjectId id);
    }
}
