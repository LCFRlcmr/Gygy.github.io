﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omrv.Data;
using Omrv.Data.Interfaces;
using Omrv.Data.mocks;
using Omrv.Data.Models;
using Omrv.Data.Repository;

namespace Omrv
{
	public class Startup
	{

		private IConfigurationRoot _confSting;
		public Startup(IHostingEnvironment hostEnv)
		{
			_confSting = new ConfigurationBuilder().SetBasePath(hostEnv.ContentRootPath).AddJsonFile("dbsettings.json").Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<AppDBContent>(options => options.UseSqlServer(_confSting.GetConnectionString("DefaultConnection")));
			services.AddTransient<IAllCars, CarRepository>();
			services.AddTransient<ICarsCategory, CatigoryRepository>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped(sp => ShopCart.GetCart(sp));


			services.AddMvc();
			services.AddMemoryCache();
			services.AddSession();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseDeveloperExceptionPage();
			app.UseStatusCodePages();
			app.UseStaticFiles();
			app.UseSession();
			//app.UseMvcWithDefaultRoute();
			app.UseMvc(routes =>
			{
				routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute(name: "categoryFilter", template: "Car/{action}/{category?}", defaults: new { Controller = "Car", action = "List" });
			}); 

			using (var scope = app.ApplicationServices.CreateScope())
			{ 
				AppDBContent content = scope.ServiceProvider.GetRequiredService<AppDBContent>();
				DBObjects.Initial(content);

			}
		}
	}
}
