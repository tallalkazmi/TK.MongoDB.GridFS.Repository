using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using TK.MongoDB.GridFS.Repository;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    [TestClass]
    public class ImageUnitTest
    {
        readonly FileRepository<Image> imgRepository;
        public ImageUnitTest()
        {
            Settings.Configure(2097152, "MongoDocConnection");
            imgRepository = new FileRepository<Image>();
        }

        [TestMethod]
        public void Get()
        {
            //Search filters
            var filter = Builders<GridFSFileInfo<ObjectId>>.Filter.And(
                Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(x => x.Filename, "securityvideo"),
                Builders<GridFSFileInfo<ObjectId>>.Filter.Gte(x => x.UploadDateTime, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                Builders<GridFSFileInfo<ObjectId>>.Filter.Lt(x => x.UploadDateTime, new DateTime(2015, 2, 1, 0, 0, 0, DateTimeKind.Utc)));

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);

            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };

            IEnumerable<Image> files = imgRepository.Get(filter, options);
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void GetById()
        {
            Image file = imgRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
            Console.WriteLine($"Output:\n{file.Filename}");
        }

        public void GetByFilename()
        {
            IEnumerable<Image> files = imgRepository.Get("Omega1.png");
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void Insert()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");
            Image img = new Image()
            {
                Filename = "Omega.png",
                Content = fileContent,
                isDisplay = false
            };

            string Id = imgRepository.Insert(img);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void Delete()
        {
            imgRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
        }
    }
}
