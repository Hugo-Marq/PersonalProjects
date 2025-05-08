
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.Connections;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Controllers;
using QuickRoomSolutions.Repositories;
using QuickRoomSolutions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuickRoomSolutions.Notificacoes;

namespace QuickRoomSolutions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            if (System.IO.File.Exists(".env.override"))
            {
                DotNetEnv.Env.Load(".env.override");
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => { 
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {

                    Description= "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();

            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
            //Necessario para aceitar o front end a posterior é necessario verificar o IP
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalHost", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                    .SetIsOriginAllowed(isOriginAllowed: _ => true)
                    .AllowAnyHeader().AllowAnyMethod();
                });
            });

            builder.Services.AddHostedService<BackgroundServices>();
            builder.Services.AddSingleton<BackgroundServices>();


            SqlServerDbConnection conection = new SqlServerDbConnection();
           
            //Medida preventiva para eliminar qualquer registo de provedor de BD.
            //Serve para eliminar possíveis conflitos com testes unitários e de integração
            builder.Services.RemoveAll(typeof(DbContextOptions<QuickRoomSolutionDatabaseContext>));

            var env = builder.Environment;

            //condição para o programa usar a BD correta consoante o ambiente, desenvolvimento ou teste.
            if (env.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<QuickRoomSolutionDatabaseContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase"));
            }
            else
            {
                builder.Services.AddEntityFrameworkSqlServer()
                .AddDbContext<QuickRoomSolutionDatabaseContext>(
                options => options.UseSqlServer(conection.GetConnectionString()));
            }
    

            //Scopes
            builder.Services.AddScoped<IReservasRepository<Reserva>,ReservasRepository>();
            builder.Services.AddScoped<ITipologiasRepository<Tipologia>, TipologiasRepository>();
            builder.Services.AddScoped<IPessoasRepository<Pessoa>, PessoasRepository>();
            builder.Services.AddScoped<IClientesRepository<Cliente>, ClientesRepository>();
            builder.Services.AddScoped<ICargoRepository<Cargo>, CargoRepository>();
            builder.Services.AddScoped<IFornecedorRepository<Fornecedor>, FornecedorRepository>();
            builder.Services.AddScoped<IFuncionarioFornecedorRepository<FuncionarioFornecedor>, FuncionarioFornecedorRepository>();
            builder.Services.AddScoped<IFuncionarioRepository<Funcionario>, FuncionarioRepository>();
            builder.Services.AddScoped<IOrcamentoRepository<Orcamento>, OrcamentoRepository>();
            builder.Services.AddScoped<ITicketLimpezaRepository<TicketLimpeza>, TicketLimpezaRepository>();
            builder.Services.AddScoped<IServicoRepository<Servico>, ServicoRepository>();
            builder.Services.AddScoped<ITicketRepository<Ticket>, TicketRepository>();
            builder.Services.AddScoped<IBaseQuartoRepository<Quarto>, QuartoRepository>();
            builder.Services.AddScoped<JWTService>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseCors("AllowLocalHost");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();
            //TESTEEEEEEE
            app.UseDeveloperExceptionPage();

            app.Run();
        }
    }
}
