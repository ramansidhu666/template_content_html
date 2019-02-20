using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

using HomeHelp.Models;
using HomeHelp.Services;
using HomeHelp.Controllers;
using HomeHelp.Infrastructure;
using HomeHelp.Entity;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using HomeHelp.Core.Infrastructure;
using AutoMapper;
using System.Configuration;

namespace HomeHelp.Controllers
{
    public class SuperAdminController : BaseController
    {
        public ICategoryService _CategoryService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public SuperAdminController(IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService,
            IUserRoleService UserRoleService, ICustomerService CustomerService, ICategoryService CategoryService, IAgencyIndividualService AgencyIndividualService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CustomerService = CustomerService;
            this._CategoryService = CategoryService;
            this._AgencyIndividualService = AgencyIndividualService;
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
        public ActionResult Dashboard(int? key)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (key == 1)
            {
                Session["CurrentPageNumber"] = null;
            }
            if (Session["CurrentPageNumber"] == null)
            {
                ViewBag.currentPageNumber = 0; 
            }
            else
            {
                ViewBag.currentPageNumber = Convert.ToInt32(Session["CurrentPageNumber"]);
            }
            List<AgencyIndividualModel> AgencyIndividualModels = new List<AgencyIndividualModel>();
            var Users = _AgencyIndividualService.GetAgencyIndividuals().Where(c=>c.IsAgency==true);
            Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
            foreach (var User in Users)
            {
                var _User = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(User);
                AgencyIndividualModels.Add(_User);

            }
            return View(AgencyIndividualModels);
        }
                
        public ActionResult Individuals(int? key)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (key == 1)
            {
                Session["CurrentPageNumber"] = null;
            }
            if (Session["CurrentPageNumber"] == null)
            {
                ViewBag.currentPageNumber = 0;
            }
            else
            {       
                ViewBag.currentPageNumber = Convert.ToInt32(Session["CurrentPageNumber"]);
            }
            List<CustomerModel> customerModels = new List<CustomerModel>();
            var Users = _CustomerService.GetCustomers();
            Mapper.CreateMap<Customer, CustomerModel>();
            foreach (var User in Users)
            {
                var _User = Mapper.Map<Customer, CustomerModel>(User);
                customerModels.Add(_User);

            }
            return View(customerModels);
        }   

        [HttpGet]
        public ActionResult Unblock(Guid id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            ViewBag.currentPageNumber = ViewBag.currentPageNumber;
            //UserPermissionAction("Account", RoleAction.detail.ToString());
            //CheckPermission();

            AgencyIndividual objAgencyIndividual = _AgencyIndividualService.GetAgencyIndividual(id);
            try
            {
                if (objAgencyIndividual != null)
                {
                    objAgencyIndividual.IsActive = true;
                    _AgencyIndividualService.UpdateAgencyIndividual(objAgencyIndividual);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully activated.";
                    CommonCls.SendMailOfAccountIsActive(objAgencyIndividual.FullName, objAgencyIndividual.EmailId, "activated");
                    return RedirectToAction("Dashboard");
                   
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Dashboard");

        }

        [HttpGet]
        public ActionResult Block(Guid id)
        {
            
            //UserPermissionAction("Account", RoleAction.detail.ToString());
            //CheckPermission();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            AgencyIndividual objAgencyIndividual = _AgencyIndividualService.GetAgencyIndividual(id);
            try
            {
                if (objAgencyIndividual != null)
                {
                    objAgencyIndividual.IsActive = false;
                    _AgencyIndividualService.UpdateAgencyIndividual(objAgencyIndividual);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully deactivated.";
                    CommonCls.SendMailOfAccountIsActive(objAgencyIndividual.FullName, objAgencyIndividual.EmailId, "deactivated");

                   
                    return RedirectToAction("Dashboard");
                   
                }

            }
            catch (Exception ex)
            {   
                ErrorLogging.LogError(ex);
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Dashboard");

        }

        [HttpGet]
        public ActionResult UnblockCustomer(Guid id)
        {
           
            //UserPermissionAction("Account", RoleAction.detail.ToString());
            //CheckPermission();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            Customer objCustomer = _CustomerService.GetCustomer(id);
            try
            {
                if (objCustomer != null)
                {
                    objCustomer.IsActive = true;
                    _CustomerService.UpdateCustomer(objCustomer);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully activated.";
                    CommonCls.SendMailOfAccountIsActive(objCustomer.FirstName, objCustomer.EmailId, "activated");
                    return RedirectToAction("Individuals");
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                RedirectToAction("Individuals");
            }
            return RedirectToAction("Individuals");

        }

        [HttpGet]
        public ActionResult BlockCustomer(Guid id)
        {
            
            //UserPermissionAction("Account", RoleAction.detail.ToString());
            //CheckPermission();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            Customer objCustomer = _CustomerService.GetCustomer(id);
            try
            {
                if (objCustomer != null)
                {   
                    objCustomer.IsActive = false;
                    _CustomerService.UpdateCustomer(objCustomer);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully deactivated.";
                    CommonCls.SendMailOfAccountIsActive(objCustomer.FirstName, objCustomer.EmailId, "deactivated");
                    string UserMessage = "Your account has been deactivated by admin.";
                    string Message = "{\"flag\":\"" + "Deactivate" + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                    var customerTo = objCustomer;

                    if (customerTo.ApplicationId != null && customerTo.ApplicationId != "")
                    {

                        if (customerTo.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                        {
                            //Send Notification another Andriod
                            CommonCls.SendFCM_Notifications(customerTo.ApplicationId, Message, true);
                        }
                        else
                        {
                            string Msg = UserMessage;

                            CommonCls.TestSendFCM_Notifications(customerTo.ApplicationId, Message, Msg, true);
                        }
                    }
                    return RedirectToAction("Individuals");
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                RedirectToAction("Individuals");
            }
            return RedirectToAction("Individuals");

        }

        [HttpGet]
        public ActionResult Pagination(int currentPageNumber, string currentView)
        {
            Session["CurrentPageNumber"] = currentPageNumber;
            return RedirectToAction(currentView);

        }
        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
