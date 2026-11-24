using System;
using System.Linq.Expressions;
using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.Models;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Utility;

namespace ShoppingCart.Tests
{
    public class OrderControllerTests
    {
        [Fact]
        public void OrderDetails_ValidId_ReturnOrder()
        {
            // Arrange
            var order = new OrderHeader { Id = 1 };

            var headerMock = new Mock<IOrderHeaderRepository>();
            var detailMock = new Mock<IOrderDetailRepository>();

            headerMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(order);

            detailMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(new List<OrderDetail>());

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            mockUnitOfWork.Setup(uow => uow.OrderDetail)
                .Returns(detailMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.OrderDetails(1);

            // Assert
            Assert.Equal(order, result.OrderHeader);
        }

        [Fact]
        public void OrderDetails_InvalidId_ReturnNull()
        {
            // Arrange
            var headerMock = new Mock<IOrderHeaderRepository>();
            var detailMock = new Mock<IOrderDetailRepository>();

            headerMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns((OrderHeader)null);

            detailMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(new List<OrderDetail>());

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            mockUnitOfWork.Setup(uow => uow.OrderDetail)
                .Returns(detailMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.OrderDetails(999);

            // Assert
            Assert.Null(result.OrderHeader);
            Assert.Empty(result.OrderDetails);
        }

        [Fact]
        public void OrderDetails_ValidId_ReturnOnlyOrderItems()
        {
            // Arrange
            var order = new OrderHeader { Id = 1 };

            var details = new List<OrderDetail>
    {
        new OrderDetail { Id = 1, OrderHeaderId = 1 },
        new OrderDetail { Id = 2, OrderHeaderId = 1 },
        new OrderDetail { Id = 3, OrderHeaderId = 2 }
    };

            var headerMock = new Mock<IOrderHeaderRepository>();
            var detailMock = new Mock<IOrderDetailRepository>();

            headerMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(order);

            detailMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(details);

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            mockUnitOfWork.Setup(uow => uow.OrderDetail)
                .Returns(detailMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.OrderDetails(1);

            // Assert
            Assert.Equal(2, result.OrderDetails.Count());
            Assert.All(result.OrderDetails,
                item => Assert.Equal(1, item.OrderHeaderId));
        }
        [Fact]
        public void SetToInProcess_ValidOrder_UpdateStatus()
        {
            // Arrange
            var vm = new OrderVM
            {
                OrderHeader = new OrderHeader { Id = 1 }
            };

            var headerMock = new Mock<IOrderHeaderRepository>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            controller.SetToInProcess(vm);

            // Assert
            headerMock.Verify(r => r.UpdateStatus(
                1, OrderStatus.StatusInProcess, null), Times.Once);

            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void SetToShipped_ValidOrder_UpdateShipping()
        {
            // Arrange
            var order = new OrderHeader { Id = 1 };

            var vm = new OrderVM
            {
                OrderHeader = new OrderHeader
                {
                    Id = 1,
                    Carrier = "Nova Poshta",
                    TrackingNumber = "123456"
                }
            };

            var headerMock = new Mock<IOrderHeaderRepository>();

            headerMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(order);

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var before = DateTime.Now;
            controller.SetToShipped(vm);

            // Assert
            Assert.Equal(OrderStatus.StatusShipped, order.OrderStatus);
            Assert.Equal("Nova Poshta", order.Carrier);
            Assert.Equal("123456", order.TrackingNumber);
            Assert.InRange(order.DateOfShipping, before, DateTime.Now);

            headerMock.Verify(r => r.Update(order), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_UnpaidOrder_CancelOrder()
        {
            // Arrange
            var order = new OrderHeader
            {
                Id = 1,
                PaymentStatus = PaymentStatus.StatusPending
            };

            var vm = new OrderVM
            {
                OrderHeader = order
            };

            var headerMock = new Mock<IOrderHeaderRepository>();

            headerMock.Setup(r => r.GetT(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(order);

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.OrderHeader)
                .Returns(headerMock.Object);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            controller.SetToCancelOrder(vm);

            // Assert
            headerMock.Verify(r => r.UpdateStatus(
                1, OrderStatus.StatusCancelled, null), Times.Once);

            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }
    }
}
