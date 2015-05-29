# dot-net-box-view

## Introduction

dot-net-box-view is a .NET wrapper for the Box View API.
The Box View API lets you upload documents and then generate secure and customized viewing sessions for them.
Our API is based on REST principles and generally returns JSON encoded responses,
and in .NET are converted to key-value pairs unless otherwise noted.

For more information about the Box View API, see the [API docs at developers.box.com](https://developers.box.com/view/).

## Installation

### Requirements

* .NET 4.5 or newer

### Install

You can download the package here: http://crocodoc.github.io/dot-net-box-view/BoxView.dll

## Getting Started

### Get an API Key

[Create a Box Application](https://app.box.com/developers/services/edit/) to get an API key.
Enter your application name, click the option for `Box View`, and click `Create Application`.
Then click `Configure your application`.

You can find your API key where it says `View API Key`.

In the future, if you need to return to this page, go to [Box Developers > My Applications](https://app.box.com/developers/services) and click on any of your Box View apps.

### Examples

You can see a number of examples on how to use this library in `Examples/Examples.cs`.
These examples are interactive and you can run this file to see `dot-net-box-view` in action.

To run these examples, open up `Examples/Examples.cs` and change this line to show your API key:

```csharp
public const string API_KEY = "YOUR_API_KEY";
```

Save the file, make sure the `Examples/Files` directory is writeable, and then run `Examples/Examples.cs`:

You should see 17 examples run with output in your terminal.
You can inspect the `Examples/Examples.cs` code to see each API call being used.

### Your Code

To start using `dot-net-box-view` in your code, set your API key:

```csharp
var boxView = new BoxViewClient("YOUR_API_KEY");
```

And now you can start using the methods in `Document` and `Session`.

## Support

Please use GitHub's issue tracker for API library support.

## Usage

### Fields

All fields are accessed using getters.
You can find a list of these fields below in their respective sections.

### Errors

Errors are handled by throwing exceptions.
We throw instances of `BoxViewException`.

Note that any Box View API call can throw an exception.
When making API calls, put them in a try/catch block.
You can see `Examples/Examples.cs` to see working code for each method using try/catch blocks.

### Document

#### Fields

Field     | Getter
--------- | ------
id        | document.Id
createdAt | document.CreatedAt
name      | document.Name
status    | document.Status

#### Upload from File

https://developers.box.com/view/#post-documents
To upload a document from a local file, use `boxView.Upload()`.
Pass in a file resource, and also a number of optional params.
This function returns a `Document` object.

```csharp
// without options
var file = new FileStream(filePath);
var document = boxView.Upload(file);

// with options
var file = new FileStream(filePath);

var document = boxView.Upload(file,
	name: "Test File",
	thumbnails: new string[] { "100x100", "200x200" },
	nonSvg: true);
```

The response looks like this:

```csharp
{BoxView.Document}
  Id: 386bd56cd42a4256b9b25342d6ba986d,
  CreatedAt: Fri Apr 17 22:21:17 PDT 2015,
  Name: Sample File,
  Status: queued
```

#### Upload by URL

https://developers.box.com/view/#post-documents
To upload a document by a URL, use `boxView.Upload()`.
Pass in the URL of the file you want to upload, and also a number of optional params.
This function returns a `Document` object.

```csharp
// without options
var document = boxView.Upload(url);

// with options
var document = boxView.Upload(url
	name: "Test File",
	thumbnails: new string[] { "100x100", "200x200" },
	nonSvg: true););
```

The response looks like this:

```csharp
{BoxView.Document}
  Id: 386bd56cd42a4256b9b25342d6ba986d,
  CreatedAt: Fri Apr 17 22:21:17 PDT 2015,
  Name: Sample File,
  Status: queued
```

#### Get Document

https://developers.box.com/view/#get-documents-id
To get a document, use `boxView.GetDocument()`.
Pass in the ID of the document you want to get.
This function returns a `Document` object.

```csharp
var document = boxView.GetDocument(documentId)`
```

The response looks like this:

```csharp
{BoxView.Document}
  Id: 386bd56cd42a4256b9b25342d6ba986d,
  CreatedAt: Fri Apr 17 22:21:17 PDT 2015,
  Name: Sample File,
  Status: queued
```

#### Find

https://developers.box.com/view/#get-documents
To get a list of documents you've uploaded, use `boxView.FindDocuments()`.
Pass optional parameters you want to filter by.
This function returns an array of `Document` objects matching the request.

```csharp
// without options
var documents = boxView.FindDocuments();

// with options
var documents = boxView.FindDocuments(
	limit: 10,
	createdAfter: DateTime.now.AddDays(-14),
	createdBefore: DateTime.now.AddDays(-7));
```

The response looks like this:

```csharp
{System.Collections.Generic.List}
  {BoxView.Document}
    Id: 386bd56cd42a4256b9b25342d6ba986d,
    CreatedAt: Fri Apr 17 22:21:17 PDT 2015,
    Name: Sample File,
    Status: queued
  {BoxView.Document}
    Id: 0971e7674469406dba53254fcbb11d05,
    CreatedAt: Fri Apr 17 22:21:17 PDT 2015,
    Name: Sample File #2,
    Status: queued
```

#### Download

https://developers.box.com/view/#get-documents-id-content
To download a document, use `document.Download()`.
This function returns the contents of the downloaded file.

```csharp
var contents = document.Download();
var filename = "/files/new-file.doc";

File.WriteAllBytes(filename, contents);
```

The response is a `byte[]` array representing the data of the file.

#### Thumbnail

https://developers.box.com/view/#get-documents-id-thumbnail
To download a document, use `document.Thumbnail()`.
Pass in the width and height in pixels of the thumbnail you want to download.
This function returns the contents of the downloaded thumbnail.

```csharp
var thumbnailContents = document.Thumbnail(100, 100);
String filename = "/files/new-thumbnail.png";

File.WriteAllBytes(filename, thumbnailContents);
```

The response is an `byte[]` array representing the data of the file.

#### Update

https://developers.box.com/view/#put-documents-id
To update the metadata of a document, use `document.Update()`.
Pass in the fields you want to update.
Right now, only the `name` field is supported.
This function returns a boolean of whether the file was updated or not.

```csharp
var updated = document.Update(name: "Updated Name");

if (updated) {
    // do something
} else {
    // do something else
}
```

The response looks like this:

```csharp
true
```

#### Delete

https://developers.box.com/view/#delete-documents-id
To delete a document, use `document.Delete()`.
This function returns a boolean of whether the file was deleted or not.

```csharp
var deleted = document.Delete();

if (deleted) {
    // do something
} else {
    // do something else
}
```

The response looks like this:

```csharp
true
```

### Session

#### Fields

Field       | Getter
----------- | ------
id          | session.Id
document    | session.Document
expiresAt   | session.ExpiresAt
assetsUrl   | session.AssetsUrl
realtimeUrl | session.RealtimeUrl
viewUrl     | session.ViewUrl

#### Create

https://developers.box.com/view/#post-sessions
To create a session, use `document.CreateSession()`.
Pass in a number of optional params.
This function returns a `Session` object.

```csharp
// without options
var session = document.c=CreateSession();

// with options
var Session = document.CreateSession(
	expiresAt: DateTime.Now.AddMinutes(10),
	isDownloadable: true,
	isTextSelectable: false);
```

The response looks like this:

```csharp
{BoxView.Session}
  Id: d1b8c35a69da43fbb2e978e99589114a,
  Document: { }
    {BoxView.Document}
  ExpiresAt: Fri Apr 17 22:21:17 PDT 2015,
  ViewUrl=https://view-api.box.com/1/sessions/31d04397460c48f2881e84a2928cf869/view,
  AssetsUrl=https://view-api.box.com/1/sessions/31d04397460c48f2881e84a2928cf869/assets/,
  RealtimeUrl=https://view-api.box.com/sse/31d04397460c48f2881e84a2928cf869
```

#### Delete

https://developers.box.com/view/#delete-sessions-id
To delete a session, use `session.Delete()`.
This function returns a boolean of whether the session was deleted or not.

```csharp
var deleted = session.Delete();

if (deleted) {
    // do something
} else {
    // do something else
}
```

The response looks like this:

```csharp
true
```
