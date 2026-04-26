using ApplePaySimulation.Database.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApplePaySimulation.Database.Context
{
    public class ApplePaySimulationContext: IdentityDbContext<User>
    {
        public ApplePaySimulationContext() { }
        public ApplePaySimulationContext(DbContextOptions<ApplePaySimulationContext> options): base(options)
        {}
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<CreditCard> CreditCard { get; set; }
      
    }
}
