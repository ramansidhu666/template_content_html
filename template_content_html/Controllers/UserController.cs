using AutoMapper;
using HomeHelp.Controllers;
using HomeHelp.Core.Infrastructure;
using HomeHelp.Entity;
using HomeHelp.Infrastructure;
using HomeHelp.Models;
using HomeHelp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HomeHelp.Web.Controllers
{
    public class UserController : BaseController
    {
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        //public IServiceItemService _ServiceItemService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public IRequestService _RequestService { get; set; }
        public ICategoryService _CategoryService { get; set; }  


        public UserController(IRequestService RequestService,ICategoryService CategoryService ,ICustomerService CustomerServices,IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService, IUserRoleService UserRoleService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CategoryService = CategoryService;
            this._RequestService = RequestService;
            this._UserRoleService = UserRoleService;
            this._UserService = UserService;
            this._CustomerService = CustomerServices;
        }

        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("Account");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }

        // GET: /User/
        public ActionResult Index(string UserName, string EmailId)
        {
            List<CustomerModel> UserModels = new List<CustomerModel>();
            var Users = _CustomerService.GetCustomers().Where(c => c.UserId != 1);
            //var Users = _UserService.GetUsers().Where(c => c.UserId != 1);
            Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
            //if (!string.IsNullOrEmpty(UserName))
            //{
            //    Users = Users.Where(x => x.UserName.Contains(UserName.Trim())).ToList();
            //}
            //if (!string.IsNullOrEmpty(EmailId))
            //{
            //    Users = Users.Where(x => x.UserEmailAddress.Contains(UserName.Trim())).ToList();
            //}
            foreach (var User in Users)
            {
                var _User = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(User);
                UserModels.Add(_User);

            }   
            return View(UserModels);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            UserPermissionAction("Account", RoleAction.detail.ToString());
            CheckPermission();

            Customer objCustomer = _CustomerService.GetCustomer(id);
            try
            {
                if (objCustomer != null)
                {
                    objCustomer.IsActive = false;
                    _CustomerService.UpdateCustomer(objCustomer);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "User successfully deactivated.";
               
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

        [HttpGet]
        public ActionResult Unblock(Guid id)
        {
            UserPermissionAction("Account", RoleAction.detail.ToString());
            CheckPermission();

            Customer objCustomer = _CustomerService.GetCustomer(id);
            try
            {
                if (objCustomer != null)
                {
                    objCustomer.IsActive = true;
                    _CustomerService.UpdateCustomer(objCustomer);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "User successfully unblocked.";

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

        [HttpGet]
        public ActionResult Block(Guid id)
        {
            UserPermissionAction("Account", RoleAction.detail.ToString());
            CheckPermission();

            Customer objCustomer = _CustomerService.GetCustomer(id);
            try
            {
                if (objCustomer != null)
                {
                    objCustomer.IsActive = false;
                    _CustomerService.UpdateCustomer(objCustomer);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "User successfully blocked.";

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

        //public ActionResult Details(int? UserId)
        //{
        //    var userid = UserId == null ? 0 : UserId;
        //    UserPermissionAction("User", RoleAction.view.ToString());
        //    CheckPermission();
        //    var Customer = _CustomerServices.GetCustomers().Where(c => c.UserId == Convert.ToInt32(userid)).FirstOrDefault();

        //    var model = new CustomerModel();
        //    if (Customer != null)
        //    {
        //        CompanyModel companymodel = new CompanyModel();
        //        var BranchModelList = new List<BranchModel>();
        //        var ServiceItemModelList = new List<ServiceItemModel>();
        //        var ProductModelList = new List<ProductModel>();

        //        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
        //        model = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(Customer);
        //        model.CompanyID = model.CompanyID == null ? 0 : model.CompanyID;

        //        //Get company detail.
        //        var CompanyDetail = _CompanyService.GetCompany(model.CompanyID);
        //        if (CompanyDetail != null)
        //        {

        //            companymodel.CompanyName = CompanyDetail.CompanyName;
        //            companymodel.CompanyAddress = CompanyDetail.CompanyAddress;
        //            companymodel.HeadOffice = CompanyDetail.HeadOffice;
        //            companymodel.PhoneNo = CompanyDetail.PhoneNo;
        //            companymodel.WebSite = CompanyDetail.WebSite;
        //            companymodel.EmailID = CompanyDetail.EmailID;
        //            companymodel.LogoPath = CompanyDetail.LogoPath;

        //            //Get branches detail.
        //            var Branches = _BranchService.GetBranchs().Where(c => c.CompanyID == CompanyDetail.CompanyID).ToList();
        //            foreach (var Branche in Branches)
        //            {
        //                BranchModel branchmodel = new BranchModel();
        //                branchmodel.BranchName = Branche.BranchName;
        //                branchmodel.CellNumber = Branche.CellNumber;
        //                branchmodel.PhoneNumber = Branche.PhoneNumber;
        //                branchmodel.Location = Branche.Location;
        //                branchmodel.Longitude = Branche.Longitude;
        //                branchmodel.Latitude = Branche.Latitude;
        //                BranchModelList.Add(branchmodel);
        //            }

        //            //Get Customer services detail.
        //            var CustomerServices = _CustomerServicesService.GetCustomerServices().Where(c => c.CustomerId == Customer.CustomerId).ToList();
        //            foreach (var CustomerService in CustomerServices)
        //            {
        //                var ServiceItem = _ServiceItemService.GetServiceItem(Convert.ToInt32(CustomerService.ServiceItemId));
        //                if (ServiceItem != null)
        //                {
        //                    ServiceItemModel ServiceItemModel = new Models.ServiceItemModel();
        //                    ServiceItemModel.ServiceItemName = ServiceItem.ServiceItemName;
        //                    ServiceItemModelList.Add(ServiceItemModel);
        //                }
        //            }

        //            //Get customer intrested product.
        //            var Products = _ProductService.GetProducts().Where(c => c.CustomerId == Customer.CustomerId).ToList();
        //            foreach (var product in Products)
        //            {
        //                ProductModel ProductModel = new ProductModel();
        //                ProductModel.ProductName = product.ProductName;

        //                //Get intrested categories of product.
        //                var Categories = _CategoryService.GetCategories().Where(c => c.ParentId == product.CategoryId).ToList();
        //                var CategoryModelList = new List<CategoryModel>();
        //                foreach (var category in Categories)
        //                {

        //                    CategoryModel CategoryModel = new CategoryModel();
        //                    CategoryModel.Name = category.Name;
        //                    CategoryModelList.Add(CategoryModel);

        //                }
        //                ProductModel.CategoryModelList = CategoryModelList;
        //                ProductModelList.Add(ProductModel);

        //            }
        //        }
        //        //Assign all model list.
        //        model.BranchList = BranchModelList;
        //        model.Company = companymodel;
        //        model.ServiceItemModelList = ServiceItemModelList;
        //        model.ProductModelList = ProductModelList;
        //    }
        //    return View(model);
        //}
        //public ActionResult UpdateUser(int id)
        //{
        //    User user = _UserService.GetUserById(id);
        //    Customer customer = _CustomerServices.GetCustomers().Where(c => c.UserId == user.UserId).FirstOrDefault();

        //    if (customer != null)
        //    {
        //        if (customer.IsActive == true)
        //        {
        //            customer.IsActive = false;
        //        }
        //        else
        //        {
        //            customer.IsActive = true;
        //        }
        //        _CustomerServices.UpdateCustomer(customer);
        //    }

        //    if (user != null)
        //    {
        //        if (user.IsActive == true)
        //        {
        //            user.IsActive = false;
        //        }
        //        else
        //        {
        //            user.IsActive = true;
        //        }
        //        _UserService.UpdateUser(user);
        //    }
        //    return RedirectToAction("Index");
        //}

        public ActionResult Details(Guid Id, string CurrentPageNo)
        {

            UserPermissionAction("User", RoleAction.view.ToString());
            CheckPermission();
            var customer=_CustomerService.GetCustomer(Id);
             var jobs=new List< JobRequest>();
            if(customer.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
            {
                jobs = _RequestService.GetRequests().Where(j => j.CustomerIdBy == Id).ToList();
            }
            else
            {
                jobs = _RequestService.GetRequests().Where(j => j.CustomerIdTo == Id).ToList();
            }

            var jobList = new List<JObRequestResponseModel>();

            if (jobs.Count()>0)
            {

                foreach (var job in jobs)
                {
                    Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>();
                    JObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>(job);
                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                    jobList.Add(JObRequestResponseModel);
                }
                return View(jobList);
            }
            else
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = " Location has no details.";
                return RedirectToAction("Index");
            }

        }


        [HttpGet]
        public ActionResult AddProduct()
        {

            List<CustomerModel> UserModels = new List<CustomerModel>();
            var Users = _CustomerService.GetCustomers().Where(c => c.UserId != 1);
            //var Users = _UserService.GetUsers().Where(c => c.UserId != 1);
            Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
            //if (!string.IsNullOrEmpty(UserName))
            //{
            //    Users = Users.Where(x => x.UserName.Contains(UserName.Trim())).ToList();
            //}
            //if (!string.IsNullOrEmpty(EmailId))
            //{
            //    Users = Users.Where(x => x.UserEmailAddress.Contains(UserName.Trim())).ToList();
            //}
            foreach (var User in Users)
            {
                var _User = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(User);
                UserModels.Add(_User);

            }
            return View(UserModels);
        }


        //public ActionResult SaveProducts(int Id, int? UserId, string productType, string notes)
        //{
        //    try
        //    {
        //        var IsFoundCustomer = _CustomerServices.GetCustomers().Where(c => c.UserId == UserId).FirstOrDefault();
        //        if (IsFoundCustomer != null)
        //        {
        //            if (productType == Convert.ToString(EnumValue.ProductType.service))
        //            {
        //                CustomerService service = new CustomerService();
        //                service.ServiceItemId = Id;
        //                service.CustomerId = IsFoundCustomer.CustomerId;
        //                service.ServiceDescription =notes==""?null: notes;
        //                _CustomerServicesService.InsertCustomerService(service);
        //            }
        //            else if (productType == Convert.ToString(EnumValue.ProductType.product))
        //            {
        //                CustomerProduct products = new CustomerProduct();
        //                products.CustomerId = IsFoundCustomer.CustomerId;
        //                products.CategoryId = Id;
        //                products.ProductDescription = notes == "" ? null : notes;
        //                products.ApplicableType = Convert.ToString((int)(EnumValue.ApplicableType.manufacturer_Supplier));
        //                products.IsActive = true;
        //                _CustomerProductService.InsertCustomerProduct(products);
        //            }
        //            else
        //            {
        //                CustomerSurplus surplus = new CustomerSurplus();
        //                surplus.CustomerId = IsFoundCustomer.CustomerId;
        //                surplus.CategoryId = Id;
        //                surplus.SurplusDescription = notes == "" ? null : notes;
        //                surplus.ApplicableType = Convert.ToString((int)(EnumValue.ApplicableType.manufacturer_Supplier));
        //                _CustomerSurplusService.InsertCustomerSurplus(surplus);
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();//
        //        ErrorLogging.LogError(ex);
        //        return Json(new { success = false, responseText = "error." }, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(new { success = true, responseText = "Saved successfuly." }, JsonRequestBehavior.AllowGet);
        //   // return RedirectToAction("Index");
        //}


        //public ActionResult SubCategories(int? UserId, string productType, int Id = 0)
        //{
        //    ViewBag.UserId = UserId;
        //    ProductType product = new ProductType();
        //    if (productType == Convert.ToString(EnumValue.ProductType.service))
        //    {
        //        List<ServiceItemModel> serviceList = new List<ServiceItemModel>();

        //        var services = _ServiceItemService.GetServiceItems().Where(s => s.ParentId == Id).ToList();
        //        Mapper.CreateMap<HomeHelp.Entity.ServiceItem, HomeHelp.Models.ServiceItemModel>();
        //        foreach (var service in services)
        //        {
        //            ServiceItemModel serviceItemModel = Mapper.Map<HomeHelp.Entity.ServiceItem, HomeHelp.Models.ServiceItemModel>(service);
        //            var count = _ServiceItemService.GetServiceItems().Where(s => s.ParentId == service.ServiceItemId).ToList().Count();
        //            serviceItemModel.countChildService = count;
        //            serviceList.Add(serviceItemModel);
        //        }
        //        product.ServiceItemData = serviceList;
        //    }
        //    else
        //    {
        //        List<CategoryModel> CategoryList = new List<CategoryModel>();

        //        var categories = _CategoryService.GetCategories().Where(c => c.ParentId == Id).ToList();
        //        Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>();
        //        foreach (var category in categories)
        //        {
        //            CategoryModel categoryModel = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>(category);
        //            var count = _CategoryService.GetCategories().Where(c => c.ParentId == categoryModel.CategoryId).ToList().Count();
        //            categoryModel.countChild = count==0?0:count;
        //            categoryModel.productType = productType;

        //            CategoryList.Add(categoryModel);
        //        }
        //        product.CategoryData = CategoryList;
        //    }
            
        //    return PartialView("_SubCategory", product);
        //}
    }
}