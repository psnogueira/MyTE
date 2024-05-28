using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyTE.Controllers;
using MyTE.Data;
using MyTE.DTO;
using MyTE.Models;
using MyTE.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyTE.Tests.Controllers
{
    public class RecordsControllerTests 
    { 
        // Configura o UserManager para simular obtenção de UserId
        private UserManager<ApplicationUser> GetUserManagerMock()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns("test-user-id");

            return userManagerMock.Object;

        }

        // Configura o contexto do banco de dados em memória para simular o banco de dados
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TEstDatabase")
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewWithDefaultDate_WhenDataSearchIsNull()
        {
            // ARRANGE
            var context = GetDbContext();
            var userManager = GetUserManagerMock();
            var controller = new RecordsController(context, userManager);

            var testWbsCode = "001";
            var testHours = 8;

            // Limpa o banco de dados antes de cada teste
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Adiciona dados de exemplo ao banco de dados
            context.WBS.Add(new WBS
            {
                WBSId = 1,
                Code = testWbsCode,
                Desc = "Description1",
                Type = (WBSType) 1
            });
            context.Record.Add(new Models.Record
            {
                RecordId = 1,
                Data = DateTime.Now.Date,
                Hours = testHours,
                UserId = "test-user-id",
                WBSId = 1
            });
            context.SaveChanges();

            // ACT
            var result = await controller.Index(null) as ViewResult; // chama o método index com um datasearch nulo (não fornece data)

            // ASSERT

            // Verifica se o resultado não é nulo
            Assert.NotNull(result); 
            var model = result.Model as List<RecordDTO>;
            Assert.NotNull(model);
            Assert.Single(model);
            //Verifica se o viewmodel contém os dados esperados
            Assert.Equal(testWbsCode, model.First().WBS.Code);
            Assert.Equal(testHours, model.First().TotalHours);

            // Obs.: Esse teste verifica se o método Index retorna os registros corretos para a data atual quando é chamado sem uma data específica
        }

        //[Fact]
        //public async Task Index_ReturnsCorrectViewWithSpecificDate_WhenDataSearchIsInvalid()
        //{
        //   TODO: desenvolver teste para lidar com entrada de data menor que o StartDateRestriction    
        //}

        [Fact]
        public async Task Index_ReturnsCorrectViewWithDefaultDate_WhenDataSearchIsValid()
        {
            // ARRANGE
            var context = GetDbContext();
            var userManager = GetUserManagerMock();
            var controller = new RecordsController(context, userManager);

            var testWbsCode = "001";
            var testHours = 8;
            var testDate = DateTime.Now.Date;

            // Limpa o banco de dados antes de cada teste
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Adiciona dados de exemplo ao banco de dados
            context.WBS.Add(new WBS
            {
                WBSId = 1,
                Code = testWbsCode,
                Desc = "Description1",
                Type = (WBSType)1
            });
            context.Record.Add(new Models.Record
            {
                RecordId = 1,
                Data = testDate,
                Hours = testHours,
                UserId = "test-user-id",
                WBSId = 1
            });
            context.SaveChanges();

            // ACT
            var result = await controller.Index(testDate) as ViewResult;

            // ASSERT

            // Verifica se o resultado não é nulo
            Assert.NotNull(result);
            var model = result.Model as List<RecordDTO>;
            Assert.NotNull(model);
            Assert.Single(model);
            //Verifica se o viewmodel contém os dados esperados
            Assert.Equal(testWbsCode, model.First().WBS.Code);
            Assert.Equal(testHours, model.First().TotalHours);

            // Obs.: Esse teste verifica se o método Index retorna os registros corretos quando é chamado com a data atual
        }

        [Fact]
        public async Task Persist_RedirecsToIndex_WithValidRecords()
        {
            // ARRANGE
            var context = GetDbContext();
            var userManager = GetUserManagerMock();
            var controller = new RecordsController(context, userManager);

            // Limpa o banco de dados antes de cada teste
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var validRecords = new List<RecordDTO>
            {
                new RecordDTO
                {
                    records = new List<Models.Record>
                    {
                        new Models.Record { UserId = "test-user-id", Data = new DateTime(2024, 5, 2), Hours = 8, WBSId = 1},
                        new Models.Record { UserId = "test-user-id", Data = new DateTime(2024, 5, 3), Hours = 8, WBSId = 1},
                    },
                    WBSId = 1,
                    WBS = new WBS { WBSId = 1, Code = "001", Desc = "Description1", Type = (WBSType)1 },
                    TotalHours = 16,
                    TotalHoursDay = new double[16]
                }
            };

            // ACT
            var result = await controller.Persist(validRecords);

            // ASSERT
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToAction = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectToAction.ActionName);
        }


        //  AJUSTAR PARA RECEBER O ERRO TEMPDATA QUE EXCEDE O NUMERO DE HORAS
        //[Fact]
        //public async Task Persist_RedirectsToIndex_WithInvalidRecords()
        //{
        //    // ARRANGE
        //    var context = GetDbContext();
        //    var userManager = GetUserManagerMock();
        //    var controller = new RecordsController(context, userManager);

        //    // Limpa o banco de dados antes de cada teste
        //    context.Database.EnsureDeleted();
        //    context.Database.EnsureCreated();

        //    var invalidRecords = new List<RecordDTO>
        //    {
        //        new RecordDTO
        //        {
        //            records = new List<Models.Record>
        //            {
        //                new Models.Record { UserId = "test-user-id", Data = new DateTime(2024, 5, 2), Hours = 4, WBSId = 1},
        //                new Models.Record { UserId = "test-user-id", Data = new DateTime(2024, 5, 3), Hours = 25, WBSId = 1},
        //            },
        //            WBSId = 1,
        //            WBS = new WBS { WBSId = 1, Code = "001", Desc = "Description1", Type = (WBSType)1 },
        //            TotalHours = 33,
        //            TotalHoursDay = new double[16]
        //        }
        //    };

        //    // ACT
        //    var result = await controller.Persist(invalidRecords);

        //    // ASSERT
        //    Assert.IsType<RedirectToActionResult>(result);
        //    var redirectToAction = (RedirectToActionResult)result;
        //    Assert.Equal("Index", redirectToAction.ActionName);
        //}
    }
}
