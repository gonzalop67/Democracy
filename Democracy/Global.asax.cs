using Democracy.Migrations;
using Democracy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Democracy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DemocracyContext, Configuration>());
            this.CheckSuperuser();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void CheckSuperuser()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CkeckRole("Admin", userContext);
            this.CkeckRole("User", userContext);

            var user = db.Users.Where(u => u.UserName.ToLower().Equals("gonzalop67@gmail.com")).FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Address = "Joaquín Sumaita N47-354 y Sebastián Arias E10-13",
                    FirstName = "Gonzalo",
                    LastName = "Peñaherrera",
                    Phone = "0984893415",
                    UserName = "gonzalop67@gmail.com",
                    Photo = "~/Content/Photos/gonzalofoto.jpg"
                };

                db.Users.Add(user);
                db.SaveChanges();
            }

            var userASP = userManager.FindByName(user.UserName);

            if (userASP == null)
            {
                // Create the ASP NET User
                userASP = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.UserName,
                    PhoneNumber = user.Phone,
                };
                userManager.Create(userASP, "Gp67M24$");
            }

            userManager.AddToRole(userASP.Id, "Admin");
        }

        private void CkeckRole(string roleName, ApplicationDbContext userContext)
        {
            // User management

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }


        }
    }
}
