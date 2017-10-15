using CrystalDecisions.CrystalReports.Engine;
using Democracy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Democracy.Controllers
{
    public class UsersController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult PDF()
        {
            var report = GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult XLS()
        {
            var report = GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
            return File(stream, "application/xls", "Users.xls");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DOC()
        {
            var report = GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
            return File(stream, "application/doc", "Users.doc");
        }

        private ReportClass GenerateUserReport()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = "SELECT * FROM Users ORDER BY LastName, FirstName";

            try
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var report = new ReportClass();
            report.FileName = Server.MapPath("/Reports/Users.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }

        [Authorize(Roles = "User")]
        public ActionResult MySettings()
        {
            var user = db.Users.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            
            var view = new UserSettingsView
            {
                Address = user.Address,
                FirstName = user.FirstName,
                Grade = user.Grade,
                Group = user.Group,
                LastName = user.LastName,
                Phone = user.Phone,
                Photo = user.Photo,
                UserId = user.UserId,
                UserName = user.UserName,
            };

            return View(view);
        }

        [HttpPost]
        public ActionResult MySettings(UserSettingsView view)
        {
            if (ModelState.IsValid)
            {

                // Upload image
                string path = string.Empty;
                string pic = string.Empty;

                if (view.NewPhoto != null)
                {
                    pic = Path.GetFileName(view.NewPhoto.FileName);
                    path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                    view.NewPhoto.SaveAs(path);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        view.NewPhoto.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                var user = db.Users.Find(view.UserId);

                user.Address = view.Address;
                user.FirstName = view.FirstName;
                user.Grade = view.Grade;
                user.Group = view.Group;
                user.LastName = view.LastName;
                user.Phone = view.Phone;

                if (!string.IsNullOrEmpty(pic))
                {
                    user.Photo = string.Format("~/Content/Photos/{0}", pic);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult OnOffAdmin(int id)
        {
            var user = db.Users.Find(id);
            
            if (user != null)
            {
                var userContext = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
                var userASP = userManager.FindByEmail(user.UserName);

                if (userManager.IsInRole(userASP.Id,"Admin"))
                {
                    userManager.RemoveFromRole(userASP.Id, "Admin");
                }
                else
                {
                    userManager.AddToRole(userASP.Id, "Admin");
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var users = db.Users.ToList();
            var usersView = new List<UserIndexView>();

            foreach (var user in users)
            {
                var userASP = userManager.FindByEmail(user.UserName);

                usersView.Add(new UserIndexView
                {
                    Address = user.Address,
                    Candidates = user.Candidates,
                    FirstName = user.FirstName,
                    Grade = user.Grade,
                    Group = user.Group,
                    GroupMembers = user.GroupMembers,
                    IsAdmin = userASP != null && userManager.IsInRole(userASP.Id, "Admin"),
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Photo = user.Photo,
                    UserId = user.UserId,
                    UserName = user.UserName
                });
                
            }
            return View(usersView);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            // Upload image
            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            // Save record
            var user = new User
            {
                Address = userView.Address,
                FirstName = userView.FirstName,
                Grade = userView.Grade,
                Group = userView.Group,
                LastName = userView.LastName,
                Phone = userView.Phone,
                Photo = (pic == string.Empty) ? string.Empty : string.Format("~/Content/Photos/{0}", pic),
                UserName = userView.UserName
            };

            db.Users.Add(user);

            try
            {
                db.SaveChanges();
                this.CreateASPUser(userView);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && 
                    ex.InnerException.InnerException != null && 
                    ex.InnerException.InnerException.Message.Contains("UserNameIndex"))
                {
                    ModelState.AddModelError(string.Empty, "The email has already used for another user");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(userView);
            }
            
            return RedirectToAction("Index");

        }

        private void CreateASPUser(UserView userView)
        {
            // User management
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            // Create User role
            string roleName = "User";

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            // Create the ASP NET User
            var userASP = new ApplicationUser
            {
                UserName = userView.UserName,
                Email = userView.UserName,
                PhoneNumber = userView.Phone
            };

            userManager.Create(userASP, userASP.UserName);

            // Add user to role
            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var userView = new UserView
            {
                Address = user.Address,
                FirstName = user.FirstName,
                Grade = user.Grade,
                Group = user.Group,
                LastName = user.LastName,
                Phone = user.Phone,
                UserId = user.UserId,
                UserName = user.UserName
            };

            return View(userView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            // Upload image
            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = db.Users.Find(userView.UserId);

            user.Address = userView.Address;
            user.FirstName = userView.FirstName;
            user.Grade = userView.Grade;
            user.Group = userView.Group;
            user.LastName = userView.LastName;
            user.Phone = userView.Phone;

            if (!string.IsNullOrEmpty(pic))
            {
                user.Photo = string.Format("~/Content/Photos/{0}", pic);
            }

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException.Message.Contains("REFERENCE"))
                {
                    ModelState.AddModelError(string.Empty, "Can't delete the record, because has related records");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(user);
            }
            
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
