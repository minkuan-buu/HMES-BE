using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Business.Utilities.TimeZoneHelper;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using Microsoft.Data.SqlClient;

namespace HMES.Business.MapperProfiles
{
    public class MapperProfileConfiguration : Profile
    {
        public MapperProfileConfiguration()
        {
            //Login
            CreateMap<User, UserLoginResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForPath(dest => dest.Auth.DeviceId, opt => opt.Ignore())
                .ForPath(dest => dest.Auth.RefreshToken, opt => opt.MapFrom(src => Authentication.GenerateRefreshToken()))
                .ForPath(dest => dest.Auth.Token, opt => opt.MapFrom(src => Authentication.GenerateJWT(src)));

            //Register
            CreateMap<UserRegisterReqModel, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Customer"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            //Profile
            CreateMap<User, UserProfileResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)));

            //Device
            CreateMap<DeviceCreateReqModel, Device>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<Device, DeviceDetailResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.IsOnline))
                .ForMember(dest => dest.Serial, opt => opt.MapFrom(src => src.Serial))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.WarrantyExpiryDate, opt => opt.MapFrom(src => src.WarrantyExpiryDate));

            CreateMap<Device, ListDeviceDetailResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.IsOnline))
                .ForMember(dest => dest.Serial, opt => opt.MapFrom(src => src.Serial))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.WarrantyExpiryDate, opt => opt.MapFrom(src => src.WarrantyExpiryDate));

            // Category
            CreateMap<Category, CategoryRecursiveResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)))
                .ForMember(dest => dest.ParentCategory, opt => opt.MapFrom(src => src.ParentCategory));

            CreateMap<CategoryCreateReqModel, Category>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CategoryStatusEnums.Active.ToString()));

            CreateMap<CategoryUpdateReqModel, Category>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId));
            CreateMap<Category, CategoryResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)));
            // Product
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Category.Name)));
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))

                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductStatusEnums.Active.ToString()));
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()));

            // Cart
            CreateMap<CartItem, CartItemResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Product.Name)));

            CreateMap<Cart, CartResponseDto>();
            CreateMap<CartItemCreateDto, CartItem>();

            // UserAddress
            CreateMap<UserAddressCreateReqModel, UserAddress>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Address)));



        }
    }
}