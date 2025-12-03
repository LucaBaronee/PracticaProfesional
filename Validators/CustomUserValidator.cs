using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ProyetoSetilPF.Validators
{
    public class CustomUserValidator : IUserValidator<IdentityUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = new List<IdentityError>();

            if (!user.Email.EndsWith("@setilviajes.com"))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidEmailDomain",
                    Description = "El correo debe pertenecer al dominio @setilviajes.com"
                });
            }

            return Task.FromResult(errors.Count == 0
                ? IdentityResult.Success
                : IdentityResult.Failed(errors.ToArray()));
        
        }
    }
}
