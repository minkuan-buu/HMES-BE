using AutoMapper;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.CategoryRepositories;

namespace HMES.Business.Services.CategoryServices;

public class CategoryServices : ICategoryServices
{
    
    private readonly ICategoryRepositories _categoryRepository;
    private readonly IMapper _mapper;
    
    public CategoryServices(ICategoryRepositories categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }
    
    
    public async Task<ResultModel<ListDataResultModel<CategoryResModel>>> GetCategories()
    {
        var categories = await _categoryRepository.GetList();

        List<CategoryResModel> resultList = new List<CategoryResModel>();
        
        foreach (var category in categories)
        {
            var result = _mapper.Map<CategoryResModel>(category);
            resultList.Add(result);
        }
        
        return new ResultModel<ListDataResultModel<CategoryResModel>>
        {
            StatusCodes = 200,
            Response = new ListDataResultModel<CategoryResModel> { Data = resultList.ToList() }
        };
    }
    
    
    private async Task<Category> GetCategoryWithAllParents(Guid categoryId)
    {
        var category = await _categoryRepository.GetSingle(
            c => c.Id == categoryId,
            includeProperties: "ParentCategory"
        );

        if (category?.ParentCategory != null)
        {
            category.ParentCategory = await GetCategoryWithAllParents(category.ParentCategory.Id);
        }

        return category;
    }
    
    public async Task<ResultModel<DataResultModel<CategoryRecursiveResModel>>> GetCategoryWithParents(Guid categoryId)
    {
        var category = await GetCategoryWithAllParents(categoryId);

        if (category == null)
        {
            return new ResultModel<DataResultModel<CategoryRecursiveResModel>>
            {
                StatusCodes = 404,
                Response = null
            };
        }

        var categoryDto = _mapper.Map<CategoryRecursiveResModel>(category);

        return new ResultModel<DataResultModel<CategoryRecursiveResModel>>
        {
            StatusCodes = 200,
            Response = new DataResultModel<CategoryRecursiveResModel> { Data = categoryDto }
        };
    }

    public async Task<ResultModel<DataResultModel<Category>>> GetCategoryById(Guid id)
    {
        var category = await _categoryRepository.GetSingle(c => c.Id == id);
        if (category == null)
        {
            throw new CustomException("Category not found!");
        }
        return new ResultModel<DataResultModel<Category>>
        {
            StatusCodes = 200,
            Response = new DataResultModel<Category> { Data = category }
        };
    }

    
    public async Task<ResultModel<DataResultModel<CategoryResModel>>> CreateCategory(CategoryCreateReqModel category)
    {

        var checkName = _categoryRepository.GetSingle(c => c.Name.Equals(category.Name));
        if (checkName != null)
        { 
            throw new CustomException("Name duplicated!");
        }
        
        var newCategory = _mapper.Map<Category>(category);

        await _categoryRepository.Insert(newCategory);
        
        var resCategory = _mapper.Map<CategoryResModel>(category);
        
        return new ResultModel<DataResultModel<CategoryResModel>>
        {
            StatusCodes = 201,
            Response = new DataResultModel<CategoryResModel> { Data = resCategory }
        };
    }

    public async Task<ResultModel<DataResultModel<CategoryResModel>>> UpdateCategory(Guid id, CategoryUpdateReqModel category)
    {
        if (id != category.Id)
            return new ResultModel<DataResultModel<CategoryResModel>>
            {
                StatusCodes = 400,
                Response = null
            };

        var updatingCategory = _mapper.Map<Category>(category);
        await _categoryRepository.Update(updatingCategory);
        
        var updatedCategory = await _categoryRepository.GetSingle(c => c.Id == id);

        var updatedCategoryRes = _mapper.Map<CategoryResModel>(updatedCategory);

        return new ResultModel<DataResultModel<CategoryResModel>>
        {
            StatusCodes = 200,
            Response = new DataResultModel<CategoryResModel> { Data = updatedCategoryRes }
        };
    }
    
    public async Task<ResultModel<MessageResultModel>> DeleteCategory(Guid id)
    {
        var category = await _categoryRepository.GetSingle(c => c.Id == id);
        if (category == null)
            return new ResultModel<MessageResultModel> { StatusCodes = 404, Response = new MessageResultModel { Message = "Category not found" } };

        await _categoryRepository.Delete(category);
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = 200,
            Response = new MessageResultModel { Message = "Category deleted successfully" }
        };
    }
    public async Task<ResultModel<List<CategoryResModel>>> GetAllRootCategories()
    {
        var rootCategories = await _categoryRepository.GetList(c => c.ParentCategoryId == null);

        var rootCategoryDtos = _mapper.Map<List<CategoryResModel>>(rootCategories);

        return new ResultModel<List<CategoryResModel>>
        {
            StatusCodes = 200,
            Response = rootCategoryDtos
        };
    }

    public async Task<ResultModel<List<CategoryResModel>>> GetChildCategories(Guid parentId)
    {
        var childCategories = await _categoryRepository.GetList(c => c.ParentCategoryId == parentId);

        var childCategoryDtos = _mapper.Map<List<CategoryResModel>>(childCategories);

        return new ResultModel<List<CategoryResModel>>
        {
            StatusCodes = 200,
            Response = childCategoryDtos
        };
    }
    
    


}