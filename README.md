# TK.MongoDB.GridFS.Repository
Repository pattern implementation of MongoDB GridFS in .NET Framework

## Usage
#### Settings

1. Default `ConnectionStringSettingName` is set to "*MongoDocConnection*", but you can configure this by calling a static method as below:

   ```c#
   Settings.ConnectionStringSettingName = "MongoDocConnection";
   ```

2. Default behavior is to always validate file name before inserting against the regex `^[\w\-. ]+$`, but this can be changed by setting the following fields:

   ```c#
   Settings.ValidateFileName = false;
   Settings.FileNameRegex = new Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase);
   ```

3. Default behavior is to always check for file size before inserting with a maximum file size of 5 MB, but this can be changed by setting the following fields:

   ```c#
   Settings.ValidateFileSize = false;
   Settings.MaximumFileSizeInMBs = 5;
   ```

4. You can configure *chunk size* for each bucket by using the method below, default setting will create a bucket with *chunk size* of 2 MB. 

   ```c#
   Settings.Configure<Document>(2);
   ```

#### Models

Create a document model by inheriting `abstract` class `BaseFile​` to use in repository. The name of this model will be used as bucket name in MongoDB.

```c#
public class Image : BaseFile
{
    public bool isDisplay { get; set; }
}
```

#### Repository methods

1. Get (by Id)

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

2. Get (by Filename)

    ```c#
    IEnumerable<Image> files = imgRepository.Get("Omega1.png");
    ```

3. Get (by Lamda Expression)

    ```c#
    IEnumerable<Image> files = imgRepository.Get(x => x.Filename.Contains("Omega") && x.UploadDateTime < DateTime.UtcNow.AddDays(-1));
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

6. Rename

    ```c#
    imgRepository.Rename(new ObjectId("5e37cdcf98d2c12ba0231fbb"), "Omega-new.png");
    ```

7. Delete

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