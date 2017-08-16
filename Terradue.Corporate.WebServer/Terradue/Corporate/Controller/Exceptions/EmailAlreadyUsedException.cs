using System;
using System.Runtime.Serialization;

namespace Terradue.Corporate.Controller {
    [Serializable]
    internal class EmailAlreadyUsedException : Exception {
        public EmailAlreadyUsedException() {
        }

        public EmailAlreadyUsedException(string message) : base(message) {
        }

        public EmailAlreadyUsedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EmailAlreadyUsedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}