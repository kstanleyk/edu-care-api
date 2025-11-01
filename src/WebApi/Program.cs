using EduCare.Application;
using EduCare.CoreApi;
using EduCare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var host = builder.WebHost;

const string allowedCorsOrigins = nameof(allowedCorsOrigins);

services.AddApiServices(configuration, host, allowedCorsOrigins);
services.AddApplicationDependencies();
services.AddInfrastructureDependencies(configuration);
//services.AddJwtAuthentication(configuration);
services.AddVersioning();
services.AddLocalizationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.SeedDatabase();

app.UseLocalizationServices();

app.UseCors(allowedCorsOrigins);

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();

app.Run();