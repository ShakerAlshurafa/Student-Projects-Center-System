using System.Net;

namespace StudentProjectsCenterSystem.Core.Entities
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorMessages { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public object Result { get; set; }

    }
}
