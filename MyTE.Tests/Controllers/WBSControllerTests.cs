using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTE.Controllers;
using MyTE.Data;
using MyTE.Models;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using MyTE.Models.Enum;
using MyTE.Models.ViewModel;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MyTE.Tests.Controllers
{
    public class WBSControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithCorrectViewModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Configura o contexto com dados de teste
            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new Models.WBS { Code = "001", Desc = "Description1", Type = (WBSType)1 });
                context.WBS.Add(new Models.WBS { Code = "002", Desc = "Description2", Type = (WBSType)2 });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Index(null, null, null); // Aguarda a conclusão da Task

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result); // Verifica o tipo do resultado
                var viewModel = Assert.IsType<WBSViewModel>(viewResult.Model);

                Assert.NotNull(viewModel.WBSList);
                Assert.NotNull(viewModel.Type);
                Assert.Equal("001", viewModel.WBSList.First().Code);
            }
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithValidId()
        {
            // Arrange 
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Configurar o contexto com dados para teste
            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new WBS { WBSId = 3, Code = "003", Desc = "Description3", Type = (WBSType)1 });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Details(3);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(3, model.WBSId);
            }
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Details(999); // ID inválido

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Delete_ReturnsRedirectToActionResult_WithValidId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Configura o contexto com um item de teste
            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new WBS { WBSId = 4, Code = "004", Desc = "Description4", Type = (WBSType) 2 });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Delete(4);

                // Assert
                Assert.IsType<RedirectToActionResult>(result);
                var redirectToAction = (RedirectToActionResult)result;
                Assert.Equal("Index", redirectToAction.ActionName);
            }
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Delete(999); // ID inválido

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Create_ReturnsViewResult()
        {
            // Arrange 
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = controller.Create();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
            }
        }

        [Fact]
        public async Task Create_Post_ReturnsRedirectToActionResult_WithValidModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                var wBS = new WBS
                {
                    WBSId = 1,
                    Code = "001",
                    Desc = "Description1",
                    Type = (WBSType)1
                };

                // Act
                var result = await controller.Create(wBS);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectToActionResult.ActionName);
            }

            // Verifica se a WBS foi adicionada a base de dados
            using (var context = new ApplicationDbContext(options))
            {
                Assert.Equal(1, context.WBS.Count());
                var wBS = context.WBS.FirstOrDefault();
                Assert.Equal("001", wBS.Code);
            }
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResult_WithInvalidModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Simula erro de validação de admin
                controller.ModelState.AddModelError("Code", "Required");

                var wBS = new WBS
                {
                    WBSId = 1,
                    Desc = "Description1",
                    Type = (WBSType)1
                };

                // Act
                var result = await controller.Create(wBS);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(wBS, model);
            }
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithValidId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new WBS { WBSId = 1, Code = "001", Desc = "Description1", Type = (WBSType)1 });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Edit(1);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<WBS>(viewResult.ViewData.Model);
                Assert.Equal(1, model.WBSId);
            }
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new WBS { WBSId = 1, Code = "001", Desc = "Description1", Type = (WBSType) 1 });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Act
                var result = await controller.Edit(2);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Edit_Post_ReturnsRedirectToActionResult_WithValidModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.WBS.Add(new WBS { WBSId = 1, Code = "001", Desc = "Description1", Type = (WBSType) 1 });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Inicializa TempData
                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                var wBS = new WBS { WBSId = 1, Code = "001", Desc = "Description1 Updated", Type = (WBSType)1 };

                // Act
                var result = await controller.Edit(1, wBS);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectToActionResult.ActionName);

                // Verify the WBS was updated in the database
                var updatedWBS = context.WBS.Find(1);
                Assert.Equal("Description1 Updated", updatedWBS.Desc);
            }
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WithInvalidModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new WBSController(context);

                // Inicializa TempData
                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                // Adiciona um WBS para editar
                var wBS = new WBS
                {
                    WBSId = 1,
                    Code = "001",
                    Desc = "Description1",
                    Type = (WBSType) 2
                };
                context.WBS.Add(wBS);
                await context.SaveChangesAsync();

                // Atualiza o WBS com dados inválidos
                var updatedWBS = new WBS
                {
                    WBSId = 1,
                    Code = "", // Campo obrigatório vazio
                    Desc = "Updated Description",
                    Type = (WBSType) 2
                };

                // Simula erro de validação no ModelState
                controller.ModelState.AddModelError("Code", "Required");

                // Act
                var result = await controller.Edit(1, updatedWBS);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(updatedWBS, model);
            }
        }
    }
}
