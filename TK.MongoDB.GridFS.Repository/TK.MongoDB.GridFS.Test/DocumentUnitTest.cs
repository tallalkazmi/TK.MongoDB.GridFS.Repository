using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using TK.MongoDB.GridFS.Repository;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    [TestClass]
    public class DocumentUnitTest
    {
        readonly FileRepository<Document> docRepository;
        public DocumentUnitTest()
        {
            //Settings.Configure(2097152);
            //Settings.Configure(connectionStringSettingName: "MongoDocConnection");
            Settings.Configure(2097152, "MongoDocConnection");
            docRepository = new FileRepository<Document>();
        }

        [TestMethod]
        public void GetById()
        {
            Document file = docRepository.Get(new ObjectId("5e36b5e698d2c103d438e163"));
            Console.WriteLine($"Output:\n{file.Filename}");
        }

        [TestMethod]
        public void GetByFilename()
        {
            IEnumerable<Document> files = docRepository.Get("sample.pdf");
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void Insert()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/sample.pdf");
            Document doc = new Document()
            {
                Filename = "sample.pdf",
                Content = fileContent,
                isPrivate = true
            };

            string Id = docRepository.Insert(doc);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void Delete()
        {
            docRepository.Delete(new ObjectId("5e36b9d698d2c124886edc67"));
        }
    }
}
