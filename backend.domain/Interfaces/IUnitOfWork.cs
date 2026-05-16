using System;
using System.Collections.Generic;
using System.Text;

namespace backend.domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
