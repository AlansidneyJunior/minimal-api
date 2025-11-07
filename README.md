# 🚀 Minimal API com JWT e PostgreSQL

API desenvolvida em **.NET 9** usando **Minimal APIs**, **Entity Framework Core**, **PostgreSQL** e **autenticação JWT**.

---

## 🧩 Tecnologias

- .NET 9  
- Entity Framework Core  
- PostgreSQL  
- JWT (JSON Web Token)  
- Swagger  

---

## ⚙️ Configuração

Edite o arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=minimal_api;Username=postgres;Password=1234"
  },
  "Jwt": {
    "Key": "sua_chave_secreta_aqui"
  }
}
```
## 🧠 JWT em Resumo

O usuário faz login e recebe um token JWT.

O token é enviado no cabeçalho HTTP:

Authorization: Bearer <seu_token>

O servidor valida o token antes de permitir acesso às rotas protegidas.

## 🧪 Como Executar

1️⃣ Restaurar dependências
```
dotnet restore

2️⃣ Aplicar migrações (se estiver usando EF Migrations)
dotnet ef database update

3️⃣ Executar com hot reload
dotnet watch run
```

Acesse o Swagger:
👉 https://localhost:7043/swagger

### 🔹 Endpoints
🔑 Autenticação
POST /administrador/login → Gera token JWT

👤 Administradores
GET /administradores
GET /administradores/{id}
POST /administradores
PUT /administradores/{id}
DELETE /administradores/{id}

🚗 Veículos
GET /veiculos
GET /veiculos/{id}
POST /veiculos
PUT /veiculos/{id}
DELETE /veiculos/{id}
Conversa com o Gemini
Transforme o seguinte texto em markdown para um readme

### 🔒 Exemplo de Endpoint Protegido
```c#
app.MapGet("/veiculos", [Authorize] async (IVeiculoServico servico) =>

    Results.Ok(await servico.ListarAsync()));
```
### 🗂️ Estrutura do Projeto
```
minimal-api/
├── Dominio/
│   ├── DTOs/
│   ├── Entidades/
│   ├── Interfaces/
│   ├── ModelViews/
│   └── Servicos/
├── Infraestrutura/
│   └── Db/
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── Program.cs
└── README.md
```

### 📚 Autor
**Alansidney Silva**
💻 Desenvolvedor em formação — foco em backend e APIs com boas práticas.


