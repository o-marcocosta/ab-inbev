using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Querying;
using Ambev.DeveloperEvaluation.ORM.Querying.FieldMaps;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.AsNoTracking().ApplyFiltering(options.Filters, SaleFieldMap.Fields);

        // Counted after filtering and before paging, so the total reflects every matching sale.
        var totalCount = await query.CountAsync(cancellationToken);
        var sales = await query
            .ApplySorting(options.Sorts, SaleFieldMap.Fields, s => s.SaleNumber)
            .ApplyPagination(options.Page, options.Size)
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

        return new PagedResult<Sale>(sales, totalCount, options.Page, options.Size);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Drop the stale aggregate so the next load reads the current version from the database.
            _context.ChangeTracker.Clear();
            throw new ConcurrencyConflictException($"Sale with ID {sale.Id} was changed by another request.", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
