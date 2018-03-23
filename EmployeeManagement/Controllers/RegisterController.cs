using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EmployeeManagement.Controllers
{
    public class RegisterController : Controller
    {
        private EmployeeMgEntities db = new EmployeeMgEntities();

        #region public ActionResult
        // GET: Register
        public ActionResult Register()
        {
            if (Session["Login"] == null || Session["Login"].ToString() == "0")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Department");
            }
        }

        // POST: Employee/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Registration reg)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.Registrations.Any(name => name.Email.Equals(reg.Email)))
                    {
                        ModelState.AddModelError(string.Empty, "User is already exists");
                    }
                    else
                    {
                        db.Registrations.Add(reg);
                        db.SaveChanges();

                        Session["Login"] = "1";
                        Session["UserName"] = reg.Email.ToString();
                        Session["Role"] = reg.Role.ToString();
                        return RedirectToAction("Index", "Department");
                        // return RedirectToAction("Login");
                    }

                }
            }
            catch (Exception ex)
            {
                string message = string.Format("<b>Message:</b> {0}<br /><br />", ex.Message);
                message += string.Format("<b>StackTrace:</b> {0}<br /><br />", ex.StackTrace.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>Source:</b> {0}<br /><br />", ex.Source.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>TargetSite:</b> {0}", ex.TargetSite.ToString().Replace(Environment.NewLine, string.Empty));
                ModelState.AddModelError(string.Empty, message);
            }

            return View(reg);
        }
        [HttpGet]
        public ActionResult LogIn()
        {
            if (Session["Login"] == null || Session["Login"].ToString() == "0")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Department");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogIn(Registration userr)
        {
            //if (ModelState.IsValid)
            //{
            if (userr.Email.Equals("") || userr.Password.Equals(""))
            {
                ModelState.AddModelError("", "Please Enter login details.");
            }
            else if (!userr.Email.Equals("") && !userr.Password.Equals(""))
            {
                var user = db.Registrations.FirstOrDefault(u => u.Email == userr.Email);
                if (user != null)
                {
                    if (user.Password == userr.Password)
                    {
                        // FormsAuthentication.SetAuthCookie(userr.Email, true);
                        Session["Login"] = "1";
                        Session["UserName"] = userr.Email.ToString();
                        Session["Role"] = user.Role.ToString();
                        return RedirectToAction("Index", "Department");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Login details are wrong.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Login details are wrong.");
                }
            }
            return View(userr);
        }

        public ActionResult LogOut()
        {
            // FormsAuthentication.SignOut();
            //Session["Login"] = "0";
            //Session["UserName"] = null;
            //Session["Role"] = null;
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("LogIn", "Register");
        }

        public ActionResult ForgotPassword()
        {
            if (Session["Login"] == null || Session["Login"].ToString() == "0")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Department");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(Registration userr)
        {


            var user = db.Registrations.FirstOrDefault(u => u.Email == userr.Email);
            if (user != null)
            {
                if (user.SecurityQuestion == userr.SecurityQuestion && user.SecurityAnswer == userr.SecurityAnswer)
                {
                    TempData["userid"] = user.UserId;
                    return RedirectToAction("CreateNewPassword");
                }
                else
                {
                    ModelState.AddModelError("", "Please Enter Valid Security Question Answer Details.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please Enter Valid Emailid.");
            }

            return View(userr);
        }

        public ActionResult CreateNewPassword()
        {
            if (Session["Login"] == null || Session["Login"].ToString() == "0")
            {
                int id = 0;
                TempData.Keep();
                if (TempData.ContainsKey("userid"))
                    int.TryParse(TempData["userid"].ToString(), out id);

                Registration reg = db.Registrations.Find(id);
                if(reg == null)
                {
                    return RedirectToAction("ForgotPassword");
                }
                reg.Password = "";
                reg.ConfirmPassword = "";
                return View(reg);

            }
            else
            {
                return RedirectToAction("Index", "Department");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewPassword(Registration userr)
        {
            try
            {
                if (userr.UserId > 0)
                {
                    Registration reg = db.Registrations.Find(userr.UserId);
                    reg.Password = userr.Password;
                    reg.ConfirmPassword = userr.ConfirmPassword;

                    db.Entry(reg).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["Login"] = "1";
                    Session["UserName"] = reg.Email.ToString();
                    Session["Role"] = reg.Role.ToString();
                    return RedirectToAction("Index", "Department");
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("<b>Message:</b> {0}<br /><br />", ex.Message);
                message += string.Format("<b>StackTrace:</b> {0}<br /><br />", ex.StackTrace.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>Source:</b> {0}<br /><br />", ex.Source.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>TargetSite:</b> {0}", ex.TargetSite.ToString().Replace(Environment.NewLine, string.Empty));
                ModelState.AddModelError(string.Empty, message);
            }
            return View(userr);
        }
        #endregion

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