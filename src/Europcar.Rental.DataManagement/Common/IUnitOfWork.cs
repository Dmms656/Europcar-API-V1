using Europcar.Rental.DataAccess.Context;

namespace Europcar.Rental.DataManagement.Common;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
