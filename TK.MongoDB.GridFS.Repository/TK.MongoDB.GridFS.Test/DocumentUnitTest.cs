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
            Settings.Configure<Document>(2);
        }

        [TestMethod]
        public void GetById()
        {
            try
            {
                Document file = DocumentRepository.Get(new ObjectId("5e36b5e698d2c103d438e163"));
                Assert.IsNotNull(file);
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
            Assert.IsNotNull(files);

            Console.WriteLine($"Output:\n{(files.Count() > 0 ? string.Join(", ", files.Select(x => x.Filename)) : "No record found")}");
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
            Assert.AreNotEqual(string.Empty, Id);
            Assert.AreNotEqual(null, Id);

            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                DocumentRepository.Delete(new ObjectId("5e36b9d698d2c124886edc67"));
                Console.WriteLine($"Output:\nFile with Id '5e36b9d698d2c124886edc67' deleted.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }
    }
}
