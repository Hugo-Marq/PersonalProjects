using Microsoft.IdentityModel.Tokens;
using QuickRoomSolutions.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickRoomSolutions.Services
{
    public class JWTService
    {

        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken<T>(T entity)
        {

            string role = MapUserRoleToClaim(entity);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;


        }



        public string MapUserRoleToClaim<T>(T entity)
        {
            if (typeof(T) == typeof(Cliente))
            {
                return "Cliente";
            }
            else if (typeof(T) == typeof(Funcionario))
            {
                if (entity is Funcionario funcionario)
                {
                    if (funcionario.CargoCargoId == 1)
                    {
                        return "Gerente";
                    }
                    else if (funcionario.CargoCargoId == 2)
                    {
                        return "Rececionista";
                    }
                    else if (funcionario.CargoCargoId == 3)
                    {
                        return "Limpeza";
                    }
                    else
                    {
                        return "Default";
                    }
                }
            }
            else if (typeof(T) == typeof(FuncionarioFornecedor))
            {
                return "FuncionarioFornecedor";
            }

            return "Default";

        }


    }
}
