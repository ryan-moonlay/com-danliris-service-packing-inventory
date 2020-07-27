﻿using Com.Danliris.Service.Packing.Inventory.Application;
using Com.Danliris.Service.Packing.Inventory.Application.DTOs;
using Com.Danliris.Service.Packing.Inventory.Application.Master.ProductPacking;
using Com.Danliris.Service.Packing.Inventory.Application.ToBeRefactored.Utilities;
using Com.Danliris.Service.Packing.Inventory.Data.Models.Product;
using Com.Danliris.Service.Packing.Inventory.Infrastructure.IdentityProvider;
using Com.Danliris.Service.Packing.Inventory.WebApi.Controllers.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Danliris.Service.Packing.Inventory.Test.Controllers.Master
{
   public class ProductPackingControllerTest
    {
        public Mock<IServiceProvider> GetServiceProvider(IProductPackingService productPackingService, IIdentityProvider identityProvider, IValidateService validateService)
        {
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(s => s.GetService(typeof(IProductPackingService)))
                .Returns(productPackingService);

            spMock.Setup(s => s.GetService(typeof(IIdentityProvider)))
              .Returns(identityProvider);

            spMock.Setup(s => s.GetService(typeof(IValidateService)))
              .Returns(validateService);

            return spMock;
        }

        private ProductPackingController GetController(IServiceProvider serviceProvider)
        {
            var claimPrincipal = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            claimPrincipal.Setup(claim => claim.Claims).Returns(claims);

            var controller = new ProductPackingController(serviceProvider)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = claimPrincipal.Object

                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = $"{It.IsAny<int>()}";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");

            return controller;
        }

        private int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        private FormDto formDto
        {
            get
            {
                return new FormDto()
                {
                    Name = "Name",
                    Code = "Code",
                    PackingSize=1,
                    ProductSKUId =1,
                    Description = "Description",
                    UOMId = 1
                };
            }
        }

        private ProductPackingDto productPackingDto
        {
            get
            {
                return new ProductPackingDto(new ProductPackingModel(), new ProductSKUModel(), new UnitOfMeasurementModel(), new UnitOfMeasurementModel(), new CategoryModel());
            }
        }
        private ServiceValidationException GetServiceValidationException()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>()
            {
                new ValidationResult("message",new string[1]{ "A" }),
                new ValidationResult("{}",new string[1]{ "B" })
            };
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(formDto, serviceProvider.Object, null);
            return new ServiceValidationException(validationContext, validationResults);
        }

        [Fact]
        public async Task Should_Success_Post()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Create(It.IsAny<FormDto>())).ReturnsAsync(1);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Post(dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public async Task Post_Return_BadRequest()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Create(It.IsAny<FormDto>())).Throws(GetServiceValidationException());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Post(dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public async Task Post_Return_InternalServerError()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Create(It.IsAny<FormDto>())).Throws(new Exception());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Post(dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task GetById_Return_OK()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync(productPackingDto);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.GetById(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public async Task GetById_Return_NotFound()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync(()=>null);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.GetById(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.NotFound, GetStatusCode(response));
        }

        [Fact]
        public async Task GetById_Return_InternalServerError()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).Throws(new Exception());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.GetById(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Get_Return_OK()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetIndex(It.IsAny<IndexQueryParam>())).ReturnsAsync(new ProductPackingIndex(new List<ProductPackingIndexInfo>(),1,1,25));
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Get(new IndexQueryParam());

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public async Task Get_Return_InternalServerError()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetIndex(It.IsAny<IndexQueryParam>())).Throws(new Exception());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Get(new IndexQueryParam());

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Delete_Return_SuccessNoContent()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Delete(It.IsAny<int>())).ReturnsAsync(1);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Delete(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public async Task Delete_Return_SuccessNotFound()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Delete(It.IsAny<int>())).ReturnsAsync(0);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Delete(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.NotFound, GetStatusCode(response));
        }

        [Fact]
        public async Task Delete_Return_InternalServerError()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.Delete(It.IsAny<int>())).Throws(new Exception());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Delete(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }


        [Fact]
        public async Task Put_Success_Return_NoContent()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync(productPackingDto);
            serviceMock.Setup(s => s.Update(It.IsAny<int>(),It.IsAny<FormDto>())).ReturnsAsync(1);
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Put(1,dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public async Task Put_Success_Return_BadRequest()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync(productPackingDto);
            serviceMock.Setup(s => s.Update(It.IsAny<int>(), It.IsAny<FormDto>())).Throws(GetServiceValidationException());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Put(1, dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public async Task Put_Success_Return_InternalServerError()
        {
            //Setup
            var dataUtil = formDto;
            var serviceMock = new Mock<IProductPackingService>();
            serviceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync(productPackingDto);
            serviceMock.Setup(s => s.Update(It.IsAny<int>(), It.IsAny<FormDto>())).Throws(new Exception());
            var service = serviceMock.Object;

            var identityProviderMock = new Mock<IIdentityProvider>();
            var identityProvider = identityProviderMock.Object;

            var validateServiceMock = new Mock<IValidateService>();
            validateServiceMock.Setup(s => s.Validate(It.IsAny<FormDto>())).Verifiable();
            var validateService = validateServiceMock.Object;

            //Act
            var controller = GetController(GetServiceProvider(service, identityProvider, validateService).Object);
            var response = await controller.Put(1, dataUtil);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
