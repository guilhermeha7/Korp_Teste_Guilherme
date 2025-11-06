namespace EstoqueAPI.Entities
{
    public class Produto
    {
        public int Codigo { get; set; }
        public string Descricao { get; set; }
        public decimal Saldo { get; set; } //quantidade disponível em estoque
    }
}
