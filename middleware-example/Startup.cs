using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace middleware_example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseAuthentication();
            // app.UseCors();
            // app.UseStaticFiles();

            // nasz custom middleware
            app.Use(async (context, next) =>
            {
                // dopoki nie wywolamy await next() mamy logike przetwarzajaca request
                await context.Response.WriteAsync("<h1>MID 1</h1>");
                await next(); // kiedy nie ma next nigdy nie przejdziemy do kolejnego middleware

                // po wywolaniu await next() mamy logike przetwarzajaca response


            });

            // inna metoda dzieki ktorej zdefiniujemy middleware to Run
            // dziala tak samo jak Use tylko ze bez next()
            // pelni wiec role middleware koncowego po ktorym juz nic nie ma wiec
            // powienien zostac wywolywany jako ostatni
            // gdybys go umiescil w srodku middlewares po nim sie nie wykonaja
            /* app.Run(async context =>
            {
                await context.Response.WriteAsync("<h1>RUN</h1>");
            }); */

            // kolejna metoda to Map
            // pozwala wykonac odpowienie middlewares dla konkretnych zadan
            // czyli wykona sie wszystko do metody Map dla zadania req a potem juz tylko to co tutaj
            // bo w srodku masz Run i dalej sie nie wykona
            app.Map("/req", action =>
            {
                action.Use(async (context, next) =>
                {
                    await context.Response.WriteAsync("<h1>MAP 1</h1>");
                    await next();
                });
                action.Run(async context => await context.Response.WriteAsync("<h1>MAP 2</h1>"));
            });

            // mamy jeszcze MapWhen, ktore pozwala nam jeszcze dokladniej okreslic warunki
            // dla ktorych ma sie wykonac dany pipeline
            app.MapWhen(
                context => context.Request.Query.ContainsKey("param"),
                action =>
                {
                    action.Run(async context =>
                    {
                        var paramName = context.Request.Query["param"];
                        await context.Response.WriteAsync($"Param = {paramName}");
                    });
                }
                );

            app.Use(async (context, next) =>
            {
                // dopoki nie wywolamy await next() mamy logike przetwarzajaca request
                await context.Response.WriteAsync("<h1>MID 2</h1>");
                await next();

                // po wywolaniu await next() mamy logike przetwarzajaca response


            });

            app.UseMvc();
        }
    }
}
