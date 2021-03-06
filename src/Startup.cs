using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Scarlet.Api;
using Scarlet.Api.Game;

using System.Diagnostics;

namespace Scarlet
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(provider => new FileCache(provider.GetRequiredService<ILogger<FileCache>>(), "cache", System.TimeSpan.FromHours(1)));

			// TODO: use ASP Net Core configuration stuff
			services.AddSingleton(new ColorsConfiguration
			{
				EE = Colors.FromFile(_configuration["ColorFiles:EE"]),
				EEU = Colors.FromFile(_configuration["ColorFiles:EEU"]),
			});

			services.AddSingleton(new Api.Game.EverybodyEdits.AuthenticationCredentials
			{
				Email = _configuration["EverybodyEdits:Login:Email"],
				Password = _configuration["EverybodyEdits:Login:Password"],
			});

			services.AddSingleton<Api.Game.EverybodyEdits.ClientProvider>();
			services.AddSingleton<IEEScarletGameApi, Api.Game.EverybodyEdits.ScarletGameApi>();

			services.AddSingleton(new Api.Game.EverybodyEditsUniverse.ClientProvider(_configuration["EverybodyEditsUniverse:GoogleLoginToken"]));
			services.AddSingleton<IEEUScarletGameApi, Api.Game.EverybodyEditsUniverse.ScarletGameApi>();

			services.AddSingleton<IEEScarletApi, ScarletApi>(provider => new ScarletApi(provider.GetRequiredService<FileCache>(), "ee", provider.GetRequiredService<IEEScarletGameApi>()));
			services.AddSingleton<IEEUScarletApi, ScarletApi>(provider => new ScarletApi(provider.GetRequiredService<FileCache>(), "eeu", provider.GetRequiredService<IEEUScarletGameApi>()));

			services.AddMvc(options =>
			{
				options.OutputFormatters.Insert(0, new RawFormatter());
			});

			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler(new ExceptionHandlerOptions
				{
					ExceptionHandler = async (httpContext) =>
					{
						httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
						httpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
						httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
						httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
					}
				});
			}

			app.UseCorsMiddleware();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}