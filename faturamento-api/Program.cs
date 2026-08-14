
using faturamento_api.Data;
using faturamento_api.Models;
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

builder.Services.AddDbContext<FaturamentoDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("PermitirAngular");

app.MapPost("/notas", async (
    NotaFiscal nota,
    FaturamentoDbContext db,
    IHttpClientFactory httpClientFactory) =>
{
    nota.Status = "Aberta";

    if (nota.Data == default)
    {
        nota.Data = DateTime.UtcNow;
    }

    if (nota.Numero == 0)
    {
        var ultimaNota = await db.NotasFiscais
            .OrderByDescending(n => n.Numero)
            .FirstOrDefaultAsync();

        nota.Numero = ultimaNota is null
            ? 1
            : ultimaNota.Numero + 1;
    }

    db.NotasFiscais.Add(nota);

    await db.SaveChangesAsync();

    return Results.Created($"/notas/{nota.Id}", nota);
});

app.MapGet("/notas", async (FaturamentoDbContext db) =>
{
    return await db.NotasFiscais
        .Include(n => n.Itens)
        .ToListAsync();
});

app.MapGet("/notas/{id}", async (
    int id,
    FaturamentoDbContext db) =>
{
    var nota = await db.NotasFiscais
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota is null)
    {
        return Results.NotFound("Nota fiscal não encontrada.");
    }

    return Results.Ok(nota);
});

app.MapPost("/notas/{id}/imprimir", async (
    int id,
    FaturamentoDbContext db,
    IHttpClientFactory httpClientFactory) =>
{
    var nota = await db.NotasFiscais
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota is null)
    {
        return Results.NotFound("Nota fiscal não encontrada.");
    }

    if (nota.Status != "Aberta")
    {
        return Results.BadRequest("A nota fiscal já está fechada.");
    }

    var httpClient = httpClientFactory.CreateClient();

    foreach (var item in nota.Itens)
    {
        var endereco =
            $"http://localhost:5255/produtos/{item.CodigoProduto}/baixar-estoque?quantidade={item.Quantidade}";

     try
{
    var resposta = await httpClient.PutAsync(endereco, null);

    if (!resposta.IsSuccessStatusCode)
    {
        var erro = await resposta.Content.ReadAsStringAsync();

        return Results.BadRequest(
            $"Não foi possível baixar o estoque do produto {item.CodigoProduto}: {erro}"
        );
    }
}
catch (HttpRequestException)
{
    return Results.Problem(
        "Serviço de estoque indisponível. Tente novamente mais tarde.",
        statusCode: 503
    );
}
    }

    nota.Status = "Fechada";

    await db.SaveChangesAsync();

    return Results.Ok(nota);
});

app.Run();