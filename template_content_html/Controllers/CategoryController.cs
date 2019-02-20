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
    public class CategoryController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public ICustomerService _CustomerService { get; set; }

        public CategoryController(ICustomerService CustomerService,IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, ICategoryService CategoryService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CustomerService = CustomerService;
            //this._ClientService = ClientService;
            this._UserRoleService = UserRoleService;
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

            var models = new List<CategoryModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var Categories = _CategoryService.GetCategories();
            if (!string.IsNullOrEmpty(Name))
            {
                Categories = Categories.Where(x => x.Name.Contains(Name.Trim())).ToList();
            }
            Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>();
            foreach (var Category in Categories)
            {
                var _Category = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>(Category);
                if (Category.Colour == "FFA500")
            {
                _Category.Colour = "orange";
            }
                else if (Category.Colour == "FFFF00")
            {
                _Category.Colour = "yellow";
            }
                else if (Category.Colour == "7CFC00")
            {
                _Category.Colour = "green";
            }
            else
            {
                _Category.Colour = "red";
            }
                var serviceProvider = _CustomerService.GetCustomers().Where(c => c.CategoryId == Category.CategoryId).ToList();
                if(serviceProvider.Count()>0)
                {
                    _Category.isServiceProviderExists = true;
                }
                else
                {
                    _Category.isServiceProviderExists = false;
                }
                models.Add(_Category);

            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
        }
        public static IEnumerable<Color> Colors = new List<Color> { new Color { ColorId = "FFA500", Name = "orange" }, new Color { ColorId = "FFFF00", Name = "Yellow" }, new Color { ColorId = "7CFC00", Name = "Green" }, new Color { ColorId = "FF0000", Name = "Red" } };
        [HttpGet]
        public ActionResult Create()
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
           ViewBag.CategoryList= Colors.ToList();

            //List<Category> objService = _CategoryService.GetCategories();
            //objService.Add(new Category { CategoryId = -1, Name = "Parent" });

            //ViewBag.Category = new SelectList(objService, "CategoryId", "Name");
            //ViewBag.CategoryList = new SelectList(_CompanyService.GetCompanies(), "CompanyID", "CompanyName", Session["CompanyID"].ToString());
            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "CategoryId,Name,Colour,CurrentPageNo")]CategoryModel CategoryModel, HttpPostedFileBase file)
        {
          
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                string URL = "";

                if (string.IsNullOrWhiteSpace(CategoryModel.Name))
                {
                    ModelState.AddModelError("Name", "Please enter category name.");
                }
                if (string.IsNullOrWhiteSpace(CategoryModel.Colour))
                {
                    ModelState.AddModelError("Description", "Please enter category name.");
                }
                if (ModelState.IsValid)
                {


                    var IsCateGoryDuplicate = _CategoryService.GetCategories().Where(c => c.Name == CategoryModel.Name.Trim()).FirstOrDefault();
                    if (IsCateGoryDuplicate != null)
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = " Category name is already exist.";
                        return RedirectToAction("Create");
                    }

                    if (file != null && file.ContentLength > 0)
                    {
                        try
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string path = Path.Combine(Server.MapPath("~/CategoryImage"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);
                                URL = CommonCls.GetURL() + "/CategoryImage/" + file.FileName;
                            }

                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        }

                    }


                    Mapper.CreateMap<CategoryModel, Category>();
                    var category = Mapper.Map<CategoryModel, Category>(CategoryModel);

                    category.PhotoPath = URL;
                    category.IsActive = true;
                    _CategoryService.InsertCategory(category);
                    //new CategoryModel
                    //{
                    //    CategoryId = Convert.ToInt32(CategoryModel.CategoryId),
                    //    Name = CategoryModel.Name,
                    //    Description=CategoryModel.Description// CategoryModel.ParentId == -1?null:Convert.ToInt32(CategoryModel.ParentId)
                    //}.InsertUpdate();

                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " Category save  successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.CategoryList = Colors.ToList();
                    return View(CategoryModel);
                }
            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
           
            return View(CategoryModel);
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
            Category objCategory = _CategoryService.GetCategory(id);
            var models = new List<CategoryModel>();
            Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>();
            HomeHelp.Models.CategoryModel CategoryModel = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>(objCategory);
            CategoryModel.CurrentPageNo = CurrentPageNo;
            if (objCategory == null)
            {
                return HttpNotFound();

            }

            ViewBag.CategoryList = Colors.ToList();
            return View(CategoryModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "CategoryId,Name,Colour,CurrentPageNo")]CategoryModel CategoryModel, HttpPostedFileBase file)
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                if (string.IsNullOrWhiteSpace(CategoryModel.Name))
                {
                    ModelState.AddModelError("Name","Please enter category name.");
                }
                if (string.IsNullOrWhiteSpace(CategoryModel.Colour))
                {
                    ModelState.AddModelError("Colour", "Please select colour.");
                }
                if (ModelState.IsValid)
                {
                    var IsParentAllReadyContainCategoryName = _CategoryService.GetCategories().Where(c => c.CategoryId != CategoryModel.CategoryId).Select(c => c.Name);
                    if (IsParentAllReadyContainCategoryName.Contains(CategoryModel.Name.Trim()))
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = " Category name is already exist.";
                        return RedirectToAction("Edit");
                    }
                    Category CategoryFound = _CategoryService.GetCategories().Where(x => x.Name == CategoryModel.Name && x.CategoryId != CategoryModel.CategoryId).FirstOrDefault();
                    var existingCategory = _CategoryService.GetCategories().Where(c => c.CategoryId == CategoryModel.CategoryId).FirstOrDefault();
                    Mapper.CreateMap<HomeHelp.Models.CategoryModel, HomeHelp.Entity.Category>();
                    HomeHelp.Entity.Category Categorys = Mapper.Map<HomeHelp.Models.CategoryModel, HomeHelp.Entity.Category>(CategoryModel);
                    if (CategoryFound == null)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                if(Categorys.PhotoPath!="")
                                {
                                    DeleteImage(Categorys.PhotoPath);
                                }
                               
                                string path = Path.Combine(Server.MapPath("~/CategoryImage"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);

                                string URL = CommonCls.GetURL() + "/CategoryImage/" + file.FileName;
                                Categorys.PhotoPath = URL;
                               
                            }
                            catch (Exception ex)
                            {
                                ViewBag.Message = "ERROR:" + ex.Message.ToString();
                            }

                        }
                        else
                        {
                            Categorys.PhotoPath = existingCategory.PhotoPath;
                        }
                        Categorys.IsActive = true;
                        _CategoryService.UpdateCategory(Categorys);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Category update  successfully.";
                        return RedirectToAction("Index", new { CurrentPageNo = CategoryModel.CurrentPageNo });
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (CategoryFound.Name.Trim().ToLower() == CategoryModel.Name.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = CategoryFound.Name + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }
                else
                {
                    ViewBag.CategoryList = Colors.ToList();
                    return View(CategoryModel);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            ViewBag.Category = new SelectList(_CategoryService.GetCategories(), "CategoryId", "Name", CategoryModel.CategoryId);
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(CategoryModel);

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
            Category objCategory = _CategoryService.GetCategory(id);
            try
            {
                if (objCategory != null)
                {
                    //var IsParent = _CategoryService.GetCategories().Where(c => c.CategoryId == objCategory.CategoryId).FirstOrDefault();
                    //if (IsParent == null)
                    //{
                    _CategoryService.DeleteCategory(objCategory);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Category successfully deleted.";
                    RedirectToAction("Index");
                    //}
                    //else
                    //{
                    //    TempData["ShowMessage"] = "error";
                    //    TempData["MessageBody"] = "First delete the childs of this category.";
                    //    RedirectToAction("Index");
                    //}
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