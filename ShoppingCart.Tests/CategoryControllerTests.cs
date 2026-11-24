using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class CategoryControllerTests
    {
        [Fact]
        public void GetCategories_All_ReturnAllCategories()
        {
            // Arrange
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(() => CategoryDataset.Categories);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.Equal(CategoryDataset.Categories, result.Categories);
        }

        [Fact]
        public void GetCategories_Empty_ReturnEmptyList()
        {
            // Arrange
            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(new List<Category>());

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.Empty(result.Categories);
        }

        [Fact]
        public void GetCategory_ValidId_ReturnCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test1" };

            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns(category);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get(1);

            // Assert
            Assert.Equal(category, result.Category);
        }

        [Fact]
        public void GetCategory_InvalidId_ReturnNull()
        {
            // Arrange
            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns((Category)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get(999);

            // Assert
            Assert.Null(result.Category);
        }

        [Fact]
        public void CreateUpdate_NewCategory_AddCategory()
        {
            // Arrange
            var category = new Category { Id = 0, Name = "Electronics" };
            var vm = new CategoryVM { Category = category };

            var repositoryMock = new Mock<ICategoryRepository>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(vm);

            // Assert
            repositoryMock.Verify(r => r.Add(category), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_ExistingCategory_UpdateCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Updated" };
            var vm = new CategoryVM { Category = category };

            var repositoryMock = new Mock<ICategoryRepository>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(vm);

            // Assert
            repositoryMock.Verify(r => r.Update(category), Times.Once);
            repositoryMock.Verify(r => r.Add(category), Times.Never);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_InvalidModel_ThrowsException()
        {
            // Arrange
            var vm = new CategoryVM();
            var repositoryMock = new Mock<ICategoryRepository>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            controller.ModelState.AddModelError("Name", "Required");

            // Act & Assert
            var exception = Assert.Throws<Exception>(
                () => controller.CreateUpdate(vm));

            Assert.Equal("Model is invalid", exception.Message);

            mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
        }

        [Fact]
        public void DeleteData_ExistingCategory_DeleteCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test1" };

            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns(category);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.DeleteData(1);

            // Assert
            repositoryMock.Verify(r => r.Delete(category), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void DeleteData_InvalidId_ThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns((Category)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(
                () => controller.DeleteData(999));

            Assert.Equal("Category not found", exception.Message);

            repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
        }
    }
}