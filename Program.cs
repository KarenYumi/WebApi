using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinhaAPI.Contexto;
using MinhaAPI.DTOs.Mappings;
using MinhaAPI.Extensions;
using MinhaAPI.Filters;
using MinhaAPI.Logging;
using MinhaAPI.Models;
using MinhaAPI.Repositories;
using MinhaAPI.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
builder.Services.AddControllers(options => //adiciona o filtro e todos os controladores poderam usa-lo
{
    options.Filters.Add(typeof(ApiExceptionFilter));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}).AddNewtonsoftJson();

//builder.Services.AddControllers().AddJsonOptions(options=> options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
//                                                USADO PARA CONEXÂO MYSQL
//string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key!!");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT: ValidAudience"],
        ValidIssuer = builder.Configuration["JWT: ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

});



//AddScoped significa que uma instancia vai ser criada da uma vez de escopo de request
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
//builder.Services.AddScoped<ApiLoggingFilter>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();//registra o servirso do unit of work
//builder.Services.AddAuthentication("Bearer").AddJwtBearer();//registrando a autenticação Json, FOI NECESSÁRIO ADD LOGS DE DIAGNÓSTICOS PARA FUNCIONAR NO APPSETTINGS.JSON (MENTIRA, N SEI COMO ARRUMEI, MAS AINDA ESTÁ FUNCIONANDO)
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();//configuramos o identity, definimos o identity user para os usuarios e outro para o pefils, o outro para armazenar os dados e autenticação o ultimo.

builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingAUTO));

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    //PARA LER AS CONFIGURAÇÔES NO .JSON
var valor1 = builder.Configuration["chave1"];
var valor2 = builder.Configuration["secao1:chave1"];
    //FIM

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); //substituido pelo debaixo
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "minhaapi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT ",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});


//Configuração do log customizado ao sistema de log ASP.NET Core
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //midlewares em fluxo numa ordem específica
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//app.UseAuthorization();

//app.Use(async (context, next) =>
//{
//    //adicioanr o codigo antes do request
//    await next(context);
//    //adicionar o codigo depois do request
//});

app.MapControllers();

//app.Run(async (context) =>
//{
//    await context.Response.WriteAsync("Middleware final"); //o método run nessa instancia é usado para adicionar um middleware terminal ao pipeline de processamento de request
//});

app.Run();
