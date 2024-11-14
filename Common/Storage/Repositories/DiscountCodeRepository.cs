using Common.Storage.Models;
using Common.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Storage.Repositories;

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly AppDbContext _context;

    public DiscountCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsCodeValidAsync(string discountCode, CancellationToken cancellationToken)
    {
        return await _context.DiscountCodes.AnyAsync(x => x.Code == discountCode.ToUpper(), cancellationToken);
    }

    public async Task<List<string>> GetDuplicatesAsync(List<string> codes, CancellationToken cancellationToken) =>
        await _context.DiscountCodes.Where(x => codes.Contains(x.Code)).Select(x => x.Code).ToListAsync(cancellationToken);

    public async Task InsertCodesAsync(List<string> codes, CancellationToken cancellationToken)
    {
        var entities = codes.Select(x => new DiscountCode
        {
            Code = x
        }).ToList();

        await _context.DiscountCodes.AddRangeAsync(entities, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
