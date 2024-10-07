using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinhaAPI.Models;

namespace MinhaAPI.Contexto
{
    //public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)   TIREI ESSA PARTE PQ N SEI SE ELA É UTILIZADA MESMO
    public class AppDbContext : IdentityDbContext <ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { }
        public DbSet<Categoria>? Categorias { get; set; }
        public DbSet<Produto>? Produtos { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
