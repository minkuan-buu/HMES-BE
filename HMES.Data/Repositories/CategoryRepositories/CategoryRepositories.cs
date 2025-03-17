using HMES.Data.DTO.Custom;
using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.CategoryRepositories;

public class CategoryRepositories : GenericRepositories<Category>, ICategoryRepositories
{
    public CategoryRepositories(HmesContext context) : base(context)
    {
    }

    public async Task<Category?> GetCategoryById(Guid id)
    {
        try
        {
            return await Context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    public async Task<Category?> GetCategoryByName(string name)
    {
        return await Context.Categories
            .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name!, $"%{name}%"));

    }

    public async Task<Category?> GetCategoryByIdWithParent(Guid id)
    {
        return await Context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(
            c => c.Id == id);
    }
    
    public async Task<bool> IsSecondLevelCategory(Guid categoryId)
    {
        var category = await Context.Categories
            .Where(c => c.Id == categoryId && c.ParentCategory != null)
            .Select(c => c.ParentCategoryId)
            .FirstOrDefaultAsync();
        
        if (category == null) return false;
        
        bool isParentRoot = await Context.Categories
            .AnyAsync(c => c.Id == category && c.ParentCategoryId == null);

        return isParentRoot;
    }

    public async Task<bool> IsCategoryInUse(Guid categoryId)
    {
        return await Context.Products.AnyAsync(p => p.CategoryId == categoryId);
    }
}