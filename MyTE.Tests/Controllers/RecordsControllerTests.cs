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

            // Adiciona dados de exemplo ao banco de dados
            context.WBS.Add(new WBS
            {
                WBSId = 1,
                Code = "001",
                Desc = "Description1",
                Type = (WBSType) 1
            });
            context.Record.Add(new Models.Record
            {
                RecordId = 1,
                Data = DateTime.Now.Date,
                Hours = 8,
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
            Assert.Equal("001", model.First().WBS.Code);
            Assert.Equal(8, model.First().TotalHours);

            // Obs.: Esse teste verifica se o método Index retorna os registros corretos para da data atual quando é chamado sem uma data específica
        }
    }
}
