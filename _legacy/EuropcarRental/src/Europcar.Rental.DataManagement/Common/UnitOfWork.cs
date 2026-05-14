using Europcar.Rental.DataAccess.Context;

namespace Europcar.Rental.DataManagement.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly RentalDbContext _context;

    public UnitOfWork(RentalDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}
