using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.CategoryServices;

public interface ICategoryServices
{
    Task<ResultModel<ListDataResultModel<CategoryResModel>>> GetCategories();
    Task<ResultModel<DataResultModel<CategoryRecursiveResModel>>> GetCategoryWithParents(Guid categoryId);
    Task<ResultModel<DataResultModel<Category>>> GetCategoryById(Guid id);
    Task<ResultModel<DataResultModel<CategoryResModel>>> CreateCategory(CategoryCreateReqModel category);
    Task<ResultModel<DataResultModel<CategoryResModel>>> UpdateCategory(Guid id, CategoryUpdateReqModel category);
    Task<ResultModel<MessageResultModel>> DeleteCategory(Guid id);
    Task<ResultModel<List<CategoryResModel>>> GetAllRootCategories();
    Task<ResultModel<List<CategoryResModel>>> GetChildCategories(Guid parentId);
}