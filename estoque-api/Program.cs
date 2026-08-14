using estoque_api.Data;
using estoque_api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors("PermitirAngular");

app.MapGet("/produtos", async (EstoqueDbContext db) =>
{
    return await db.Produtos.ToListAsync();
});

app.MapPost("/produtos", async (Produto produto, EstoqueDbContext db) =>
{
    var produtoExistente = await db.Produtos
        .FirstOrDefaultAsync(p => p.Codigo == produto.Codigo);

    if (produtoExistente is not null)
    {
        return Results.BadRequest("Já existe um produto com esse código.");
    }

    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.Codigo}", produto);
});

app.MapPut("/produtos/{codigo}/baixar-estoque", async (
    string codigo,
    int quantidade,
    EstoqueDbContext db) =>
{
    var produto = await db.Produtos
        .FirstOrDefaultAsync(p => p.Codigo == codigo);

    if (produto is null)
    {
        return Results.NotFound("Produto não encontrado.");
    }

    if (quantidade <= 0)
    {
        return Results.BadRequest("A quantidade deve ser maior que zero.");
    }

    if (produto.Saldo < quantidade)
    {
        return Results.BadRequest("Estoque insuficiente.");
    }

    produto.Saldo -= quantidade;

    await db.SaveChangesAsync();

    return Results.Ok(produto);
});
app.Run();