using System.Net;
using AutoMapper;
using HMES.Business.Services.CloudServices;
using HMES.Business.Utilities.Converter;
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
    private readonly ICloudServices _cloudServices;
    
    public CategoryServices(ICategoryRepositories categoryRepository, IMapper mapper, ICloudServices cloudServices)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cloudServices = cloudServices;
    }
    
    
    public async Task<ResultModel<ListDataResultModel<CategoryFamiliesResModel>>> GetCategories()
    {
        var categories = await _categoryRepository.GetList();
        
        var categoryMap = categories.ToDictionary(c => c.Id, c => _mapper.Map<CategoryFamiliesResModel>(c));
        
        var rootCategories = new List<CategoryFamiliesResModel>();

        foreach (var category in categories)
        {
            if (category.ParentCategoryId != null)
            {
                if (categoryMap.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(categoryMap[category.Id]);
                }
            }
            else
            {
                rootCategories.Add(categoryMap[category.Id]);
            }
        }

        return new ResultModel<ListDataResultModel<CategoryFamiliesResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new ListDataResultModel<CategoryFamiliesResModel>
            {
                Data = rootCategories
            }
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

        var nameConverted = TextConvert.ConvertToUnicodeEscape(category.Name);
        var checkName = await _categoryRepository.GetCategoryByName(nameConverted);
        if (checkName != null)
        { 
            throw new CustomException("Name duplicated!");
        }

        try
        {
            var newCategory = _mapper.Map<Category>(category);

            newCategory.Status = CategoryStatusEnums.Active.ToString();
            
            if(category.Attachment != null)
            {
                var filePath = $"category/{newCategory.Id}/attachments";
                var mainImage = await _cloudServices.UploadSingleFile(category.Attachment, filePath);
                newCategory.Attachment = mainImage;
            }
            
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

    public async Task<ResultModel<DataResultModel<CategoryResModel>>> UpdateCategory(CategoryUpdateReqModel category)
    {
        try
        {
            
            var oldCategory = await _categoryRepository.GetCategoryById(category.Id);
            if(oldCategory == null)
            {
                throw new CustomException("Category not found!");
            }
            
            var updatingCategory = _mapper.Map<Category>(category);
            updatingCategory.Attachment = oldCategory.Attachment;
            
            if(category.Attachment != null)
            {
                var filePath = $"category/{category.Id}/attachments";
                var mainImage = await _cloudServices.UploadSingleFile(category.Attachment, filePath);
                updatingCategory.Attachment = mainImage;
            }
            
            await _categoryRepository.Update(updatingCategory);
            
            var updatedCategoryRes = _mapper.Map<CategoryResModel>(updatingCategory);

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
                    StatusCodes = (int)HttpStatusCode.NotFound,
                    Response = new MessageResultModel { Message = "Category not found" }
                };

            var isCategoryInUse = await _categoryRepository.IsCategoryInUse(id);
            if (isCategoryInUse)
                throw new CustomException("Cannot delete (in use)!");
            
            var filePath = $"category/{category.Id}/attachments";
            await _cloudServices.DeleteFilesInPathAsync(filePath);
            
            await _categoryRepository.Delete(category);

            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel { Message = "Category deleted successfully" }
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