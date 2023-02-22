


using ApiGoldenstarServices.Data;
using ApiGoldenstarServices.Data.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Sql connection 
var SqlConnectionConfiguration = new SqlConfiguration(builder.Configuration.GetConnectionString("SQLServerConnection"));
builder.Services.AddSingleton(SqlConnectionConfiguration);

//Global Connections services
builder.Services.AddScoped<ICustomer, DACustomer>();

// Culture info settings
var cultureInfo = new System.Globalization.CultureInfo("es-MX");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

//Settings
builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//}
//swagger config
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Goldenstar v1"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
