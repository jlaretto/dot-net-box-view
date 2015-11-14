using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace BoxView
{
    /// <summary>
    /// Makes a request to the Box View API.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Request error codes.
        /// </summary>
        public const string BAD_REQUEST_ERROR = "bad_request";
        public const string HTTP_CLIENT_ERROR = "http_client_error";
        public const string JSON_RESPONSE_ERROR = "server_response_not_valid_json";
        public const string METHOD_NOT_ALLOWED_ERROR = "method_not_allowed";
        public const string NOT_FOUND_ERROR = "not_found";
        public const string REQUEST_TIMEOUT_ERROR = "request_timeout";
        public const string SERVER_ERROR = "server_error";
        public const string TOO_MANY_REQUESTS_ERROR = "too_many_requests";
        public const string UNAUTHORIZED_ERROR = "unauthorized";
        public const string UNSUPPORTED_MEDIA_TYPE_ERROR = "unsupported_media_type";

        /// <summary>
        /// The default protocol (Box View uses HTTPS).
        /// </summary>
        public const string PROTOCOL = "https";

        /// <summary>
        /// The default host.
        /// </summary>
        public const string HOST = "view-api.box.com";

        /// <summary>
        /// The default base path on the server where the API lives.
        /// </summary>
        public const string BASE_PATH = "/1";

        /// <summary>
        /// The number of seconds before timing out when in a retry loop.
        /// </summary>
        public const int DEFAULT_RETRY_TIMEOUT = 60;

        /// <summary>
        /// The API key.
        /// </summary>
        string ApiKey { get; set; }

        /// <summary>
        /// A stopwatch for determining if a request should timeout or not.
        /// </summary>
        Stopwatch requestTimeoutStopwatch = new Stopwatch();

        Stopwatch RequestTimeoutStopwatch
        {
            get { return requestTimeoutStopwatch; }
        }

        /// <summary>
        /// Set the API key.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        public Request(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// Send an HTTP request and return an HttpWebResponse.
        /// </summary>
        /// <param name="path">The path to add after the base path.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">A key-value pair of request options that may modify the way the request is made.</param>
        /// <returns>The HTTP content from the response.</returns>
        /// <exception cref="BoxViewException"></exception>
        public HttpContent RequestHttpContent(
            string path,
            IDictionary<string, string> getParams,
            IDictionary<string, string> postParams,
            IDictionary<string, object> requestOptions)
        {
            var response = Execute(
                               path: path,
                               getParams: getParams,
                               postParams: postParams,
                               requestOptions: requestOptions,
                               timeout: CreateTimeout(requestOptions));
            return response.Content;
        }

        /// <summary>
        /// Send an HTTP request and return a key-value pair.
        /// </summary>
        /// <param name="path">The path to add after the base path.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">A key-value pair of request options that may modify the way the request is made.</param>
        /// <returns>A key-value pair decoded from JSON.</returns>
        /// <exception cref="BoxViewException"></exception>
        public IDictionary<string, object> RequestJson(
            string path,
            IDictionary<string, string> getParams,
            IDictionary<string, string> postParams,
            IDictionary<string, object> requestOptions)
        {
            var response = Execute(
                               path: path,
                               getParams: getParams,
                               postParams: postParams,
                               requestOptions: requestOptions,
                               timeout: CreateTimeout(requestOptions));
            return HandleJsonResponse(response);
        }

        /// <summary>
        /// Handle an error. We handle errors by throwing an exception.
        /// </summary>
        /// <param name="error">An error code representing the error (use_underscore_separators).</param>
        /// <param name="message">The error message.</param>
        /// <param name="request">The HTTP request object.</param>
        /// <param name="response">The HTTP response object.</param>
        /// <returns>void</returns>
        /// <exception cref="BoxViewException"></exception>
        protected static void Error(
            string error,
            string message,
            HttpRequestMessage request,
            HttpResponseMessage response)
        {
            if (request != null)
            {
                message += "\n";
                message += "Method: " + request.Method + "\n";
                message += "URL: " + request.RequestUri + "\n";
                message += "Query: " + request.RequestUri.Query + "\n";

                var headers = request.Headers;
                message += "Headers: " + (new JavaScriptSerializer()).Serialize(headers) + "\n";

                var requestBody = "";

                if (request.Method == HttpMethod.Post)
                    requestBody = request.Content.ReadAsStringAsync().Result;

                message += "Request Body: " + requestBody + "\n";
            }

            if (response != null)
            {
                message += "\n";
                message += "Response Body: " + response.Content.ReadAsStringAsync().Result + "\n";
            }

            throw new BoxViewException(message, error);
        }

        /// <summary>
        /// Prepare and create an HTTP request object.
        /// </summary>
        /// <param name="path">The path to add after the base path.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">A key-value pair of request options that may modify the way the request is made.</param>
        /// <returns>The HTTP request object.</returns>
        HttpRequestMessage CreateRequest(
            string path,
            IDictionary<string, string> getParams,
            IDictionary<string, string> postParams,
            IDictionary<string, object> requestOptions)
        {
            RequestTimeoutStopwatch.Restart();

            if (getParams == null)
                getParams = new Dictionary<string, string>();

            if (postParams == null)
                postParams = new Dictionary<string, string>();

            if (requestOptions == null)
                requestOptions = new Dictionary<string, object>();

            var hostName = HOST;

            if (requestOptions.ContainsKey("host") && requestOptions["host"] != null)
                hostName = requestOptions["host"].ToString();

            HttpContent content = null;

            var method = HttpMethod.Get;

            if (requestOptions.ContainsKey("file") && requestOptions["file"] != null)
            {
                method = HttpMethod.Post;
                var multipartContent = new MultipartFormDataContent();

                multipartContent.Add(new FormUrlEncodedContent(postParams));

                var file = (FileStream)requestOptions["file"];
                multipartContent.Add(new StreamContent(file), "file", file.Name);

                content = multipartContent;
            }
            else if (postParams.Keys.Count > 0)
            {
                method = HttpMethod.Post;
                var json = (new JavaScriptSerializer()).Serialize(postParams);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (requestOptions.ContainsKey("httpMethod") && requestOptions["httpMethod"] != null)
                method = (HttpMethod)requestOptions["httpMethod"];
             
            var uri = GetUri(hostName, path, getParams);
            return GetRequest(
                method: method,
                uri: uri,
                content: content,
                requestOptions: requestOptions);
        }

        /// <summary>
        /// Execute a request to the server and return the response, while retrying based on any Retry-After headers
        /// that are sent back.
        /// </summary>
        /// <param name="path">The path to add after the base path.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">A key-value pair of request options that may modify the way the request is made.</param>
        /// <param name="timeout">The maximum number of seconds to retry for.</param>
        /// <returns>The HTTP response object.</returns>
        /// <exception cref="BoxViewException"></exception>
        HttpResponseMessage Execute(
            string path,
            IDictionary<string, string> getParams,
            IDictionary<string, string> postParams,
            IDictionary<string, object> requestOptions,
            int timeout)
        {
            var request = CreateRequest(
                              path: path,
                              getParams: getParams,
                              postParams: postParams,
                              requestOptions: requestOptions);

            var client = GetHttpClient();
            //var response = ExecuteAsync(client, request).Result;
            //JAL: Hack per https://github.com/crocodoc/dot-net-box-view/issues/2
            var response = Task.Run(() => ExecuteAsync(client, request)).Result;
            if (response.Headers.RetryAfter != null)
            {
                var seconds = RequestTimeoutStopwatch.ElapsedMilliseconds / 1000;
               
                if (seconds >= timeout)
                {
                    var message = "The request timed out after retrying for " + seconds + " seconds.";
                    Error(REQUEST_TIMEOUT_ERROR, message, request, response);
                }

                var retryAfter = response.Headers.RetryAfter.ToString();
                Thread.Sleep(int.Parse(retryAfter) * 1000);

                return Execute(
                    path: path,
                    getParams: getParams,
                    postParams: postParams,
                    requestOptions: requestOptions,
                    timeout: timeout);
            }

            HandleRequestError(request, response, null);
            return response;
        }

        async Task<HttpResponseMessage> ExecuteAsync(HttpClient client, HttpRequestMessage request)
        {
            return await client.SendAsync(request);
        }

        /// <summary>
        /// Create an HTTP request object.
        /// </summary>
        /// <param name="method">The HTTP method to call.</param>
        /// <param name="uri">The URI to request to.</param>
        /// <param name="content">The body for POST/PUT operations.</param>
        /// <param name="requestOptions">
        /// A key-value pair of request options that may modify the way the request is made.
        /// </param>
        /// <returns>The HTTP request object.</returns>
        HttpRequestMessage GetRequest(
            HttpMethod method,
            Uri uri,
            HttpContent content,
            IDictionary<string, object> requestOptions)
        {
            var request = new HttpRequestMessage(method, uri) { Content = content };

            request.Headers.Authorization = new AuthenticationHeaderValue("Token", ApiKey);

            if (requestOptions.ContainsKey("rawResponse") && (bool)requestOptions["rawResponse"])
            {
                request.Headers.Remove("Accept");
                request.Headers.Add("Accept", "*/*");
            }

            return request;
        }

        /// <summary>
        /// Get a timeout for the request.
        /// </summary>
        /// <param name="requestOptions">
        /// A key-value pair of request options that may modify the way the request is made.
        /// </param>
        /// <returns>The timeout.</returns>
        static int CreateTimeout(IDictionary<string, object> requestOptions)
        {
            return (requestOptions != null
            && requestOptions.ContainsKey("timeout")
            && ((int)requestOptions["timeout"] > 0))
                ? (int)requestOptions["timeout"]
                    : DEFAULT_RETRY_TIMEOUT;
        }

        /// <summary>
        /// Get a new HttpClient instance using sensible defaults.
        /// </summary>
        /// <returns>A new HttpClient instance.</returns>
        static HttpClient GetHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "box-view-java");

            return client;
        }

        /// <summary>
        /// Create a URI, given a hostname, path, and GET params.
        /// </summary>
        /// <param name="hostName">The hostname to make the request to.</param>
        /// <param name="path">The path to make the request to.</param>
        /// <param name="getParams">A key-value pair of POST params to be sent in the body.</param>
        /// <returns>The URI to make the request to.</returns>
        static Uri GetUri(string hostName, string path, IDictionary<string, string> getParams)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = PROTOCOL,
                Host = hostName,
                Path = BASE_PATH + path
            };

            if (getParams.Count > 0)
            {
                var collection = HttpUtility.ParseQueryString(string.Empty);

                foreach (var item in getParams)
                {
                    if (item.Value != null)
                        collection[item.Key] = item.Value;
                }

                uriBuilder.Query = collection.ToString();
            }

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Check if there is an HTTP error, and returns a brief error description if there is.
        /// </summary>
        /// <param name="httpCode">The HTTP code returned by the API server.</param>
        /// <returns>Brief error description.</returns>
        static string HandleHttpError(int httpCode)
        {
            var errorCodes = new Dictionary<int, string>
            {
                { 400, BAD_REQUEST_ERROR },
                { 401, UNAUTHORIZED_ERROR },
                { 404, NOT_FOUND_ERROR },
                { 405, METHOD_NOT_ALLOWED_ERROR },
                { 415, UNSUPPORTED_MEDIA_TYPE_ERROR },
                { 429, TOO_MANY_REQUESTS_ERROR }
            };

            if (errorCodes.ContainsKey(httpCode))
            {
                return errorCodes[httpCode];
            }

            if (httpCode >= 500 && httpCode < 600)
            {
                return SERVER_ERROR;
            }

            return null;
        }

        /// <summary>
        /// Handle the response from the server. Raw responses are returned without checking anything. JSON responses
        /// are decoded and then checked for any errors.
        /// </summary>
        /// <param name="response">The HTTP response object.</param>
        /// <returns>A key-value pair decoded from JSON.</returns>
        /// <exception cref="BoxViewException"></exception>
        static IDictionary<string, object> HandleJsonResponse(HttpResponseMessage response)
        {
            var body = response.Content.ReadAsStringAsync().Result;
            var jsonDecoded = (new JavaScriptSerializer()).Deserialize<IDictionary<string, object>>(body);

            if (jsonDecoded == null)
                Error(JSON_RESPONSE_ERROR, null, response.RequestMessage, response);

            if (jsonDecoded.ContainsKey("status") && jsonDecoded["status"].ToString() == "error")
            {
                var message = jsonDecoded.ContainsKey("error_message")
                              ? jsonDecoded["error_message"].ToString()
                              : "Server Error";
                Error(SERVER_ERROR, message, response.RequestMessage, response);
            }

            return jsonDecoded;
        }

        /// <summary>
        /// Handle a request error.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="e">Any error exception that triggered this call.</param>
        /// <returns>void</returns>
        /// <exception cref="BoxViewException"></exception>
        static void HandleRequestError(HttpRequestMessage request, HttpResponseMessage response, Exception e)
        {
            String error = HandleHttpError((int)response.StatusCode);
            String message = "Server Error";

            if (error == null)
            {
                if (e == null)
                {
                    return;
                }

                error = HTTP_CLIENT_ERROR;
                message = e.Message;
            }

            Error(error, message, request, response);
        }
    }
}

