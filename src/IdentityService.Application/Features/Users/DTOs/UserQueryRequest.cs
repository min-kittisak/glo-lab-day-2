using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Users.DTOs
{
    public sealed class UserQueryRequest
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 20;

        public string? Search { get; init; }

        public string? DepartmentCode { get; init; }

        public bool? IsActive { get; init; }

        public string? SortBy { get; init; }

        public string SortDirection { get; init; } = "asc";
    }
}
