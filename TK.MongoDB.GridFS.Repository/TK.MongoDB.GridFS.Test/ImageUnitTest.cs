using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using TK.MongoDB.GridFS.Classes;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    [TestClass]
    public class ImageUnitTest : BaseTest
    {
        public ImageUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
        }

        [TestMethod]
        public void Get()
        {
            IEnumerable<Image> files = ImageRepository.Get(x => x.Filename.Contains("Omega") && x.UploadDateTime < DateTime.UtcNow.AddDays(-1));
            Assert.IsNotNull(files);
            Console.WriteLine($"Output:\n {(files.Count() > 0 ? string.Join(", ", files.Select(x => x.Filename)) : "No record found")}");
        }

        [TestMethod]
        public void GetById()
        {
            try
            {
                Image file = ImageRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
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
            IEnumerable<Image> files = ImageRepository.Get("Omega1.png");
            Assert.IsNotNull(files);

            Console.WriteLine($"Output:\n{(files.Count() > 0 ? string.Join(", ", files.Select(x => x.Filename)) : "No record found")}");
        }

        [TestMethod]
        public void Insert()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");
            DateTime now = DateTime.UtcNow;
            Image img = new Image()
            {
                Filename = $"Omega-{now.Year}{now.Month:D2}{now.Day:D2}.png",
                Content = fileContent,
                IsDisplay = false
            };

            string Id = ImageRepository.Insert(img);
            Assert.AreNotEqual(string.Empty, Id);
            Assert.AreNotEqual(null, Id);

            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void InsertLargeFile()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/LargeFile.jpg");

            DateTime now = DateTime.UtcNow;
            Image img = new Image()
            {
                Filename = $"LargeFile-{now.Year}{now.Month:D2}{now.Day:D2}.jpg",
                Content = fileContent,
                IsDisplay = false
            };

            Assert.ThrowsException<FileSizeException>(() => { string id = ImageRepository.Insert(img); });
        }

        [TestMethod]
        public void InsertWithId()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");
            DateTime now = DateTime.UtcNow;
            Image img = new Image()
            {
                Id = ObjectId.GenerateNewId(),
                Filename = $"Omega-{now.Year}{now.Month:D2}{now.Day:D2}.png",
                Content = fileContent,
                IsDisplay = false
            };

            string Id = ImageRepository.Insert(img);
            Assert.AreNotEqual(string.Empty, Id);
            Assert.AreNotEqual(null, Id);

            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void Rename()
        {
            try
            {
                ImageRepository.Rename(new ObjectId("5e37cdcf98d2c12ba0231fbb"), "Omega-new.png");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                ImageRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
                Console.WriteLine($"Output:\nFile with Id '5e36b5a698d2c14fe8b0ecbe' deleted.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }
    }
}
