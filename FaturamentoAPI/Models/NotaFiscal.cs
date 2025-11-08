using FaturamentoAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace FaturamentoAPI.Models
{
    public class NotaFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Status Status { get; set; } = Status.Aberta;

        [Required]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public List<ItemNotaFiscal> Itens { get; set; } = new List<ItemNotaFiscal>();
    }
}
