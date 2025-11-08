using FaturamentoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoAPI.Data
{
    public class FaturamentoContext : DbContext
    {
        public FaturamentoContext(DbContextOptions<FaturamentoContext> options) : base(options)
        {

        }

        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento 1:N NotaFiscal → ItemNotaFiscal
            modelBuilder.Entity<ItemNotaFiscal>()
                .HasOne(i => i.NotaFiscal)
                .WithMany(n => n.Itens)
                .HasForeignKey(i => i.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade); // Exclui os itens da nota quando ela for deletada

            // Converte enum Status para string no banco
            modelBuilder.Entity<NotaFiscal>()
                .Property(n => n.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        }

    }
}
