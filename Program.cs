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
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

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

app.MapGet("administradores", (
    int pagina,
    string? email,
    [FromServices] IAdministradorServico administradorServico
) =>
{
    var administradores = administradorServico.Todos(pagina, email);
    return Results.Ok(administradores);
});

app.MapGet("administradores/{id}", (int id, [FromServices] IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    return administrador is not null ? Results.Ok(administrador) : Results.NotFound();
});

app.MapPost("administradores", (AdministradorDTO dto, [FromServices] IAdministradorServico administradorServico) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Senha) ||
        string.IsNullOrWhiteSpace(dto.Perfil))
    {
        return Results.BadRequest("Todos os campos (Email, Senha e Perfil) são obrigatórios.");
    }

    var administrador = new Administrador
    {
        Email = dto.Email,
        Senha = dto.Senha,
        Perfil = dto.Perfil
    };

    administradorServico.Incluir(administrador);
    return Results.Created($"/administradores/{administrador.Id}", administrador);
});

app.MapPut("administradores/{id}", (int id, AdministradorDTO dto, [FromServices] IAdministradorServico administradorServico) =>
{
    var existente = administradorServico.BuscarPorId(id);
    if (existente is null)
        return Results.NotFound();

    existente.Email = dto.Email;
    existente.Senha = dto.Senha;
    existente.Perfil = dto.Perfil;

    administradorServico.Atualizar(existente);
    return Results.Ok(existente);
});

app.MapDelete("administradores/{id}", (int id, [FromServices] IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador is null)
        return Results.NotFound();

    administradorServico.Remover(administrador);
    return Results.NoContent();
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
    if (string.IsNullOrWhiteSpace(dto.Nome) ||
        string.IsNullOrWhiteSpace(dto.Marca) ||
        dto.Ano <= 0)
    {
        return Results.BadRequest("Todos os campos (Nome, Marca e Ano) são obrigatórios.");
    }

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
