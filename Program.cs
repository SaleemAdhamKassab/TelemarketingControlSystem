using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Models.Data;
using TelemarketingControlSystem.Services.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.OpenApi.Models;
using TelemarketingControlSystem.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TelemarketingControlSystem.Services.NotificationHub;
using TelemarketingControlSystem.Services.ProjectService;
using TelemarketingControlSystem.Services.ProjectStatisticService;
using TelemarketingControlSystem.Services.SegmentService;
using TelemarketingControlSystem.Services.ProjectEvaluationService;
using TelemarketingControlSystem.Services.MistakeReportService;
using TelemarketingControlSystem.Services.ExcelService;
using TelemarketingControlSystem.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Sql server Db Connections
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectStatisticsService, ProjectStatisticService>();
builder.Services.AddScoped<ISegmentsService, SegmentService>();
builder.Services.AddScoped<IProjectsEvaluationService, ProjectsEvaluationService>();
builder.Services.AddScoped<IMistakeReportService, MistakeReportService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IWindowsAuthService, WindowsAuthService>();
builder.Services.AddScoped<IHubService, HubService>();


//Add Windows Auth
builder.Services.AddSwaggerGen(setup =>
{

	// Include 'SecurityScheme' to use JWT Authentication
	var jwtSecurityScheme = new OpenApiSecurityScheme
	{
		BearerFormat = "JWT",
		Name = "JWT Authentication",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = JwtBearerDefaults.AuthenticationScheme,
		Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

		Reference = new OpenApiReference
		{
			Id = JwtBearerDefaults.AuthenticationScheme,
			Type = ReferenceType.SecurityScheme
		}
	};

	setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

	setup.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ jwtSecurityScheme, Array.Empty<string>() }
	});

	//setup.OperationFilter<SwaggerTenantParam>();

});
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);


var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
	builder.AllowAnyMethod()
			.AllowAnyHeader().WithExposedHeaders("X-Content-Type-Options")
            .SetIsOriginAllowed(origin => origins.Contains("all") || origins
			.Select(x => x.ToLower()).Contains(origin.ToLower())) // allow any origin you can change here to allow localhost:4200
			.AllowCredentials();
}));

var app = builder.Build();

// Register Audit Middleware
app.UseMiddleware<RequestAuditMiddleware>();

//add anti-clickjacking header (security issues)
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'none';");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    await next();
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotifiyHub>("/Notify");

app.MapControllers();

app.Run();