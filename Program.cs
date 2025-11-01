using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

// Swagger (compatível com .NET 8)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<DbContexto>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Home
app.MapGet("/", () => Results.Json(new Home()));
#endregion

#region Administradores
app.MapPost("administrador/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login realizado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
});
#endregion

#region Veiculos
app.MapGet("veiculos", (
    int pagina,
    string? nome,
    string? marca,
    [FromServices] IVeiculoServico veiculoServico
) =>
{
    var veiculos = veiculoServico.Todos(pagina, nome, marca);
    return Results.Ok(veiculos);
});

app.MapGet("veiculos/{id}", (int id,[FromServices] IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    return veiculo is not null ? Results.Ok(veiculo) : Results.NotFound();
});

app.MapPost("veiculos", (VeiculoDTO dto,[FromServices] IVeiculoServico veiculoServico) =>
{
    var veiculo = new Veiculo
    {
        Nome = dto.Nome,
        Marca = dto.Marca,
        Ano = dto.Ano
    };

    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
});

app.MapPut("veiculos/{id}", (int id, VeiculoDTO dto,[FromServices] IVeiculoServico veiculoServico) =>
{
    var existente = veiculoServico.BuscarPorId(id);
    if (existente is null)
        return Results.NotFound();

    existente.Nome = dto.Nome;
    existente.Marca = dto.Marca;
    existente.Ano = dto.Ano;

    veiculoServico.Atualizar(existente);
    return Results.Ok(existente);
});

app.MapDelete("veiculos/{id}", (int id,[FromServices] IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo is null)
        return Results.NotFound();

    veiculoServico.Remover(veiculo);
    return Results.NoContent();
});
#endregion

app.Run();
