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
            if (exception is ValidationException validationException)
            {
                var validationProblem =
                    CreateValidationProblem(validationException);

                validationProblem.Instance =
                    httpContext.Request.Path;

                validationProblem.Extensions["traceId"] =
                    httpContext.TraceIdentifier;

                if (httpContext.Items.TryGetValue(
                        "CorrelationId",
                        out var correlationId))
                {
                    validationProblem.Extensions["correlationId"] =
                        correlationId?.ToString();
                }

                httpContext.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                await httpContext.Response.WriteAsJsonAsync(
                    validationProblem,
                    cancellationToken);

                return true;
            }

            var problemDetails = exception switch
            {

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


                UnauthorizedException =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "ไม่สามารถเข้าสู่ระบบได้",
                        Detail = exception.Message
                    },

                _ =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "เกิดข้อผิดพลาดภายในระบบ",
                        Detail =
                            "ระบบไม่สามารถดำเนินการได้ กรุณาลองใหม่อีกครั้ง"
                    }
            };

            problemDetails.Instance =
                httpContext.Request.Path;

            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            if (httpContext.Items.TryGetValue(
                    "CorrelationId",
                    out var correlationIdValue))
            {
                problemDetails.Extensions["correlationId"] =
                    correlationIdValue?.ToString();
            }

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

        private static ValidationProblemDetails CreateValidationProblem(
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
