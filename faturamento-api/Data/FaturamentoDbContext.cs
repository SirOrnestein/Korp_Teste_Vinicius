using faturamento_api.Models;
using Microsoft.EntityFrameworkCore;

namespace faturamento_api.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(
        DbContextOptions<FaturamentoDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNota> ItensNota { get; set; }
}