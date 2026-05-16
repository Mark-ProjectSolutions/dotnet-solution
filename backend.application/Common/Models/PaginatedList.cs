using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.Common.Models
{
    public sealed record PaginatedList<T>(
        IReadOnlyList<T> Data,
        int Total,
        int Page,
        int Limit)
    {
        public int TotalPages => (int)Math.Ceiling(Total / (double)Limit);
    }

}
