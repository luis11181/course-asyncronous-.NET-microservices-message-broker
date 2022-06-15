using System.Reflection;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
  public static class Extensions
  {
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
    {
      services.AddMassTransit(
     configure =>
     {


       //registra los consumidores de este broker
       configure.AddConsumers(Assembly.GetEntryAssembly());


       configure.UsingRabbitMq((context, configurator) =>
       {

         var Configuration = context.GetService<IConfiguration>();

         var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
         // trae el host de rabbitmq de la configuracion
         var rabbitMQSettings = Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
         // configura el host de rabbitmq
         configurator.Host(rabbitMQSettings.Host);
         // configura el endpoint de rabbitmq
         configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
       });
     });

      // starts now the rabbitMq process
      services.AddMassTransitHostedService();

      return services;
    }
  }
}