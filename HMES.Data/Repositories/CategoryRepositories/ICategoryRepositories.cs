using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.CategoryRepositories
{
    public interface ICategoryRepositories: IGenericRepositories<Category>
    {
        
        Task<Category?> GetCategoryById(Guid id);
        Task<Category?> GetCategoryByName(string name);
        Task<Category?> GetCategoryByIdWithParent(Guid id);
        Task<bool> IsSecondLevelCategory(Guid categoryId);

    }
}

