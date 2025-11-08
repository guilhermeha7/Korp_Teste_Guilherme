using AutoMapper;
using FaturamentoAPI.Data;
using FaturamentoAPI.DTOs;
using FaturamentoAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var connectionString = builder.Configuration.GetConnectionString("FaturamentoDb");
            builder.Services.AddAutoMapper(typeof(NotaFiscalMappingProfile));
            builder.Services.AddHttpClient<IProdutoService, ProdutoService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Services:ProdutosBaseUrl"]); // configurar em appsettings
                client.Timeout = TimeSpan.FromSeconds(5);
            });
            builder.Services.AddHttpClient("EstoqueApi", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7105/");
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(); 
            builder.Services.AddDbContext<FaturamentoContext>(options =>
                 options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
                 new MySqlServerVersion(new Version(8, 0, 43))));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
