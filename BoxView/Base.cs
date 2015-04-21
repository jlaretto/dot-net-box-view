using System;
using System.Globalization;
using System.Net.Http;
using System.Collections.Generic;

namespace BoxView
{
    /// <summary>Acts as a base class for the different Box View APIs.</summary>
    public abstract class Base
    {
        /// <summary>
        /// The client instance to make requests from.
        /// </summary>
        protected BoxViewClient Client;

        /// <summary>
        /// Take a date object, and return a date string that is formatted as an RFC 3339 timestamp.
        /// </summary>
        /// <param name="date">A date object.</param>
        /// <returns>An RFC 3339 timestamp.</returns>
        protected static string Date(DateTime date)
        {
            const string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(date);
            return utcDate.ToString(format, DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        ///  Take a date string in almost any format, and return a date string that is formatted as an RFC 3339
        /// timestamp.
        /// </summary>
        /// <param name="dateString">A date string in almost any format.</param>
        /// <returns>An RFC 3339 timestamp.</returns>
        protected static string Date(string dateString)
        {
            return Date(ParseDate(dateString));
        }

        /// <summary>
        /// Handle an error. We handle errors by throwing an exception.
        /// </summary>
        /// <param name="error">An error code representing the error (use_underscore_separators).</param>
        /// <param name="message">The error message.</param>
        /// <returns>void</returns>
        /// <exception cref="BoxViewException"></exception>
        protected static void Error(string error, string message)
        {
            throw new BoxViewException(message, error);
        }

        /// <summary>
        /// Take a date object or date string in RFC 3339 format, and return a date object.
        /// </summary>
        /// <param name="dateString">A date or date string in RFC 3339 format.</param>
        /// <returns>The date representation of the dateString.</returns>
        protected static DateTime ParseDate(string dateString)
        {
            return DateTime.Parse(dateString);
        }

        /// <summary>
        /// Send a new request to the API and return a string.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="path">The path to make a request to.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">
        /// A key-value pair of request options that may modify the way the request is made.
        /// </param>
        /// <returns>The response is pass-thru from Request.</returns>
        /// <exception>BoxViewException</exception>
        protected static HttpContent RequestHttpContent(
            BoxViewClient client,
            string path,
            IDictionary<string, string> getParams = null,
            IDictionary<string, string> postParams = null,
            IDictionary<string, object> requestOptions = null)
        {
            requestOptions["rawResponse"] = true;
            return client.RequestHandler.RequestHttpContent(
                path: path,
                getParams: getParams,
                postParams: postParams,
                requestOptions: requestOptions);
        }

        /// <summary>
        /// Send a new request to the API and return a key-value pair.
        /// </summary>
        /// <param name="client">The client instance to make requests from.</param>
        /// <param name="path">The path to make a request to.</param>
        /// <param name="getParams">A key-value pair of GET params to be added to the URL.</param>
        /// <param name="postParams">A key-value pair of POST params to be sent in the body.</param>
        /// <param name="requestOptions">
        /// A key-value pair of request options that may modify the way the request is  made.
        /// </param>
        /// <returns>The response is pass-thru from Request.</returns>
        /// <exception cref="BoxViewException"></exception>
        protected static IDictionary<string, object> RequestJson(
            BoxViewClient client,
            string path,
            IDictionary<string, string> getParams = null,
            IDictionary<string, string> postParams = null,
            IDictionary<string, object> requestOptions = null)
        {
            return client.RequestHandler.RequestJson(
                path: path,
                getParams: getParams,
                postParams: postParams,
                requestOptions: requestOptions);
        }
    }
}

