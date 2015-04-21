using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Collections;

namespace BoxView
{
    /// <summary>
    /// Provide access to the Box View Document API. The Document API is used for uploading, checking status, and
    /// deleting documents.
    /// </summary>
    public class Document : Base
    {
        /// <summary>
        /// Document error codes.
        /// </summary>
        public const string INVALID_FILE_ERROR = "invalid_file";
        public const string INVALID_RESPONSE_ERROR = "invalid_response";

        /// <summary>
        /// An alternate hostname that file upload requests are sent to.
        /// </summary>
        public const string FILE_UPLOAD_HOST = "upload.view-api.box.com";

        /// <summary>
        /// The Document API path relative to the base API path.
        /// </summary>
        const string PATH = "/documents";

        /// <summary>
        /// The date the document was created, formatted as RFC 3339.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// The document ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The document title.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The document status, which can be 'queued', 'processing', 'done', or 'error'.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Instantiate the document.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="data">
        /// A key-value pair to instantiate the object with. Use the following values:
        ///   - string 'id' The document ID.
        ///   - string|DateTime 'createdAt' The date the document was created.
        ///   - string 'name' The document title.
        ///   - string 'status' The document status, which can be 'queued', 'processing', 'done', or 'error'.
        /// </param>
        public Document(BoxViewClient client, IDictionary<string, object> data)
        {
            Client = client;
            Id = (string)data["id"];

            SetValues(data);
        }

