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
    public class MonthlySubscriptionController : BaseController
    {
        public ICustomerServices _CustomerService { get; set; }
        public ICompanyService _CompanyService { get; set; }
        public ICityService _CityService { get; set; }
        public ICountryService _CountryService { get; set; }
        public IStateService _StateService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IMonthlySubscriptionService _MonthlySubscriptionService { get; set; }
       
        public ICompanyLocationService _CompanyLocationService { get; set; }

        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("MonthlySubscription");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }
        public MonthlySubscriptionController(IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, ICompanyService CompanyService,
           ICityService CityService, ICountryService CountryService, IStateService StateService, ICustomerServices CustomerService, IMonthlySubscriptionService IMonthlySubscriptionService, ICompanyLocationService CompanyLocationService, IMonthlySubscriptionService MonthlySubscriptionService, IVIPsetupService VIPsetupService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService, VIPsetupService)
        {
            this._CompanyService = CompanyService;
            this._CityService = CityService;
            this._CountryService = CountryService;
            this._StateService = StateService;
            this._MonthlySubscriptionService = MonthlySubscriptionService;
            this._UserRoleService = UserRoleService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            
           
            this._CompanyLocationService = CompanyLocationService;
        }
        public ActionResult Index(string MonthlySubscriptionName, string operation, string ShowMessage, string MessageBody)
        {
            UserPermissionAction("MonthlySubscription", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<MonthlySubscriptionModel>();
            var MonthlySubscriptions = _MonthlySubscriptionService.GetMonthlySubscriptions();
            if (!string.IsNullOrEmpty(MonthlySubscriptionName))
            {
                MonthlySubscriptions = MonthlySubscriptions.Where(x => x.SubscriptionMonth.Contains(MonthlySubscriptionName.Trim())).ToList();
            }
            Mapper.CreateMap<Onlo.Entity.MonthlySubscription, Onlo.Models.MonthlySubscriptionModel>();
            foreach (var MonthlySubscription in MonthlySubscriptions)
            {
                var _MonthlySubscription = Mapper.Map<Onlo.Entity.MonthlySubscription, Onlo.Models.MonthlySubscriptionModel>(MonthlySubscription);
                models.Add(_MonthlySubscription);
            }
            if (models.ToList().Count==0)
            {
                TempData["ShowMessage"] = "error";
               TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
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
        [HttpGet]
        public ActionResult Create() 
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "MonthlySubscriptionId,SubscriptionMonth,SubscriptionPrice")]MonthlySubscriptionModel MonthlySubscriptionModel)
        {

            UserPermissionAction("MonthlySubscription", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();
                    Mapper.CreateMap<Onlo.Models.MonthlySubscriptionModel, Onlo.Entity.MonthlySubscription>();
                    Onlo.Entity.MonthlySubscription MonthlySubscriptions = Mapper.Map<Onlo.Models.MonthlySubscriptionModel, Onlo.Entity.MonthlySubscription>(MonthlySubscriptionModel);
                    MonthlySubscription Monthly = _MonthlySubscriptionService.GetMonthlySubscriptions().Where(x => x.SubscriptionMonth == MonthlySubscriptions.SubscriptionMonth).FirstOrDefault();

                    if (Monthly == null)
                    {
                        MonthlySubscriptions.IsActive = true;
                        _MonthlySubscriptionService.InsertMonthlySubscription(MonthlySubscriptions);


                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Monthly Subscription save  successfully.";
                        return RedirectToAction("Index");
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (Monthly.SubscriptionMonth.Trim().ToLower() == MonthlySubscriptionModel.SubscriptionMonth.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = Monthly.SubscriptionMonth + " already exist.";
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

            return View(MonthlySubscriptionModel);
        }
        //public ActionResult Details(int id)
        //{
        //    UserPermissionAction("MonthlySubscription", RoleAction.detail.ToString());
        //    CheckPermission();
        //    if (id < 0)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var models = new CustomerModel();
        //    var customer = _CustomerService.GetCustomers().Where(c => c.UserId == id).FirstOrDefault();
        //    Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();

        //    var _CustomerModel = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(customer);

        //    _CustomerModel.CompanyName = _CompanyService.GetCompany(customer.CompanyID).CompanyName;
        //    _CustomerModel.CompanyAddress = _CompanyService.GetCompany(customer.CompanyID).CompanyAddress;
        //    _CustomerModel.LogoPath = _CompanyService.GetCompany(customer.CompanyID).LogoPath;
        //    _CustomerModel.VisionMission = _CompanyService.GetCompany(customer.CompanyID).VisionMission;
        //    _CustomerModel.AboutCompany = _CompanyService.GetCompany(customer.CompanyID).AboutCompany;
        //    _CustomerModel.OtherInformation = _CompanyService.GetCompany(customer.CompanyID).OtherInformation;
        //    var CompanyID = _CompanyService.GetCompany(customer.CompanyID).CompanyID;
        //   // var CompanyEquimentList = _CompanyEquipmentService.GetCompanyEquipments().Where(x => x.CompanyID == Convert.ToInt32(CompanyID));
        //    if (CompanyEquimentList != null)
        //    {
        //        //var CompanyEquiment = new List<CompanyEquipmentModel>();
        //        //Mapper.CreateMap<Onlo.Entity.CompanyEquipment, Onlo.Models.CompanyEquipmentModel>();
        //        //foreach (var companyEquipment in CompanyEquimentList)
        //        //{
        //        //    //var Equiment = Mapper.Map<Onlo.Entity.CompanyEquipment, Onlo.Models.CompanyEquipmentModel>(companyEquipment);
        //        //    //Equiment.MonthlySubscriptionName = _MonthlySubscriptionService.GetMonthlySubscription(companyEquipment.MonthlySubscriptionId).MonthlySubscriptionName;
        //        //    //CompanyEquiment.Add(Equiment);

        //        //}
        //        //_CustomerModel.CompanyEquipment = CompanyEquiment;
        //    }
        //    models = _CustomerModel;
        //    return View(models);
        //}
        [HttpGet]
        public ActionResult Edit(int id)
        {
            UserPermissionAction("MonthlySubscription", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DateTime dt = DateTime.Now;

            var month = dt.ToString("MMMM");
            

            MonthlySubscription objMonthlySubscription = _MonthlySubscriptionService.GetMonthlySubscription(id);

            var models = new List<MonthlySubscriptionModel>();
            Mapper.CreateMap<Onlo.Entity.MonthlySubscription, Onlo.Models.MonthlySubscriptionModel>();
            Onlo.Models.MonthlySubscriptionModel MonthlySubscriptionModelrmodel = Mapper.Map<Onlo.Entity.MonthlySubscription, Onlo.Models.MonthlySubscriptionModel>(objMonthlySubscription);
            if (objMonthlySubscription == null)
            {
                return HttpNotFound();

            }

            return View(MonthlySubscriptionModelrmodel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "MonthlySubscriptionId,SubscriptionMonth,SubscriptionPrice")]MonthlySubscriptionModel MonthlySubscriptionModel)
        {
            UserPermissionAction("MonthlySubscription", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();


                   
                    Mapper.CreateMap<Onlo.Models.MonthlySubscriptionModel, Onlo.Entity.MonthlySubscription>();
                    Onlo.Entity.MonthlySubscription MonthlySubscriptions = Mapper.Map<Onlo.Models.MonthlySubscriptionModel, Onlo.Entity.MonthlySubscription>(MonthlySubscriptionModel);
                    MonthlySubscription MonthlySubscriptionFound = _MonthlySubscriptionService.GetMonthlySubscriptions().Where(x => x.SubscriptionMonth == MonthlySubscriptions.SubscriptionMonth && x.MonthlySubscriptionId != MonthlySubscriptions.MonthlySubscriptionId).FirstOrDefault();
                    if (MonthlySubscriptionFound == null)
                    {
                        MonthlySubscriptions.IsActive = true;
                        _MonthlySubscriptionService.UpdateMonthlySubscription(MonthlySubscriptions);   
 
                       
                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Monthly Subscription update  successfully.";
                        return RedirectToAction("Index");
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (MonthlySubscriptionFound.SubscriptionMonth.Trim().ToLower() == MonthlySubscriptionModel.SubscriptionMonth.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = MonthlySubscriptionFound.SubscriptionMonth + " already exist.";
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
           
            return View(MonthlySubscriptionModel);

        }
        
    }
}