using Microsoft.EntityFrameworkCore;
using estoque_api.Models;

namespace estoque_api.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }
}