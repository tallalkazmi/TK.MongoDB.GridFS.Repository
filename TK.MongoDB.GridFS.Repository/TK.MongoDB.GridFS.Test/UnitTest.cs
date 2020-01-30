using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TK.MongoDB.GridFS.Repository;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    [TestClass]
    public class UnitTest
    {
        public UnitTest()
        {
            //Settings.Configure(2097152);
            //Settings.Configure(connectionStringSettingName: "MongoDocConnection");
            Settings.Configure(2097152, "MongoDocConnection");
        }

        //Get
        [TestMethod]
        public void GetImageById()
        {
            FileRepository<Image> imgRepository = new FileRepository<Image>();
            var file = imgRepository.GetById("5c63cb9998d2c42d405279fa");
        }

        public void GetImageByFilename()
        {
            FileRepository<Image> imgRepository = new FileRepository<Image>();
            var file = imgRepository.GetByFilename("Omega1.png");
        }

        //Insert
        [TestMethod]
        public void InsertDocument()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/sample.pdf");
            Document doc = new Document()
            {
                Filename = "sample.pdf",
                Content = fileContent,
                UploadDate = DateTime.UtcNow
            };

            FileRepository<Document> docRepository = new FileRepository<Document>();
            string Id = docRepository.Insert(doc);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void InsertImage()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");

            string Dimensions = null;
            using (MemoryStream ms = new MemoryStream(fileContent))
            {
                using (System.Drawing.Image _img = System.Drawing.Image.FromStream(ms))
                {
                    Dimensions = $"{_img.Width} x {_img.Height}";
                }
            }

            Image img = new Image()
            {
                Filename = "Omega.png",
                Content = fileContent,
                Dimensions = Dimensions
            };

            FileRepository<Image> imgRepository = new FileRepository<Image>();
            string Id = imgRepository.Insert(img);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        [TestMethod]
        public void InsertPicture()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");

            string Dimensions = null;
            using (MemoryStream ms = new MemoryStream(fileContent))
            {
                using (System.Drawing.Image _img = System.Drawing.Image.FromStream(ms))
                {
                    Dimensions = $"{_img.Width} x {_img.Height}";
                }
            }

            Picture pic = new Picture()
            {
                Filename = "Omega.png",
                Content = fileContent,
                IsDirty = true,
                UploadedDate = DateTime.UtcNow
            };

            FileRepository<Picture> imgRepository = new FileRepository<Picture>();
            string Id = imgRepository.Insert(pic);
            Console.WriteLine($"Inserted document Id: {Id}");
        }

        //Delete
        [TestMethod]
        public void DeleteImage()
        {
            FileRepository<Image> imgRepository = new FileRepository<Image>();
            imgRepository.Delete("5c63cb9998d2c42d405279fa");
        }
    }
}
