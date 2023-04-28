


using ApiGoldenstarServices.Data;
using ApiGoldenstarServices.Data.DataAccess.Auth;
using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using ApiGoldenstarServices.Data.DataAccess.Roltec;
using ApiGoldenstarServices.Data.Utils;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using ApiGoldenstarServices.HttpServices.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Sql connection 
var SqlConnectionConfiguration = new SqlConfiguration(builder.Configuration.GetConnectionString("SQLServerConnection"));
builder.Services.AddSingleton(SqlConnectionConfiguration);

//builder.Services.AddScoped(IConfiguration, ConfigurationManager);
//.Get<List<ExpandoObject>>();
var HashKey = new GetValuesFromEnvFile(builder.Configuration.GetSection("ConfigEnv").Get<List<string>>());
builder.Services.AddSingleton(HashKey);

//Global Connections services to db
builder.Services.AddScoped<IUser, DAUser>();
builder.Services.AddScoped<ICustomer, DACustomer>();
builder.Services.AddScoped<IOrder, DAOrder>();


//Global connections to API Services
builder.Services.AddScoped<IRoltecApi, HttpRoltecServicesAPI>();
builder.Services.AddScoped<ITokenServices, HttpTokenServices>();

// Culture info settings
var cultureInfo = new System.Globalization.CultureInfo("es-MX");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

//Settings
builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Goldenstar Services Gateway", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("SecretKey"))),
        ClockSkew = TimeSpan.Zero
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//}

//CORS configuration
app.UseCors(options =>
{
    options.WithOrigins(builder.Configuration.GetValue<string>("RoltecAPI"));
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});


//swagger config
app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Goldenstar v1")

    );

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.Run();
