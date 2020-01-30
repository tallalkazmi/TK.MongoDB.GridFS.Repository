using TK.MongoDB.GridFS.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;

namespace TK.MongoDB.GridFS.Repository
{
    public interface IFileRepository<T> : IDisposable
        where T : class, IBaseFile
    {
        /// <summary>
        /// Resets/Initializes Bucket
        /// </summary>
        /// <returns></returns>
        void InitBucket();

        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <param name="condition">Search filter</param>
        /// <param name="options">Search options</param>
        /// <returns>Matching documents</returns>
        IEnumerable<T> Get(FilterDefinition<GridFSFileInfo<ObjectId>> condition, GridFSFindOptions options = null);
        
        /// <summary>
        /// Gets a single document by id
        /// </summary>
        /// <param name="id">ObjectId as string</param>
        /// <returns>Matching document</returns>
        T GetById(string id);
        
        /// <summary>
        /// Gets all documents with specified filename
        /// </summary>
        /// <param name="filename">Document filename</param>
        /// <returns>Matching documents</returns>
        IEnumerable<T> GetByFilename(string filename);
        
        /// <summary>
        /// Deletes a single document by id
        /// </summary>
        /// <param name="id">ObjectId as string</param>
        /// <returns></returns>
        void Delete(string id);
        
        /// <summary>
        /// Inserts a single document
        /// </summary>
        /// <param name="obj">Document object</param>
        /// <returns>Inserted document's ObjectId as string</returns>
        string Insert(T obj);
    }
}