        /// <summary>
        /// Create a session for a specific document.
        /// </summary>
        /// <param name="duration">The number of minutes for the session to last.</param>
        /// <param name="expiresAt">When the session should expire.</param>
        /// <param name="isDownloadable">Should the user be allowed to download the original file</param>
        /// <param name="isTextSelectable">Should the user be allowed to select text?</param>
        /// <returns>A new session instance.</returns>
        /// <exception cref="BoxViewException"></exception>
        public Session CreateSession(
            int? duration = null,
            object expiresAt = null,
            bool? isDownloadable = null,
            bool? isTextSelectable = null)
        {
            return Session.Create(Client, Id,
                duration: duration,
                expiresAt: expiresAt,
                isDownloadable: isDownloadable,
                isTextSelectable: isTextSelectable);
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <returns>Was the file deleted?</returns>
        /// <exception cref="BoxViewException"></exception>
        public Boolean Delete()
        {
            var requestOptions = new Dictionary<string, object>
            {
                { "httpMethod", HttpMethod.Delete },
                { "rawResponse", true }
            };

            var response = RequestHttpContent(
                               client: Client,
                               path: PATH + "/" + Id,
                               requestOptions: requestOptions);

            // a successful delete returns nothing, so we return true in that case
            return (response.ReadAsStringAsync().Result == "");
        }

        /// <summary>
        /// Download a file using a specific extension.
        /// </summary>
        /// <param name="extension">
        /// The extension to download the file in, which can be pdf or zip. If no extension is provided, the file will
        /// be downloaded using the original extension.
        /// </param>
        /// <returns>The contents of the downloaded file.</returns>
        /// <exception cref="BoxViewException"></exception>
        public byte[] Download(string extension = null)
        {
            if (extension != null && extension.Length > 0)
                extension = "." + extension;

            var requestOptions = new Dictionary<string, object>
            {
                { "rawResponse", true }
            };

            var response = RequestHttpContent(
                               client: Client,
                               path: PATH + "/" + Id + "/content" + extension,
                               requestOptions: requestOptions);
            return response.ReadAsByteArrayAsync().Result;
        }

        /// <summary>
        /// Download a thumbnail of a specific size for a file.
        /// </summary>
        /// <param name="width">The width of the thumbnail in pixels.</param>
        /// <param name="height">The height of the thumbnail in pixels.</param>
        /// <returns>The contents of the downloaded thumbnail.</returns>
        /// <exception cref="BoxViewException"></exception>
        public byte[] Thumbnail(int width, int height)
        {
            var getParams = new Dictionary<string, string>
            {
                { "height", height.ToString() },
                { "width", width.ToString() }
            };

            var requestOptions = new Dictionary<string, object>
            {
                { "rawResponse", true }
            };

            var response = RequestHttpContent(
                               client: Client,
                               path: PATH + "/" + Id + "/thumbnail",
                               getParams: getParams,
                               requestOptions: requestOptions);
            return response.ReadAsByteArrayAsync().Result;
        }

        /// <summary>
        /// Update specific fields for the metadata of a file.
        /// </summary>
        /// <param name="name">The document name.</param>
        /// <returns>Was the file updated?</returns>
        /// <exception cref="BoxViewException"></exception>
        public bool Update(string name)
        {
            var postParams = new Dictionary<string, string>
            {
                { "name", name }
            };

            var requestOptions = new Dictionary<string, object>
            {
                { "httpMethod", HttpMethod.Put }
            };

            var metadata = RequestJson(
                               client: Client,
                               path: PATH + "/" + Id,
                               postParams: postParams,
                               requestOptions: requestOptions);
            SetValues(metadata);
            return true;
        }

        /// <summary>
        /// Get a list of all documents that meet the provided criteria.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="limit">The number of documents to return.</param>
        /// <param name="createdBefore">Upper date limit to filter by.</param>
        /// <param name="createdAfter">Lower date limit to filter by.</param>
        /// <returns>An array containing document instances matching the request.</returns>
        /// <exception cref="BoxViewException"></exception>
        public static List<Document> Find(
            BoxViewClient client,
            int? limit = null,
            object createdBefore = null,
            object createdAfter = null)
        {
            var getParams = new Dictionary<string, string>();

            if (limit != null && limit > 0)
                getParams["limit"] = limit.ToString();

            if (createdBefore != null)
                getParams["created_before"] = (createdBefore is DateTime)
                                              ? Date((DateTime)createdBefore)
                                              : Date(createdBefore.ToString());

            if (createdAfter != null)
                getParams["created_after"] = (createdAfter is DateTime)
                                           ? Date((DateTime)createdAfter)
                                           : Date(createdAfter.ToString());

            var response = RequestJson(
                               client: client,
                               path: PATH,
                               getParams: getParams);
            
            if (response.Count == 0
                || !response.ContainsKey("document_collection")
                || !((IDictionary<string, object>)response["document_collection"]).
                ContainsKey("entries"))
            {
                Error(INVALID_RESPONSE_ERROR, "response is not in a valid format.");
            }

            var collection = (IDictionary<string, object>)response["document_collection"];
            var entries = (ArrayList)collection["entries"];
               
            var documents = new List<Document>();

            foreach (var entry in entries)
            {
                documents.Add(new Document(client, (IDictionary<string, object>)entry));
            }

            return documents;
        }

        /// <summary>
        /// Create a new document instance by ID, and load it with values requested from the API.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="id">The document ID.</param>
        /// <returns>A document instance using data from the API.</returns>
        /// <exception cref="BoxViewException"></exception>
        public static Document Get(BoxViewClient client, string id)
        {
            string[] fields = { "id", "created_at", "name", "status" };

            var getParams = new Dictionary<string, string>
            {
                { "fields", String.Join(",", fields) }
            };

            var metadata = RequestJson(
                               client: client,
                               path: PATH + "/" + id,
                               getParams: getParams);
            return new Document(client, metadata);
        }

        /// <summary>
        /// Upload a local file and return a new document instance.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="file">The file resource to upload.</param>
        /// <param name="name">Override the filename of the file being uploaded.</param>
        /// <param name="thumbnails">
        /// An array of dimensions in pixels, with each dimension formatted as [width]x[height], this can also be a
        /// comma-separated string.
        /// </param>
        /// <param name="nonSvg">
        /// Create a second version of the file that doesn't use SVG, for users with browsers that don't support SVG?
        /// </param>
        /// <returns>A new document instance.</returns>
        /// <exception cref="BoxViewException"></exception>
        public static Document Upload(
            BoxViewClient client,
            FileStream file,
            string name = null,
            object thumbnails = null,
            bool? nonSvg = null)
        {
            return Upload(
                client: client,
                options: new Dictionary<string, object>
                {
                    { "name", name },
                    { "thumbnails", thumbnails },
                    { "nonSvg", nonSvg }
                },
                requestOptions: new Dictionary<string, object>
                {
                    { "file", file },
                    { "host", FILE_UPLOAD_HOST }
                });
        }

        /// <summary>
        /// Upload a file by URL and return a new document instance.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="url">The URL of the file to upload.</param>
        /// <param name="name">Override the filename of the file being uploaded.</param>
        /// <param name="thumbnails">
        /// An array of dimensions in pixels, with each dimension formatted as [width]x[height], this can also be a
        /// comma-separated string.
        /// </param>
        /// <param name="nonSvg">
        /// Create a second version of the file that doesn't use SVG, for users with browsers that don't support SVG?
        /// </param>
        /// <returns>A new document instance.</returns>
        /// <exception cref="BoxViewException"></exception>
        public static Document Upload(
            BoxViewClient client,
            string url,
            string name = null,
            object thumbnails = null,
            bool? nonSvg = null)
        {
            return Upload(
                client: client,
                options: new Dictionary<string, object>
                {
                    { "name", name },
                    { "thumbnails", thumbnails },
                    { "nonSvg", nonSvg }
                },
                postParams: new Dictionary<string, string>
                {
                    { "url", url }
                });
        }

        /// <summary>
        /// Update the current document instance with new metadata.
        /// </summary>
        /// <param name="data">
        /// A key-value pair to instantiate the object with. Use the following values:
        ///   - string|DateTime 'createdAt' The date the document was created.
        ///   - string 'name' The document title.
        ///   - string 'status' The document status, which can be 'queued', 'processing', 'done', or 'error'.
        /// </param>
        /// <returns>void</returns>
        void SetValues(IDictionary<string, object> data)
        {
            if (data.ContainsKey("created_at"))
            {
                data["createdAt"] = data["created_at"];
                data.Remove("created_at");
            }

            if (data.ContainsKey("createdAt") && data["createdAt"] != null)
                CreatedAt = (data["createdAt"] is DateTime)
                            ? (DateTime)data["createdAt"]
                            : ParseDate(data["createdAt"].ToString());

            if (data.ContainsKey("name") && data["name"] != null)
                Name = (String)data["name"];

            if (data.ContainsKey("status") && data["status"] != null)
                Status = (String)data["status"];
        }

        /// <summary>
        /// Generic upload function used by the two other upload functions, which are more specific than this one, and
        /// know how to handle upload by URL and upload from filesystem.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="options">
        /// A key-value pair of options relating to the file upload. Pass-thru from the other upload functions.
        /// </param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">
        /// A key-value pair of request options that may modify the way the request is made.
        /// </param>
        /// <returns>A new document instance.</returns>
        /// <exception cref="BoxViewException"></exception>
        static Document Upload(
            BoxViewClient client,
            IDictionary<string, object> options,
            IDictionary<string, string> postParams = null,
            IDictionary<string, object> requestOptions = null)
        {
            if (postParams == null)
                postParams = new Dictionary<string, string>();

            postParams["name"] = (string)options["name"];

            if (options["thumbnails"] != null)
            {
                var thumbnails = options["thumbnails"];

                if (thumbnails is string[])
                    thumbnails = ((string[])thumbnails).ToList();

                if (thumbnails is List<string>)
                    thumbnails = ((List<string>)thumbnails).ToArray();

                if (thumbnails is string[])
                    thumbnails = String.Join(",", (string[])thumbnails);

                postParams["thumbnails"] = (string)thumbnails;
            }

            if (options["nonSvg"] != null)
                postParams["non_svg"] = (bool)options["nonSvg"] ? "1" : "0";

            var metadata = RequestJson(
                               client: client,
                               path: PATH, 
                               postParams: postParams,
                               requestOptions: requestOptions);
            return new Document(client, metadata);
        }
    }
}

