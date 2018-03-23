using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using EmployeeManagement;

namespace EmployeeManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private EmployeeMgEntities db = new EmployeeMgEntities();


        #region Private Method

        private byte[] GetImageFromDataBase(int Id)
        {
            var q = from temp in db.EmployeeMasters where temp.Employee_Id == Id select temp.Image;
            byte[] cover = q.First();
            return cover;
        }


        private byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }
        #endregion

        #region public ActionResult
        // GET: Employee
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                //using Storeprocedure
                //var employeeMasters = db.EmployeeLists();

                //Using Linq
                ViewBag.idSortParm = String.IsNullOrEmpty(sortOrder) ? "Id" : "";
                ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;


                var employeeMasters = db.EmployeeMasters.Where(e => e.IsDelete == 0 && e.DepartmentMaster.IsDelete == 0);

                if (!String.IsNullOrEmpty(searchString))
                {
                    employeeMasters = employeeMasters.Where(s => s.Employee_Name.Contains(searchString)
                                           || s.Email.Contains(searchString) || s.DepartmentMaster.Department_Name.Contains(searchString) || s.MobileNo.Contains(searchString) || s.JoiningDate.ToString().Contains(searchString));
                }

                switch (sortOrder)
                {
                    case "id":
                        employeeMasters = employeeMasters.OrderBy(s => s.Employee_Id);
                        break;
                    case "Name":
                        employeeMasters = employeeMasters.OrderBy(s => s.Employee_Name);
                        break;
                    case "name_desc":
                        employeeMasters = employeeMasters.OrderByDescending(s => s.Employee_Name);
                        break;
                    case "Date":
                        employeeMasters = employeeMasters.OrderBy(s => s.JoiningDate);
                        break;
                    case "date_desc":
                        employeeMasters = employeeMasters.OrderByDescending(s => s.JoiningDate);
                        break;
                    default:
                        employeeMasters = employeeMasters.OrderBy(s => s.Employee_Id);
                        break;
                }

                int pageSize = 4;
                int pageNumber = (page ?? 1);
                return View(employeeMasters.ToPagedList(pageNumber, pageSize));


              //  return View(employeeMasters);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        public ActionResult RetrieveImage(int id)
        {
            byte[] cover = GetImageFromDataBase(id);
            if (cover != null)
            {
                return File(cover, "image/jpg");
            }
            else
            {
                return null;
            }
        }

        // GET: Employee/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                EmployeeMaster employeeMaster = db.EmployeeMasters.Find(id);
                if (employeeMaster == null)
                {
                    return HttpNotFound();
                }
                ViewBag.id = id;
                return View(employeeMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            if (Session["Role"] != null && Session["Login"] != null && Session["Login"].ToString() == "1")
            {
                if (Session["Role"].ToString().Equals("GuestUser"))
                {
                    return RedirectToAction("Index");
                }
                var DepartmentLists = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
                ViewBag.Department = new SelectList(DepartmentLists, "Department_Id", "Department_Name");
                return View();
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeMaster employeeMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.EmployeeMasters.Any(name => name.Email.Equals(employeeMaster.Email)))
                    {
                        ModelState.AddModelError(string.Empty, "Employee is already exists");
                    }
                    else
                    {
                        //ImgUpload code
                        HttpPostedFileBase file = Request.Files["ImageData"];
                        if (file != null)
                        {
                            var supportedTypes = new[] { "jpg", "jpeg", "png", "gif", "bmp" };
                            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                            if (!supportedTypes.Contains(fileExt))
                            {
                                string ErrorMessage = "File Extension Is InValid - Only Upload Image File";
                                ModelState.AddModelError(string.Empty, ErrorMessage);
                                var DepartmentList = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
                                ViewBag.Department = new SelectList(DepartmentList, "Department_Id", "Department_Name", employeeMaster.Department);
                                return View(employeeMaster);
                            }
                            employeeMaster.Image = ConvertToBytes(file);
                        }
                        employeeMaster.IsDelete = 0;
                        db.EmployeeMasters.Add(employeeMaster);
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
            var DepartmentLists = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
            ViewBag.Department = new SelectList(DepartmentLists, "Department_Id", "Department_Name", employeeMaster.Department);
            return View(employeeMaster);
        }



        // GET: Employee/Edit/5
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
                EmployeeMaster employeeMaster = db.EmployeeMasters.Find(id);
                if (employeeMaster == null)
                {
                    return HttpNotFound();
                }
                //ViewBag.id = id;
                var DepartmentLists = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
                ViewBag.Department = new SelectList(DepartmentLists, "Department_Id", "Department_Name", employeeMaster.Department);
                return View(employeeMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }

        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeMaster employeeMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.EmployeeMasters.Any(name => name.Email.Equals(employeeMaster.Email) && name.Employee_Id != employeeMaster.Employee_Id))
                    {
                        ModelState.AddModelError(string.Empty, "Employee is already exists");
                    }
                    else
                    {
                        HttpPostedFileBase file = Request.Files["ImageData"];
                        if (file != null)
                        {
                            var supportedTypes = new[] { "jpg", "jpeg", "png", "gif", "bmp" };
                            if (!file.FileName.Equals(""))
                            {
                                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                                if (!supportedTypes.Contains(fileExt))
                                {
                                    string ErrorMessage = "File Extension Is InValid - Only Upload Image File";
                                    ModelState.AddModelError(string.Empty, ErrorMessage);
                                    var DepartmentList = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
                                    ViewBag.Department = new SelectList(DepartmentList, "Department_Id", "Department_Name", employeeMaster.Department);
                                    return View(employeeMaster);
                                }
                                employeeMaster.Image = ConvertToBytes(file);
                            }
                            else
                            {
                                //EmployeeMaster employee = db.EmployeeMasters.Find(employeeMaster.Employee_Id);
                                //if (employee != null)
                                //{
                                //    employeeMaster.Image = employee.Image;
                                //}
                            }


                        }
                        employeeMaster.IsDelete = 0;
                        db.Entry(employeeMaster).State = EntityState.Modified;
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
            var DepartmentLists = db.DepartmentMasters.Where(s => s.IsDelete == 0).ToList();
            ViewBag.Department = new SelectList(DepartmentLists, "Department_Id", "Department_Name", employeeMaster.Department);
            return View(employeeMaster);
        }

        // GET: Employee/Delete/5
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
                EmployeeMaster employeeMaster = db.EmployeeMasters.Find(id);
                if (employeeMaster == null)
                {
                    return HttpNotFound();
                }
                ViewBag.id = id;
                return View(employeeMaster);
            }
            else
            {
                return RedirectToAction("LogIn", "Register");
            }
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                EmployeeMaster employeeMaster = db.EmployeeMasters.Find(id);
                employeeMaster.IsDelete = 1;
                db.Entry(employeeMaster).State = EntityState.Modified;
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
