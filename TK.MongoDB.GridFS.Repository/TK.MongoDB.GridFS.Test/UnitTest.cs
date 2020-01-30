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
            Settings.Configure("MongoDocConnection", 2097152);
        }

        [TestMethod]
        public void GetImage()
        {
            FileRepository<Image> imgRepository = new FileRepository<Image>();
            //var file = imgRepository.GetById("5c63cb9998d2c42d405279fa").Result;
            var file = imgRepository.GetByFileName("Omega1.png");
        }

        [TestMethod]
        public void UploadDocument()
        {
            byte[] fileContent = File.ReadAllBytes("../../Files/sample.pdf");
            Document doc = new Document()
            {
                Filename = "sample.pdf",
                Content = fileContent,
                UploadDate = DateTime.UtcNow
            };

            FileRepository<Document> docRepository = new FileRepository<Document>();
            var id = docRepository.Insert(doc);
        }

        [TestMethod]
        public void UploadImage()
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
            var id = imgRepository.Insert(img);
        }

        [TestMethod]
        public void UploadPicture()
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
            var id = imgRepository.Insert(pic);
        }
    }
}
