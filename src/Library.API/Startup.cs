using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Library.API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc((opt)=>
            {
                opt.ReturnHttpNotAcceptable = true;
                opt.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
                opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
              
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler((appBuilder)=>
                {
                    appBuilder.Run(async (context) =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An Unexpected fault has occured at the server.Please try again after sometime.");
                    });
                });
            }

            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();

            AutoMapper.Mapper.Initialize((config) =>
            {
                config.CreateMap<Entities.Author, DTO.AuthorDTO>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(x => $"{x.FirstName} {x.LastName}"))
                .ForMember(dest => dest.Age, opts => opts.MapFrom(x => x.DateOfBirth.GetCurrentAge()));

                config.CreateMap<Entities.Book, DTO.BookDTO>();

                config.CreateMap<DTO.AuthorForCreationDTO, Entities.Author>();

                config.CreateMap<DTO.BookForCreationDTO, Entities.Book>();

                config.CreateMap<DTO.BookForUpdateDTO, Entities.Book>();

                config.CreateMap< Entities.Book, DTO.BookForUpdateDTO>();
            });
        }
    }
}
