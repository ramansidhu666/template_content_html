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
    public class ProductController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IProductService _ProductService { get; set; }
        public ICustomerService _CustomerService { get; set; }

        public ProductController(ICustomerService CustomerService,IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, IProductService ProductService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CustomerService = CustomerService;
            //this._ClientService = ClientService;
            this._UserRoleService = UserRoleService;
            this._ProductService = ProductService;
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

        #region Basic crude oprations of Product
        public ActionResult Index(string Name, string operation, string ShowMessage, string MessageBody, string CurrentPageNo)
        {
            UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<ProductModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var Products = _ProductService.GetProducts();
            if (!string.IsNullOrEmpty(Name))
            {
                Products = Products.Where(x => x.ProductName.Contains(Name.Trim())).ToList();
            }
            Mapper.CreateMap<HomeHelp.Entity.Product, HomeHelp.Models.ProductModel>();
            foreach (var Product in Products)
            {
                var _Product = Mapper.Map<HomeHelp.Entity.Product, HomeHelp.Models.ProductModel>(Product);
                models.Add(_Product);

            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
        }
        public static IEnumerable<Size> Size = new List<Size>{new Size()
                {
                    SizeId = 1,
                    Name =EnumValue.GetEnumDescription(EnumValue.ProductSizes.M)
                }, new Size()
                {
                    SizeId = 2,
                    Name = EnumValue.GetEnumDescription(EnumValue.ProductSizes.L)
                }, new Size()
                {
                    SizeId = 3,
                    Name = EnumValue.GetEnumDescription(EnumValue.ProductSizes.XL)
                },
                new Size()
                {
                    SizeId = 4,
                    Name =EnumValue.GetEnumDescription(EnumValue.ProductSizes.XXL)
                }};
        [HttpGet]
        public ActionResult Create()
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            ViewBag.ProductList = Size.ToList();
            //List<Product> objService = _ProductService.GetProducts();
            //objService.Add(new Product { ProductId = -1, Name = "Parent" });

            //ViewBag.Product = new SelectList(objService, "ProductId", "Name");
            //ViewBag.ProductList = new SelectList(_CompanyService.GetCompanies(), "CompanyID", "CompanyName", Session["CompanyID"].ToString());
            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "ProductId,ProductName,Price,Size")]ProductsModel ProductModel, HttpPostedFileBase file)
        {

            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                string URL = "";
                if (TempData["ImageUrl"] != null)
                {
                    URL = ViewBag.ImageUrl = (TempData["ImageUrl"]).ToString();
                }

                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string path = Path.Combine(Server.MapPath("~/ProductImage"), Path.GetFileName(file.FileName));
                            file.SaveAs(path);
                            URL = CommonCls.GetURL() + "/ProductImage/" + file.FileName;
                            ViewBag.ImageUrl = URL;
                        }

                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }

                }

                if (string.IsNullOrWhiteSpace(ProductModel.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Please enter product name.");
                }
                if (ProductModel.Price==null)
                {
                    ModelState.AddModelError("Price", "Please enter price.");
                }
                if (ProductModel.Size.Count()==0)
                {
                    ModelState.AddModelError("Size", "Please enter size.");
                }
                
                
                if (ModelState.IsValid)
                {


                    //var IsProductDuplicate = _ProductService.GetProducts().Where(c => c.ProductName == ProductModel.ProductName.Trim()).FirstOrDefault();
                    //if (IsProductDuplicate != null)
                    //{
                    //    TempData["ShowMessage"] = "error";
                    //    TempData["MessageBody"] = " Product name is already exist.";
                    //    return RedirectToAction("Create");
                    //}



                    Mapper.CreateMap<ProductsModel, Product>();
                    var Product = Mapper.Map<ProductsModel, Product>(ProductModel);
                    string Size = "";
                    string FinalSize = "";
                    foreach (var item in ProductModel.Size)
                    {

                        if (item == 1)
                        {
                            Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.M);
                        }
                        else if (item == 2)
                        {
                            Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.L);
                        }
                        else if (item == 3)
                        {
                            Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.XL);
                        }
                        else
                        {
                            Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.XXL);
                        }
                        FinalSize = FinalSize + "," + Size;
                    }
                    Product.Size = FinalSize.TrimEnd(',').TrimStart(',');
                    Product.productImage = URL;
                    _ProductService.InsertProduct(Product);
                    //new ProductModel
                    //{
                    //    ProductId = Convert.ToInt32(ProductModel.ProductId),
                    //    Name = ProductModel.Name,
                    //    Description=ProductModel.Description// ProductModel.ParentId == -1?null:Convert.ToInt32(ProductModel.ParentId)
                    //}.InsertUpdate();

                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " Product save  successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ProductList = Size.ToList();
                    return View(ProductModel);
                }
            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
           
            return View(ProductModel);
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
            Product objProduct = _ProductService.GetProduct(id);
            var models = new List<ProductModel>();
            Mapper.CreateMap<HomeHelp.Entity.Product, HomeHelp.Models.ProductModel>();
            HomeHelp.Models.ProductModel ProductModel = Mapper.Map<HomeHelp.Entity.Product, HomeHelp.Models.ProductModel>(objProduct);
            //ProductModel.CurrentPageNo = CurrentPageNo;
            if (objProduct == null)
            {
                return HttpNotFound();

            }
            var checkBoxListItems = new List<Size>();
            foreach (var genre in Size.ToList())
            {
                checkBoxListItems.Add(new Size()
                {
                    SizeId = genre.SizeId,
                    Name = genre.Name,
                    Check = objProduct.Size.Contains(genre.Name) ? true : false
                });
            }
            ViewBag.ProductList = checkBoxListItems;
            return View(ProductModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ProductId,ProductName,Price,Size")]ProductsModel ProductModel, HttpPostedFileBase file)
        {
            UserPermissionAction("Category", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                string URL = "";
                var existingProduct = _ProductService.GetProducts().Where(c => c.ProductId == ProductModel.ProductId).FirstOrDefault();
                    Mapper.CreateMap<HomeHelp.Models.ProductsModel, HomeHelp.Entity.Product>();
                    HomeHelp.Entity.Product Products = Mapper.Map<HomeHelp.Models.ProductsModel, HomeHelp.Entity.Product>(ProductModel);
                    if (existingProduct != null)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                if (existingProduct.productImage != "")
                                {
                                    DeleteImage(existingProduct.productImage);
                                }

                                string path = Path.Combine(Server.MapPath("~/ProductImage"), Path.GetFileName(file.FileName));
                                file.SaveAs(path);

                                URL = CommonCls.GetURL() + "/ProductImage/" + file.FileName;
                                Products.productImage = URL;
                                _ProductService.UpdateProduct(Products);
                                ProductModel.productImage = URL;
                            }
                            catch (Exception ex)
                            {
                                ViewBag.Message = "ERROR:" + ex.Message.ToString();
                            }

                        }
                        else
                        {
                            ProductModel.productImage = Products.productImage = existingProduct.productImage;
                            _ProductService.UpdateProduct(Products);
                            Products.productImage = existingProduct.productImage;
                        }
                    }
                
                if (string.IsNullOrWhiteSpace(ProductModel.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Please enter product name.");
                }
                if (ProductModel.Price == null)
                {
                    ModelState.AddModelError("Price", "Please enter price.");
                }
                if (ProductModel.Size.Count()==0)
                {
                    ModelState.AddModelError("Size", "Please enter size.");
                }
                if (ModelState.IsValid)
                {
                    //var IsParentAllReadyContainProductName = _ProductService.GetProducts().Where(c => c.ProductId != ProductModel.ProductId).Select(c => c.Name);
                    //if (IsParentAllReadyContainProductName.Contains(ProductModel.Name.Trim()))
                    //{
                    //    TempData["ShowMessage"] = "error";
                    //    TempData["MessageBody"] = " Product name is already exist.";
                    //    return RedirectToAction("Edit");
                    //}
                    //Product ProductFound = _ProductService.GetProducts().Where(x => x.Name == ProductModel.Name && x.ProductId != ProductModel.ProductId).FirstOrDefault();
                     if (existingProduct != null)
                    {
                        string Size = "";
                        string FinalSize = "";
                        foreach (var item in ProductModel.Size)
                        {

                            if (item == 1)
                            {
                                Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.M);
                            }
                            else if (item == 2)
                            {
                                Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.L);
                            }
                            else if (item == 3)
                            {
                                Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.XL);
                            }
                            else
                            {
                                Size = EnumValue.GetEnumDescription(EnumValue.ProductSizes.XXL);
                            }
                            FinalSize = FinalSize + "," + Size;
                        }
                        Products.Size = FinalSize.TrimEnd(',').TrimStart(',');
                        _ProductService.UpdateProduct(Products);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Product update  successfully.";
                        return RedirectToAction("Index" );
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        //if (ProductFound.Name.Trim().ToLower() == ProductModel.Name.Trim().ToLower()) //Check User Name
                        //{
                        //    TempData["MessageBody"] = ProductFound.Name + " already exist.";
                        //}

                        //else
                        //{
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        //}


                    }
                }
                else
                {
                    ViewBag.ProductList = Size.ToList();
                    return View(ProductModel);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            ViewBag.Product = new SelectList(_ProductService.GetProducts(), "ProductId", "Name", ProductModel.ProductId);
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(ProductModel);

        }
        public void DeleteImage(string filePath)
        {
            try
            {
                var uri = new Uri(filePath);
                var fileName = Path.GetFileName(uri.AbsolutePath);
                var subPath = Server.MapPath("~/ProductImage");
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
            Product objProduct = _ProductService.GetProduct(id);
            try
            {
                if (objProduct != null)
                {
                    //var IsParent = _ProductService.GetProducts().Where(c => c.ProductId == objProduct.ProductId).FirstOrDefault();
                    //if (IsParent == null)
                    //{
                    _ProductService.DeleteProduct(objProduct);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Product successfully deleted.";
                    RedirectToAction("Index");
                    //}
                    //else
                    //{
                    //    TempData["ShowMessage"] = "error";
                    //    TempData["MessageBody"] = "First delete the childs of this Product.";
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