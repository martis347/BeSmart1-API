using System;

namespace Lunch.Domain.ErrorHandling
{
    public class ApiError
    {
        public string message { get; set; }
        public string detail { get; set; }

        public ApiError(string message)
        {
            this.message = message;
        }

        public ApiError(Exception exception)
        {
            message = exception.Message;
            detail = exception.InnerException?.Message;
        }
    }
}