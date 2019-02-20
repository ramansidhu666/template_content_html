using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HomeHelp.Services;
using HomeHelp.Controllers;
using HomeHelp.Infrastructure;
using HomeHelp.Entity;
using AutoMapper;
using HomeHelp.Models;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using HomeHelp.Core.Infrastructure;
//using HomeHelp.Core.UtilityManager;
using System.Data;


namespace HomeHelp.Controllers
{
    public class MembershipController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IMembershipService _MembershipService { get; set; }

        public ICategoryService _CategoryService { get; set; }
        public MembershipController(ICategoryService CategoryService, IMembershipService MembershipService, IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._MembershipService = MembershipService;
            this._CategoryService = CategoryService;
        }
        #endregion

        #region Security fuctions
        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("Category");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }
        #endregion

        #region Basic crude oprations of category
        public ActionResult Index(string Name, string operation, string ShowMessage, string MessageBody, string CurrentPageNo)
        {
            UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<MembershipModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var memberships = _MembershipService.GetMemberships();
            
            Mapper.CreateMap<HomeHelp.Entity.Membership, HomeHelp.Models.MembershipModel>();
            foreach (var membership in memberships)
            {
                var objMembership = Mapper.Map<HomeHelp.Entity.Membership, HomeHelp.Models.MembershipModel>(membership);

                models.Add(objMembership);

            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
        }

        [HttpGet]
        public ActionResult Create()
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            //List<Category> objService = _CategoryService.GetCategories();
            //objService.Add(new Category { CategoryId = -1, Name = "Parent" });

            //ViewBag.Category = new SelectList(objService, "CategoryId", "Name");
            //ViewBag.CategoryList = new SelectList(_CompanyService.GetCompanies(), "CompanyID", "CompanyName", Session["CompanyID"].ToString());
            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "Name,Cost,Smileys,Hangouts")]MembershipModel MembershipModel)
        {

            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                if (string.IsNullOrWhiteSpace(MembershipModel.Name))
                {
                    ModelState.AddModelError("Name", "Please enter name.");
                }
                if (string.IsNullOrWhiteSpace(MembershipModel.Cost))
                {
                    ModelState.AddModelError("Cost", "Please enter cost.");
                }
                if (MembershipModel.Smileys == 0)
                {
                    ModelState.AddModelError("Smileys", "Please enter smileys.");
                }
                if (string.IsNullOrWhiteSpace(MembershipModel.Hangouts))
                {
                    ModelState.AddModelError("Hangouts", "Please enter hangouts.");
                }
                if (ModelState.IsValid)
                {


                    var IsMembershipDuplicate = _MembershipService.GetMemberships().Where(c => c.Name == MembershipModel.Name.Trim()).FirstOrDefault();
                    if (IsMembershipDuplicate != null)
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = " Membership name already exists.";
                        return RedirectToAction("Create");
                    }


                    Mapper.CreateMap<MembershipModel, Membership>();
                    var Membership = Mapper.Map<MembershipModel, Membership>(MembershipModel);

                    _MembershipService.InsertMembership(Membership);

                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " Membership save  successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(MembershipModel);
                }
            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return View(MembershipModel);
        }
        [HttpGet]
        public ActionResult Edit(int id, string CurrentPageNo)
        {
            UserPermissionAction("Category", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership objMembership = _MembershipService.GetMembership(id);
            var models = new List<MembershipModel>();
            Mapper.CreateMap<HomeHelp.Entity.Membership, HomeHelp.Models.MembershipModel>();
            HomeHelp.Models.MembershipModel MembershipModel = Mapper.Map<HomeHelp.Entity.Membership, HomeHelp.Models.MembershipModel>(objMembership);
            MembershipModel.CurrentPageNo = CurrentPageNo;
            if (objMembership == null)
            {
                return HttpNotFound();

            }


            return View(MembershipModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "MembershipId,Name,Cost,Smileys,Hangouts")]MembershipModel MembershipModel)
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                if (string.IsNullOrWhiteSpace(MembershipModel.Name))
                {
                    ModelState.AddModelError("Name", "Please enter name.");
                }
                if (string.IsNullOrWhiteSpace(MembershipModel.Cost))
                {
                    ModelState.AddModelError("Cost", "Please enter cost.");
                }
                if (MembershipModel.Smileys==0)
                {
                    ModelState.AddModelError("Smileys", "Please enter smileys.");
                }
                if (string.IsNullOrWhiteSpace(MembershipModel.Hangouts))
                {
                    ModelState.AddModelError("Hangouts", "Please enter hangouts.");
                }
                if (ModelState.IsValid)
                {

                    Membership MembershipFound = _MembershipService.GetMemberships().Where(x => x.Name == MembershipModel.Name && x.MembershipId != MembershipModel.MembershipId).FirstOrDefault();
                    var existingMembership = _MembershipService.GetMemberships().Where(c => c.MembershipId == MembershipModel.MembershipId).FirstOrDefault();
                    Mapper.CreateMap<MembershipModel, Membership>();
                    var Membership = Mapper.Map<MembershipModel, Membership>(MembershipModel);
                    if (MembershipFound == null && existingMembership!=null)
                    {
                        _MembershipService.UpdateMembership(Membership);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Membership update  successfully.";
                        return RedirectToAction("Index", new { CurrentPageNo = MembershipModel.CurrentPageNo });
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (MembershipFound.Name.Trim().ToLower() == MembershipModel.Name.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = MembershipModel.Name + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }
                else
                {
                    return View(MembershipModel);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            //ViewBag.Category = new SelectList(_CategoryService.GetCategories(), "CategoryId", "Name", MembershipModel.MembershipId);
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(MembershipModel);

        }
        public void DeleteImage(string filePath)
        {
            try
            {
                var uri = new Uri(filePath);
                var fileName = Path.GetFileName(uri.AbsolutePath);
                var subPath = Server.MapPath("~/CategoryImage");
                var path = Path.Combine(subPath, fileName);

                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            UserPermissionAction("Category", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership objMembership = _MembershipService.GetMembership(id);
            try
            {
                if (objMembership != null)
                {
                   _MembershipService.DeleteMembership(objMembership);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Membership successfully deleted.";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }
        #endregion


    }
}