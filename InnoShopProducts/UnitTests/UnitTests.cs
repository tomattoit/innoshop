using Domain.Entities;
using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.Get;
using Application.Products.Update;
using Application.Data;
using Moq;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Products.GetAll;
using Application.Products.GetWithUser;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Application.Products.Search;

namespace UnitTests
{
    public class ProductHandlersTests
    {
        private IApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var mockPublisher = new Mock<IPublisher>();
            var context = new ApplicationDbContext(options, mockPublisher.Object);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task Handle_ShouldCreateProduct()
        {
            var productName = "Test Product";
            var productDescription = "This is a test product.";
            var productPrice = 100.0m;
            var productQuantity = 10;
            var userId = Guid.NewGuid();

            var mockProductRepository = new Mock<IProductRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var handler = new CreateProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);

            await handler.Handle(new CreateProductCommand(productName, productDescription, productPrice, productQuantity, userId), CancellationToken.None);

            mockProductRepository.Verify(repo => repo.Add(It.Is<Product>(p =>
                p.Name == productName &&
                p.Description == productDescription &&
                p.Price == productPrice &&
                p.Quantity == productQuantity &&
                p.UserId == userId)), Times.Once);

            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRemoveProduct_WhenProductExistsAndUserIsAuthorized()
        {
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var product = new Product(productId, "Test Product", "This is a test product.", 100.0m, 10, userId);
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            var handler = new DeleteProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);
            await handler.Handle(new DeleteProductCommand(productId, userId), CancellationToken.None);

            mockProductRepository.Verify(repo => repo.Remove(product), Times.Once);
            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist_Delete()
        {
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            var handler = new DeleteProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);

            await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(new DeleteProductCommand(productId, userId), CancellationToken.None));

