// Importación de librerías necesarias para autenticación, seguridad, base de datos y documentación
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAPI.Context;
using WebAPI.Services.IServices;
using WebAPI.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// ?? Clave secreta para la generación y validación de tokens JWT
// Si no se encuentra en la configuración, se usa una clave por defecto
var key = builder.Configuration["Jwt:Key"] ?? "clave_super_secreta_1234567890_ABCDEF";

// ? Configuración de la autenticación utilizando JWT (JSON Web Token)
builder.Services.AddAuthentication(options =>
{
    // Especifica que se utilizará JWT para autenticar y desafiar
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    // Desactiva la necesidad de HTTPS para los tokens (solo útil en desarrollo)
    opt.RequireHttpsMetadata = false;

    // Guarda el token en el contexto de la autenticación
    opt.SaveToken = true;

    // Parámetros de validación del token JWT
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // No se valida el emisor del token
        ValidateAudience = false, // No se valida el receptor del token
        ValidateLifetime = true, // Se valida la vigencia del token
        ValidateIssuerSigningKey = true, // Se valida la firma del token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // Clave para validar la firma
    };
});

// ? Agrega servicios de autorización
builder.Services.AddAuthorization();

// ? Configuración de Swagger para documentar la API y permitir pruebas con JWT
builder.Services.AddSwaggerGen(c =>
{
    // Información básica del documento Swagger
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi29AV", Version = "v1" });

    // Definición de seguridad para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // Nombre del encabezado
        Type = SecuritySchemeType.Http, // Tipo de seguridad
        Scheme = "Bearer", // Esquema Bearer
        BearerFormat = "JWT", // Formato JWT
        In = ParameterLocation.Header, // Ubicación del token
        Description = "Introduce tu token JWT en este formato: **Bearer {token}**"
    });

    // Requisito de seguridad para que Swagger lo use en las peticiones
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
            Array.Empty<string>() // No se especifican scopes
        }
    });
});

// ? Agrega servicios de controladores
builder.Services.AddControllers();

// ? Habilita la exploración de endpoints para Swagger
builder.Services.AddEndpointsApiExplorer();

// ? Configura el contexto de base de datos con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Registro de servicios personalizados en el contenedor de dependencias
builder.Services.AddTransient<IUserServices, UserServices>(); // Servicio de usuarios
builder.Services.AddTransient<IRolServices, RolServices>();   // Servicio de roles

// ? Construye la aplicación
var app = builder.Build();

// ? Middleware de desarrollo: activa Swagger si el entorno es de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Genera documentación JSON de la API
    app.UseSwaggerUI();    // Interfaz gráfica para probar endpoints
}

app.UseHttpsRedirection();   // Redirección automática de HTTP a HTTPS

app.UseAuthentication();     // Habilita el middleware de autenticación (¡debe ir antes de Authorization!)
app.UseAuthorization();      // Habilita la autorización para los endpoints

app.MapControllers();        // Mapea los controladores a las rutas configuradas

app.Run();                   // Ejecuta la aplicación

// Autor: Gonzalez Madrigal Jonathan Arturo - Grupo: 29AV
