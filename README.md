# TK.MongoDB.GridFS.Repository
Repository pattern implementation of MongoDB GridFS in .NET Framework

## Usage
#### Settings

1. Default `BucketChunkSizeBytes` is set to 2097152 bytes or 2 MiB, but you can configure this by calling a static method as below:

   ```c#
   Settings.Configure(2097152);
   ```

2. Default `ConnectionStringSettingName` is set to "*MongoDocConnection*", but you can configure this by calling a static method as below:

   ```c#
   Settings.Configure(connectionStringSettingName: "MongoDocConnection");
   ```

3. You can also set both of the settings as below:

   ```c#
   Settings.Configure(2097152, "MongoDocConnection");
   ```

#### Models

Create a document model implementing $BaseFile$ to use in repository. The name of this model will be used as bucket name in MongoDB.

```c#
public class Image : BaseFile<ObjectId>
{
    public bool isDisplay { get; set; }
}
```

#### Repository methods

1. Get (by Filter Definition)

   ```c#
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
   ```

2. Get (by Id)

   ```c#
   Image file = imgRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
   ```
   
3. Get (by Filename)

   ```c#
   IEnumerable<Image> files = imgRepository.Get("Omega1.png");
   ```
   
4. Insert

   ```c#
   byte[] fileContent = File.ReadAllBytes("../../Files/Omega.png");
   
   Image img = new Image()
   {
       Filename = "Omega.png",
       Content = fileContent,
       isDisplay = false
   };
   
   string id = imgRepository.Insert(img);
   ```

5. Delete

   ```c#
   imgRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
   ```

#### Tests

Refer to **TK.MongoDB.GridFS.Test** project for all Unit Tests.