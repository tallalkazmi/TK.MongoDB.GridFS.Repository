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

Create a document model by inheriting `abstract` class `BaseFile​` of type `ObjectId` to use in repository. The name of this model will be used as bucket name in MongoDB.

```c#
public class Image : BaseFile<ObjectId>
{
    public bool isDisplay { get; set; }
}
```

#### Repository methods

1. Get (by Lamda Expression)

   ```c#
   IEnumerable<Image> files = imgRepository.Get(x => x.Filename.Contains("Omega") && x.UploadDateTime < DateTime.UtcNow.AddDays(-1));
   ```
   
2. Get (by Id)

   ```c#
   try
   {
       Image file = imgRepository.Get(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
       Console.WriteLine($"Output:\n{file.Filename}");
   }
   catch (FileNotFoundException ex)
   {
       Console.WriteLine($"Output:\n{ex.Message}");
   }
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

5. Rename

   ```c#
   imgRepository.Rename(new ObjectId("5e37cdcf98d2c12ba0231fbb"), "Omega-new.png");
   ```
   
6. Delete

   ```c#
   try
   {
       imgRepository.Delete(new ObjectId("5e36b5a698d2c14fe8b0ecbe"));
   }
   catch (FileNotFoundException ex)
   {
       Console.WriteLine($"Output:\n{ex.Message}");
   }
   ```

#### Tests

Refer to **TK.MongoDB.GridFS.Test** project for all Unit Tests.