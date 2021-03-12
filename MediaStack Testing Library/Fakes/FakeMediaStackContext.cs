using MediaStack_Library.Data_Access_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediaStack_Testing_Library.Fakes
{
    public class FakeMediaStackContext : MediaStackContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                optionsBuilder.UseInMemoryDatabase("FakeMediaStackContext")
                    .UseInternalServiceProvider(serviceProvider);
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
