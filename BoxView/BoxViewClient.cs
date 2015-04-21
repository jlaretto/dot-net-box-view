using System.IO;
using System.Collections.Generic;

namespace BoxView
{
    /// <summary>
    /// Provides access to the Box View API.
    /// </summary>
    public class BoxViewClient
    {
        /// <summary>
        /// The developer's Box View API key.
        /// </summary>
        string ApiKey { get; set; }

        /// <summary>
        /// The request handler.
        /// </summary>
        Request requestHandler;

        public Request RequestHandler
        {
            get
            {
                if (requestHandler == null)
                    requestHandler = new Request(ApiKey);
                return requestHandler;
            }
            set { requestHandler = value; }
        }

        /// <summary>
        /// Instantiate the client.
        /// </summary>
        /// <param name="apiKey">The API key to use.</param>
        public BoxViewClient(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// Get a list of all documents that meet the provided criteria.
        /// </summary>
        /// <param name="limit">The number of documents to return.</param>
        /// <param name="createdBefore">Upper date limit to filter by.</param>
        /// <param name="createdAfter">Lower date limit to filter by.</param>
        /// <returns>An array containing document instances matching the request.</returns>
        /// <exception cref="BoxViewException"></exception>
        public List<Document> FindDocuments(
            int? limit = null,
            object createdBefore = null,
            object createdAfter = null)
        {
            return Document.Find(this,
                limit: limit,
                createdBefore: createdBefore,
                createdAfter: createdAfter);
        }

        /// <summary>
        /// Create a new document instance by ID, and load it with values requested from the API.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <returns>A document instance using data from the API.</returns>
        /// <exception cref="BoxViewException"></exception>
        public Document GetDocument(string id)
        {
            return Document.Get(this, id);
        }

        /// <summary>
        /// Upload a local file and return a new document instance.
        /// </summary>
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
        public Document Upload(FileStream file, string name = null, object thumbnails = null, bool? nonSvg = null)
        {
            return Document.Upload(this, file,
                name: name,
                thumbnails: thumbnails,
                nonSvg: nonSvg);
        }

        /// <summary>
        /// Upload a file by URL and return a new document instance.
        /// </summary>
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
        public Document Upload(string url, string name = null, object thumbnails = null, bool? nonSvg = null)
        {
            return Document.Upload(this, url,
                name: name,
                thumbnails: thumbnails,
                nonSvg: nonSvg);
        }
    }
}

