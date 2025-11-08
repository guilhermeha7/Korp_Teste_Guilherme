using FaturamentoAPI.Models;

namespace FaturamentoAPI.DTOs
{
    public class NotaFiscalInputDto
    {
        public List<ItemNotaFiscalInputDto> Itens { get; set; } = new List<ItemNotaFiscalInputDto>();
    }
}
