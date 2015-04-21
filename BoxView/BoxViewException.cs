using System;

namespace BoxView
{
    /// <summary>
    /// Exception extends the default exception class.
    /// It doesn't do anything fancy except be a unique kind of Exception.
    /// </summary>
    public class BoxViewException : Exception
    {
        /// <summary>
        /// A string representing the short form error code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// The constructor function for <see cref="BoxView.BoxViewException"/>.
        /// </summary>
        /// <param name="message">The long form error message.</param>
        /// <param name="code">The short form error code.</param>
        public BoxViewException(string message, string code)
            : base(message)
        {
            Code = code;
        }
    }
}

