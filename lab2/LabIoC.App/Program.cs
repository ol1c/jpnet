using Autofac;
using Microsoft.Extensions.Configuration;
using Autofac.Configuration;
using LabIoC.Core;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var builder = new ContainerBuilder();
builder.RegisterModule(new ConfigurationModule(config.GetSection("autofac")));
builder.RegisterType<Worker>();

builder.RegisterType<NumLogger>()
       .Named<ILogger>("numlog")
       .WithParameter("initialValue", 100)
       .SingleInstance();
builder.RegisterType<PropertyWorker>()
       .OnActivated(e => e.Instance.Logger = e.Context.ResolveNamed<ILogger>("numlog"));
builder.RegisterType<MethodWorker>()
       .OnActivated(e => e.Instance.SetLogger(e.Context.ResolveNamed<ILogger>("numlog")));

builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

builder.RegisterType<TransactionContext>()
       .As<ITransactionContext>()
       .InstancePerMatchingLifetimeScope("transaction");

builder.RegisterType<StepService>();

using var container = builder.Build();

// Rozwiązanie zależności z kontenera
var worker = container.Resolve<Worker>();
worker.Execute(2);

var propertyWorker = container.Resolve<PropertyWorker>();
propertyWorker.Work();

var methodWorker = container.Resolve<MethodWorker>();
methodWorker.Work();

using (var scope1 = container.BeginLifetimeScope())
{
    var uow1 = scope1.Resolve<IUnitOfWork>();
    Console.WriteLine($"UnitOfWork 1 ID: {uow1.Id}");
    var uow2 = scope1.Resolve<IUnitOfWork>();
    Console.WriteLine($"UnitOfWork 2 ID: {uow2.Id}");
}

using (var scope2 = container.BeginLifetimeScope())
{
    var uow2 = scope2.Resolve<IUnitOfWork>();
    Console.WriteLine($"UnitOfWork 3 ID: {uow2.Id}");
}

// var transactionContext = container.Resolve<ITransactionContext>();
using var txScope = container.BeginLifetimeScope("transaction");
var stepService = txScope.Resolve<StepService>();
Console.WriteLine($"Current Transaction ID: {stepService.GetCurrentTransaction()}");