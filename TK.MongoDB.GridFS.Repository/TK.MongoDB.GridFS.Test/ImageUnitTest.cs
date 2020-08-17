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
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void GetById()
        {
            try
            {
                Image file = ImageRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
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
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
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
            Console.WriteLine($"Inserted document Id: {Id}");
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

            ImageRepository.Insert(img);
            Console.WriteLine($"Inserted document Id: {img.Id}");
        }

        //[TestMethod]
        public void Rename()
        {
            ImageRepository.Rename(new ObjectId("5e37cdcf98d2c12ba0231fbb"), "Omega-new.png");
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                ImageRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }
    }
}
