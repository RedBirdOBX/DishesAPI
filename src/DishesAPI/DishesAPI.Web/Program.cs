using DishesAPI.DbContexts;
using DishesAPI.Web.Extensions;
using DishesAPI.Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

// To get some structure into the response, we want to use the problem details format.
// To enable that, we call to builder.Services.AddProblemDetails.
// This will ensure that the responses are formatted according to the
// problem details standard. https://datatracker.ietf.org/doc/html/rfc9457
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MustBeAdmin", policy =>
    {
        policy.RequireAuthenticatedUser()
                .RequireRole("Admin")
                .RequireClaim("Country", "USA");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStatusCodePages();

// app.UseAuthentication() / UseAuthorization().
// You typically do that pretty high up
// in your request pipeline because everything that requires
// an authentication check should come after that.
// This is not strictly necessary for minimal APIs. 
// When we added AddAuthentication() / AddAuthorization(), the web application builder
// will automatically add the middleware to the request pipeline.
// However, writing it as we did has the advantage of allowing
// us to choose exactly where the middleware is added to the request pipeline,
// and it also has the advantage of being explicit.
// Whether or not you want to do that is totally up to you. 
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}
else
{
    // this is enabled by **default** in development environment and not really needed here
    app.UseDeveloperExceptionPage();
}

// Error routes
app.MapGet("/error", () => {throw new NotImplementedException();});

// registering the endpoints
app.RegisterDishesEndpoints();
app.RegisterIngredientsEndpoints();

// recreate & migrate the database on startup
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
}

app.Run();
