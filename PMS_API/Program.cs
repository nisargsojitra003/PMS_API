using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PMS_API_BAL.Interfaces;
using PMS_API_BAL.Services;
using PMS_API_DAL.DataContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("PMS_API_Connection_String")));
builder.Services.AddScoped<IProduct , ProductService>();
builder.Services.AddScoped<ICategory, CategoryService>();
builder.Services.AddScoped<IJwt, JwtServices>();
builder.Services.AddScoped<ILogin , LoginService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments")),
    RequestPath = "/UploadDocuments"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
