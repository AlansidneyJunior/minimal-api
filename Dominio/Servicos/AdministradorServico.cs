using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores
                .FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int pagina = 1, string? email = null)
        {
            var query = _contexto.Administradores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(a => a.Email.ToLower().Contains(email.ToLower()));

            return query
                .OrderBy(a => a.Id)
                .Skip((pagina - 1) * 10)
                .Take(10)
                .ToList();
        }

        public Administrador? BuscarPorId(int id)
        {
            return _contexto.Administradores.FirstOrDefault(a => a.Id == id);
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public void Atualizar(Administrador administrador)
        {
            _contexto.Administradores.Update(administrador);
            _contexto.SaveChanges();
        }

        public void Remover(Administrador administrador)
        {
            _contexto.Administradores.Remove(administrador);
            _contexto.SaveChanges();
        }
    }
}
