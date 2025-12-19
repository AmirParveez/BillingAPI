using ApiBilling.Helpers;
using ApiBilling.BLL;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SqlHelper DI
builder.Services.AddScoped<SqlHelper>(sp =>
{
    var conn = builder.Configuration.GetConnectionString("Default");
    return new SqlHelper(conn!);
});

// BLL DI
builder.Services.AddScoped<InvoiceBLL>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
