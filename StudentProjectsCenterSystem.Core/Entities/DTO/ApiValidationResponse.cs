namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class ApiValidationResponse : ApiResponse
    {
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public ApiValidationResponse(IEnumerable<string> errors, int? statusCode = 400) : base(statusCode)
        {
            Errors = errors ?? new List<string>();
        }

    }
}
