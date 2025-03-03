using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Enums;

namespace HMES.Business.Services.ProductServices;

public interface IProductServices
{
    Task<ResultModel<ListDataResultModel<ProductResponseDto>>> GetAllProducts(int pageIndex, int pageSize, ProductStatusEnums status);

    Task<ResultModel<DataResultModel<ProductResponseDto>>> GetProductById(Guid id);

    Task<ResultModel<ListDataResultModel<ProductResponseDto>>> SearchProducts( 
        string? keyword, 
        Guid? categoryId, 
        int? minAmount, 
        int? maxAmount,
        decimal? minPrice, 
        decimal? maxPrice,
        ProductStatusEnums? status, 
        DateTime? createdAfter, 
        DateTime? createdBefore,
        int pageIndex, int pageSize);
    

    Task<ResultModel<DataResultModel<ProductResponseDto>>> AddProduct(ProductCreateDto productDto);

    Task<ResultModel<DataResultModel<ProductResponseDto>>> UpdateProduct(ProductUpdateDto productDto);

    Task<ResultModel<MessageResultModel>> DeleteProduct(Guid id);
}