using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        List<Administrador> Todos(int pagina = 1, string? email = null);
        Administrador? BuscarPorId(int id);
        void Incluir(Administrador administrador);
        void Atualizar(Administrador administrador);
        void Remover(Administrador administrador);
    }
}
