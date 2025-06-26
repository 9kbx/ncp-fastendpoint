using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using My.FastNCP.Domain.AggregatesModel.OrderAggregate;
using My.FastNCP.Infrastructure.EntityConfigurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using My.FastNCP.Domain.AggregatesModel.DeliverAggregate;

namespace My.FastNCP.Infrastructure
{
    public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
        : AppDbContextBase(options, mediator)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ConfigureStronglyTypedIdValueConverter(configurationBuilder);
            base.ConfigureConventions(configurationBuilder);
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<DeliverRecord> DeliverRecords => Set<DeliverRecord>();
    }
}