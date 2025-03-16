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
using HMES.Data.Repositories.ProductRepositories;
using Microsoft.IdentityModel.Tokens;

namespace HMES.Business.Services.ProductServices;

public class ProductServices : IProductServices
{
    private readonly IProductRepositories _productRepository;
    private readonly ICategoryRepositories _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICloudServices _cloudServices;

    public ProductServices(IProductRepositories productRepository, ICategoryRepositories categoryRepository,
        IMapper mapper, ICloudServices cloudServices)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cloudServices = cloudServices;
    }

    public async Task<ResultModel<ListDataResultModel<ProductBriefResponseDto>>> GetAllProducts(int pageIndex, int pageSize,
        ProductStatusEnums status)
    {
        try
        {
            var (products, totalItems) = await _productRepository.GetListWithPagination(pageIndex, pageSize, status);
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var result = new ListDataResultModel<ProductBriefResponseDto>
            {
                Data = _mapper.Map<List<ProductBriefResponseDto>>(products),
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            return new ResultModel<ListDataResultModel<ProductBriefResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK, Response = result
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
                    StatusCodes = (int)HttpStatusCode.NotFound, Response = null
                };
            }

            return new ResultModel<DataResultModel<ProductResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<ProductResponseDto>
                {
                    Data = _mapper.Map<ProductResponseDto>(product)
                }
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    public async Task<ResultModel<ListDataResultModel<ProductResponseDto>>> SearchProducts(string? keyword,
        Guid? categoryId, int? minAmount, int? maxAmount, decimal? minPrice, decimal? maxPrice,
        ProductStatusEnums? status, DateTime? createdAfter, DateTime? createdBefore, int pageIndex, int pageSize)
    {
        try
        {
            var encodedKeyword = TextConvert.ConvertToUnicodeEscape(keyword??string.Empty);
            var (products, totalItems) = await _productRepository.GetProductsWithPagination(encodedKeyword, categoryId,
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
                StatusCodes = (int)HttpStatusCode.OK, Response = result
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
            var isSecondLevel = await _categoryRepository.IsSecondLevelCategory(productDto.CategoryId);
            if (!isSecondLevel)
            {
                throw new CustomException("The category parent is not at second level!");
            }

            var product = _mapper.Map<Product>(productDto);
            var filePath = $"product/{product.Id}/attachments";
            if (!productDto.Images.IsNullOrEmpty())
            {
                var attachments = await _cloudServices.UploadFile(productDto.Images, filePath);
                
                var productAttachments = new List<ProductAttachment>();
                productAttachments.AddRange(attachments.Select(attachment =>
                    new ProductAttachment { Id = Guid.NewGuid(), ProductId = product.Id, Attachment = attachment }));
                product.ProductAttachments = productAttachments;
                
            }
            var mainImage = await _cloudServices.UploadSingleFile(productDto.MainImage, filePath);
            product.MainImage = mainImage;

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
                    StatusCodes = (int)HttpStatusCode.NotFound, Response = null
                };
            }

            // Update product details
            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            // Handle attachments
            var existingAttachments = product.ProductAttachments.Select(pa => pa.Attachment).ToList();
            var updatedAttachments = productDto.OldImages;

            // Identify attachments to delete
            var attachmentsToDelete = existingAttachments.Except(updatedAttachments).ToList();
            foreach (var attachment in attachmentsToDelete)
            {
                var productAttachment = product.ProductAttachments.FirstOrDefault(pa => pa.Attachment == attachment);
                if (productAttachment != null)
                {
                    product.ProductAttachments.Remove(productAttachment);
                }
            }

            // Identify and add new images
            if (!productDto.NewImages.IsNullOrEmpty() && productDto.NewImages.Count != 0)
            {
                var filePath = $"product/{product.Id}/attachments";
                var newAttachments = await _cloudServices.UploadFile(productDto.NewImages, filePath);
                foreach (var attachment in newAttachments)
                {
                    product.ProductAttachments.Add(new ProductAttachment
                    {
                        Id = Guid.NewGuid(), ProductId = product.Id, Attachment = attachment
                    });
                }
            }

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

            product.ProductAttachments.Clear();
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