            mockProductRepository.Verify(repo => repo.Remove(It.IsAny<Product>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorizedToDelete()
        {
            var productId = Guid.NewGuid();
            var productUserId = Guid.NewGuid();
            var requestUserId = Guid.NewGuid();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var product = new Product(productId, "Test Product", "This is a test product.", 100.0m, 10, productUserId);
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            var handler = new DeleteProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new DeleteProductCommand(productId, requestUserId), CancellationToken.None));

            mockProductRepository.Verify(repo => repo.Remove(It.IsAny<Product>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnProductResponse_WhenProductExists()
        {
            var productId = Guid.NewGuid();
            var product = new Product(
                productId,
                "Test Product",
                "This is a test product.",
                99.99m,
                10,
                Guid.NewGuid()
            );

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                 .ReturnsAsync(product);

            var handler = new GetProductQueryHandler(mockProductRepository.Object);

            var result = await handler.Handle(new GetProductQuery(productId), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(product.Description, result.Description);
            Assert.Equal(product.Price, result.Price);
            Assert.Equal(product.Quantity, result.Quantity);
            Assert.Equal(product.UserId, result.UserId);
            Assert.Equal(product.CreatedAt, result.CreatedAt);

            mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist()
        {
            var productId = Guid.NewGuid();
            var mockProductRepository = new Mock<IProductRepository>();

            var handler = new GetProductQueryHandler(mockProductRepository.Object);

            await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(new GetProductQuery(productId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateProduct_WhenProductExistsAndUserIsAuthorized()
        {
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingProduct = new Product(productId, "Old Product", "Old Description", 50.0m, 5, userId);

            var updatedName = "Updated Product";
            var updatedDescription = "Updated Description";
            var updatedPrice = 150.0m;
            var updatedQuantity = 20;

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var handler = new UpdateProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);

            var command = new UpdateProductCommand(productId, updatedName, updatedDescription, updatedPrice, updatedQuantity, userId);

            await handler.Handle(command, CancellationToken.None);

            mockProductRepository.Verify(repo => repo.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.Name == updatedName &&
                p.Description == updatedDescription &&
                p.Price == updatedPrice &&
                p.Quantity == updatedQuantity
            )), Times.Once);

            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorizedToUpdate()
        {
            var productId = Guid.NewGuid();
            var productUserId = Guid.NewGuid();
            var requestUserId = Guid.NewGuid();

            var existingProduct = new Product(productId, "Old Product", "Old Description", 50.0m, 5, productUserId);
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var handler = new UpdateProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);
            var command = new UpdateProductCommand(productId, "New Name", "New Description", 100.0m, 10, requestUserId);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist_Update()
        {
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var handler = new UpdateProductCommandHandler(mockProductRepository.Object, mockUnitOfWork.Object);

            var command = new UpdateProductCommand(productId, "New Name", "New Description", 100.0m, 10, userId);

            await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnListOfProductResponse_WhenProductsExist()
        {
            var products = new List<Product>
            {
                new Product(Guid.NewGuid(), "Product1", "Description1", 50.0m, 10, Guid.NewGuid()),
                new Product(Guid.NewGuid(), "Product2", "Description2", 30.0m, 5, Guid.NewGuid())
            };

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            var handler = new GetAllProductsQueryHandler(mockProductRepository.Object);

            var result = await handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(products.Count, result.Count);
            for (int i = 0; i < products.Count; i++)
            {
                Assert.Equal(products[i].Id, result[i].Id);
                Assert.Equal(products[i].Name, result[i].Name);
                Assert.Equal(products[i].Price, result[i].Price);
            }

            mockProductRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNoProductsException_WhenNoProductsExist()
        {
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Product>());

            var handler = new GetAllProductsQueryHandler(mockProductRepository.Object);

            await Assert.ThrowsAsync<NoProductsException>(() => handler.Handle(new GetAllProductsQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnProductWithUserResponse_WhenProductAndUserExist()
        {
            var product = new Product(Guid.NewGuid(), "Test Product", "Description", 100.0m, 5, Guid.NewGuid());
            var userResponse = new UserResponse(product.UserId, "Name", "UserName", "user@example.com");

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);

            var mockHttpClient = new Mock<HttpMessageHandler>();
            mockHttpClient.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(userResponse)),
                });

            var httpClient = new HttpClient(mockHttpClient.Object);
            var handler = new GetProductWithUserQueryHandler(mockProductRepository.Object, httpClient);

            var result = await handler.Handle(new GetProductWithUserQuery(product.Id), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(userResponse.Id, result.User.Id);

            mockProductRepository.Verify(repo => repo.GetByIdAsync(product.Id), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist_ProductWithUser()
        {
            var productId = Guid.NewGuid();

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((Product)null);

            var handler = new GetProductWithUserQueryHandler(mockProductRepository.Object, new HttpClient());

            await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(new GetProductWithUserQuery(productId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredProducts_WhenProductsMatchSearchCriteria()
        {
            var products = new List<Product>
            {
                new Product(Guid.NewGuid(), "Matching Product", "Description", 50.0m, 10, Guid.NewGuid()),
                new Product(Guid.NewGuid(), "Another Matching Product", "Description", 40.0m, 15, Guid.NewGuid())
            };

            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.SearchProductsAsync(
                    It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            var handler = new SearchProductsQueryHandler(mockProductRepository.Object);

            var result = await handler.Handle(new SearchProductsQuery("Matching", 30.0m, 60.0m, 5, 20), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(products.Count, result.Count);
            foreach (var product in products)
            {
                Assert.Contains(result, r => r.Id == product.Id && r.Name == product.Name);
            }

            mockProductRepository.Verify(repo => repo.SearchProductsAsync("Matching", 30.0m, 60.0m, 5, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNoProductsException_WhenNoProductsMatchSearchCriteria()
        {
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(repo => repo.SearchProductsAsync(
                    It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var handler = new SearchProductsQueryHandler(mockProductRepository.Object);

            await Assert.ThrowsAsync<NoProductsException>(() => handler.Handle(new SearchProductsQuery("No Match", 100.0m, 200.0m, 50, 100), CancellationToken.None));
        }
    }
}
