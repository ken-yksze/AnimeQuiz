namespace AnimeQuiz.Models
{
    public enum ServiceStatus { Ok, Created, Updated, Deleted, BadRequest, NotFound, Conflict, UnprocessableEntity, Error }

    public class ServiceResponse
    {
        public ServiceStatus Status { get; set; }

        public List<string> Messages { get; set; } = [];
    }

    [Serializable]
    public class UploadException : Exception
    {
        public UploadException() : base() { }
        public UploadException(string message) : base(message) { }
    }
}
