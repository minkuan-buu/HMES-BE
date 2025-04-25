using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using HMES.Business.Services.OrderServices;
using HMES.Business.Tests.Utilities;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.OrderDetailRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.ProductRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using HMES.Data.Repositories.UserAddressRepositories;
using HMES.Data.Repositories.UserRepositories;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using System.Net.Http;
using System.Threading;
using System.Text;
using System.Reflection;
using Net.payOS;

namespace HMES.Business.Tests.Services.OrderServices;


[TestSubject(typeof(Business.Services.OrderServices.OrderServices))]
public class OrderServicesTest
{
    private readonly Mock<ILogger<Business.Services.OrderServices.OrderServices>> _loggerMock;
    private readonly Mock<IUserRepositories> _userRepositoriesMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IOrderRepositories> _orderRepositoriesMock;
    private readonly Mock<IOrderDetailRepositories> _orderDetailRepositoriesMock;
    private readonly Mock<ITransactionRepositories> _transactionRepositoriesMock;
    private readonly Mock<ICartRepositories> _cartRepositoriesMock;
    private readonly Mock<ICartItemsRepositories> _cartItemsRepositoriesMock;
    private readonly Mock<IUserAddressRepositories> _userAddressRepositoriesMock;
    private readonly Mock<IDeviceRepositories> _deviceRepositoriesMock;
    private readonly Mock<IProductRepositories> _productRepositoriesMock;
    private readonly Mock<IDeviceItemsRepositories> _deviceItemsRepositoriesMock;
    private readonly Mock<IPayOSService> _payOSServiceMock;
    private Guid _testUserId = Guid.NewGuid();
    
    public OrderServicesTest()
    {
        // Set environment variables 
        Environment.SetEnvironmentVariable("GHN_ID_SHOP", "4403980");
        Environment.SetEnvironmentVariable("GHN_TOKEN", "test-token");
        Environment.SetEnvironmentVariable("PAYMENT_RETURN_URL", "https://example.com/return");

        _loggerMock = new Mock<ILogger<Business.Services.OrderServices.OrderServices>>();
        _userRepositoriesMock = new Mock<IUserRepositories>();
        _mapperMock = new Mock<IMapper>();
        _orderRepositoriesMock = new Mock<IOrderRepositories>();
        _orderDetailRepositoriesMock = new Mock<IOrderDetailRepositories>();
        _transactionRepositoriesMock = new Mock<ITransactionRepositories>();
        _cartRepositoriesMock = new Mock<ICartRepositories>();
        _cartItemsRepositoriesMock = new Mock<ICartItemsRepositories>();
        _userAddressRepositoriesMock = new Mock<IUserAddressRepositories>();
        _deviceRepositoriesMock = new Mock<IDeviceRepositories>();
        _productRepositoriesMock = new Mock<IProductRepositories>();
        _deviceItemsRepositoriesMock = new Mock<IDeviceItemsRepositories>();
        _payOSServiceMock = new Mock<IPayOSService>();
        
        // Setup User for token validation
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        
        _userRepositoriesMock.Setup(repo => repo.GetSingle(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<string>()))
        .ReturnsAsync(user);
    }

    private Business.Services.OrderServices.OrderServices CreateOrderServices()
    {
        return new Business.Services.OrderServices.OrderServices(
            _loggerMock.Object,
            _userRepositoriesMock.Object,
            _mapperMock.Object,
            _orderRepositoriesMock.Object,
            _orderDetailRepositoriesMock.Object,
            _transactionRepositoriesMock.Object,
            _cartRepositoriesMock.Object,
            _userAddressRepositoriesMock.Object,
            _deviceRepositoriesMock.Object,
            _productRepositoriesMock.Object,
            _deviceItemsRepositoriesMock.Object,
            _cartItemsRepositoriesMock.Object,
            _payOSServiceMock.Object
        );
    }

