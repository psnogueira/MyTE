﻿// Esse arquivo foi alterado para incluir os inputs para as informações das colunas adicionadas à tabela de usuários. Serve de base para a View Register. 

#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyTE.Data;
using MyTE.Models;

namespace MyTE.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        // Acrescenta Role Manager para gerenciamento de Roles
        private readonly RoleManager<IdentityRole> _roleManager;
        // Acrescenta o contexto do banco de dados para obter a lista de departamentos disponíveis para atribuir ao usuário que está sendo criado
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            // Acrescenta Role Manager e contexto do banco de dados ao construtor
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            // Faz atribuição do Role Manager e do contexto do banco de dados
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            // Ajusta e traduz os campos já existentes de E-mail, senha e confirmação de senha 
            [Required]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "As senhas digitadas não correspondem.")]
            public string ConfirmPassword { get; set; }

            // Cria os inputs para as colunas novas que foram inseridas na tabela de usuários
            [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
            [Display(Name = "Nome")]
            [StringLength(50, ErrorMessage = "O nome do funcionário deve ter no máximo 50 caracteres.")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "O sobrenome do funcionário é obrigatório")]
            [MaxLength(50, ErrorMessage = "O sobrenome do funcionário deve ter até 50 caracteres")]
            [Display(Name = "Sobrenome")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "A data de contratação é obrigatória.")]
            [DataType(DataType.Date)]
            [Display(Name = "Data de Contratação")]
            public DateTime HiringDate { get; set; }

            [Required(ErrorMessage = "O código PID do funcionário é obrigatório.")]
            [StringLength(11, MinimumLength = 11, ErrorMessage = "O PID do funcionário tem apenas 11 caracteres")]
            public string PID { get; set; }

            [Required(ErrorMessage = "O departamento é obrigatório.")]
            [Display(Name = "Departamento")]
            public int DepartmentId { get; set; }

            public IEnumerable<SelectListItem> Departments { get; set; }

            [Display(Name = "Nível de Acesso")]
            public string Role { get; set; }

            public IEnumerable<SelectListItem> RolesList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Acrescenta a seleção de departamentos entre os existentes na tabela
            var departments = _context.Department.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            }).ToList();

            // Acrescenta a seleção de Níveis de acesso
            var roles = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            }).ToList();

            // Ajuste no InputModel para incluir a lista de departamentos e roles
            Input = new InputModel
            {
                Departments = departments,
                RolesList = roles
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Substitui o método CreateUser() adicionando as informações obtidas nos input às colunas adicionadas à tabela de usuários
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    HiringDate = Input.HiringDate,
                    PID = Input.PID,
                    DepartmentId = Input.DepartmentId,
                    EmailConfirmed = true
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Verifica se um role foi selecionado e adiciona o usuário a este role
                    if (!string.IsNullOrEmpty(Input.Role))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, Input.Role);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se algo falhar, carrega novamente o formulário e certifica-se de que as listas de departamentos e roles são recarregadas
            var departments = _context.Department.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            }).ToList();

            var roles = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            }).ToList();

            Input.Departments = departments;
            Input.RolesList = roles;

            return Page();
        }

        // Substitui onde era utilizado IdentityUser por ApplicationUser (Classe com colunas adicionais)
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Substitui onde era utilizado IdentityUser por ApplicationUser (Classe com colunas adicionais)
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
