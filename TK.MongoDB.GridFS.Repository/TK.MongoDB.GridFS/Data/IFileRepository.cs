using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace TK.MongoDB.GridFS.Data
{
    /// <summary>
    /// File Repository
    /// </summary>
    /// <typeparam name="T">Type of BaseFile</typeparam>
    public interface IFileRepository<T> : IDisposable where T : BaseFile
    {
        /// <summary>
        /// Drops Bucket
        /// </summary>
        /// <returns></returns>
        void DropBucket();

        /// <summary>
        /// Drops Bucket
        /// </summary>
        /// <returns></returns>
        void DropBucketAsync();

        /// <summary>
        /// Gets a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns>Matching file</returns>
        T Get(ObjectId id);

        /// <summary>
        /// Gets a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns>Matching file</returns>
        Task<T> GetAsync(ObjectId id);

        /// <summary>
        /// Gets all file with specified filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(string filename);

        /// <summary>
        /// Gets all file with specified filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Matching files</returns>
        Task<IEnumerable<T>> GetAsync(string filename);

        /// <summary>
        /// Gets all files
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(Expression<Func<GridFSFileInfo<ObjectId>, bool>> condition);

        /// <summary>
        /// Gets all files
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Matching files</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<GridFSFileInfo<ObjectId>, bool>> condition);

        /// <summary>
        /// Gets all files
        /// </summary>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort Definition</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> Get(FilterDefinition<GridFSFileInfo> filter, SortDefinition<GridFSFileInfo> sort);

        /// <summary>
        /// Gets all files
        /// </summary>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort Definition</param>
        /// <returns>Matching files</returns>
        Task<IEnumerable<T>> GetAsync(FilterDefinition<GridFSFileInfo> filter, SortDefinition<GridFSFileInfo> sort);

        /// <summary>
        /// Inserts a single file
        /// </summary>
        /// <param name="obj">File object</param>
        /// <returns>Inserted file's ObjectId</returns>
        string Insert(T obj);

        /// <summary>
        /// Inserts a single file
        /// </summary>
        /// <param name="obj">File object</param>
        /// <returns>Inserted file's ObjectId</returns>
        Task<string> InsertAsync(T obj);

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="id">Id of the file to rename</param>
        /// <param name="newFilename">New filename</param>
        void Rename(ObjectId id, string newFilename);

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="id">Id of the file to rename</param>
        /// <param name="newFilename">New filename</param>
        Task RenameAsync(ObjectId id, string newFilename);

        /// <summary>
        /// Deletes a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns></returns>
        void Delete(ObjectId id);

        /// <summary>
        /// Deletes a single file by Id
        /// </summary>
        /// <param name="id">ObjectId</param>
        /// <returns></returns>
        Task DeleteAsync(ObjectId id);

        /// <summary>
        /// Gets files with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> In<TField>(Expression<Func<GridFSFileInfo, TField>> field, IEnumerable<TField> values) where TField : class;

        /// <summary>
        /// Gets files with In filter.
        /// </summary>
        /// <typeparam name="TField">Field type to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        Task<IEnumerable<T>> InAsync<TField>(Expression<Func<GridFSFileInfo, TField>> field, IEnumerable<TField> values) where TField : class;

        /// <summary>
        /// Gets files with In (ObjectId) filter.
        /// </summary>
        /// <typeparam name="ObjectId">ObjectId to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        IEnumerable<T> InObjectId<ObjectId>(Expression<Func<GridFSFileInfo, ObjectId>> field, IEnumerable<ObjectId> values);

        /// <summary>
        /// Gets files with In (ObjectId) filter.
        /// </summary>
        /// <typeparam name="ObjectId">ObjectId to search in</typeparam>
        /// <param name="field">Field name to search in</param>
        /// <param name="values">Values to search in</param>
        /// <returns>Matching files</returns>
        Task<IEnumerable<T>> InObjectIdAsync<ObjectId>(Expression<Func<GridFSFileInfo, ObjectId>> field, IEnumerable<ObjectId> values);
    }
}
