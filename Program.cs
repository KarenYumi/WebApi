using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinhaAPI.Contexto;
using MinhaAPI.DTOs.Mappings;
using MinhaAPI.Extensions;
using MinhaAPI.Filters;
using MinhaAPI.Logging;
using MinhaAPI.Models;
using MinhaAPI.RateLimitOptions;
using MinhaAPI.Repositories;
using MinhaAPI.Services;
using NuGet.Configuration;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
         policy =>
         {
            policy.WithOrigins("https://apirequest.io").WithMethods("Get");
         });
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddControllers().AddJsonOptions(options=> options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
//                                                USADO PARA CONEXÂO MYSQL
//string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(secretKey))
    };
});

var myOptions = new MyRateLimitOptions();
builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions); //pega a exceção dentro da classe e associa as conf. obtidas à instância da classe MyRateLimitOptions
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(policyName: "fixedwindow", options =>
    {
        options.PermitLimit = myOptions.PermitLimit;//1;
        options.Window = TimeSpan.FromSeconds(myOptions.Window);
        options.QueueLimit = myOptions.QueueLimit;//2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests; //caso ultrapasse a limitação retorna o statuscode
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpcontext =>
                            RateLimitPartition.GetFixedWindowLimiter(
                                               partitionKey: httpcontext.User.Identity?.Name ??
                                                             httpcontext.Request.Headers.Host.ToString(),                               //Aplicação Genérica em todos os controllers
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 2,
                                QueueLimit = 0,
                                Window = TimeSpan.FromSeconds(10)
                            }));
});

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true; //se a versão n for especificado mostrará a versão padrão
    o.ReportApiVersions = true; //as versões da api devem ser adicionadas no cabeçãlho headers do response
    o.ApiVersionReader = ApiVersionReader.Combine(
                          new QueryStringApiVersionReader(),
                          new UrlSegmentApiVersionReader());

}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; //a forma q a versão será mostrada, os três v é o numero da versão
    options.SubstituteApiVersionInUrl = true; //gera links diferentes em versões diferentes

});
//AddScoped significa que uma instancia vai ser criada da uma vez de escopo de request
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
//builder.Services.AddScoped<ApiLoggingFilter>(); 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();//registra o servirso do unit of work
//builder.Services.AddAuthentication("Bearer").AddJwtBearer();//registrando a autenticação Json, FOI NECESSÁRIO ADD LOGS DE DIAGNÓSTICOS PARA 
//FUNCIONAR NO APPSETTINGS.JSON (MENTIRA, N SEI COMO ARRUMEI, MAS AINDA ESTÁ FUNCIONANDO)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    options.AddPolicy("SuperAdminOnly", policy =>
                       policy.RequireRole("Admin").RequireClaim("id", "yumi"));

    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));

    options.AddPolicy("ExclusiveOnly", policy =>
                      policy.RequireAssertion(context =>
                      context.User.HasClaim(claim =>
                                           claim.Type == "id" && claim.Value == "yumi")
                                           || context.User.IsInRole("SuperAdmin")));
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();//configuramos o identity, definimos o identity user para os usuarios e outro para o pefils, o outro para armazenar os dados e autenticação o ultimo.

builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingAUTO));

//PARA LER AS CONFIGURAÇÔES NO .JSON
//var valor1 = builder.Configuration["chave1"];
//var valor2 = builder.Configuration["secao1:chave1"];
//FIM

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//builder.Services.AddSwaggerGen(); //substituido pelo debaixo
builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "minhaapi", Version = "v1" });
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version ="v1",
        Title = "MinhaApi",
        Description = "Catélogo de Produtos e Cateogorias",
        TermsOfService = new Uri("https://www.karenapi.com/termns"),
        Contact = new OpenApiContact
        {
            Name = "Karen",
            Email = "karen.silva@brisabr.com.br",
            Url = new Uri("https://www.karenapi.com"),
        },
        License = new OpenApiLicense 
        { 
            Name = "Usar sobre LICX",
            Url = new Uri("https://www.karenapi.com/license"),
        }
    });

   var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));

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
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();  // Middleware de autenticação deve vir primeiro
app.UseAuthorization();   // Middleware de autorização deve vir depois
app.MapControllers();
app.Run();
