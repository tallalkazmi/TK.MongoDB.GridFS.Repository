# TK.MongoDB.GridFS.Repository
[![Nuget](https://img.shields.io/nuget/v/TK.MongoDB.GridFS.Repository)](https://www.nuget.org/packages/TK.MongoDB.GridFS.Repository)
[![Nuget](https://img.shields.io/nuget/dt/TK.MongoDB.GridFS.Repository)](https://www.nuget.org/packages/TK.MongoDB.GridFS.Repository)
![Azure DevOps builds](https://img.shields.io/azure-devops/build/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/1) 
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/1) 
![Azure DevOps releases](https://img.shields.io/azure-devops/release/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/1/1) 

Repository pattern implementation of MongoDB GridFS in .NET Framework

## Usage
#### Settings

1. Default `ConnectionStringSettingName` is set to "*MongoDocConnection*", but you can configure this by calling a static method as below:

   ```c#
   Settings.ConnectionStringSettingName = "MongoDocConnection";
   ```


#### Models

Create a document model by inheriting `abstract` class `BaseFile​` to use in repository. The name of this model will be used as bucket name in MongoDB.

```c#
public class Image : BaseFile
{
    public bool isDisplay { get; set; }
}
```

###### Bucket Attribute

You can configure the GridFS bucket attributes by decoration the model with `Bucket` attribute, for example:

```c#
[Bucket(PluralizeBucketName = false, MaximumFileSizeInMBs = 1, BucketChunkSizeInMBs = 1)]
public class Document : BaseFile
{
	/*...*/
}
```

The `Bucket` attribute has the following properties that you can set:

```c#
public class BucketAttribute : Attribute
{
    /// <summary>
    /// Pluralize bucket's mame. Default value is set to True.
    /// </summary>
    public bool PluralizeBucketName { get; set; }

    /// <summary>
    /// Validate file name on insert and update from FileNameRegex field. Default value is set to True.
    /// </summary>
    public bool ValidateFileName { get; set; }

    /// <summary>
    /// Validate file size on insert from MaximumFileSizeInMBs field. Default value is set to True.
    /// </summary>
    public bool ValidateFileSize { get; set; }

    /// <summary>
    /// File name Regex to validate. Default value is set to Regex(@"^[\w\-. ]+$", RegexOptions.IgnoreCase).
    /// </summary>
    public Regex FileNameRegex { get; set; }

    /// <summary>
    /// Maximum file size in MBs. Default value is set to 5.
    /// </summary>
    public int MaximumFileSizeInMBs { get; set; }

    /// <summary>
    /// GridFS bucket chunk size in MBs. Default value is set to 2.
    /// </summary>
    public int BucketChunkSizeInMBs { get; set; }
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
