namespace QuizCompetitionManager.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }

        public static ServiceResult Ok(string message = "") =>
            new ServiceResult { Success = true, Message = message };

        public static ServiceResult Fail(string message, string? errorCode = null) =>
            new ServiceResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
    }
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "") =>
            new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };

        public static ServiceResult<T> Fail(string message, string? errorCode = null) =>
            new ServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Data = default
            };
    }
}