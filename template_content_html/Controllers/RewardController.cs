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
    public class RewardController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IRewardService _RewardService { get; set; }

        public ICategoryService _CategoryService { get; set; }
        public RewardController(ICategoryService CategoryService, IRewardService RewardService, IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._RewardService = RewardService;
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

            var models = new List<RewardModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var Rewards = _RewardService.GetRewards();
            Mapper.CreateMap<HomeHelp.Entity.Reward, HomeHelp.Models.RewardModel>();
            foreach (var Reward in Rewards)
            {
                var objReward = Mapper.Map<HomeHelp.Entity.Reward, HomeHelp.Models.RewardModel>(Reward);

                models.Add(objReward);

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
        public ActionResult Create([Bind(Include = "Title,Description,Smileys")]RewardModel RewardModel, HttpPostedFileBase file)
        {

            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                if (string.IsNullOrWhiteSpace(RewardModel.Title))
                {
                    ModelState.AddModelError("Title", "Please enter Title.");
                }
                if (string.IsNullOrWhiteSpace(RewardModel.Description))
                {
                    ModelState.AddModelError("Description", "Please enter Description.");
                }
                if (RewardModel.Smileys == 0)
                {
                    ModelState.AddModelError("Smileys", "Please enter smileys.");
                }
               
                if (ModelState.IsValid)
                {
                    string URL = "";
                    if (file != null && file.ContentLength > 0)
                    {
                        try
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string path = Path.Combine(Server.MapPath("~/RewardImage"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);
                                URL = CommonCls.GetURL() + "/RewardImage/" + file.FileName;
                            }

                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        }

                    }
                    Mapper.CreateMap<RewardModel, Reward>();
                    var Reward = Mapper.Map<RewardModel, Reward>(RewardModel);
                    Reward.ImagePath = URL;
                    _RewardService.InsertReward(Reward);

                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " Reward save  successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(RewardModel);
                }
            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return View(RewardModel);
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
            Reward objReward = _RewardService.GetReward(id);
            var models = new List<RewardModel>();
            Mapper.CreateMap<HomeHelp.Entity.Reward, HomeHelp.Models.RewardModel>();
            HomeHelp.Models.RewardModel RewardModel = Mapper.Map<HomeHelp.Entity.Reward, HomeHelp.Models.RewardModel>(objReward);
           
            if (objReward == null)
            {
                return HttpNotFound();

            }


            return View(RewardModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "RewardId,Title,Description,Smileys")]RewardModel RewardModel, HttpPostedFileBase file)
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                if (string.IsNullOrWhiteSpace(RewardModel.Title))
                {
                    ModelState.AddModelError("Title", "Please enter Title.");
                }
                if (string.IsNullOrWhiteSpace(RewardModel.Description))
                {
                    ModelState.AddModelError("Description", "Please enter Description.");
                }
                if (RewardModel.Smileys == 0)
                {
                    ModelState.AddModelError("Smileys", "Please enter smileys.");
                }
               
                if (ModelState.IsValid)
                {

                    //Reward RewardFound = _RewardService.GetRewards().Where(x => x.Name == RewardModel.Name && x.RewardId != RewardModel.RewardId).FirstOrDefault();
                    var existingReward = _RewardService.GetRewards().Where(c => c.RewardId == RewardModel.RewardId).FirstOrDefault();
                    Mapper.CreateMap<RewardModel, Reward>();
                    var Reward = Mapper.Map<RewardModel, Reward>(RewardModel);
                    if (file != null && file.ContentLength > 0)
                    {
                        try
                        {
                            if (existingReward.ImagePath != "")
                            {
                                DeleteImage(existingReward.ImagePath);
                            }

                            string path = Path.Combine(Server.MapPath("~/RewardImage"), Path.GetFileName(file.FileName));
                            file.SaveAs(path);

                            string URL = CommonCls.GetURL() + "/RewardImage/" + file.FileName;
                            Reward.ImagePath = URL;

                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        }

                    }
                    else
                    {
                        Reward.ImagePath = existingReward.ImagePath;
                    }
                   
                    if (existingReward!=null)
                    {
                        _RewardService.UpdateReward(Reward);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Reward update  successfully.";
                        return RedirectToAction("Index");
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                            TempData["MessageBody"] = "reward not found.";
                      
                    }
                }
                else
                {
                    return View(RewardModel);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            //ViewBag.Category = new SelectList(_CategoryService.GetCategories(), "CategoryId", "Name", RewardModel.RewardId);
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(RewardModel);

        }
        public void DeleteImage(string filePath)
        {
            try
            {
                var uri = new Uri(filePath);
                var fileName = Path.GetFileName(uri.AbsolutePath);
                var subPath = Server.MapPath("~/RewardImage");
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
            Reward objReward = _RewardService.GetReward(id);
            try
            {
                if (objReward != null)
                {
                   _RewardService.DeleteReward(objReward);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Reward successfully deleted.";
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