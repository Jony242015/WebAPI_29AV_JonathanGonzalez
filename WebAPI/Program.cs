using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Context;               // Contexto de base de datos
using WebAPI.Services.IServices;   // Interfaces de servicios
using WebAPI.Services.Services;    // Implementación de servicios

// Crear el constructor de la aplicación web
var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de controladores para manejar peticiones HTTP
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI para generar documentación y facilitar pruebas de la API
builder.Services.AddEndpointsApiExplorer(); // Habilita exploración de endpoints
builder.Services.AddSwaggerGen();           // Genera la documentación Swagger

// Configurar DbContext con SQL Server, usando la cadena de conexión definida en appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Registrar servicios para inyección de dependencias con ciclo de vida Scoped
// Scoped es recomendable para servicios que usan DbContext
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IRolServices, RolServices>();

// Configurar CORS (Cross-Origin Resource Sharing) para permitir solicitudes desde cualquier origen
// Opcional, útil si la API será consumida desde frontend en otros dominios
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración de autenticación JWT para proteger la API
// La clave y otros valores se leen desde appsettings.json
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    // Configura JWT como esquema de autenticación predeterminado
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Parámetros para validar el token JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                 // Valida el emisor del token
        ValidateAudience = true,               // Valida el destinatario del token
        ValidateLifetime = true,               // Valida que el token no haya expirado
        ValidateIssuerSigningKey = true,       // Valida la firma digital del token
        ValidIssuer = builder.Configuration["Jwt:Issuer"],     // Emisor válido
        ValidAudience = builder.Configuration["Jwt:Audience"], // Audiencia válida
        IssuerSigningKey = new SymmetricSecurityKey(key),      // Clave secreta para validar firma

        // Define el claim que se usará para el control de roles en autorización
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

// Construcción de la aplicación web
var app = builder.Build();

// Middleware para manejar peticiones HTTP

// Habilitar Swagger solo en entorno de desarrollo para facilitar pruebas
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // Activa Swagger JSON
    app.UseSwaggerUI();     // Activa la interfaz visual de Swagger
}

app.UseHttpsRedirection(); // Redirecciona peticiones HTTP a HTTPS

app.UseCors("AllowAll");   // Aplicar política CORS para permitir cualquier origen

app.UseAuthentication();   // Habilitar autenticación con JWT
app.UseAuthorization();    // Habilitar autorización basada en roles y políticas

app.MapControllers();      // Mapea las rutas a los controladores

app.Run();                 // Ejecuta la aplicación web

// Autor: Gonzalez Madrigal Jonathan Arturo - Grupo: 29AV
