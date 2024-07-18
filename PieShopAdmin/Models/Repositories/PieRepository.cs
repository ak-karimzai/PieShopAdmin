using Microsoft.EntityFrameworkCore;

namespace PieShopAdmin.Models.Repositories;

public class PieRepository : IPieRepository
{
    private readonly PieShopDbContext _dbContext;

    public PieRepository(PieShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Pie>> GetAllPiesAsync()
    {
        return await _dbContext.Pies
            .OrderBy(pie => pie.PieId)
            .ToListAsync();
    }

    public async Task<Pie?> GetPieByIdAsync(int pieId)
    {
       return await _dbContext.Pies
            .AsNoTracking()
            .Include(pie => pie.Category)
            .Include(pie => pie.Ingredients)
            .FirstOrDefaultAsync(pie => pie.PieId == pieId);
    }

    public async Task<int> AddPieAsync(Pie pie)
    {
        _dbContext.Pies.Add(pie);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<int> UpdatePieAsync(Pie pie)
    {
        var pieToUpdate = await _dbContext
            .Pies.FirstOrDefaultAsync(c => c.PieId == pie.PieId);
        if (pieToUpdate != null)
        {
            _dbContext
                .Entry(pieToUpdate)
                .Property("RowVersion")
                .OriginalValue = pie.RowVersion;
            pieToUpdate.CategoryId = pie.CategoryId;
            pieToUpdate.ShortDescription = pie.ShortDescription;
            pieToUpdate.LongDescription = pie.LongDescription;
            pieToUpdate.Price = pie.Price;
            pieToUpdate.AllergyInformation = pie.AllergyInformation;
            pieToUpdate.ImageThumbnailUrl = pie.ImageThumbnailUrl;
            pieToUpdate.ImageUrl = pie.ImageUrl;
            pieToUpdate.InStock = pie.InStock;
            pieToUpdate.IsPieOfTheWeek = pie.IsPieOfTheWeek;
            pieToUpdate.Name = pie.Name;

            _dbContext.Pies.Update(pieToUpdate);
            return await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException($"The pie to update can't be found.");
        }
    }

    public async Task<int> DeletePieAsync(int id)
    {
        var pieToDelete = await _dbContext.Pies
            .FirstOrDefaultAsync(p => p.PieId == id);
        if (pieToDelete != null)
        {
            _dbContext.Pies.Remove(pieToDelete);
            return await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException($"The category to delete can't be found.");
        }
    }

    public async Task<int> GetAllPiesCountAsync()
    {
        var count = await _dbContext.Pies.CountAsync();
        return count;
    }

    public async Task<IEnumerable<Pie>> GetPiesPagesAsync(int? pageNumber, int pageSize)
    {
        IQueryable<Pie> pies = from p in _dbContext.Pies
            select p;

        pageNumber ??= 1;
        pies = pies.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);
        
        return await pies.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Pie>> GetPiesSortedAndPagedAsync(string sortBy, int? pageNumber, int pageSize)
    {
        IQueryable<Pie> pies = from p in _dbContext.Pies
            select p;

        switch (sortBy)
        {
            case "name_desc":
                pies = pies.OrderByDescending(p => p.Name);
                break;
            case "name":
                pies = pies.OrderBy(p => p.Name);
                break;
            case "id_desc":
                pies = pies.OrderByDescending(p => p.PieId);
                break;
            case "id":
                pies = pies.OrderBy(p => p.PieId);
                break;
            case "price_desc":
                pies = pies.OrderByDescending(p => p.Price);
                break;
            case "price":
                pies = pies.OrderBy(p => p.Price);
                break;
        }

        pageNumber ??= 1;
        pies = pies.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);
        return await pies.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Pie>> SearchPies(string searchQuery, int? categoryId)
    {
        var pies = from p in _dbContext.Pies 
            select p;

        if (!string.IsNullOrEmpty(searchQuery))
        {
            pies = pies.Where(s =>
                s.Name.Contains(searchQuery) || s.ShortDescription.Contains(searchQuery) ||
                s.LongDescription.Contains(searchQuery));
        }

        if (categoryId != null)
        {
            pies = pies.Where(s => s.CategoryId == categoryId.Value);
        }

        return await pies.AsNoTracking().ToListAsync();
    }

}