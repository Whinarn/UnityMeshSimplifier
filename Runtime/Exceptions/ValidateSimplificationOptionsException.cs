using System;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// An exception thrown when validating simplification options.
    /// </summary>
    public sealed class ValidateSimplificationOptionsException : Exception
    {
        private readonly string propertyName;

        /// <summary>
        /// Creates a new simplification options validation exception.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="message">The exception message.</param>
        public ValidateSimplificationOptionsException(string propertyName, string message)
            : base(message)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Creates a new simplification options validation exception.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that caused the validation error.</param>
        public ValidateSimplificationOptionsException(string propertyName, string message, Exception innerException)
            : base(message, innerException)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Gets the property name that caused the validation error.
        /// </summary>
        public string PropertyName
        {
            get { return propertyName; }
        }

        /// <summary>
        /// Gets the message of the exception.
        /// </summary>
        public override string Message
        {
            get { return base.Message + Environment.NewLine + "Property name: " + propertyName; }
        }
    }
}
