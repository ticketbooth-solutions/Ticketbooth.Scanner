using Easy.MessageHub;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartContract.Essentials.Ciphering;
using System.Reflection;
using Ticketbooth.Scanner.Application.Background;
using Ticketbooth.Scanner.Application.Messaging;
using Ticketbooth.Scanner.Application.Services;
using Ticketbooth.Scanner.Domain.Data;
using Ticketbooth.Scanner.Domain.Interfaces;
using Ticketbooth.Scanner.Infrastructure;
using Ticketbooth.Scanner.Infrastructure.Data;
using Ticketbooth.Scanner.Infrastructure.Services;
using Ticketbooth.Scanner.ViewModels;

namespace Ticketbooth.Scanner
{
    public class Startup
    {
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<NodeOptions>(Configuration.GetSection("FullNode"));

            // infrastructure
            services.AddSingleton<IBlockStoreService, BlockStoreService>();
            services.AddSingleton<INodeService, NodeService>();
            services.AddSingleton<ISmartContractService, SmartContractService>();
            services.AddSingleton<ITicketRepository, TicketRepository>();
            services.AddTransient<IQrCodeScanner, QrCodeScanner>();

            // node
            services.AddSingleton<IHealthMonitor, HealthMonitor>();
            services.AddHostedService(services => (HealthMonitor)services.GetRequiredService<IHealthMonitor>());
            services.AddSingleton<IHealthChecker, HealthChecker>();
            services.AddSingleton<INetworkResolver, NetworkResolver>();

            // application
            services.AddSingleton<ITicketChecker, TicketChecker>();
            services.AddSingleton<IMessageHub, MessageHub>();
            services.AddSingleton<ICipherFactory, AesCipherFactory>();
            services.AddMediatR(config => config.Using<ParallelMediator>().AsSingleton(), Assembly.GetExecutingAssembly());
            services.AddTransient<IQrCodeValidator, QrCodeValidator>();

            // presentation
            services.AddTransient<DetailsViewModel>();
            services.AddTransient<IndexViewModel>();
            services.AddTransient<NodeViewModel>();
            services.AddTransient<ScanViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.ConfigureOptions<JsonOptionsConfiguration>();
            services.AddRazorPages().AddNewtonsoftJson();

            // blazor server configures static file middleware, so have to do this in container config
            var fileExtensionProvider = new FileExtensionContentTypeProvider();
            fileExtensionProvider.Mappings.Add(".webmanifest", "application/manifest+json");
            services.Configure<StaticFileOptions>(config =>
            {
                config.ContentTypeProvider = fileExtensionProvider;
            });

            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                if (_environment.IsDevelopment())
                {
                    o.DetailedErrors = true;
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
