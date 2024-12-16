using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Model.Custom;
using Repository;
using Model;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRegistroService, RegistroService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IStudioService, StudioService>();
builder.Services.AddDbContext<DbContextClass>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//configuracion de la cadena de conexion


//configuracion de encriptacion
builder.Services.AddSingleton<Usefulness>();

builder.Services.AddAuthentication(config =>
{
	//configuracion por medio de JWT
	config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(config =>
{
	config.RequireHttpsMetadata = false;
	config.SaveToken = true;
	config.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		//validar que las app externas puedan utilizar nuestra url y quien
		ValidateIssuer = false,
		//podemos poner servidor o dominio
		ValidateAudience = false,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero,
		IssuerSigningKey = new SymmetricSecurityKey
		(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"]!))
	};
});
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", app =>
	{
		app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
	});
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
