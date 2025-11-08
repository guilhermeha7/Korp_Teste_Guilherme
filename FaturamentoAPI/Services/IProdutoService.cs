using FaturamentoAPI.DTOs;

namespace FaturamentoAPI.Services
{
    public interface IProdutoService
    {
        Task<ProdutoDto?> GetByCodigoAsync(int codigo);
    }
}
