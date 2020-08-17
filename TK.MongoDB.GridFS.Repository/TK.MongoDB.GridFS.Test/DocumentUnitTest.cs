using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    [TestClass]
    public class DocumentUnitTest : BaseTest
    {
        public DocumentUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
        }

        [TestMethod]
        public void GetById()
        {
            try
            {
                Document file = DocumentRepository.Get(new ObjectId("5e36b5e698d2c103d438e163"));
                Console.WriteLine($"Output:\n{file.Filename}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }

        [TestMethod]
        public void GetByFilename()
        {
            IEnumerable<Document> files = DocumentRepository.Get("sample.pdf");
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
                IsPrivate = true
            };

            string Id = DocumentRepository.Insert(doc);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                DocumentRepository.Delete(new ObjectId("5e36b9d698d2c124886edc67"));
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }
    }
}
