using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>();
                configuration.NotificationPublisher = new TaskWhenAllPublisher();
            });

            return services;
        }
    }
}
