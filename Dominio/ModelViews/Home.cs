namespace minimal_api.Dominio.ModelViews;

public struct Home
{
    public string Documentacao { get => "/swagger"; }
    public string Mensagem { get => "Bem Vindo a API de Veiculos"; }
}