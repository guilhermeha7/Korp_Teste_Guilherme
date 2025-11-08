using FaturamentoAPI.DTOs;

namespace FaturamentoAPI.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly HttpClient _http;
        public ProdutoService(HttpClient http) => _http = http;

        public async Task<ProdutoDto?> GetByCodigoAsync(int codigo)
        {
            var resp = await _http.GetAsync($"/api/produtos/{codigo}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProdutoDto>();
        }
    }
}
