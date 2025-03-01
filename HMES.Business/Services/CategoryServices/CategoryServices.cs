using System.Net;
using AutoMapper;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
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
            StatusCodes = (int) HttpStatusCode.OK,
            Response = new ListDataResultModel<CategoryResModel> { Data = resultList.ToList() }
        };
    }
    
    
    private async Task<Category?> GetCategoryWithAllParents(Guid categoryId)
    {
        var category = await _categoryRepository.GetCategoryByIdWithParent(categoryId);

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
                StatusCodes = (int) HttpStatusCode.NotFound,
                Response = null
            };
        }

        var categoryDto = _mapper.Map<CategoryRecursiveResModel>(category);

        return new ResultModel<DataResultModel<CategoryRecursiveResModel>>
        {
            StatusCodes = (int) HttpStatusCode.OK,
            Response = new DataResultModel<CategoryRecursiveResModel> { Data = categoryDto }
        };
    }

    public async Task<ResultModel<DataResultModel<Category>>> GetCategoryById(Guid id)
    {
        
        var category = await _categoryRepository.GetCategoryById(id);
        if (category == null)
        {
            throw new CustomException("Category not found!");
        }
        return new ResultModel<DataResultModel<Category>>
        {
            StatusCodes = (int) HttpStatusCode.OK,
            Response = new DataResultModel<Category> { Data = category }
        };
    }

    
    public async Task<ResultModel<DataResultModel<CategoryResModel>>> CreateCategory(CategoryCreateReqModel category)
    {
        if (category.ParentCategoryId != null)
        {
             bool isSecondLevel = await _categoryRepository.IsSecondLevelCategory(category.ParentCategoryId.Value);
            if (isSecondLevel)
            {
                throw new CustomException("The category parent is at second level!");
            }
        }
        var checkName = await _categoryRepository.GetCategoryByName(category.Name);
        if (checkName != null)
        { 
            throw new CustomException("Name duplicated!");
        }

        try
        {
            var newCategory = _mapper.Map<Category>(category);

            newCategory.Status = CategoryStatusEnums.Active.ToString();

            await _categoryRepository.Insert(newCategory);

            var resCategory = _mapper.Map<CategoryResModel>(newCategory);

            return new ResultModel<DataResultModel<CategoryResModel>>
            {
                StatusCodes = (int)HttpStatusCode.Created,
                Response = new DataResultModel<CategoryResModel> { Data = resCategory }
            };
           
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }

    public async Task<ResultModel<DataResultModel<CategoryResModel>>> UpdateCategory(Guid id, CategoryUpdateReqModel category)
    {
        if (id != category.Id)
            return new ResultModel<DataResultModel<CategoryResModel>>
            {
                StatusCodes = (int) HttpStatusCode.BadRequest,
                Response = null
            };
        try
        {
            var updatingCategory = _mapper.Map<Category>(category);
            await _categoryRepository.Update(updatingCategory);

            var updatedCategory = await _categoryRepository.GetSingle(c => c.Id == id);

            var updatedCategoryRes = _mapper.Map<CategoryResModel>(updatedCategory);

            return new ResultModel<DataResultModel<CategoryResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<CategoryResModel> { Data = updatedCategoryRes }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }

    public async Task<ResultModel<MessageResultModel>> DeleteCategory(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int) HttpStatusCode.NotFound, Response = new MessageResultModel { Message = "Category not found" }
                };

            await _categoryRepository.Delete(category);

            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int) HttpStatusCode.OK, Response = new MessageResultModel { Message = "Category deleted successfully" }
            };
        }
        catch (Exception e)
        {
            throw new CustomException("Cannot delete (in use or having child)!");
        }
    }
    public async Task<ResultModel<List<CategoryResModel>>> GetAllRootCategories()
    {
        
        try
        {
            var rootCategories = await _categoryRepository.GetList(c => c.ParentCategoryId == null);

            var rootCategoryDtos = _mapper.Map<List<CategoryResModel>>(rootCategories);

            return new ResultModel<List<CategoryResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = rootCategoryDtos
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }

    public async Task<ResultModel<List<CategoryResModel>>> GetChildCategories(Guid parentId)
    {
        try
        {
            var childCategories = await _categoryRepository.GetList(c => c.ParentCategoryId == parentId);

            var childCategoryDtos = _mapper.Map<List<CategoryResModel>>(childCategories);

            return new ResultModel<List<CategoryResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = childCategoryDtos
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }
    
    


}