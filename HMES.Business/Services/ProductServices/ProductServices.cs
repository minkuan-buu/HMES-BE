using System.Net;
using AutoMapper;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CategoryRepositories;
using HMES.Data.Repositories.ProductRepositories;

namespace HMES.Business.Services.ProductServices;

public class ProductServices : IProductServices
{
    private readonly IProductRepositories _productRepository;
    private readonly ICategoryRepositories _categoryRepository;
    private readonly IMapper _mapper;

    public ProductServices(IProductRepositories productRepository,ICategoryRepositories categoryRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ResultModel<ListDataResultModel<ProductResponseDto>>> GetAllProducts(int pageIndex, int pageSize,ProductStatusEnums status)
    {
        try
        {
            var (products, totalItems) = await _productRepository.GetListWithPagination(pageIndex, pageSize, status);
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = new ListDataResultModel<ProductResponseDto>
            {
                Data = _mapper.Map<List<ProductResponseDto>>(products),
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return new ResultModel<ListDataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }

    public async Task<ResultModel<DataResultModel<ProductResponseDto>>> GetProductById(Guid id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return new ResultModel<DataResultModel<ProductResponseDto>>
                {
                    StatusCodes = (int)HttpStatusCode.NotFound,
                    Response = null
                };
            }

            return new ResultModel<DataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<ProductResponseDto> { Data = _mapper.Map<ProductResponseDto>(product) }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        
    }

    public async Task<ResultModel<ListDataResultModel<ProductResponseDto>>> SearchProducts( string? keyword, 
        Guid? categoryId, 
        int? minAmount, int? maxAmount,
        decimal? minPrice, decimal? maxPrice,
        ProductStatusEnums? status, 
        DateTime? createdAfter, DateTime? createdBefore,
        int pageIndex, int pageSize)
    {
        
        try
        {
            var (products, totalItems) = await _productRepository.GetProductsWithPagination(keyword, categoryId,
                minAmount, maxAmount, minPrice, maxPrice, status, createdAfter, createdBefore, pageIndex, pageSize);
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = new ListDataResultModel<ProductResponseDto>
            {
                Data = _mapper.Map<List<ProductResponseDto>>(products),
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return new ResultModel<ListDataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        
    }

    public async Task<ResultModel<DataResultModel<ProductResponseDto>>> AddProduct(ProductCreateDto productDto)
    {
        try
        {
            bool isSecondLevel = await _categoryRepository.IsSecondLevelCategory(productDto.CategoryId);
            if (!isSecondLevel)
            {
                throw new CustomException("The category parent is not at second level!");
            }

            var product = _mapper.Map<Product>(productDto);

            await _productRepository.Insert(product);

            var createdProduct = _mapper.Map<ProductResponseDto>(product);

            return new ResultModel<DataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.Created,
                Response = new DataResultModel<ProductResponseDto> { Data = createdProduct }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
    }

    public async Task<ResultModel<DataResultModel<ProductResponseDto>>> UpdateProduct(ProductUpdateDto productDto)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productDto.Id);
            if (product == null)
            {
                return new ResultModel<DataResultModel<ProductResponseDto>>
                {
                    StatusCodes = (int)HttpStatusCode.NotFound,
                    Response = null
                };
            }

            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.Update(product);

            var updatedProduct = _mapper.Map<ProductResponseDto>(product);

            return new ResultModel<DataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<ProductResponseDto> { Data = updatedProduct }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    public async Task<ResultModel<MessageResultModel>> DeleteProduct(Guid id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int)HttpStatusCode.NotFound,
                    Response = new MessageResultModel { Message = "Product not found" }
                };
            }

            await _productRepository.Delete(product);

            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel { Message = "Product deleted successfully" }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }
}
