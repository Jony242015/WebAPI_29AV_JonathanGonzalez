// Importa los paquetes necesarios para trabajar con Entity Framework Core y los servicios de la aplicación
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;                      // Contexto de la base de datos
using WebAPI.Services.IServices;          // Interfaces de servicios
using WebAPI.Services.Services;           // Implementaciones de los servicios

// Crea el constructor de la aplicación web (WebApplication)
var builder = WebApplication.CreateBuilder(args);

// Agrega los controladores al proyecto (manejan las peticiones HTTP)
builder.Services.AddControllers();

// Configura Swagger para documentar y probar la API (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer(); // Habilita la exploración de endpoints
builder.Services.AddSwaggerGen();          // Genera la documentación de la API

// Configura el contexto de base de datos con SQL Server usando una cadena de conexión definida en appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Inyección de dependencias: registra los servicios de usuarios y roles con una vida útil transitoria (se crea una instancia nueva cada vez que se solicite)
builder.Services.AddTransient<IUserServices, UserServices>();
builder.Services.AddTransient<IRolServices, RolServices>();

// Construye la aplicación
var app = builder.Build();

// Configura el middleware del pipeline de peticiones HTTP

// Si está en entorno de desarrollo, se habilita Swagger para probar la API desde el navegador
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // Habilita Swagger
    app.UseSwaggerUI();     // Habilita la interfaz de usuario de Swagger
}

app.UseHttpsRedirection();  // Redirige todas las peticiones HTTP a HTTPS

app.UseAuthorization();     // Habilita la autorización (middleware para validar accesos)

// Mapea los controladores para que puedan responder a las rutas correspondientes
app.MapControllers();

// Ejecuta la aplicación
app.Run();

// Autor: Gonzalez Madrigal Jonathan Arturo - Grupo: 29AV
