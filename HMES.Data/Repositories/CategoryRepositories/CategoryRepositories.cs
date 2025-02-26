using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.CategoryRepositories;

public class CategoryRepositories : GenericRepositories<Category>, ICategoryRepositories
{
    public CategoryRepositories(HmesContext context) : base(context)
    {
    }
}