using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployeeManagement;

namespace EmployeeManagement.Controllers
{
    public class DepartmentController : Controller
    {
        private EmployeeMgEntities db = new EmployeeMgEntities();

        #region public ActionResult
        // GET: Department
        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
               
                var DepartmentLists = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
                return View(DepartmentLists);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // GET: Department/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentMaster departmentMaster = db.DepartmentMasters.Find(id);
                if (departmentMaster == null)
                {
                    return HttpNotFound();
                }
                return View(departmentMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }

        }

        // GET: Department/Create
        public ActionResult Create()
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (Session["Role"].ToString().Equals("GuestUser"))
                {
                    return RedirectToAction("Index");
                }
                return View();
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Department_Id,Department_Name")] DepartmentMaster departmentMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.DepartmentMasters.Any(name => name.Department_Name.Equals(departmentMaster.Department_Name)))
                    {
                        ModelState.AddModelError(string.Empty, "Department is already exists");
                    }
                    else
                    {
                        departmentMaster.IsDelete = 0;
                        db.DepartmentMasters.Add(departmentMaster);
                        db.SaveChanges();
                        return RedirectToAction("Index");
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
            return View(departmentMaster);
        }

        // GET: Department/Edit/id
        public ActionResult Edit(int? id)
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (Session["Role"].ToString().Equals("GuestUser"))
                {
                    return RedirectToAction("Index");
                }
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentMaster departmentMaster = db.DepartmentMasters.Find(id);
                if (departmentMaster == null)
                {
                    return HttpNotFound();
                }
                return View(departmentMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // POST: Department/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Department_Id,Department_Name")] DepartmentMaster departmentMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.DepartmentMasters.Any(name => name.Department_Name.Equals(departmentMaster.Department_Name) && name.Department_Id != departmentMaster.Department_Id))
                    {
                        ModelState.AddModelError(string.Empty, "Department is already exists");
                    }
                    else
                    {
                        departmentMaster.IsDelete = 0;
                        db.Entry(departmentMaster).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
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
            return View(departmentMaster);
        }

        // GET: Department/Delete/id
        public ActionResult Delete(int? id)
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (Session["Role"].ToString().Equals("GuestUser"))
                {
                    return RedirectToAction("Index");
                }
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentMaster departmentMaster = db.DepartmentMasters.Find(id);
                if (departmentMaster == null)
                {
                    return HttpNotFound();
                }
                return View(departmentMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // POST: Department/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DepartmentMaster departmentMaster = db.DepartmentMasters.Find(id);
                departmentMaster.IsDelete = 1;
                db.Entry(departmentMaster).State = EntityState.Modified;
                //db.DepartmentMasters.Remove(departmentMaster);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string message = string.Format("<b>Message:</b> {0}<br /><br />", ex.Message);
                message += string.Format("<b>StackTrace:</b> {0}<br /><br />", ex.StackTrace.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>Source:</b> {0}<br /><br />", ex.Source.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>TargetSite:</b> {0}", ex.TargetSite.ToString().Replace(Environment.NewLine, string.Empty));
                ModelState.AddModelError(string.Empty, message);
            }
            return RedirectToAction("Index");
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
