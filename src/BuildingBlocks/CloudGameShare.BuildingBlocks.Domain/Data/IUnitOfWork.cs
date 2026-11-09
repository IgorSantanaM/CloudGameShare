using System;
using System.Collections.Generic;
using System.Text;

namespace CloudGameShare.Domain.Core.Data
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
