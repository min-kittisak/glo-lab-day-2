using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Users.Models
{
    public sealed class UserQuery
    {
        public int Page { get; init; }

        public int PageSize { get; init; }

        public string? Search { get; init; }

        public string? DepartmentCode { get; init; }

        public bool? IsActive { get; init; }

        public string? SortBy { get; init; }

        public string SortDirection { get; init; } = "asc";
    }
}
