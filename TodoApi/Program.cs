using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Coolify inyecta estas variables de entorno
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "db";
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "todos";
var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

builder.Services.AddDbContext<TodoDb>(o => o.UseNpgsql(connectionString));
builder.Services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres");
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

// Crea la tabla al arrancar (demo, sin migraciones)
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<TodoDb>();
    ctx.Database.EnsureCreated();
}

app.MapHealthChecks("/health");

app.MapGet("/api/todos", async (TodoDb ctx) =>
    await ctx.Todos.OrderBy(t => t.Id).ToListAsync());

app.MapGet("/api/todos/{id:int}", async (int id, TodoDb ctx) =>
    await ctx.Todos.FindAsync(id) is Todo t ? Results.Ok(t) : Results.NotFound());

app.MapPost("/api/todos", async (Todo todo, TodoDb ctx) =>
{
    ctx.Todos.Add(todo);
    await ctx.SaveChangesAsync();
    return Results.Created($"/api/todos/{todo.Id}", todo);
});

app.MapPut("/api/todos/{id:int}", async (int id, Todo input, TodoDb ctx) =>
{
    var todo = await ctx.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = input.Title;
    todo.IsDone = input.IsDone;
    await ctx.SaveChangesAsync();
    return Results.Ok(todo);
});

app.MapDelete("/api/todos/{id:int}", async (int id, TodoDb ctx) =>
{
    var todo = await ctx.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    ctx.Todos.Remove(todo);
    await ctx.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
}

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();
}
