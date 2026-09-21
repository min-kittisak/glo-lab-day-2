using IdentityService.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace IdentityService.Api.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = exception switch
            {
                ValidationException validationException =>
                    CreateValidationProblem(validationException),

                NotFoundException =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "ไม่พบข้อมูล",
                        Detail = exception.Message
                    },

                ConflictException =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "ข้อมูลซ้ำ",
                        Detail = exception.Message
                    },

                _ =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "เกิดข้อผิดพลาดภายในระบบ",
                        Detail = "ระบบไม่สามารถดำเนินการได้ กรุณาลองใหม่อีกครั้ง"
                    }
            };

            problemDetails.Instance = httpContext.Request.Path;

            problemDetails.Extensions["traceId"] =  httpContext.TraceIdentifier;
            problemDetails.Extensions["correlationId"] = httpContext.Response.Headers["X-Correlation-ID"].ToString();

            if (exception is not ValidationException &&
                exception is not NotFoundException &&
                exception is not ConflictException)
            {
                _logger.LogError(
                    exception,
                    "เกิดข้อผิดพลาดที่ไม่ได้รับการจัดการ TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
            }

            httpContext.Response.StatusCode =
                problemDetails.Status
                ?? StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        private static ProblemDetails CreateValidationProblem(
            ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .Select(e => e.ErrorMessage)
                        .Distinct()
                        .ToArray());

            return new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "ข้อมูลไม่ถูกต้อง",
                Detail = "กรุณาตรวจสอบข้อมูลที่ระบุ"
            };
        }
    }
}
