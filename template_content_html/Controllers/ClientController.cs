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
namespace Onlo.Controllers
{
    public class ClientController : BaseController
    {
        public ICustomerServices _CustomerService { get; set; }
        public ICompanyService _CompanyService { get; set; }
        public ICityService _CityService { get; set; }
        public ICountryService _CountryService { get; set; }
        public IStateService _StateService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
       
        // GET: Client
        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("client");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }

        public ClientController(IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, ICompanyService CompanyService,
           ICityService CityService, ICountryService CountryService, IStateService StateService, ICustomerServices CustomerService, IVIPsetupService VIPsetupService)
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
           
        }
        [ValidateInput(false)]
        //public ActionResult Index(string FirstName, string Email, string operation, string ShowMessage, string MessageBody)
        //{
        //    UserPermissionAction("client", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
        //    CheckPermission();
        //    var models = new List<CustomerModel>();
        //    var Clients = _CustomerService.GetCustomers().Where(x => x.RoleType == EnumValue.GetEnumDescription(EnumValue.RoleType.Client));
        //    if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(Email))
        //    {
        //        Clients = Clients.Where(x => x.FirstName.Contains(FirstName.Trim()) && x.EmailId.Contains(Email.Trim()));
        //    }

        //    else if (!string.IsNullOrEmpty(FirstName))
        //    {
        //        Clients = Clients.Where(x => x.FirstName.Contains(FirstName.Trim()));
        //    }
        //    else if (!string.IsNullOrEmpty(Email))
        //    {
        //        Clients = Clients.Where(x => x.EmailId.Contains(Email.Trim()));
        //    }
        //    Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
        //    foreach (var Client in Clients)
        //    {
        //        var _Client = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(Client);
        //        var Company = _CompanyService.GetCompany(Client.CompanyID);

        //        if (Company != null)
        //        {
        //            _Client.CompanyName = Company.CompanyName;
        //            _Client.CompanyAddress = Company.CompanyAddress;
        //            _Client.LogoPath = Company.LogoPath;

        //        }
              
        //        models.Add(_Client);
        //    }
        //    if (models.ToList().Count == 0)
        //    {
        //        TempData["ShowMessage"] = "error";
        //        TempData["MessageBody"] = "No Record found !";
        //    }
        //    return View(models);
        //}

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
        public ActionResult Details(int id)
        {
            UserPermissionAction("client", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = _CustomerService.GetCustomers().Where(c => c.UserId == id).FirstOrDefault();
            Mapper.CreateMap<Onlo.Entity.Customer, Onlo.Models.CustomerModel>();
            Onlo.Models.CustomerModel CustomerModel = Mapper.Map<Onlo.Entity.Customer, Onlo.Models.CustomerModel>(customer);
            var Company=_CompanyService.GetCompany(customer.CompanyID);
            if(Company!=null)
            {
            CustomerModel.CompanyName = Company.CompanyName;
            CustomerModel.CompanyAddress =Company.CompanyAddress;
            CustomerModel.LogoPath = Company.LogoPath;
            CustomerModel.VisionMission = Company.VisionMission;
            CustomerModel.AboutCompany = Company.AboutCompany;
            CustomerModel.OtherInformation =Company.OtherInformation;
            }
           
            return View(CustomerModel);
        }

    }
}