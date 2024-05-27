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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var codeFirst = "001";

                context.WBS.Add(new Models.WBS { Code = codeFirst, Desc = "Description1", Type = (WBSType) 1 });
                context.WBS.Add(new Models.WBS { Code = "002", Desc = "Description2", Type = (WBSType) 2 });
                context.WBS.Add(new Models.WBS { Code = "003", Desc = "Description3", Type = (WBSType) 1 });
                await context.SaveChangesAsync();

                var controller = new WBSController(context);

                // Act
                var result = await controller.Index(null, null, null); 

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result); 
                var viewModel = Assert.IsType<WBSViewModel>(viewResult.Model);

                Assert.NotNull(viewModel.WBSList);
                Assert.NotNull(viewModel.Type);
                Assert.Equal(codeFirst, viewModel.WBSList.First().Code);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wbsIdValid = 1;

                context.WBS.Add(new WBS { WBSId = wbsIdValid, Code = "001", Desc = "Description1", Type = (WBSType) 1 });
                await context.SaveChangesAsync();

                var controller = new WBSController(context);

                // Act
                var result = await controller.Details(wbsIdValid);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(wbsIdValid, model.WBSId);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wbsIdInvalid = 999;

                var controller = new WBSController(context);

                // Act
                var result = await controller.Details(wbsIdInvalid); 

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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wbsIdValid = 1;

                context.WBS.Add(new WBS { WBSId = wbsIdValid, Code = "001", Desc = "Description1", Type = (WBSType) 1 });
                await context.SaveChangesAsync();

                var controller = new WBSController(context);

                // Act
                var result = await controller.Delete(wbsIdValid);

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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wbsIdInvalid = 999;

                var controller = new WBSController(context);

                // Act
                var result = await controller.Delete(wbsIdInvalid); // ID inválido

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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var controller = new WBSController(context);
                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                var validWbsCode = "001";

                var validWbs = new WBS
                {
                    WBSId = 1,
                    Code = validWbsCode,
                    Desc = "Description1",
                    Type = (WBSType) 1
                };

                // Act
                var result = await controller.Create(validWbs);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectToActionResult.ActionName);
  
                Assert.Equal(1, context.WBS.Count());
                var wBS = context.WBS.FirstOrDefault();
                Assert.Equal(validWbsCode, validWbs.Code);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var controller = new WBSController(context);

                // Simula erro de validação de admin
                controller.ModelState.AddModelError("Code", "Required");

                var wbs = new WBS
                {
                    WBSId = 1,
                    Code = "001",
                    Desc = "Description1",
                    Type = (WBSType)1
                };

                // Act
                var result = await controller.Create(wbs);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(wbs, model);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wbsId = 1;

                context.WBS.Add(new WBS { WBSId = wbsId, Code = "001", Desc = "Description1", Type = (WBSType)1 });
                context.SaveChanges();

                var controller = new WBSController(context);

                // Act
                var result = await controller.Edit(wbsId);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<WBS>(viewResult.ViewData.Model);
                Assert.Equal(wbsId, model.WBSId);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var validWbsId = 1;
                var invalidWbsId = 2;

                context.WBS.Add(new WBS { WBSId = validWbsId, Code = "001", Desc = "Description1", Type = (WBSType) 1 });
                context.SaveChanges();

                var controller = new WBSController(context);

                // Act
                var result = await controller.Edit(invalidWbsId);

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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                
                var wbsId = 1;
                var descriptionOld = "Old Description";


                context.WBS.Add(new WBS { WBSId = wbsId, Code = "001", Desc = descriptionOld, Type = (WBSType) 1 });
                context.SaveChanges();

                var controller = new WBSController(context);

                var descriptionUpdated = "Description Updated";

                // Inicializa TempData
                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                var existingWbs = await context.WBS.FindAsync(wbsId);
                existingWbs.Desc = descriptionUpdated;

                // Act
                var result = await controller.Edit(wbsId, existingWbs);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectToActionResult.ActionName);

                // Verify the WBS was updated in the database
                var updatedWBS = context.WBS.Find(wbsId);
                Assert.Equal(descriptionUpdated, updatedWBS.Desc);
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
                // Limpa o banco de dados antes de cada teste
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var controller = new WBSController(context);

                var wbsId = 1;

                // Inicializa TempData
                var tempData = new Mock<ITempDataDictionary>();
                controller.TempData = tempData.Object;

                // Adiciona um WBS para editar
                var wbs = new WBS
                {
                    WBSId = 1,
                    Code = "001",
                    Desc = "Description1",
                    Type = (WBSType) 2
                };
                context.WBS.Add(wbs);
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
                var result = await controller.Edit(wbsId, updatedWBS);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<WBS>(viewResult.ViewData.Model);
                Assert.Equal(updatedWBS, model);
            }
        }
    }
}
