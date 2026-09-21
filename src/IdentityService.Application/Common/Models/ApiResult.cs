namespace IdentityService.Application.Common.Models
{
    public sealed class ApiResult<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }

        public static ApiResult<T> Ok(
            T? data,
            string message = "ดำเนินการสำเร็จ")
        {
            return new ApiResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
    }
}
