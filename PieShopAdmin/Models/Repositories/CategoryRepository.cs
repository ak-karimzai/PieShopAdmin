using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PieShopAdmin.Models.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly PieShopDbContext _dbContext;
    private IMemoryCache _memoryCache;
    private const string AllCategoriesCacheKey = "AllCategories";

    public CategoryRepository(PieShopDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        List<Category> allCategories = null;

        if (!_memoryCache.TryGetValue(AllCategoriesCacheKey, out allCategories))
        {
            allCategories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryId)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60));
            _memoryCache.Set(AllCategoriesCacheKey, allCategories, cacheEntryOptions);
        }

        return allCategories;
    }

    public IEnumerable<Category> GetAllCategories()
    {
        return _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.CategoryId);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Include(category => category.Pies)
            .FirstOrDefaultAsync(category => category.CategoryId == id);
    }

    public async Task<int> AddCategoryAsync(Category category)
    {
        bool categoryWithSameNameExist = await _dbContext.Categories
            .AnyAsync(cat => cat.Name.Equals(category.Name));
        
        if (categoryWithSameNameExist)
            throw new Exception("A category with the same name already exists");
        
        _dbContext.Categories.Add(category);
        
        int result =  await _dbContext.SaveChangesAsync();
        
        _memoryCache.Remove(AllCategoriesCacheKey);
        
        return result;
    }

    public async Task<int> UpdateCategoryAsync(Category category)
    {
        bool categoryWithSameNameExist = await
            _dbContext.Categories.AnyAsync(c => c.Name == category.Name
                                                && c.CategoryId != category.CategoryId);
        if (categoryWithSameNameExist)
        {
            throw new Exception("A category with the same name already exists");
        }

        var categoryToUpdate = await
            _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);
        if (categoryToUpdate != null)
        {
            categoryToUpdate.Name = category.Name;
            categoryToUpdate.Description = category.Description;

            _dbContext.Categories.Update(categoryToUpdate);
            int result = await _dbContext.SaveChangesAsync();
            
            _memoryCache.Remove(AllCategoriesCacheKey);

            return result;
        }
        else
        {
            throw new ArgumentException($"The category to update not found.");
        }
    }

    public async Task<int> DeleteCategoryAsync(int id)
    {
        var categoryToDelete = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        var piesInCategory = await _dbContext.Pies.AnyAsync(pie => pie.CategoryId == id);

        if (piesInCategory)
        {
            throw new Exception(
                "Pies exist in this category. Delete all pies in this category before deleting the category.");
        }
        
        if (categoryToDelete != null)
        {
            _dbContext.Categories.Remove(categoryToDelete);
            return await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException($"The category to delete can't be found.");
        }
    }

    public async Task<int> UpdateCategoryNamesAsync(List<Category> categories)
    {
        foreach (var category in categories)
        {
            var categoryToUpdate = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (categoryToUpdate != null)
            {
                categoryToUpdate.Name = category.Name;

                _dbContext.Categories.Update(categoryToUpdate);
            }
        }

        int result = await _dbContext.SaveChangesAsync();
        
        _memoryCache.Remove(AllCategoriesCacheKey);
        
        return result;
    }
}