using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Onlo.Services;
using Onlo.Controllers;
using Onlo.Infrastructure;
using Onlo.Entity;
using AutoMapper;
using Onlo.Models;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Onlo.Core.Infrastructure;
using Onlo.Core.UtilityManager;


namespace Onlo.Controllers
{
    public class SerivceItemController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IServiceItemService _ServiceItemService { get; set; }

        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("ServiceItem");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }

        public SerivceItemController(IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, IServiceItemService ServiceItemService, IVIPsetupService VIPsetupService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService, VIPsetupService)
        {
            this._UserRoleService = UserRoleService;
            this._UserService = UserService;
            this._ServiceItemService = ServiceItemService;
        }
#endregion

        #region Basic crude oprations of Service items
        public ActionResult Index(string SerivceItemName, string operation, string ShowMessage, string MessageBody, string CurrentPageNo)
        {
            UserPermissionAction("ServiceItem", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<ServiceItemModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var SerivceItems = _ServiceItemService.GetServiceItems();
            if (!string.IsNullOrEmpty(SerivceItemName))
            {
                SerivceItems = SerivceItems.Where(x => x.ServiceItemName.Contains(SerivceItemName.Trim())).ToList();
            }
            Mapper.CreateMap<Onlo.Entity.ServiceItem, Onlo.Models.ServiceItemModel>();
            foreach (var SerivceItem in SerivceItems)
            {
                var _SerivceItem = Mapper.Map<Onlo.Entity.ServiceItem, Onlo.Models.ServiceItemModel>(SerivceItem);
                models.Add(_SerivceItem);
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
            UserPermissionAction("ServiceItem", RoleAction.view.ToString());
            CheckPermission();
            List<ServiceItem> objService = _ServiceItemService.GetServiceItems();
            objService.Add(new ServiceItem { ServiceItemId = -1, ServiceItemName = "Parent" });
            ViewBag.ServicesList = new SelectList(objService, "ServiceItemId", "ServiceItemName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "ServiceItemId,ParentId,ServiceItemName,IsActive")]ServiceItemModel SerivceItemModel)
        {

            UserPermissionAction("ServiceItem", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();

                    ServiceItem SerivceItemFound = _ServiceItemService.GetServiceItems().Where(x => x.ServiceItemName == SerivceItemModel.ServiceItemName).FirstOrDefault();
                    Mapper.CreateMap<Onlo.Models.ServiceItemModel, Onlo.Entity.ServiceItem>();
                    Onlo.Entity.ServiceItem SerivceItem = Mapper.Map<Onlo.Models.ServiceItemModel, Onlo.Entity.ServiceItem>(SerivceItemModel);
                    if (SerivceItemFound == null)
                    {
                        var IsParentAllReadyContainCategoryName = _ServiceItemService.GetServiceItems().Where(c => c.ParentId == SerivceItem.ParentId).Select(c => c.ServiceItemName);
                        if (IsParentAllReadyContainCategoryName.Contains(SerivceItem.ServiceItemName.Trim()))
                        {
                            TempData["ShowMessage"] = "error";
                            TempData["MessageBody"] = " Service name is already exist.";
                            return RedirectToAction("Create");
                        }
                        var IsServiceDuplicate = _ServiceItemService.GetServiceItems().Where(c => c.ServiceItemName == SerivceItem.ServiceItemName.Trim() && c.ParentId == null).FirstOrDefault();
                        if (IsServiceDuplicate != null)
                        {
                            TempData["ShowMessage"] = "error";
                            TempData["MessageBody"] = " Service name is already exist.";
                            return RedirectToAction("Create");
                        }

                        SerivceItem.IsActive = true;
                        if (SerivceItemModel.ParentId==-1)
                        {
                            SerivceItem.ParentId = null;
                        }
                        _ServiceItemService.InsertServiceItem(SerivceItem);


                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " service save  successfully.";
                        return RedirectToAction("Index");
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (SerivceItemFound.ServiceItemName.Trim().ToLower() == SerivceItemModel.ServiceItemName.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = SerivceItemFound.ServiceItemName + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }

            return View(SerivceItemModel);
        }
        [HttpGet]
        public ActionResult Edit(int id, string CurrentPageNo)
        {
            UserPermissionAction("ServiceItem", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceItem objSerivceItem = _ServiceItemService.GetServiceItem(id);
            var models = new List<ServiceItemModel>();
            Mapper.CreateMap<Onlo.Entity.ServiceItem, Onlo.Models.ServiceItemModel>();
            Onlo.Models.ServiceItemModel SerivceItemmodel = Mapper.Map<Onlo.Entity.ServiceItem, Onlo.Models.ServiceItemModel>(objSerivceItem);
            if (objSerivceItem == null)
            {
                return HttpNotFound();
            }
            SerivceItemmodel.CurrentPageNo = CurrentPageNo;
            List<ServiceItem> objService = _ServiceItemService.GetServiceItems();
            objService.Add(new ServiceItem { ServiceItemId = -1, ServiceItemName = "Parent" });
            ViewBag.ServicesList = new SelectList(objService, "ServiceItemId", "ServiceItemName", objSerivceItem.ParentId!=null?objSerivceItem.ParentId:-1);
            //ViewBag.ServicesList = new SelectList(_ServiceItemService.GetServiceItems(), "ServiceItemId", "ServiceItemName", objSerivceItem.ServiceItemId);
            return View(SerivceItemmodel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ServiceItemId,ParentId,ServiceItemName,IsActive,CurrentPageNo")]ServiceItemModel serivceitemmodel)
        {
            UserPermissionAction("ServiceItem", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();
                    var IsParentAllReadyContainCategoryName = _ServiceItemService.GetServiceItems().Where(c => c.ParentId == serivceitemmodel.ParentId && c.ServiceItemId != serivceitemmodel.ServiceItemId).Select(c => c.ServiceItemName);
                    if (IsParentAllReadyContainCategoryName.Contains(serivceitemmodel.ServiceItemName.Trim()))
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = " Service name is already exist.";
                        return RedirectToAction("Create");
                    }
                    ServiceItem SerivceItemFound = _ServiceItemService.GetServiceItems().Where(x => x.ServiceItemName == serivceitemmodel.ServiceItemName && x.ParentId == null && x.ServiceItemId != serivceitemmodel.ServiceItemId).FirstOrDefault();
                    Mapper.CreateMap<Onlo.Models.ServiceItemModel, Onlo.Entity.ServiceItem>();
                    Onlo.Entity.ServiceItem item = Mapper.Map<Onlo.Models.ServiceItemModel, Onlo.Entity.ServiceItem>(serivceitemmodel);
                    if (SerivceItemFound == null)
                    {
                        item.IsActive = true;
                        if (serivceitemmodel.ParentId==-1)
                        {
                            item.ParentId = null;
                        }
                        _ServiceItemService.UpdateServiceItem(item);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = "service update  successfully.";
                        return RedirectToAction("Index", new { CurrentPageNo = serivceitemmodel.CurrentPageNo });

                    }
                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (SerivceItemFound.ServiceItemName.Trim().ToLower() == serivceitemmodel.ServiceItemName.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = SerivceItemFound.ServiceItemName + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }

                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            }

            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }

            return View(serivceitemmodel);

        }

        #endregion
       
    }
}