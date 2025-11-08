using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FaturamentoAPI.Models
{
    public class ItemNotaFiscal
    {
        public int Id { get; set; }

        [Required]
        public int ProdutoCodigo { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [ForeignKey("NotaFiscal")]
        public int NotaFiscalId { get; set; }

        [JsonIgnore]
        public NotaFiscal NotaFiscal { get; set; } //propriedade de navegação
    }
}
