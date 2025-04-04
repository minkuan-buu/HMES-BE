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

            CreateMap<CreateModUserModel, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            //Profile
            CreateMap<User, UserProfileResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)));
            CreateMap<User, StaffBriefInfoModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)));


            //Device
            CreateMap<DeviceCreateReqModel, Device>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<Device, DeviceDetailResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));


            CreateMap<Device, ListDeviceDetailResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachment))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            //DeviceItems
            CreateMap<DeviceItem, ListActiveDeviceResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.PlantName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Plant.Name)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => src.IsOnline))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.WarrantyExpiryDate, opt => opt.MapFrom(src => src.WarrantyExpiryDate));

            // Category
            CreateMap<Category, CategoryRecursiveResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)))
                .ForMember(dest => dest.ParentCategory, opt => opt.MapFrom(src => src.ParentCategory));

            CreateMap<Category, CategoryFamiliesResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)));

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
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description ?? "")))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Category.Name)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductAttachments.Select(pa => pa.Attachment)));

            CreateMap<Product, ProductBriefResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Category.Name)));


            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()));

            // Cart
            CreateMap<CartItem, CartItemResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Product.Name)))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Product.MainImage ?? "")));

            CreateMap<Cart, CartResponseDto>();
            CreateMap<CartItemCreateDto, CartItem>();

            // UserAddress
            CreateMap<UserAddressCreateReqModel, UserAddress>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Address)));

            CreateMap<UserAddressUpdateReqModel, UserAddress>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Address)));

            CreateMap<UserAddress, ListUserAddressResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Address)));


            // Ticket
            CreateMap<Ticket, TicketBriefDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.User.Name)))
                .ForMember(dest => dest.HandledBy, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Technician.Name ?? "")))
                .ForMember(dest => dest.BriefDescription, opt => opt.MapFrom(src => src.Description.Length > 100 ? TextConvert.ConvertFromUnicodeEscape(src.Description.Substring(0, 100)) : TextConvert.ConvertFromUnicodeEscape(src.Description)));

            CreateMap<Ticket, TicketDetailsDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.User.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Description)))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.TicketAttachments.Select(ta => ta.Attachment)))
                .ForMember(dest => dest.TicketResponses, opt => opt.MapFrom(src => src.TicketResponses)) // Map trực tiếp từ TicketResponses
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<TicketResponse, TicketResponseDetailsDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.User.Name)))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Message)))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.TicketResponseAttachments.Select(tra => tra.Attachment)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<TicketCreateDto, Ticket>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Description)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TicketStatusEnums.Pending.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.IsProcessed, opt => opt.MapFrom(src => false));

            CreateMap<TicketResponseDto, TicketResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Message)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => TimeZoneHelper.GetCurrentHoChiMinhTime()));

            // Device Item for end-user
            CreateMap<DeviceItem, DeviceItemDetailResModel>()
                .ForMember(dest => dest.DeviceItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceItemName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Device.Name)))
                .ForMember(dest => dest.PlantName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Plant != null ? src.Plant.Name : string.Empty)))
                .ForMember(dest => dest.isOnline, opt => opt.MapFrom(src => src.IsOnline))
                .ForMember(dest => dest.Serial, opt => opt.MapFrom(src => src.Serial))
                .ForMember(dest => dest.WarrantyExpiryDate, opt => opt.MapFrom(src => src.WarrantyExpiryDate))
                .ForMember(dest => dest.LastUpdatedDate, opt => opt.MapFrom(src => src.NutritionReports != null && src.NutritionReports.Count > 0 ? src.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault().CreatedAt : DateTime.Now))
                .ForMember(dest => dest.IoTData, opt => opt.MapFrom(src => new IoTResModel()
                {
                    SoluteConcentration = src.NutritionReports != null && src.NutritionReports.Count > 0 ? src.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault().NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "SoluteConcentration").RecordValue : 0,
                    Temperature = src.NutritionReports != null && src.NutritionReports.Count > 0 ? src.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault().NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Temperature").RecordValue : 0,
                    Ph = src.NutritionReports != null && src.NutritionReports.Count > 0 ? src.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault().NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Ph").RecordValue : 0,
                    WaterLevel = src.NutritionReports != null && src.NutritionReports.Count > 0 ? src.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault().NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "WaterLevel").RecordValue : 0
                }));

            // Plant

            CreateMap<Plant, PlantResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<PlantReqModel, Plant>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Plant, PlantResModelWithTarget>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Target, opt => opt.MapFrom(src => src.TargetOfPlants.Select(top => top.TargetValue)));
            // Target value
            CreateMap<TargetValue, TargetResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
                .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue));

            CreateMap<TargetReqModel, TargetValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
                .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue));

            CreateMap<TargetValue, TargetResModelWithPlants>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
                .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue))
                .ForMember(dest => dest.Plants, opt => opt.MapFrom(src => src.TargetOfPlants.Select(t => t.Plant)));
            // Order
            
            CreateMap<Order,OrderResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.User.Name)))
                .ForMember(dest => dest.UserAddressId, opt => opt.MapFrom(src => src.UserAddressId))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<Order, OrderDetailsResModel>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.OrderDetailsItems, opt => opt.MapFrom(src => src.OrderDetails))
                .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.UserAddress))
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));
            
            CreateMap<OrderDetail, OrderDetailsItemResModel>()
                .ForMember(dest => dest.OrderDetailsId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Product != null ? src.Product.Name : src.Device.Name)))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null ? src.Product.MainImage : src.Device.Attachment));

            CreateMap<Transaction, OrderTransactionResModel>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            
            CreateMap<UserAddress, OrderAddressResModel>()
                .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => TextConvert.ConvertFromUnicodeEscape(src.Address)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude));
                
                





        }
    }
}