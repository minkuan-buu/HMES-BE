using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using HMES.Business.Services.CloudServices;
using HMES.Business.Services.ProductServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CategoryRepositories;
using HMES.Data.Repositories.ProductRepositories;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HMES.Business.Tests.Services.ProductServices;

[TestSubject(typeof(Business.Services.ProductServices.ProductServices))]
public class ProductServicesTest
{
    private readonly Mock<IProductRepositories> _productRepositoriesMock;
    private readonly Mock<ICategoryRepositories> _categoryRepositoriesMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICloudServices> _cloudServicesMock;

    public ProductServicesTest()
    {
        _productRepositoriesMock = new Mock<IProductRepositories>();
        _categoryRepositoriesMock = new Mock<ICategoryRepositories>();
        _mapperMock = new Mock<IMapper>();
        _cloudServicesMock = new Mock<ICloudServices>();
    }

    private Business.Services.ProductServices.ProductServices CreateProductServices()
    {
        return new Business.Services.ProductServices.ProductServices(
            _productRepositoriesMock.Object,
            _categoryRepositoriesMock.Object,
            _mapperMock.Object,
            _cloudServicesMock.Object
        );
    }

    [Fact]
    public async Task GetAllProducts_ValidRequest_ReturnsProductList()
    {
        // Arrange
        int pageIndex = 1;
        int pageSize = 10;
        ProductStatusEnums? status = ProductStatusEnums.Active;

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Status = "Active", Price = 100 },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Status = "Active", Price = 200 }
        };

        var productDtos = new List<ProductBriefResponseDto>
        {
            new ProductBriefResponseDto { Id = products[0].Id, Name = products[0].Name, Status = products[0].Status, Price = products[0].Price },
            new ProductBriefResponseDto { Id = products[1].Id, Name = products[1].Name, Status = products[1].Status, Price = products[1].Price }
        };

        int totalItems = products.Count;

        _productRepositoriesMock.Setup(repo => repo.GetListWithPagination(pageIndex, pageSize, status))
            .ReturnsAsync((products, totalItems));

        _mapperMock.Setup(mapper => mapper.Map<List<ProductBriefResponseDto>>(It.IsAny<List<Product>>()))
            .Returns(productDtos);

        var service = CreateProductServices();

        // Act
        var result = await service.GetAllProducts(pageIndex, pageSize, status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(2, result.Response.Data.Count);
        Assert.Equal(products[0].Id, result.Response.Data[0].Id);
        Assert.Equal(products[1].Id, result.Response.Data[1].Id);
        Assert.Equal(1, result.Response.CurrentPage);
        Assert.Equal(1, result.Response.TotalPages);
        Assert.Equal(2, result.Response.TotalItems);
        Assert.Equal(10, result.Response.PageSize);
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product 
        { 
            Id = productId, 
            Name = "Test Product", 
            Description = "Test Description",
            Price = 100, 
            Status = "Active",
            CategoryId = Guid.NewGuid(),
            ProductAttachments = new List<ProductAttachment>()
        };

        var productDto = new ProductResponseDto 
        { 
            Id = productId, 
            Name = "Test Product", 
            Description = "Test Description",
            Price = 100, 
            Status = "Active",
            CategoryId = product.CategoryId,
            Images = new List<string>()
        };

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _mapperMock.Setup(mapper => mapper.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns(productDto);

        var service = CreateProductServices();

        // Act
        var result = await service.GetProductById(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(productId, result.Response.Data.Id);
        Assert.Equal(product.Name, result.Response.Data.Name);
    }

    [Fact]
    public async Task GetProductById_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        var service = CreateProductServices();

        // Act
        var result = await service.GetProductById(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCodes);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task SearchProducts_ValidParams_ReturnsFilteredProducts()
    {
        // Arrange
        string keyword = "test";
        Guid? categoryId = Guid.NewGuid();
        int? minAmount = 1;
        int? maxAmount = 10;
        decimal? minPrice = 50;
        decimal? maxPrice = 200;
        ProductStatusEnums? status = ProductStatusEnums.Active;
        DateTime? createdAfter = DateTime.Now.AddDays(-30);
        DateTime? createdBefore = DateTime.Now;
        int pageIndex = 1;
        int pageSize = 10;

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Test Product 1", Status = "Active", Price = 100 },
            new Product { Id = Guid.NewGuid(), Name = "Test Product 2", Status = "Active", Price = 150 }
        };

        var productDtos = new List<ProductBriefResponseDto>
        {
            new ProductBriefResponseDto { Id = products[0].Id, Name = products[0].Name, Status = products[0].Status, Price = products[0].Price },
            new ProductBriefResponseDto { Id = products[1].Id, Name = products[1].Name, Status = products[1].Status, Price = products[1].Price }
        };

        int totalItems = products.Count;

        _productRepositoriesMock.Setup(repo => repo.GetProductsWithPagination(
            It.IsAny<string>(), categoryId, minAmount, maxAmount, minPrice, maxPrice, 
            status, createdAfter, createdBefore, pageIndex, pageSize))
            .ReturnsAsync((products, totalItems));

        _mapperMock.Setup(mapper => mapper.Map<List<ProductBriefResponseDto>>(It.IsAny<List<Product>>()))
            .Returns(productDtos);

        var service = CreateProductServices();

        // Act
        var result = await service.SearchProducts(keyword, categoryId, minAmount, maxAmount, 
            minPrice, maxPrice, status, createdAfter, createdBefore, pageIndex, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(2, result.Response.Data.Count);
        Assert.Equal(1, result.Response.CurrentPage);
        Assert.Equal(1, result.Response.TotalPages);
        Assert.Equal(2, result.Response.TotalItems);
    }

    [Fact]
    public async Task AddProduct_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var mainImageMock = new Mock<IFormFile>();
        mainImageMock.Setup(f => f.FileName).Returns("main.jpg");
        
        var image1Mock = new Mock<IFormFile>();
        image1Mock.Setup(f => f.FileName).Returns("image1.jpg");
        
        var image2Mock = new Mock<IFormFile>();
        image2Mock.Setup(f => f.FileName).Returns("image2.jpg");

        var productCreateDto = new ProductCreateDto
        {
            Name = "New Product",
            Description = "New Description",
            CategoryId = categoryId,
            Price = 150,
            Amount = 5,
            Status = ProductStatusEnums.Active,
            MainImage = mainImageMock.Object,
            Images = new List<IFormFile> { image1Mock.Object, image2Mock.Object }
        };

        var product = new Product
        {
            Id = productId,
            Name = productCreateDto.Name,
            Description = productCreateDto.Description,
            CategoryId = productCreateDto.CategoryId,
            Price = productCreateDto.Price,
            Amount = productCreateDto.Amount,
            Status = productCreateDto.Status.ToString(),
            ProductAttachments = new List<ProductAttachment>()
        };

        var productResponseDto = new ProductResponseDto
        {
            Id = productId,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Price = product.Price,
            Amount = product.Amount,
            Status = product.Status,
            Images = new List<string> { "url1", "url2" }
        };

        _categoryRepositoriesMock.Setup(repo => repo.IsSecondLevelCategory(categoryId))
            .ReturnsAsync(true);

        _mapperMock.Setup(mapper => mapper.Map<Product>(It.IsAny<ProductCreateDto>()))
            .Returns(product);

        _cloudServicesMock.Setup(service => service.UploadSingleFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("main-image-url");

        _cloudServicesMock.Setup(service => service.UploadFile(It.IsAny<List<IFormFile>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<string> { "url1", "url2" });

        _productRepositoriesMock.Setup(repo => repo.Insert(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(mapper => mapper.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns(productResponseDto);

        var service = CreateProductServices();

        // Act
        var result = await service.AddProduct(productCreateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(productId, result.Response.Data.Id);
        Assert.Equal(product.Name, result.Response.Data.Name);
        Assert.Equal(2, result.Response.Data.Images.Count);
    }

    [Fact]
    public async Task UpdateProduct_ExistingProduct_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var mainImageMock = new Mock<IFormFile>();
        mainImageMock.Setup(f => f.FileName).Returns("main-updated.jpg");
        
        var image1Mock = new Mock<IFormFile>();
        image1Mock.Setup(f => f.FileName).Returns("image1-updated.jpg");

        var productUpdateDto = new ProductUpdateDto
        {
            Id = productId,
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryId = categoryId,
            Price = 200,
            Amount = 10,
            Status = ProductStatusEnums.Active,
            MainImage = mainImageMock.Object,
            OldImages = new List<string> { "existing-url1" },
            NewImages = new List<IFormFile> { image1Mock.Object }
        };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Product",
            Description = "Original Description",
            CategoryId = categoryId,
            Price = 150,
            Amount = 5,
            Status = "Active",
            MainImage = "original-main-image",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment { Id = Guid.NewGuid(), ProductId = productId, Attachment = "existing-url1" },
                new ProductAttachment { Id = Guid.NewGuid(), ProductId = productId, Attachment = "existing-url2" }
            }
        };

        var updatedProduct = new Product
        {
            Id = productId,
            Name = productUpdateDto.Name,
            Description = productUpdateDto.Description,
            CategoryId = productUpdateDto.CategoryId,
            Price = productUpdateDto.Price,
            Amount = productUpdateDto.Amount,
            Status = productUpdateDto.Status.ToString(),
            MainImage = "updated-main-image",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment { Id = Guid.NewGuid(), ProductId = productId, Attachment = "existing-url1" },
                new ProductAttachment { Id = Guid.NewGuid(), ProductId = productId, Attachment = "new-url1" }
            }
        };

        var productResponseDto = new ProductResponseDto
        {
            Id = productId,
            Name = updatedProduct.Name,
            Description = updatedProduct.Description,
            CategoryId = updatedProduct.CategoryId,
            Price = updatedProduct.Price,
            Amount = updatedProduct.Amount,
            Status = updatedProduct.Status,
            MainImage = updatedProduct.MainImage,
            Images = new List<string> { "existing-url1", "new-url1" }
        };

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _cloudServicesMock.Setup(service => service.UploadSingleFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("updated-main-image");

        _cloudServicesMock.Setup(service => service.UploadFile(It.IsAny<List<IFormFile>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<string> { "new-url1" });

        _productRepositoriesMock.Setup(repo => repo.Update(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(mapper => mapper.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns(productResponseDto);

        var service = CreateProductServices();

        // Act
        var result = await service.UpdateProduct(productUpdateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal(productId, result.Response.Data.Id);
        Assert.Equal(updatedProduct.Name, result.Response.Data.Name);
        Assert.Equal(2, result.Response.Data.Images.Count);
    }

    [Fact]
    public async Task UpdateProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        var productUpdateDto = new ProductUpdateDto
        {
            Id = productId,
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryId = Guid.NewGuid(),
            Price = 200,
            Amount = 10,
            Status = ProductStatusEnums.Active
        };

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        var service = CreateProductServices();

        // Act
        var result = await service.UpdateProduct(productUpdateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCodes);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        var product = new Product
        {
            Id = productId,
            Name = "Product to Delete",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            Price = 150,
            Amount = 5,
            Status = "Active",
            MainImage = "main-image-url",
            ProductAttachments = new List<ProductAttachment>
            {
                new ProductAttachment { Id = Guid.NewGuid(), ProductId = productId, Attachment = "url1" }
            }
        };

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _cloudServicesMock.Setup(service => service.DeleteFilesInPathAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _productRepositoriesMock.Setup(repo => repo.Delete(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        var service = CreateProductServices();

        // Act
        var result = await service.DeleteProduct(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal("Product deleted successfully", result.Response.Message);
    }

    [Fact]
    public async Task DeleteProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productRepositoriesMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        var service = CreateProductServices();

        // Act
        var result = await service.DeleteProduct(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCodes);
        Assert.NotNull(result.Response);
        Assert.Equal("Product not found", result.Response.Message);
    }
} 