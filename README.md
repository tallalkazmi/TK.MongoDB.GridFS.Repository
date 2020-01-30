# TK.MongoDB.GridFS.Repository
Repository pattern implementation of MongoDB GridFS in .NET Framework

## Usage
#### Settings

1. Default $BucketChunkSizeBytes$ is set to 2097152 bytes or 2 MiB, but you can configure this by calling a static method as below:

   ```c#
   Settings.Configure(2097152);
   ```

2. Default $ConnectionStringSettingName$ is set to "*MongoDocConnection*", but you can configure this by calling a static method as below:

   ```c#
   Settings.Configure(connectionStringSettingName: "MongoDocConnection");
   ```

3. You can also set both of the settings as below:

   ```c#
   Settings.Configure(2097152, "MongoDocConnection");
   ```

#### Models

Create a document model implementing $IBaseFile$ to use in repository. The name of this model will be used for bucket name in MongoDB.

```c#
public class Image : IBaseFile
{
    public string Id { get; set; }
    public string Filename { get; set; }
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
    public string Dimensions { get; set; }
    public long ContentLength { get; set; }
}
```

#### Repository methods

1. Get file by Id

   ```c#
   FileRepository<Image> imgRepository = new FileRepository<Image>();
   var file = imgRepository.GetById("5c63cb9998d2c42d405279fa").Result;
   ```

2. Get file by filename

   ```c#
   FileRepository<Image> imgRepository = new FileRepository<Image>();
   var file = imgRepository.GetByFileName("Omega1.png");
   ```

3. Insert

   ```c#
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
   ```

4. Delete

   ```c#
   FileRepository<Image> imgRepository = new FileRepository<Image>();
   imgRepository.Delete("5c63cb9998d2c42d405279fa");
   ```

#### Tests

Refer to **TK.MongoDB.GridFS.Test** project for all Unit Tests.