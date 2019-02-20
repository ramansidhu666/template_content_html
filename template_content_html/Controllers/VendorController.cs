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
using Onlo.Infrastructure.AsyncTask;
using System.Globalization;
namespace Onlo.Controllers
{
    public class VendorController : BaseController
    {
        #region Dependancy Injection
        public ICustomerServices _CustomerService { get; set; }
        public ICompanyService _CompanyService { get; set; }
        public ICityService _CityService { get; set; }
        public ICountryService _CountryService { get; set; }
        public IStateService _StateService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public ICustomerProductService _CustomerProductService { get; set; }

        public ICompanyLocationService _CompanyLocationService { get; set; }
        public IMonthlySubscriptionService _MonthlySubscriptionService { get; set; }


        public VendorController(IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, ICompanyService CompanyService, ICustomerProductService CustomerProductService,
           ICityService CityService, ICountryService CountryService, IStateService StateService, ICustomerServices CustomerService, ICompanyLocationService CompanyLocationService, IMonthlySubscriptionService MonthlySubscriptionService, IVIPsetupService VIPsetupService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService, VIPsetupService)
        {
            this._CompanyService = CompanyService;
            this._CityService = CityService;
            this._CountryService = CountryService;
            this._StateService = StateService;
            //this._ClientService = ClientService;
            this._UserRoleService = UserRoleService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._CustomerProductService = CustomerProductService;
            this._CompanyLocationService = CompanyLocationService;
            this._MonthlySubscriptionService = MonthlySubscriptionService;
        }
        #endregion

        #region Security function
        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("vendor");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }
        #endregion

