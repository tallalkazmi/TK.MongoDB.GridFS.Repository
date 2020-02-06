using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(Expression<Func<GridFSFileInfo<ObjectId>, bool>> condition);

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
        /// Gets files with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> In<TField>(Expression<Func<GridFSFileInfo, TField>> field, IEnumerable<TField> values) where TField : class;

        /// <summary>
        /// Inserts a single file
        /// </summary>
        /// <param name="obj">File object</param>
        /// <returns>Inserted file's ObjectId</returns>
        string Insert(T obj);

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="id">Id of the file to rename</param>
        /// <param name="newFilename">New filename</param>
        void Rename(ObjectId id, string newFilename);

        /// <summary>
        /// Deletes a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns></returns>
        void Delete(ObjectId id);
    }
}
