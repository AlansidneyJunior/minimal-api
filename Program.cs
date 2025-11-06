using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<DbContexto>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();



#region Home
app.MapGet("/", () => Results.Json(new Home()));
#endregion

#region Administradores
app.MapPost("administrador/login", (
    [FromBody] LoginDTO loginDTO,
    [FromServices] IAdministradorServico administradorServico,
    IConfiguration config
) =>
{
    var admin = administradorServico.Login(loginDTO);

    if (admin is null)
        return Results.Unauthorized();

    // 1️⃣ Chave secreta (a mesma que está no appsettings.json)
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // 2️⃣ Claims (informações do usuário no token)
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, admin.Email),
        new Claim("perfil", admin.Perfil),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    // 3️⃣ Criação do token
    var token = new JwtSecurityToken(
        expires: DateTime.Now.AddHours(2),
        signingCredentials: creds,
        claims: claims
    );

    // 4️⃣ Serializa o token (vira uma string)
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    // 5️⃣ Retorna o token
    return Results.Ok(new { token = tokenString });
});

app.MapGet("administradores", [Authorize](
    int pagina,
    string? email,
    [FromServices] IAdministradorServico administradorServico
) =>
{
    var administradores = administradorServico.Todos(pagina, email);
    return Results.Ok(administradores);
});

app.MapGet("administradores/{id}", [Authorize] (int id, [FromServices] IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    return administrador is not null ? Results.Ok(administrador) : Results.NotFound();
});

app.MapPost("administradores", [Authorize]( AdministradorDTO dto, [FromServices] IAdministradorServico administradorServico) =>
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

app.MapPut("administradores/{id}", [Authorize] (int id, AdministradorDTO dto, [FromServices] IAdministradorServico administradorServico) =>
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

app.MapDelete("administradores/{id}", [Authorize] (int id, [FromServices] IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador is null)
        return Results.NotFound();

    administradorServico.Remover(administrador);
    return Results.NoContent();
});
#endregion


#region Veiculos
app.MapGet("veiculos", [Authorize] (
    int pagina,
    string? nome,
    string? marca,
    [FromServices] IVeiculoServico veiculoServico
) =>
{
    var veiculos = veiculoServico.Todos(pagina, nome, marca);
    return Results.Ok(veiculos);
});

app.MapGet("veiculos/{id}", [Authorize] (int id,[FromServices] IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    return veiculo is not null ? Results.Ok(veiculo) : Results.NotFound();
});

app.MapPost("veiculos", [Authorize] (VeiculoDTO dto,[FromServices] IVeiculoServico veiculoServico) =>
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

app.MapPut("veiculos/{id}", [Authorize] (int id, VeiculoDTO dto,[FromServices] IVeiculoServico veiculoServico) =>
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

app.MapDelete("veiculos/{id}", [Authorize] (int id,[FromServices] IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo is null)
        return Results.NotFound();

    veiculoServico.Remover(veiculo);
    return Results.NoContent();
});
#endregion

app.Run();