        #region Basic crude oprations
        [ValidateInput(false)]
        public ActionResult Index(string CompanyName, string Email, int? RoleTypeId, string Name, string operation, string ShowMessage, string MessageBody)
        {
            UserPermissionAction("vendor", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();
            var model = new NewCustomerModel();
            var models = new List<CustomerModel>();
            model.UserTypeTitle = "All Product";
            var Vendors = _CustomerService.GetCustomers().Where(c => c.UserId != 1).AsEnumerable();
            if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email))
            {

                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()));

            }
            if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Name))
            {

                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()) && x.FirstName.Contains(Name.Trim()));
            }
            if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Name))
            {

                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.FirstName.Contains(Name.Trim()));
            }
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email))
            {

                Vendors = Vendors.Where(x => x.FirstName.Contains(Name.Trim()) && x.EmailId.Contains(Email.Trim()));
            }
            if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email))
            {

                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()));
            }
            if (!string.IsNullOrEmpty(CompanyName))
            {

                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()));
            }
            if (!string.IsNullOrEmpty(Email))
            {

                Vendors = Vendors.Where(x => x.EmailId.Contains(Email.Trim()));
            }

            if (!string.IsNullOrEmpty(Name))
            {

                Vendors = Vendors.Where(x => x.FirstName.Contains(Name.Trim()));
            }
            if (!string.IsNullOrEmpty(RoleTypeId.ToString()))
            {

                Vendors = Vendors.Where(x => x.RoleType.Contains(RoleTypeId.ToString()));

                if (Convert.ToInt32(EnumValue.RoleType.Engineer_Procurement) == RoleTypeId)
                {
                    model.UserTypeTitle = EnumValue.RoleType.Engineer_Procurement.ToString();
                }
                else if (Convert.ToInt32(EnumValue.RoleType.manufacturer_Supplier) == RoleTypeId)
                {
                    model.UserTypeTitle = EnumValue.RoleType.manufacturer_Supplier.ToString();
                }
                else if (Convert.ToInt32(EnumValue.RoleType.Field_Consultant) == RoleTypeId)
                {
                    model.UserTypeTitle = EnumValue.RoleType.Field_Consultant.ToString();
                }
                else if (Convert.ToInt32(EnumValue.RoleType.Enter_As_Guest) == RoleTypeId)
                {
                    model.UserTypeTitle = EnumValue.RoleType.Enter_As_Guest.ToString();
                }

            }
            Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
            foreach (var Vendor in Vendors)
            {
                var _vendor = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(Vendor);
                var Company = _CompanyService.GetCompany(Vendor.CompanyID);
                if (Company != null)
                {
                    _vendor.CompanyName = Company.CompanyName;
                    _vendor.CompanyAddress = Company.CompanyAddress;
                    _vendor.LogoPath = Company.LogoPath;
                    _vendor.BusinessBrochureUrl = Company.BusinessBrochureUrl;

                }

                models.Add(_vendor);
            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }
            model.CustomerModelList = models;
            IEnumerable<EnumValue.RoleType> RoleTypes = Enum.GetValues(typeof(EnumValue.RoleType))
                                                     .Cast<EnumValue.RoleType>();
            model.RoleTypes = from action in RoleTypes
                              select new SelectListItem
                              {
                                  Text = action.ToString(),
                                  Value = ((int)action).ToString()
                              };


            return View(model);
        }

        public ActionResult UpdateUser(int id)
        {
            User user = _UserService.GetUserById(id);
            Customer customer = _CustomerService.GetCustomers().Where(c => c.UserId == user.UserId).FirstOrDefault();

            if (customer != null)
            {
                if (customer.IsActive == true)
                {
                    customer.IsActive = false;
                }
                else
                {
                    customer.IsActive = true;
                }
                _CustomerService.UpdateCustomer(customer);
            }

            if (user != null)
            {
                if (user.IsActive == true)
                {
                    user.IsActive = false;
                }
                else
                {
                    user.IsActive = true;
                }
                _UserService.UpdateUser(user);
            }
            return RedirectToAction("Index");
        }


        public ActionResult Create()
        {
            UserPermissionAction("vendor", RoleAction.create.ToString());
            CheckPermission();
            //var Equiment = _IItemService.GetItems();
            //var model = new CustomerModel();
            // model.EquimentList = Equiment.Select(x => new SelectListItem { Value = x.ItemId.ToString(), Text = x.ItemName }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "CustomerId,FirstName,EmailID,CompanyName,CompanyAddress,AboutCompany,OtherInformation,SelectedEquiments,LocationLists,VisionMission,LogoPath,BusinessBrochureUrl")]CustomerModel CustomerModel, FormCollection form, HttpPostedFileBase file1, HttpPostedFileBase file2)
        {
            UserPermissionAction("vendor", RoleAction.create.ToString());
            CheckPermission();

            string CustomerID = "-1";
            int UserID = 0;
            int CompanyID = 0;
            CustomerModel.CompanyAddress = "qq";
            try
            {
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();
                    Mapper.CreateMap<Onlo.Models.CustomerModel, Onlo.Entity.Customer>();
                    Onlo.Entity.Customer Customer = Mapper.Map<Onlo.Models.CustomerModel, Onlo.Entity.Customer>(CustomerModel);
                    Customer customerFound = _CustomerService.GetCustomers().Where(x => x.EmailId == Customer.EmailId).FirstOrDefault();
                    if (customerFound == null)
                    {
                        string EmailId = Customer.EmailId;
                        string FirstName = CustomerModel.FirstName;
                        string Password = CustomerModel.Password;

                        if (CustomerModel.FirstName == "" || CustomerModel.FirstName == null)
                        {
                            FirstName = Customer.EmailId.Split('@')[0].Trim();

                        }
                        Customer.CompanyID = 1; //There is no session in API Controller. So we will find solution in future
                        Customer.Address = "";
                        Customer.MobileNo = "";
                        Customer.ZipCode = "";
                        Customer.PhotoPath = "";
                        //Insert User first
                        Onlo.Entity.User user = new Onlo.Entity.User();
                        //user.UserId =0; //New Case
                        user.FirstName = CustomerModel.FirstName;
                        user.LastName = Customer.LastName;
                        user.UserName = CustomerModel.FirstName; ;
                        user.Password = SecurityFunction.EncryptString(FirstName + "@123");
                        //user.Password = SecurityFunction.EncryptString(Password);
                        user.UserEmailAddress = EmailId;
                        user.CompanyID = Customer.CompanyID;
                        user.CreatedOn = DateTime.Now;
                        user.LastUpdatedOn = DateTime.Now;
                        user.IsActive = true;
                        _UserService.InsertUser(user);
                        //End : Insert User firstss

                        UserID = user.UserId;
                        if (user.UserId > 0)
                        {
                            //Insert User Role
                            Onlo.Entity.UserRole userRole = new Onlo.Entity.UserRole();
                            userRole.UserId = user.UserId;
                            userRole.RoleId = 3; //By Default set new Customer/user role id=3
                            _UserRoleService.InsertUserRole(userRole);
                            //End : Insert User Role

                            //Insert the Customer
                            Customer.FirstName = FirstName;
                            Customer.UserId = user.UserId;
                            Customer.MobileVerifyCode = CommonClass.GetNumericCode();
                            Customer.EmailVerifyCode = CommonClass.GetNumericCode();
                            if (CustomerModel.IsEmailVerified != null)
                            {
                                Customer.IsEmailVerified = CustomerModel.IsEmailVerified;
                            }
                            else
                            {
                                Customer.IsEmailVerified = false;
                            }
                            if ((CustomerModel.PhotoPath != null) && (CustomerModel.PhotoPath != ""))
                            {
                                if (!CustomerModel.PhotoPath.Contains('.'))
                                {
                                    Customer.PhotoPath = CommonClass.SaveImage(CustomerModel.PhotoPath, "CustomerPhoto", ".png");//SaveImage(CustomerModel.PhotoPath);
                                }
                            }
                            // Customer.RoleType = EnumValue.GetEnumDescription(EnumValue.RoleType.Manufacturer);
                            Customer.IsActive = true;
                            Customer.SubscriptionStartDate = DateTime.Now;
                            Customer.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
                            _CustomerService.InsertCustomer(Customer);
                            CustomerID = Convert.ToString(Customer.CustomerId);
                            if (CustomerID != null)
                            {
                                var Customers = _CustomerService.GetCustomers().Where(x => x.CustomerId == Convert.ToInt32(CustomerID));
                                if (Customers != null)
                                {
                                    Company Company = new Company();
                                    Company.CompanyName = CustomerModel.CompanyName;
                                    Company.AboutCompany = CustomerModel.AboutCompany;
                                    Company.CompanyAddress = "aa";
                                    Company.CityID = 0;
                                    Company.StateID = 0;
                                    Company.CountryID = 39;
                                    Company.OtherInformation = CustomerModel.OtherInformation;
                                    Company.VisionMission = CustomerModel.VisionMission;

                                    if (file1 != null)
                                    {
                                        var fileExt = Path.GetExtension(file1.FileName);
                                        string fileName = Guid.NewGuid() + fileExt;
                                        var subPath = Server.MapPath("~/CompanyLogo");
                                        //Check SubPath Exist or Not
                                        if (!Directory.Exists(subPath))
                                        {
                                            Directory.CreateDirectory(subPath);
                                        }
                                        //End : Check SubPath Exist or Not
                                        var path = Path.Combine(subPath, fileName);
                                        var shortPath = "~/CompanyLogo/" + fileName;
                                        file1.SaveAs(path);
                                        CommonClass.CreateThumbnail(shortPath, 218, 84, false);
                                        string URL = CommonClass.GetURL() + "/CompanyLogo/" + fileName;
                                        Company.LogoPath = URL;
                                        //Company.LogoPath = shortPath;
                                        //_CompanyService.UpdateCompany(Company);
                                    }
                                    if (file2 != null)
                                    {

                                        var fileExt = Path.GetExtension(file2.FileName);
                                        string fileName = Guid.NewGuid() + fileExt;
                                        var subPath = Server.MapPath("~/BusinessBrochure");
                                        //Check SubPath Exist or Not
                                        if (!Directory.Exists(subPath))
                                        {
                                            Directory.CreateDirectory(subPath);
                                        }
                                        //End : Check SubPath Exist or Not
                                        var path = Path.Combine(subPath, fileName);
                                        var shortPath = "~/BusinessBrochure/" + fileName;
                                        file2.SaveAs(path);
                                        CommonClass.CreateThumbnail(shortPath, 218, 84, false);
                                        string URL = CommonClass.GetURL() + "/BusinessBrochure/" + fileName;
                                        Company.BusinessBrochureUrl = URL;
                                    }
                                    Company.IsActive = true;
                                    _CompanyService.InsertCompany(Company);

                                    CompanyID = Convert.ToInt32(Company.CompanyID);
                                    //SAVE company//

                                    //Insert CompanyEquipments
                                    if (CustomerModel.SelectedEquiments != null)
                                    {

                                        //foreach (var ItemId in CustomerModel.SelectedEquiments)
                                        //{
                                        //    CompanyEquipment CompanyEquipments = new CompanyEquipment();
                                        //    CompanyEquipments.ItemId = Convert.ToInt32(ItemId);
                                        //    CompanyEquipments.CompanyID = Convert.ToInt32(CompanyID);
                                        //    _CompanyEquipmentService.InsertCompanyEquipment(CompanyEquipments);
                                        //}
                                    }
                                    //insert CompanyLocation 
                                    if (CompanyID > 0)
                                    {
                                        if (CustomerModel.LocationLists != null)
                                        {


                                            foreach (var Locationlist in CustomerModel.LocationLists)
                                            {
                                                CompanyLocation companyLocation = new CompanyLocation();
                                                var _location = GoogleOperation.GetLatLong(Locationlist);

                                                var lat = _location[0];
                                                var log = _location[1];
                                                var address = Locationlist;//latlong1[2];

                                                companyLocation.CompanyID = CompanyID;
                                                companyLocation.Location = address.ToString();//"Latitude":30.71000000,"Longitude":76.69000000 
                                                companyLocation.Latitude = Convert.ToDecimal(lat);
                                                companyLocation.Longitude = Convert.ToDecimal(log);
                                                _CompanyLocationService.InsertCompanyLocation(companyLocation);


                                            }
                                        }
                                    }
                                    //End insert CompanyLocation 
                                    //update company_id in customer table
                                    var customers = _CustomerService.GetCustomers().Where(x => x.CustomerId == Convert.ToInt32(CustomerID));
                                    foreach (var custormer in customers)
                                    {
                                        var customer = _CustomerService.GetCustomer(custormer.CustomerId);
                                        customer.CompanyID = CompanyID;
                                        _CustomerService.UpdateCustomer(customer);
                                    }
                                    string UserPassword = SecurityFunction.DecryptString(user.Password);
                                    JobScheduler.Registration(Customer.FirstName, Customer.EmailId, UserPassword);// using Scheduler//
                                }
                                TempData["ShowMessage"] = "success";
                                TempData["MessageBody"] = "Manufacturer is saved successfully.";
                                return RedirectToAction("Index");
                            }

                        }
                    }
                    else
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = "vendors is already exists.";
                    }

                }
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            }
            catch (Exception ex)
            {
                var UserRole = _UserRoleService.GetUserRoles().Where(x => x.UserId == UserID).FirstOrDefault();
                if (UserRole != null)
                {
                    _UserRoleService.DeleteUserRole(UserRole); // delete user role
                }
                var User = _UserService.GetUsers().Where(x => x.UserId == UserID).FirstOrDefault();
                if (User != null)
                {
                    _UserService.DeleteUser(User); // delete user 
                }
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            //var Equiment = _IItemService.GetItems();
            //CustomerModel.EquimentList = Equiment.Select(x => new SelectListItem { Value = x.ItemId.ToString(), Text = x.ItemName }).ToList();
            return View(CustomerModel);
        }

        public ActionResult Edit(int id)
        {
            UserPermissionAction("vendor", RoleAction.edit.ToString());
            CheckPermission();
            if (id < 0)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Customer objVendor = _CustomerService.GetCustomers().Where(x => x.CompanyID == Convert.ToInt32(id)).FirstOrDefault();
            Customer objVendor = _CustomerService.GetCustomer(id);
            var models = new List<CustomerModel>();
            Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
            Onlo.Models.CustomerModel customermodel = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(objVendor);
            if (objVendor == null)
            {
                return HttpNotFound();

            }
            //var companyEquiment = _CompanyEquipmentService.GetCompanyEquipments();
            //var item = _IItemService.GetItems();
            //var Equiment = _IItemService.GetItems();
            //var selected = _CompanyEquipmentService.GetCompanyEquipments().Where(x => x.CompanyID == objVendor.CompanyID).Select(x => x.ItemId);
            var company = _CompanyService.GetCompany(objVendor.CompanyID);
            if (company != null)
            {
                customermodel.CompanyName = company.CompanyName;
                customermodel.AboutCompany = company.AboutCompany;

                customermodel.VisionMission = company.VisionMission;
                customermodel.LogoPath = company.LogoPath;
                customermodel.OtherInformation = company.OtherInformation;
                customermodel.BusinessBrochureUrl = company.BusinessBrochureUrl;
                //customermodel.EquimentList = _IItemService.GetItems().AsEnumerable().Select(c => new SelectListItem { Value = c.ItemId.ToString(), Text = c.ItemName, Selected = selected.Contains(c.ItemId) }).ToList();


            }
            var CompanyLocation = _CompanyLocationService.GetCompanyLocations();
            List<CompanyLocation> objcountrylist = (from data in CompanyLocation where data.CompanyID == objVendor.CompanyID select data).ToList();
            SelectList objmodeldata = new SelectList(objcountrylist, "Id", "CountryName", 0);
            /*Assign value to model*/
            CustomerModel objcountrymodel = new CustomerModel();
            customermodel.CompanyLocationList = _CompanyLocationService.GetCompanyLocations().Where(x => x.CompanyID == objVendor.CompanyID).Select(x => x.Location).ToList();


            return View(customermodel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "CustomerId,FirstName,EmailID,CompanyName,CompanyAddress,AboutCompany,OtherInformation,SelectedEquiments,LocationLists,VisionMission,LogoPath,BusinessBrochureUrl,CompanyID,UserId,CompanyLocationList")]CustomerModel CustomerModel, FormCollection form, HttpPostedFileBase file1, HttpPostedFileBase file2)
        {
            UserPermissionAction("vendor", RoleAction.edit.ToString());
            CheckPermission();

            //string CustomerID = "-1";
            //int UserID = 0;
            //int CompanyID = 0;
            CustomerModel.CompanyAddress = "qq";
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();


                    Customer customerFound = _CustomerService.GetCustomers().Where(x => x.EmailId == CustomerModel.EmailID && x.CustomerId != CustomerModel.CustomerId).FirstOrDefault();
                    Mapper.CreateMap<Onlo.Models.CustomerModel, Onlo.Entity.Customer>();
                    Onlo.Entity.Customer Customer = Mapper.Map<Onlo.Models.CustomerModel, Onlo.Entity.Customer>(CustomerModel);
                    Company Company = _CompanyService.GetCompanies().Where(x => x.CompanyID == CustomerModel.CompanyID).FirstOrDefault();
                    if (customerFound == null)
                    {
                        User User = _UserService.GetUsers().Where(x => x.UserEmailAddress == CustomerModel.EmailID && x.UserId != CustomerModel.UserId).FirstOrDefault();

                        if (User == null)
                        {
                            Customer Customers = _CustomerService.GetCustomers().Where(x => x.CustomerId == CustomerModel.CustomerId).FirstOrDefault();
                            Customers.FirstName = CustomerModel.FirstName;
                            _CustomerService.UpdateCustomer(Customers);

                            User user = _UserService.GetUsers().Where(x => x.UserId == CustomerModel.UserId).FirstOrDefault();

                            user.FirstName = CustomerModel.FirstName;
                            _UserService.UpdateUser(user);


                            Company.CompanyName = CustomerModel.CompanyName;
                            Company.AboutCompany = CustomerModel.AboutCompany;
                            Company.VisionMission = CustomerModel.VisionMission;
                            Company.OtherInformation = CustomerModel.OtherInformation;
                            _CompanyService.UpdateCompany(Company);

                        }
                        var shortPath1 = Company.LogoPath;
                        if (file1 != null)
                        {
                            if (Company.LogoPath != null)
                            {
                                string pathDel = Server.MapPath("~/CompanyLogo");

                                //string pathDel = Server.MapPath(Company.LogoPath);
                                FileInfo objfile = new FileInfo(pathDel);
                                if (objfile.Exists) //check file exsit or not
                                {
                                    objfile.Delete();
                                }
                            }
                            var fileExt = Path.GetExtension(file1.FileName);
                            string fileName = Guid.NewGuid() + fileExt;
                            var subPath = Server.MapPath("~/CompanyLogo");
                            //Check SubPath Exist or Not
                            if (!Directory.Exists(subPath))
                            {
                                Directory.CreateDirectory(subPath);
                            }
                            //End : Check SubPath Exist or Not
                            var path = Path.Combine(subPath, fileName);
                            var shortPath = "~/CompanyLogo/" + fileName;
                            file1.SaveAs(path);
                            CommonClass.CreateThumbnail(shortPath, 218, 84, false);
                            string URL = CommonClass.GetURL() + "/CompanyLogo/" + fileName;
                            //driver.PhotoPath = CommonClass.GetURL() + "/DriverPhoto/" + fileName;
                            Company.LogoPath = URL;
                            _CompanyService.UpdateCompany(Company);

                        }
                        var shortPathBrochure = Company.BusinessBrochureUrl;
                        if (file2 != null)
                        {
                            if (Company.LogoPath != null)
                            {
                                string pathDel = Server.MapPath("~/BusinessBrochure");

                                //string pathDel = Server.MapPath(Company.LogoPath);
                                FileInfo objfile = new FileInfo(pathDel);
                                if (objfile.Exists) //check file exsit or not
                                {
                                    objfile.Delete();
                                }
                            }
                            var fileExt = Path.GetExtension(file2.FileName);
                            string fileName = Guid.NewGuid() + fileExt;
                            var subPath = Server.MapPath("~/BusinessBrochure");
                            //Check SubPath Exist or Not
                            if (!Directory.Exists(subPath))
                            {
                                Directory.CreateDirectory(subPath);
                            }
                            //End : Check SubPath Exist or Not
                            var path = Path.Combine(subPath, fileName);
                            var shortPath = "~/BusinessBrochure/" + fileName;
                            file2.SaveAs(path);
                            CommonClass.CreateThumbnail(shortPath, 218, 84, false);
                            string URL = CommonClass.GetURL() + "/BusinessBrochure/" + fileName;
                            //driver.PhotoPath = CommonClass.GetURL() + "/DriverPhoto/" + fileName;
                            Company.BusinessBrochureUrl = URL;
                            _CompanyService.UpdateCompany(Company);
                        }
                        //  Update CompanyEquipments

                        //var CompanyEquipment = _CompanyEquipmentService.GetCompanyEquipments().Where(x => x.CompanyID == CustomerModel.CompanyID).AsEnumerable();

                        //if (CompanyEquipment != null)
                        //{
                        //    foreach (var CompanyEquipments in CompanyEquipment)
                        //    {
                        //        _CompanyEquipmentService.DeleteCompanyEquipment(CompanyEquipments);
                        //    }
                        //}

                        if (CustomerModel.SelectedEquiments != null)
                        {

                            //foreach (var ItemId in CustomerModel.SelectedEquiments)
                            //{
                            //    CompanyEquipment CompanyEquipments = new CompanyEquipment();
                            //    CompanyEquipments.ItemId = Convert.ToInt32(ItemId);
                            //    CompanyEquipments.CompanyID = CustomerModel.CompanyID;
                            //    _CompanyEquipmentService.InsertCompanyEquipment(CompanyEquipments);
                            //}
                        }
                        var _companyLocation = _CompanyLocationService.GetCompanyLocations().Where(x => x.CompanyID == CustomerModel.CompanyID).AsEnumerable();
                        if (_companyLocation != null)
                        {
                            foreach (var companyLocations in _companyLocation)
                            {
                                _CompanyLocationService.DeleteCompanyLocation(companyLocations);

                            }

                        }

                        if (CustomerModel.CompanyLocationList != null)
                        {
                            foreach (var Locationlist in CustomerModel.CompanyLocationList)
                            {
                                CompanyLocation companyLocation = new CompanyLocation();
                                var _location = GoogleOperation.GetLatLong(Locationlist);
                                var lat = _location[0];
                                var log = _location[1];
                                var address = Locationlist;//latlong1[2];
                                companyLocation.CompanyID = CustomerModel.CompanyID;
                                companyLocation.Location = address.ToString();//"Latitude":30.71000000,"Longitude":76.69000000 
                                companyLocation.Latitude = Convert.ToDecimal(lat);
                                companyLocation.Longitude = Convert.ToDecimal(log);
                                _CompanyLocationService.InsertCompanyLocation(companyLocation);
                            }
                        }
                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Manufacturer update  successfully.";
                        return RedirectToAction("Index");
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (customerFound.EmailId.Trim().ToLower() == CustomerModel.EmailID.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = customerFound.EmailId + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            //var companyEquiment = _CompanyEquipmentService.GetCompanyEquipments();
            //var item = _IItemService.GetItems();
            //var Equiment = _IItemService.GetItems();
            //var selected = _CompanyEquipmentService.GetCompanyEquipments().Where(x => x.CompanyID == CustomerModel.CompanyID).Select(x => x.ItemId);
            //CustomerModel.EquimentList = _IItemService.GetItems().AsEnumerable().Select(c => new SelectListItem { Value = c.ItemId.ToString(), Text = c.ItemName, Selected = selected.Contains(c.ItemId) }).ToList();
            return View(CustomerModel);

        }

        #endregion

        #region Send email functionaly
        public ActionResult SendEmailBysubscription(string CompanyName, string Email, string Name, string operation, string ShowMessage, string MessageBody)
        {
            UserPermissionAction("vendor", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<CustomerModel>();
            var Vendors = _CustomerService.GetCustomers().AsEnumerable();
            if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email))
            {
                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()));
            }
            else if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Name))
            {
                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()) && x.FirstName.Contains(Name.Trim()));
            }
            else if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Name))
            {
                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.FirstName.Contains(Name.Trim()));
            }
            else if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email))
            {
                Vendors = Vendors.Where(x => x.FirstName.Contains(Name.Trim()) && x.EmailId.Contains(Email.Trim()));
            }
            else if (!string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(Email))
            {
                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()) && x.EmailId.Contains(Email.Trim()));
            }
            else if (!string.IsNullOrEmpty(CompanyName))
            {
                Vendors = Vendors.Where(x => x.Companies.CompanyName.Contains(CompanyName.Trim()));
            }
            else if (!string.IsNullOrEmpty(Email))
            {
                Vendors = Vendors.Where(x => x.EmailId.Contains(Email.Trim()));
            }

            else if (!string.IsNullOrEmpty(Name))
            {
                Vendors = Vendors.Where(x => x.FirstName.Contains(Name.Trim()));
            }
            Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
            foreach (var Vendor in Vendors)
            {
                var _vendor = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(Vendor);

                var Company = _CompanyService.GetCompany(Vendor.CompanyID);
                if (Company != null)
                {
                    _vendor.CompanyName = Company.CompanyName;
                    _vendor.CompanyAddress = Company.CompanyAddress;
                    _vendor.LogoPath = Company.LogoPath;
                    _vendor.BusinessBrochureUrl = Company.BusinessBrochureUrl;

                }


                models.Add(_vendor);
            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
        }
        public ActionResult SendEmailToVendorForsubscription(int id)
        {
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var Customer = _CustomerService.GetCustomers().Where(x => x.CustomerId == id).FirstOrDefault();

            Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
            Onlo.Models.CustomerModel CustomerModel = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(Customer);

            string FirstName = Customer.FirstName;
            string EmailId = Customer.EmailId;
            string SubscriptionStartDate = Convert.ToString(Customer.SubscriptionStartDate);
            string SubscriptionEndDate = Convert.ToString(Customer.SubscriptionEndDate);
            string AccountRenewalDate = Convert.ToString(Customer.SubscriptionEndDate.Value.AddDays(1));
            var Payment = _MonthlySubscriptionService.GetMonthlySubscriptions().Where(x => x.SubscriptionMonth == DateTime.Now.ToString("MMMM")).FirstOrDefault();
            string Amount = Payment.SubscriptionPrice;
            JobScheduler.SubscriptionJob(FirstName, EmailId, SubscriptionStartDate, SubscriptionEndDate, AccountRenewalDate, Amount);
            TempData["ShowMessage"] = "success";
            TempData["MessageBody"] = "subscription mail send successfully.";
            return RedirectToAction("SendEmailBysubscription");

        }
        #endregion

        
    }
}