    [Fact]
    public async Task GetOrderDetails_ValidId_ReturnsOrderDetails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderEnums.Success.ToString(),
            UserAddressId = Guid.NewGuid(),
            UserId = _testUserId,
            TotalPrice = 200,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Product",
                        Price = 100
                    }
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = "Default"
            }
        };

        var orderDetailsResponse = new OrderDetailsResModel
        {
            OrderId = orderId,
            Status = OrderEnums.Success.ToString(),
            TotalPrice = 250, // 200 + 50 shipping
            ShippingFee = 50,
            OrderDetailsItems = new List<OrderDetailsItemResModel>
            {
                new OrderDetailsItemResModel
                {
                    OrderDetailsId = order.OrderDetails.First().Id,
                    ProductName = "Test Product",
                    Quantity = 2,
                    Price = 100
                }
            },
            UserAddress = new OrderAddressResModel
            {
                AddressId = order.UserAddress.Id,
                Name = order.UserAddress.Name,
                Phone = order.UserAddress.Phone,
                Address = order.UserAddress.Address
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
            It.IsAny<Expression<Func<UserAddress, bool>>>(),
            It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
            It.IsAny<string>()))
        .ReturnsAsync(order.UserAddress);

        _mapperMock.Setup(mapper => mapper.Map<OrderDetailsResModel>(It.IsAny<Order>()))
            .Returns(orderDetailsResponse);
            
        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 50000
                    }
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act
        var result = await service.GetOrderDetails(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(orderId, result.Response.Data.OrderId);
    }

    [Fact]
    public async Task GetOrderDetails_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoriesMock.Setup(repo => repo.GetOrderByIdAsync(orderId)).ReturnsAsync(null as Order);
        
        var service = CreateOrderServices();

        // Act
        var result = await service.GetOrderDetails(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCodes);
    }

    [Fact]
    public async Task GetOrderList_ValidParameters_ReturnsOrderList()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Status = OrderEnums.Success.ToString() },
            new Order { Id = Guid.NewGuid(), Status = OrderEnums.Delivering.ToString() }
        };

        var orderResponseModels = new List<OrderResModel>
        {
            new OrderResModel { Id = orders[0].Id, Status = orders[0].Status },
            new OrderResModel { Id = orders[1].Id, Status = orders[1].Status }
        };

        int totalCount = orders.Count;
        
        _orderRepositoriesMock.Setup(repo => repo.GetAllOrdersAsync(
                It.IsAny<string>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((orders, totalCount));

        _mapperMock.Setup(mapper => mapper.Map<List<OrderResModel>>(It.IsAny<List<Order>>()))
            .Returns(orderResponseModels);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetOrderList(null, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(2, result.Response.Data.Count);
    }

    [Fact]
    public async Task GetSelfOrderList_ValidToken_ReturnsUserOrders()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        // Mock Authentication
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = _testUserId, Status = OrderEnums.Success.ToString() }
        };

        var orderResponseModels = new List<OrderResModel>
        {
            new OrderResModel { Id = orders[0].Id, Status = orders[0].Status }
        };

        int totalCount = orders.Count;
        
        _orderRepositoriesMock.Setup(repo => repo.GetSelfOrdersAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((orders, totalCount));

        _mapperMock.Setup(mapper => mapper.Map<List<OrderResModel>>(It.IsAny<List<Order>>()))
            .Returns(orderResponseModels);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetSelfOrderList(token, null, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Single(result.Response.Data);
    }

    [Fact]
    public async Task CancelOrder_InvalidOrder_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);

        var service = CreateOrderServices();

        // Act
        try
        {
            var result = await service.CancelOrder(orderId, token);
        }
        catch (Exception ex)
        {
            // Assert
            Assert.IsType<CustomException>(ex);
            Assert.Equal("Order not found", ex.Message);
        }
    }

    [Fact]
    public async Task CancelOrder_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Delivering.ToString(),
            ShippingOrderCode = "TEST123456789",
            Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    PaymentMethod = PaymentMethodEnums.COD.ToString(),
                    Status = TransactionEnums.PROCESSING.ToString()
                }
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
            
        _transactionRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
            
        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock CancelShipping response
            var cancelResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {}
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("cancel")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(cancelResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act
        var result = await service.CancelOrder(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
    }

    [Fact]
    public async Task CashOnDeliveryHandle_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward"
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        _transactionRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
            
            // Mock CreateShippingGHN response
            var createShippingResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""order_code"": ""TEST123456789""
                    }
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("create")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createShippingResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act
        var result = await service.CashOnDeliveryHandle(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
    }

    [Fact]
    public async Task CashOnDeliveryHandle_TooManyRequests_ProcessesOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 10,
            MainImage = "product.jpg",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Attachment = "attachment.jpg"
                }
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        _transactionRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
            
            // THIS IS THE KEY PART: Mock "Too many request" error response but with order_code
            var createShippingTooManyRequestsResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 400,
                    ""message"": ""Too many request. This request is processing"",
                    ""data"": {
                        ""order_code"": ""TEST123456789""
                    }
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("create")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createShippingTooManyRequestsResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act
        var result = await service.CashOnDeliveryHandle(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        _orderRepositoriesMock.Verify(repo => repo.Update(It.Is<Order>(o => 
            o.ShippingOrderCode == "TEST123456789" && 
            o.Status == OrderEnums.Delivering.ToString())), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetCODBilling_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Delivering.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Product",
                        MainImage = "product.jpg",
                        ProductAttachments = new List<ProductAttachment>
                        {
                            new ProductAttachment
                            {
                                Id = Guid.NewGuid(),
                                ProductId = Guid.NewGuid(),
                                Attachment = "attachment.jpg"
                            }
                        }
                    }
                },
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    DeviceId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 300,
                    Device = new Device
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Device",
                        Attachment = "device.jpg"
                    }
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetCODBilling(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.NotNull(result.Response.Data);
        Assert.Equal(orderId, result.Response.Data.Id);
        Assert.Equal(200, result.Response.Data.OrderPrice);
        Assert.Equal(50, result.Response.Data.ShippingPrice);
        Assert.Equal(250, result.Response.Data.TotalPrice);
        Assert.Equal(OrderEnums.Delivering.ToString(), result.Response.Data.StatusPayment);
        Assert.NotNull(result.Response.Data.UserAddress);
        Assert.Equal(order.UserAddress.Id, result.Response.Data.UserAddress.Id);
        Assert.Equal(order.UserAddress.Name, result.Response.Data.UserAddress.Name);
        Assert.Equal(order.UserAddress.Phone, result.Response.Data.UserAddress.Phone);
        Assert.Equal(order.UserAddress.Address, result.Response.Data.UserAddress.Address);
        Assert.True(result.Response.Data.UserAddress.IsDefault);
        Assert.NotNull(result.Response.Data.OrderProductItem);
        Assert.Equal(2, result.Response.Data.OrderProductItem.Count);
        
        // Product validation
        var productOrderDetail = order.OrderDetails.First(d => d.ProductId != null);
        var productDetail = result.Response.Data.OrderProductItem.First(d => d.ProductName.Contains("Product"));
        Assert.Equal(productOrderDetail.Id, productDetail.Id);
        Assert.Equal(productOrderDetail.Product.Name, productDetail.ProductName);
        Assert.Equal(productOrderDetail.Product.ProductAttachments.First().Attachment, productDetail.Attachment);
        Assert.Equal(productOrderDetail.Quantity, productDetail.Quantity);
        Assert.Equal(productOrderDetail.UnitPrice, productDetail.UnitPrice);
        
        // Device validation
        var deviceOrderDetail = order.OrderDetails.First(d => d.DeviceId != null);
        var deviceDetail = result.Response.Data.OrderProductItem.First(d => d.Attachment.Contains("device"));
        Assert.Equal(deviceOrderDetail.Id, deviceDetail.Id);
       // Assert.Equal(deviceOrderDetail.Device.Name, deviceDetail.ProductName);
        Assert.Equal(deviceOrderDetail.Device.Attachment, deviceDetail.Attachment);
        Assert.Equal(deviceOrderDetail.Quantity, deviceDetail.Quantity);
        Assert.Equal(deviceOrderDetail.UnitPrice, deviceDetail.UnitPrice);
    }
    
    [Fact]
    public async Task GetCODBilling_OrderNotFound_ThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => service.GetCODBilling(orderId, token));
        Assert.Equal("Order not found", exception.Message);
    }
    
    [Fact]
    public async Task GetCODBilling_OrderNotDelivering_ThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(), // Not in Delivering status
            TotalPrice = 200,
            ShippingFee = 50
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => service.GetCODBilling(orderId, token));
        Assert.Equal("Order is not in pending status", exception.Message);
    }
    
    [Fact]
    public async Task GetCODBilling_NoUserAddress_ReturnsNullAddress()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Delivering.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Product",
                        MainImage = "product.jpg"
                    }
                }
            },
            UserAddress = null // No user address
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetCODBilling(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.NotNull(result.Response.Data);
        Assert.Equal(orderId, result.Response.Data.Id);
        Assert.Null(result.Response.Data.UserAddress);
    }
    
    [Fact]
    public async Task GetCODBilling_ProductNoAttachments_UsesMainImage()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Delivering.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Product",
                        MainImage = "main_image.jpg",
                        ProductAttachments = new List<ProductAttachment>() // Empty attachments
                    }
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Status = "Regular" // Not default
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetCODBilling(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response.Data.OrderProductItem);
        Assert.Single(result.Response.Data.OrderProductItem);
        Assert.Equal("main_image.jpg", result.Response.Data.OrderProductItem[0].Attachment);
        Assert.NotNull(result.Response.Data.UserAddress);
        Assert.False(result.Response.Data.UserAddress.IsDefault);
    }
    
    [Fact]
    public async Task GetCODBilling_ProductNoAttachmentsNoMainImage_ReturnsEmptyString()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Delivering.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Product",
                        MainImage = null, // No main image
                        ProductAttachments = new List<ProductAttachment>() // Empty attachments
                    }
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Status = "Default"
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var service = CreateOrderServices();

        // Act
        var result = await service.GetCODBilling(orderId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response.Data.OrderProductItem);
        Assert.Single(result.Response.Data.OrderProductItem);
        Assert.Equal("", result.Response.Data.OrderProductItem[0].Attachment);
    }

    [Fact]
    public async Task HandleCheckTransaction_TransactionNotFound_ThrowsException()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var paymentLinkInfo = new Net.payOS.Types.PaymentLinkInformation(
            id: "payment_link_123",
            orderCode: 123123123,
            amount: 25000,
            amountPaid: 25000,
            amountRemaining: 0,
            status: TransactionEnums.PAID.ToString(),
            createdAt: DateTime.Now.AddMinutes(-10).ToString("o"),
            transactions: new List<Net.payOS.Types.Transaction>
            {
                new Net.payOS.Types.Transaction(
                    reference: "trans_ref_123",
                    amount: 25000,
                    accountNumber: "123456789",
                    description: "Test payment",
                    transactionDateTime: DateTime.Now.ToString("o"),
                    virtualAccountName: "Virtual Account",
                    virtualAccountNumber: "VA12345",
                    counterAccountBankId: "VCB",
                    counterAccountBankName: "VCB Bank",
                    counterAccountName: "Test User",
                    counterAccountNumber: "9876543210"
                )
            },
            canceledAt: null,
            cancellationReason: null
        );

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Transaction)null);

        _payOSServiceMock.Setup(service => service.GetPaymentLinkInformation(It.IsAny<long>()))
            .ReturnsAsync(paymentLinkInfo);

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.HandleCheckTransaction(paymentLinkId, token));
        Assert.Equal("Transaction not found", exception.Message);
    }
    
    [Fact]
    public async Task HandleCheckTransaction_OrderNotFound_ThrowsException()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentLinkId = paymentLinkId,
            OrderPaymentRefId = 123123123,
            Status = TransactionEnums.PENDING.ToString(),
            Order = null // Order is null
        };

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(transaction);

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.HandleCheckTransaction(paymentLinkId, token));
        Assert.Equal("Order not found", exception.Message);
    }
    
    [Fact]
    public async Task HandleCheckTransaction_PaymentLinkInfoNotFound_ThrowsException()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            MainImage = "product.jpg",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Attachment = "attachment.jpg"
                }
            }
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
                    {
                        Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province", 
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentLinkId = paymentLinkId,
            OrderPaymentRefId = 123123123,
            Status = TransactionEnums.PENDING.ToString(), // PENDING
            Order = order
        };

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(transaction);
            
        // Setup the PayOS service to return null (payment info not found)
        _payOSServiceMock.Setup(service => service.GetPaymentLinkInformation(It.IsAny<long>()))
            .ReturnsAsync((Net.payOS.Types.PaymentLinkInformation)null);

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.HandleCheckTransaction(paymentLinkId, token));
            
        Assert.Equal("Transaction not found in payment system", exception.Message);
    }
    
    [Fact]
    public async Task HandleCheckTransaction_TransactionPaid_UpdatesStatusAndCreatesShipping()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 10,
            MainImage = "product.jpg",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Attachment = "attachment.jpg"
                }
            }
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentLinkId = paymentLinkId,
            OrderPaymentRefId = 123123123,
            Status = TransactionEnums.PENDING.ToString(), // PENDING
            Order = order
        };

        var paymentLinkInfo = new Net.payOS.Types.PaymentLinkInformation(
            id: "payment_link_123",
            orderCode: 123123123,
            amount: 25000,
            amountPaid: 25000,
            amountRemaining: 0,
            status: TransactionEnums.PAID.ToString(), // PAID
            createdAt: DateTime.Now.AddMinutes(-10).ToString("o"),
            transactions: new List<Net.payOS.Types.Transaction>
            {
                new Net.payOS.Types.Transaction(
                    reference: "trans_ref_123",
                    amount: 25000,
                    accountNumber: "123456789",
                    description: "Test payment",
                    transactionDateTime: DateTime.Now.ToString("o"),
                    virtualAccountName: "Virtual Account",
                    virtualAccountNumber: "VA12345",
                    counterAccountBankId: "VCB",
                    counterAccountBankName: "VCB Bank",
                    counterAccountName: "Test User",
                    counterAccountNumber: "9876543210"
                )
            },
            canceledAt: null,
            cancellationReason: null
        );

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(transaction);

        _payOSServiceMock.Setup(service => service.GetPaymentLinkInformation(It.IsAny<long>()))
            .ReturnsAsync(paymentLinkInfo);
            
        _transactionRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
        
        // Prepare order payment response model for verification
        var orderPaymentResModel = new OrderPaymentResModel
        {
            Id = order.Id,
            OrderPrice = order.TotalPrice,
            ShippingPrice = order.ShippingFee ?? 0,
            TotalPrice = order.TotalPrice + (order.ShippingFee ?? 0),
            StatusPayment = TransactionEnums.PAID.ToString(), // Should be updated to PAID
            UserAddress = new OrderUserAddress
            {
                Id = order.UserAddress.Id,
                Name = order.UserAddress.Name,
                Phone = order.UserAddress.Phone,
                Address = order.UserAddress.Address,
                IsDefault = true
            },
            OrderProductItem = new List<OrderDetailResModel>
            {
                new OrderDetailResModel
                {
                    Id = order.OrderDetails.First().Id,
                    ProductName = order.OrderDetails.First().Product.Name,
                    Quantity = order.OrderDetails.First().Quantity,
                    UnitPrice = order.OrderDetails.First().UnitPrice,
                    Attachment = order.OrderDetails.First().Product.ProductAttachments.First().Attachment
                }
            }
        };
            
        // Setup cart for testing cart item deletion
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };
        
        _cartRepositoriesMock.Setup(repo => repo.GetSingle(
            It.IsAny<Expression<Func<Cart, bool>>>(),
            It.IsAny<Func<IQueryable<Cart>, IOrderedQueryable<Cart>>>(),
            It.IsAny<string>()))
        .ReturnsAsync(cart);
        
        _cartItemsRepositoriesMock.Setup(repo => repo.DeleteRange(It.IsAny<List<CartItem>>()))
            .Returns(Task.CompletedTask);
            
        _deviceItemsRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<DeviceItem>()))
            .Returns(Task.CompletedTask);

        // Setup for GHN API response mocking
        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
            
            // Mock CreateShippingGHN response
            var createShippingResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""order_code"": ""TEST123456789""
                    }
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("create")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createShippingResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act
        var result = await service.HandleCheckTransaction(paymentLinkId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify that transaction was updated with correct status
        _transactionRepositoriesMock.Verify(repo => repo.Update(It.Is<Transaction>(t => 
            t.Status == TransactionEnums.PAID.ToString() && 
            t.Order.Status == OrderEnums.Delivering.ToString() &&
            t.TransactionReference == "trans_ref_123")), 
            Times.Once);
            
        // Verify cart items were deleted
        _cartItemsRepositoriesMock.Verify(repo => repo.DeleteRange(It.IsAny<List<CartItem>>()), Times.Once);
    }
    
    [Fact]
    public async Task HandleCheckTransaction_TransactionCancelled_UpdatesStatusAndReturnsItems()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 8,
            MainImage = "product.jpg",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Attachment = "attachment.jpg"
                }
            }
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
                    {
                        Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentLinkId = paymentLinkId,
            OrderPaymentRefId = 123123123,
            Status = TransactionEnums.PENDING.ToString(), // PENDING
            Order = order
        };

        var paymentLinkInfo = new Net.payOS.Types.PaymentLinkInformation(
            id: "payment_link_123",
            orderCode: 123123123,
            amount: 25000,
            amountPaid: 0,
            amountRemaining: 25000,
            status: TransactionEnums.CANCELLED.ToString(), // CANCELLED
            createdAt: DateTime.Now.AddMinutes(-10).ToString("o"),
            transactions: new List<Net.payOS.Types.Transaction>(),
            canceledAt: DateTime.Now.ToString("o"),
            cancellationReason: "Test cancellation"
        );

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(transaction);
            
        _payOSServiceMock.Setup(service => service.GetPaymentLinkInformation(It.IsAny<long>()))
            .ReturnsAsync(paymentLinkInfo);
            
        _transactionRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
            
        _productRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Prepare the response model
        var orderPaymentResModel = new OrderPaymentResModel
        {
            Id = order.Id,
            OrderPrice = order.TotalPrice,
            ShippingPrice = order.ShippingFee ?? 0,
            TotalPrice = order.TotalPrice + (order.ShippingFee ?? 0),
            StatusPayment = TransactionEnums.CANCELLED.ToString(), // Should be updated to CANCELLED
            UserAddress = new OrderUserAddress
            {
                Id = order.UserAddress.Id,
                Name = order.UserAddress.Name,
                Phone = order.UserAddress.Phone,
                Address = order.UserAddress.Address,
                IsDefault = true
            },
            OrderProductItem = new List<OrderDetailResModel>
            {
                new OrderDetailResModel
                {
                    Id = order.OrderDetails.First().Id,
                    ProductName = order.OrderDetails.First().Product.Name,
                    Quantity = order.OrderDetails.First().Quantity,
                    UnitPrice = order.OrderDetails.First().UnitPrice,
                    Attachment = order.OrderDetails.First().Product.ProductAttachments.First().Attachment
                }
            }
        };

        var service = CreateOrderServices();

        // Act
        var result = await service.HandleCheckTransaction(paymentLinkId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify that transaction was updated with correct status
        _transactionRepositoriesMock.Verify(repo => repo.Update(It.Is<Transaction>(t => 
            t.Status == TransactionEnums.CANCELLED.ToString() && 
            t.Order.Status == OrderEnums.Cancelled.ToString())), 
            Times.Once);
            
        // Verify product quantity was returned
        _productRepositoriesMock.Verify(repo => repo.Update(It.Is<Product>(p => 
            p.Id == productId && 
            p.Amount == 10)), // 8 + 2
            Times.Once);
    }
    
    [Fact]
    public async Task HandleCheckTransaction_TransactionNotPending_ReturnsSuccess()
    {
        // Arrange
        var paymentLinkId = "paylink_123";
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
                        Name = "Test Product",
                        MainImage = "product.jpg",
                        ProductAttachments = new List<ProductAttachment>
                        {
                            new ProductAttachment
                            {
                                Id = Guid.NewGuid(),
                    ProductId = productId,
                                Attachment = "attachment.jpg"
                            }
                        }
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Status = OrderEnums.Success.ToString(),
            TotalPrice = 200,
            ShippingFee = 50,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Status = UserAddressEnums.Default.ToString()
            }
        };
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentLinkId = paymentLinkId,
            OrderPaymentRefId = 123123123,
            Status = TransactionEnums.PAID.ToString(), // Not PENDING - this is key
            Order = order
        };

        _transactionRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(transaction);

        // Create a response model that the mapper will return
        var orderPaymentResModel = new OrderPaymentResModel
        {
            Id = order.Id,
            OrderPrice = order.TotalPrice,
            ShippingPrice = order.ShippingFee ?? 0,
            TotalPrice = order.TotalPrice + (order.ShippingFee ?? 0),
            StatusPayment = transaction.Status,
            UserAddress = new OrderUserAddress
            {
                Id = order.UserAddress.Id,
                Name = order.UserAddress.Name,
                Phone = order.UserAddress.Phone,
                Address = order.UserAddress.Address,
                IsDefault = true
            },
            OrderProductItem = new List<OrderDetailResModel>
            {
                new OrderDetailResModel
                {
                    Id = order.OrderDetails.First().Id,
                    ProductName = order.OrderDetails.First().Product.Name,
                    Quantity = order.OrderDetails.First().Quantity,
                    UnitPrice = order.OrderDetails.First().UnitPrice,
                    Attachment = order.OrderDetails.First().Product.ProductAttachments.First().Attachment
                }
            }
        };

        var service = CreateOrderServices();

        // Act
        var result = await service.HandleCheckTransaction(paymentLinkId, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify that PayOS was not called since transaction is not pending
        _payOSServiceMock.Verify(s => s.GetPaymentLinkInformation(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task CashOnDeliveryHandle_GHNApiError_ThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            Role = "Customer"
        };
        var token = Authentication.GenerateJWT(user);
        
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 10,
            MainImage = "product.jpg",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Attachment = "attachment.jpg"
                }
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 100,
                    Product = product
                }
            },
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward",
                Status = UserAddressEnums.Default.ToString()
            }
        };

        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        _transactionRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        var service = CreateOrderServices();
        
        // Get the _httpClient field using reflection
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (httpClientField != null)
        {
            // Create a mock HttpMessageHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
            
            // THIS IS THE KEY PART: Create a regular error response (not "Too many requests")
            var createShippingErrorResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 400,
                    ""message"": ""Invalid delivery information"",
                    ""data"": null
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("create")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createShippingErrorResponse);
                
            // Create a new HttpClient with the mocked handler
            var client = new HttpClient(mockHandler.Object);
            
            // Set the mock client in the service
            httpClientField.SetValue(service, client);
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.CashOnDeliveryHandle(orderId, token));
            
        Assert.Contains("Không thể tạo đơn hàng trên GHN", exception.Message);
        Assert.Contains("Invalid delivery information", exception.Message);
    }

    [Fact]
    public async Task CreateOrder_CreateNewOrder_ReturnsOrderId()
    {
        // Arrange
        var userId = _testUserId;
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var productId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = productId,
                    UnitPrice = 100,
                    Quantity = 2
                }
            },
            Devices = new List<OrderDeviceReqModel>
            {
                new OrderDeviceReqModel
                {
                    Id = deviceId,
                    UnitPrice = 150,
                    Quantity = 1
                }
            }
        };
        
        // Setup user address
        var userAddress = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test User",
            Phone = "1234567890",
            Address = "123 Test St",
            Status = "Default"
        };
        
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<UserAddress, bool>>>(),
                It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(userAddress);
        
        // No existing order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);
        
        _orderRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();

        // Act
        var result = await service.CreateOrder(createOrderRequest, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.NotEqual(Guid.Empty, result.Response.Data);
        
        // Verify order was created with correct data
        _orderRepositoriesMock.Verify(repo => repo.Insert(It.Is<Order>(o => 
            o.UserId == userId && 
            o.UserAddressId == userAddress.Id && 
            o.Status == OrderEnums.Pending.ToString() && 
            o.TotalPrice == 350 &&  // 2 x 100 + 1 x 150
            o.OrderDetails.Count == 2 && 
            o.OrderDetails.Any(d => d.ProductId == productId && d.Quantity == 2 && d.UnitPrice == 100) &&
            o.OrderDetails.Any(d => d.DeviceId == deviceId && d.Quantity == 1 && d.UnitPrice == 150))),
            Times.Once);
    }
    
    [Fact]
    public async Task CreateOrder_UpdateExistingOrder_ReturnsOrderId()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var productId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = productId,
                    UnitPrice = 100,
                    Quantity = 2
                }
            },
            Devices = new List<OrderDeviceReqModel>
            {
                new OrderDeviceReqModel
                {
                    Id = deviceId,
                    UnitPrice = 150,
                    Quantity = 1
                }
            }
        };
        
        // Setup existing order with existing order details
        var existingOrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 50
            }
        };
        
        var existingOrder = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            TotalPrice = 50,
            CreatedAt = DateTime.Now.AddDays(-1),
            OrderDetails = existingOrderDetails
        };
        
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(existingOrder);
        
        _orderDetailRepositoriesMock.Setup(repo => repo.DeleteRange(It.IsAny<List<OrderDetail>>()))
            .Returns(Task.CompletedTask);
            
        _orderRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();

        // Act
        var result = await service.CreateOrder(createOrderRequest, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(orderId, result.Response.Data);
        
        
        // Verify order was updated with correct data
        _orderRepositoriesMock.Verify(repo => repo.Update(It.Is<Order>(o => 
            o.Id == orderId &&
            o.UserId == userId && 
            o.Status == OrderEnums.Pending.ToString() && 
            o.TotalPrice == 350 &&  // 2 x 100 + 1 x 150
            o.OrderDetails.Count == 2 && 
            o.OrderDetails.Any(d => d.ProductId == productId && d.Quantity == 2 && d.UnitPrice == 100) &&
            o.OrderDetails.Any(d => d.DeviceId == deviceId && d.Quantity == 1 && d.UnitPrice == 150))),
            Times.Once);
    }
    
    [Fact]
    public async Task CreateOrder_NoUserAddress_CreatesOrderWithNullAddress()
    {
        // Arrange
        var userId = _testUserId;
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var productId = Guid.NewGuid();
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = productId,
                    UnitPrice = 100,
                    Quantity = 2
                }
            },
            Devices = new List<OrderDeviceReqModel>()
        };
        
        // Setup no user address found
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<UserAddress, bool>>>(),
                It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((UserAddress)null);
        
        // No existing order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);
        
        _orderRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();

        // Act
        var result = await service.CreateOrder(createOrderRequest, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify order was created with null UserAddressId
        _orderRepositoriesMock.Verify(repo => repo.Insert(It.Is<Order>(o => 
            o.UserId == userId && 
            o.UserAddressId == null && 
            o.TotalPrice == 200)),  // 2 x 100
            Times.Once);
    }
    
    [Fact]
    public async Task CreateOrder_EmptyProducts_CreatesOrderWithOnlyDevices()
    {
        // Arrange
        var userId = _testUserId;
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var deviceId = Guid.NewGuid();
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>(),
            Devices = new List<OrderDeviceReqModel>
            {
                new OrderDeviceReqModel
                {
                    Id = deviceId,
                    UnitPrice = 150,
                    Quantity = 1
                }
            }
        };
        
        // Setup user address
        var userAddress = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test User",
            Phone = "1234567890",
            Address = "123 Test St",
            Status = "Default"
        };
        
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<UserAddress, bool>>>(),
                It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(userAddress);
        
        // No existing order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);
        
        _orderRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();

        // Act
        var result = await service.CreateOrder(createOrderRequest, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify order was created with correct data
        _orderRepositoriesMock.Verify(repo => repo.Insert(It.Is<Order>(o => 
            o.UserId == userId && 
            o.UserAddressId == userAddress.Id && 
            o.TotalPrice == 150 &&  // 1 x 150
            o.OrderDetails.Count == 1 && 
            o.OrderDetails.Any(d => d.DeviceId == deviceId))),
            Times.Once);
    }
    
    [Fact]
    public async Task CreateOrder_EmptyDevices_CreatesOrderWithOnlyProducts()
    {
        // Arrange
        var userId = _testUserId;
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var productId = Guid.NewGuid();
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = productId,
                    UnitPrice = 100,
                    Quantity = 2
                }
            },
            Devices = new List<OrderDeviceReqModel>()
        };
        
        // Setup user address
        var userAddress = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test User",
            Phone = "1234567890",
            Address = "123 Test St",
            Status = "Default"
        };
        
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<UserAddress, bool>>>(),
                It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(userAddress);
        
        // No existing order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((Order)null);
        
        _orderRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = CreateOrderServices();

        // Act
        var result = await service.CreateOrder(createOrderRequest, token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        
        // Verify order was created with correct data
        _orderRepositoriesMock.Verify(repo => repo.Insert(It.Is<Order>(o => 
            o.UserId == userId && 
            o.UserAddressId == userAddress.Id && 
            o.TotalPrice == 200 &&  // 2 x 100
            o.OrderDetails.Count == 1 && 
            o.OrderDetails.Any(d => d.ProductId == productId))),
            Times.Once);
    }
    
    [Fact]
    public async Task CreateOrder_TokenInvalid_ThrowsException()
    {
        // Arrange
        var invalidToken = "invalid_token";
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = Guid.NewGuid(),
                    UnitPrice = 100,
                    Quantity = 2
                }
            }
        };

        var service = CreateOrderServices();

        // Act & Assert
        await Assert.ThrowsAsync<CustomException>(() => 
            service.CreateOrder(createOrderRequest, invalidToken));
    }
    
    [Fact]
    public async Task CreateOrder_RepositoryThrowsException_ThrowsCustomException()
    {
        // Arrange
        var userId = _testUserId;
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        var createOrderRequest = new CreateOrderDetailReqModel
        {
            Products = new List<OrderProductReqModel>
            {
                new OrderProductReqModel
                {
                    Id = Guid.NewGuid(),
                    UnitPrice = 100,
                    Quantity = 2
                }
            }
        };
        
        // Setup repository to throw exception
        _userAddressRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<UserAddress, bool>>>(),
                It.IsAny<Func<IQueryable<UserAddress>, IOrderedQueryable<UserAddress>>>(),
                It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection error"));

        var service = CreateOrderServices();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.CreateOrder(createOrderRequest, token));
            
        Assert.Equal("Database connection error", exception.Message);
    }

    [Fact]
    public async Task CreatePaymentUrl_ValidOrder_ReturnsCheckoutUrl()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var paymentLinkId = "test_payment_link_id";
        var checkoutUrl = "https://pay.payos.vn/checkout/test_checkout_url";
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        // Create test order with products and devices
        var productId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 10 // Enough quantity
        };
        
        var device = new Device
        {
            Id = deviceId,
            Name = "Test Device",
            Quantity = 5 // Enough quantity
        };
        
        var orderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 100,
                Product = product
            },
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                DeviceId = deviceId,
                Quantity = 1,
                UnitPrice = 150,
                Device = device
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            TotalPrice = 350, // 2 * 100 + 1 * 150
            OrderDetails = orderDetails,
            Transactions = new List<Transaction>(), // No pending transactions
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Test User",
                Phone = "1234567890",
                Address = "123 Test St",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward"
            },
            UserAddressId = Guid.NewGuid() // UserAddressId not null
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);
            
        // Setup product repository to return the test products
        // Make a new product with plenty of quantity to avoid quantity issues
        var productWithEnoughQuantity = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 100 
        };
        var deviceWithEnoughQuantity = new Device
        {
            Id = deviceId,
            Name = "Test Device",
            Quantity = 50 
        };
        
        _productRepositoriesMock.Setup(repo => repo.GetList(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            ))
            .ReturnsAsync(new List<Product> { productWithEnoughQuantity }.AsEnumerable());
        
        
        
        _productRepositoriesMock.Setup(repo => repo.GetListInRange(
                It.IsAny<List<Guid?>>()))
            .ReturnsAsync([productWithEnoughQuantity]);
        
        _deviceRepositoriesMock.Setup(repo => repo.GetListInRange(
            It.IsAny<List<Guid?>>()
            )).ReturnsAsync([deviceWithEnoughQuantity]);

            
        // Setup device repository to return the test devices
        _deviceRepositoriesMock.Setup(repo => repo.GetList(
                It.IsAny<Expression<Func<Device, bool>>>(),
                It.IsAny<Func<IQueryable<Device>, IOrderedQueryable<Device>>>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            ))
            .ReturnsAsync(new List<Device> { device }.AsEnumerable());
            
        // Mock CalculateShippingFee
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var service = CreateOrderServices();
        if (httpClientField != null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
            
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
            
            var client = new HttpClient(mockHandler.Object);
            httpClientField.SetValue(service, client);
        }
            
        // Mock the payment service
        var paymentResult = new Net.payOS.Types.CreatePaymentResult(
            bin: null, 
            accountNumber: null, 
            amount: 350 + 30000, // Order total + shipping fee
            description: "", 
            orderCode: int.Parse(GenerateTestRandomRefId()), 
            currency: "VND", 
            paymentLinkId: paymentLinkId, 
            status: "PENDING", 
            expiredAt: DateTimeOffset.Now.AddMinutes(15).ToUnixTimeSeconds(), 
            checkoutUrl: checkoutUrl, 
            qrCode: "data:image/png;base64,testQrCode"
        );
        
        _payOSServiceMock.Setup(service => service.CreatePaymentLink(It.IsAny<Net.payOS.Types.PaymentData>()))
            .ReturnsAsync(paymentResult);
            
        // Helper method to generate a random order ref ID for testing
        string GenerateTestRandomRefId()
        {
            return new Random().Next(10000000, 99999999).ToString();
        }
        
        // Setup transaction repository
        _transactionRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
            
        // Setup product update
        _productRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);
            
        // Setup device update
        _deviceRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Device>()))
            .Returns(Task.CompletedTask);
            
        // Setup environment variable
        Environment.SetEnvironmentVariable("PAYMENT_RETURN_URL", "https://example.com/return");
        
        // Act
        var result = await service.CreatePaymentUrl(token, orderId);
        
        // Assert
        Assert.Equal(checkoutUrl, result);
        
        // Verify transaction was inserted
        _transactionRepositoriesMock.Verify(repo => repo.Insert(It.Is<Transaction>(t => 
            t.OrderId == orderId && 
            t.Status == TransactionEnums.PENDING.ToString() &&
            t.PaymentLinkId == paymentLinkId &&
            t.PaymentMethod == PaymentMethodEnums.BANK.ToString())), 
            Times.Once);
            
        // Verify product quantity was decreased
        _productRepositoriesMock.Verify(repo => repo.Update(It.Is<Product>(p => 
            p.Id == productId && 
            p.Amount == 98)), // Original 10 - 2
            Times.Once);
            
        // Verify device quantity was decreased
        _deviceRepositoriesMock.Verify(repo => repo.Update(It.Is<Device>(d => 
            d.Id == deviceId && 
            d.Quantity == 49)), // Original 5 - 1
            Times.Once);
    }
    
    [Fact]
    public async Task CreatePaymentUrl_ExistingPendingTransaction_ReturnsExistingUrl()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var existingPaymentLinkId = "existing_payment_link_id";
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        // Create test order with a pending transaction
        var pendingTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = TransactionEnums.PENDING.ToString(),
            PaymentLinkId = existingPaymentLinkId
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            Transactions = new List<Transaction> { pendingTransaction },
            UserAddressId = Guid.NewGuid(), // UserAddressId not null
            UserAddress = new UserAddress { Id = Guid.NewGuid() }
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);
        
        var service = CreateOrderServices();
        
        // Act
        var result = await service.CreatePaymentUrl(token, orderId);
        
        // Assert
        Assert.Equal($"https://pay.payos.vn/web/{existingPaymentLinkId}", result);
        
        // Verify no new transaction is created
        _transactionRepositoriesMock.Verify(repo => repo.Insert(It.IsAny<Transaction>()), Times.Never);
    }
    
    [Fact]
    public async Task CreatePaymentUrl_NullUserAddressId_ThrowsException()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        // Create test order with no UserAddressId
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            Transactions = new List<Transaction>(),
            UserAddressId = null // No UserAddressId
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);
        
        var service = CreateOrderServices();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.CreatePaymentUrl(token, orderId));
            
        Assert.Equal("Người dùng chưa có địa chỉ cho đơn hàng.", exception.Message);
    }
    
    [Fact]
    public async Task CreatePaymentUrl_ProductQuantityInsufficient_ThrowsException()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        // Create test order with product that has insufficient quantity
        var productId = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 1 // Not enough quantity (order needs 2)
        };
        
        var orderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                Quantity = 2, // Trying to order 2
                UnitPrice = 100,
                Product = product
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = orderDetails,
            Transactions = new List<Transaction>(),
            UserAddressId = Guid.NewGuid(),
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Test User",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward"
            }
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);
        
        
        _productRepositoriesMock.Setup(repo => repo.GetListInRange(
                It.IsAny<List<Guid?>>()))
            .ReturnsAsync([product]);

        
        
        
        // Mock HTTP client for district/service lookup
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var service = CreateOrderServices();
        if (httpClientField != null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
                
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            var client = new HttpClient(mockHandler.Object);
            httpClientField.SetValue(service, client);
        }
        
        
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.CreatePaymentUrl(token, orderId));
            
        Assert.Contains($"Sản phẩm {productId} không đủ số lượng để thanh toán", exception.Message);
    }
    
    [Fact]
    public async Task CreatePaymentUrl_DeviceQuantityInsufficient_ThrowsException()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
     
        var deviceId = Guid.NewGuid();
        
        var device = new Device
        {
            Id = deviceId,
            Name = "Test Device",
            Quantity = 1 
        };
        
        var orderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                DeviceId = deviceId,
                Quantity = 2, 
                UnitPrice = 100,
                Device = device
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = orderDetails,
            Transactions = new List<Transaction>(),
            UserAddressId = Guid.NewGuid(),
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Test User",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward"
            }
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);

        var deviceWithEnoughQuantity = new Device
        {
            Id = deviceId,
            Name = "Test Device",
            Quantity = 1 
        };
        
        _deviceRepositoriesMock.Setup(repo => repo.GetListInRange(
            It.IsAny<List<Guid?>>()
        )).ReturnsAsync([deviceWithEnoughQuantity]);
            
        // Set up HTTP client for district and service lookup
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        var service = CreateOrderServices();
        if (httpClientField != null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
                
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            var client = new HttpClient(mockHandler.Object);
            httpClientField.SetValue(service, client);
        }
        
        
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
            service.CreatePaymentUrl(token, orderId));
            
        Assert.Contains($"Thiết bị {deviceId} không đủ số lượng để thanh toán", exception.Message);
    }
    
    [Fact]
    public async Task CreatePaymentUrl_MissingReturnUrl_ThrowsException()
    {
        // Arrange
        var userId = _testUserId;
        var orderId = Guid.NewGuid();
        var token = Authentication.GenerateJWT(new User { Id = userId, Email = "test@example.com", Role = "Customer" });
        
        // Create test order with products
        var productId = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Amount = 10000 // Extremely high quantity
        };

        IEnumerable<Product> products = [product];
    
        
        var orderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 100,
                Product = product
            }
        };
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderEnums.Pending.ToString(),
            OrderDetails = orderDetails,
            Transactions = new List<Transaction>(),
            UserAddressId = Guid.NewGuid(),
            UserAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Test User",
                Province = "Test Province",
                District = "Test District",
                Ward = "Test Ward"
            }
        };
        
        // Setup order repository to return the test order
        _orderRepositoriesMock.Setup(repo => repo.GetSingle(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string>()))
            .ReturnsAsync(order);
            
        // Setup product repository to return the test products
        var productsWithEnoughQuantity = products.Select(p => {
            p.Amount = p.Amount * 10; // Make sure there's plenty of quantity
            return p;
        }).ToList();
        
        // Match the specific lambda expression that the service is using
        _productRepositoriesMock.Setup(repo => repo.GetList(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()
        )).ReturnsAsync(productsWithEnoughQuantity.AsEnumerable());
        
        _productRepositoriesMock.Setup(repo => repo.GetListInRange(
                It.IsAny<List<Guid?>>()))
            .ReturnsAsync(productsWithEnoughQuantity);

      
            
        // Mock HTTP client for district and service lookup
        var httpClientField = typeof(Business.Services.OrderServices.OrderServices)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var service = CreateOrderServices();
        if (httpClientField != null)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            
            // Mock GetDistrictId response
            var districtResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""DistrictID"": 1462,
                            ""DistrictName"": ""Test District""
                        }
                    ]
                }")
            };
            
            // Mock GetWardId response
            var wardResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""WardCode"": 12345,
                            ""WardName"": ""Test Ward""
                        }
                    ]
                }")
            };
            
            // Mock GetService response
            var serviceResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": [
                        {
                            ""service_type_id"": 2
                        }
                    ]
                }")
            };
            
            // Mock CalculateShippingFee response
            var shippingFeeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""code"": 200,
                    ""message"": ""Success"",
                    ""data"": {
                        ""total_fee"": 30000
                    }
                }")
            };
                
            // Setup the request/response mapping
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("district")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(districtResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("ward")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(wardResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("available-services")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(serviceResponse);
                
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("preview")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(shippingFeeResponse);
                
            var client = new HttpClient(mockHandler.Object);
            httpClientField.SetValue(service, client);
        }
        
        // Remove the environment variable for PAYMENT_RETURN_URL
        Environment.SetEnvironmentVariable("PAYMENT_RETURN_URL", null);
        
       
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            service.CreatePaymentUrl(token, orderId));
            
        Assert.Equal("PAYMENT_RETURN_URL is missing", exception.Message);
    }
}
