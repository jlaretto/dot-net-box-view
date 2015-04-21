using System;
using System.Collections.Generic;
using System.Net.Http;

namespace BoxView
{
    /// <summary>
    /// Provide access to the Box View Session API. The Session API is used to create sessions for specific documents
    /// that can be used to view a document using a specific session-based URL.
    /// </summary>
    public class Session : Base
    {
        /// <summary>
        /// The Session API path relative to the base API path.
        /// </summary>
        const string PATH = "/sessions";

        /// <summary>
        /// The document that created this session.
        /// </summary>
        public Document Document { get; private set; }

        /// <summary>
        /// The session ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The date the session expires, formatted as RFC 3339.
        /// </summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>
        /// The URLs for a session.
        /// </summary>
        IDictionary<string, string> urls = new Dictionary<string, string>();

        /// <summary>
        /// The session assets URL.
        /// </summary>
        public string AssetsUrl
        {
            get { return urls.ContainsKey("assets") ? urls["assets"] : null; }
            private set { urls["assets"] = value; }
        }

        /// <summary>
        /// The session realtime URL.
        /// </summary>
        public string RealtimeUrl
        {
            get { return urls.ContainsKey("realtime") ? urls["realtime"] : null; }
            private set { urls["realtime"] = value; }
        }

        /// <summary>
        /// The session view URL.
        /// </summary>
        public string ViewUrl
        {
            get { return urls.ContainsKey("view") ? urls["view"] : null; }
            private set { urls["view"] = value; }
        }

        /// <summary>
        /// Instantiate the session.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="data">
        /// A key-value pair used to instantiate the object with. Use the following values:
        ///   - Document 'document' The document the session was created for.
        ///   - string 'id' The session ID.
        ///   - string|DateTime 'expiresAt' The date the session was created.
        ///   - object 'urls' A key-value pair of URLs for 'assets', 'realtime', and 'view'.
        /// </param>
        public Session(BoxViewClient client, IDictionary<string, object> data)
        {
            Client = client;
            Id = (string)data["id"];

            SetValues(data);
        }

        /// <summary>
        /// Delete a session.
        /// </summary>
        /// <returns>Was the session deleted?</returns>
        /// <exception cref="BoxViewException"></exception>
        public bool Delete()
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
        /// Create a session for a specific document by ID that may expire.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="id">The ID of the file to create a session for.</param>
        /// <param name="duration">The number of minutes for the session to last.</param>
        /// <param name="expiresAt">When the session should expire.</param>
        /// <param name="isDownloadable">Should the user be allowed to download the original file</param>
        /// <param name="isTextSelectable">Should the user be allowed to select text?</param>
        /// <returns>A new session instance.</returns>
        /// <exception cref="BoxViewException"></exception>
        public static Session Create(
            BoxViewClient client,
            string id,
            int? duration = null,
            object expiresAt = null,
            bool? isDownloadable = null,
            bool? isTextSelectable = null)
        {
            var postParams = new Dictionary<string, string>();
            postParams["document_id"] = id;

            if (duration != null && duration > 0)
                postParams["duration"] = duration.ToString();

            if (expiresAt != null)
                postParams["expires_at"] = (expiresAt is DateTime)
                                           ? Date((DateTime)expiresAt)
                                           : Date(expiresAt.ToString());

            if (isDownloadable != null)
                postParams["is_downloadable"] = isDownloadable.Equals(true) ? "1" : "0";

            if (isTextSelectable != null)
                postParams["is_text_selectable"] = isTextSelectable.Equals(true) ? "1" : "0";

            var metadata = RequestJson(
                               client: client,
                               path: PATH,
                               postParams: postParams);
            return new Session(client, metadata);
        }

        /// <summary>
        /// Update the current document instance with new metadata.
        /// </summary>
        /// <param name="data">
        /// A key-value pair to instantiate the object with. Use the following values:
        ///   - Document 'document' The document the session was created for.
        ///   - string|DateTime 'expiresAt' The date the session was created.
        ///   - object 'urls' A key-value pair of URLs for 'assets', 'realtime', and 'view'.
        /// </param>
        /// <returns>void</returns>
        void SetValues(IDictionary<string, object> data)
        {
            if (data.ContainsKey("document"))
                Document = new Document(Client, (IDictionary<string, object>)data["document"]);

            if (data.ContainsKey("expires_at"))
            {
                data["expiresAt"] = data["expires_at"];
                data.Remove("expires_at");
            }

            if (data.ContainsKey("expiresAt") && data["expiresAt"] != null)
                ExpiresAt = (data["expiresAt"] is DateTime)
                            ? (DateTime)data["expiresAt"]
                            : ParseDate(data["expiresAt"].ToString());

            if (data.ContainsKey("urls"))
            {
                var urlValues = (IDictionary<string, object>)data["urls"];

                if (urlValues.ContainsKey("assets"))
                    AssetsUrl = urlValues["assets"].ToString();

                if (urlValues.ContainsKey("realtime"))
                    RealtimeUrl = urlValues["realtime"].ToString();

                if (urlValues.ContainsKey("view"))
                    ViewUrl = urlValues["view"].ToString();
            }
        }
    }
}

