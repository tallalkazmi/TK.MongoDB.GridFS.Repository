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
            IEnumerable<Image> files = imgRepository.Get(x => x.Filename.Contains("Omega") && x.UploadDateTime < DateTime.UtcNow.AddDays(-1));
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void GetById()
        {
            try
            {
                Image file = imgRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
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
            IEnumerable<Image> files = imgRepository.Get("Omega1.png");
            Console.WriteLine($"Output:\n{string.Join(", ", files.Select(x => x.Filename))}");
        }

        [TestMethod]
        public void Insert()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");
            DateTime now = DateTime.UtcNow;
            Image img = new Image()
            {
                Filename = $"Omega-{now.Year}{now.Month.ToString("D2")}{now.Day.ToString("D2")}.png",
                Content = fileContent,
                isDisplay = false
            };

            string Id = imgRepository.Insert(img);
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
                Filename = $"Omega-{now.Year}{now.Month.ToString("D2")}{now.Day.ToString("D2")}.png",
                Content = fileContent,
                isDisplay = false
            };

            imgRepository.InsertWithId(img);
            Console.WriteLine($"Inserted document Id: {img.Id}");
        }

        [TestMethod]
        public void Rename()
        {
            imgRepository.Rename(new ObjectId("5e37cdcf98d2c12ba0231fbb"), "Omega-new.png");
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                imgRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Output:\n{ex.Message}");
            }
        }
    }
}
