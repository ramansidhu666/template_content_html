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
    public class AccountController : BaseController
    {
        public ICategoryService _CategoryService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public AccountController(IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService,
            IUserRoleService UserRoleService, ICustomerService CustomerService, ICategoryService CategoryService, IAgencyIndividualService AgencyIndividualService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CategoryService = CategoryService;
            this._AgencyIndividualService = AgencyIndividualService;
        }

        [AllowAnonymous]
        public ActionResult LogOn()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _UserService.ValidateUser(model.UserName, SecurityFunction.EncryptString(model.Password));
                if (user != null)
                {
                    SetSessionVariables(model.UserName);
                    var tt = Session["RoleType"].ToString();
                    if (tt == "SuperAdmin")
                    {
                        return Json("superadmin", JsonRequestBehavior.AllowGet);
                    }
                    else if (tt == "Agency")
                    {
                        return Json("agency", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["RoleType"] = null;
                        Session["UserId"] = null;
                        return Json("notAllow", JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    Session["RoleType"] = null;
                    Session["UserId"] = null;
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            ModelState.AddModelError("Password", "The user name or password provided is incorrect.");

            // If we got this far, something failed, redisplay form
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(model);
            
        }

        public ActionResult LogOff()
        {
            System.Web.HttpContext.Current.Response.Cookies.Clear();

            Session["UserPermission"] = null;
            Session["UserId"] = null;
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("LogOn");
            //return RedirectToAction("Account", "LogOn");
        }

        public ActionResult ForgotPassword()
        {
            //UserPermissionAction("vendor", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            return View();
        }

        public ActionResult SignUp()
        {
            AgencyIndividualModel agencyIndividualModel = new Models.AgencyIndividualModel();
            List<CategoryModel> CategoryList = new List<CategoryModel>();

            var categories = _CategoryService.GetCategories();
            Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>();
            foreach (var category in categories)
            {
                CategoryModel categoryModel = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>(category);
                CategoryList.Add(categoryModel);
            }
            agencyIndividualModel.CategoryData = CategoryList;
            //UserPermissionAction("vendor", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            return View(agencyIndividualModel);
        }   

        [HttpPost]
        public ActionResult SignUp([Bind(Include = "FullName,EmailId,Password,WorkRate,ContactNumber,Address,CategoryId,IsAgency,Latitude,Longitude")]AgencyIndividualModel model)
        {
            AgencyIndividualModel agencyIndividualModel = new Models.AgencyIndividualModel();
            List<CategoryModel> CategoryList = new List<CategoryModel>();
            var categories = _CategoryService.GetCategories();
            //UserPermissionAction("vendor", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            if (string.IsNullOrEmpty(model.FullName))
            {
                ModelState.AddModelError("FullName", "");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.EmailId))
            {
                ModelState.AddModelError("EmailId", "");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "");
                return View(model);
            }
            if (model.WorkRate == null)
            {
                ModelState.AddModelError("WorkRate", "");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.ContactNumber))
            {
                ModelState.AddModelError("ContactNumber", "");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Address))
            {
                ModelState.AddModelError("Address", "");
                return View(model);
            }
            
            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = _UserService.GetUserByEmailId(model.EmailId);
                    if (existingUser == null)
                    {
                        UserModel userModel = new UserModel();
                        userModel.EmailId = model.EmailId;
                        userModel.Password = SecurityFunction.EncryptString(model.Password);
                        userModel.FirstName = model.FullName;
                        userModel.CompanyId = 2;
                        userModel.IsActive = true;
                        userModel.CreatedOn = DateTime.Now;
                        userModel.LastUpdatedOn = DateTime.Now;
                        Mapper.CreateMap<UserModel, User>();
                        var User = Mapper.Map<UserModel, User>(userModel);
                        _UserService.InsertUser(User);

                        var getUserDetail=_UserService.GetUserByEmailId(model.EmailId);
                        UserRoleModel userRoleModel = new UserRoleModel();
                        userRoleModel.UserId=getUserDetail.UserId;
                        userRoleModel.RoleId=4;
                        Mapper.CreateMap<UserRoleModel, UserRole>();
                        var UserRole = Mapper.Map<UserRoleModel, UserRole>(userRoleModel);
                        _UserRoleService.InsertUserRole(UserRole);

                        model.CreatedOn = DateTime.Now;
                        model.LastUpdatedOn = DateTime.Now;
                        model.Password = SecurityFunction.EncryptString(model.Password);
                        model.UserId = getUserDetail.UserId;
                        model.IsAgency = true;
                        model.IsActive = true;
                        model.IsInvited = true;
                        model.ParentId = new Guid();
                        Mapper.CreateMap<AgencyIndividualModel, AgencyIndividual>();
                        var agencyIndividual = Mapper.Map<AgencyIndividualModel, AgencyIndividual>(model);
                        _AgencyIndividualService.InsertAgencyIndividual(agencyIndividual);
                        TempData["MessageBody"] = "Registeration done.";
                        ViewBag.Error = TempData["MessageBody"];
                        return RedirectToAction("LogOn");
                    }
                    else
                    {
                      
                        TempData["MessageBody"] = "Email already exists.";
                        ViewBag.Error = TempData["MessageBody"];
                        return View(model);
                    }
                }
                else
                {
                    TempData["MessageBody"] = "Please fill the required fields.";
                    ViewBag.Error = TempData["MessageBody"];
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);
            }
           
            Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>();
            foreach (var category in categories)
            {
                CategoryModel categoryModel = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.CategoryModel>(category);
                CategoryList.Add(categoryModel);
            }
            model.CategoryData = CategoryList;
            TempData["MessageBody"] = "Something get wrong. please try again";
            ViewBag.Error = TempData["MessageBody"];
            return View(model);

        }
        [HttpPost]
        public ActionResult ForgotPassword([Bind(Include = "UserName")]ForgotPasswordModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserName))
                {
                    ModelState.AddModelError("Name", "Please enter email.");
                }
                if (ModelState.IsValid)
                {
                    string UserName = "";
                    var user = _UserService.GetUserByEmailId(model.UserName);
                    if (user != null) //By Email Id
                    {
                        UserName = user.FirstName; //Get User Name

                        //Send Email to User
                        string Password = SecurityFunction.DecryptString(user.Password);
                        CommonCls.SendMailToUser(UserName, model.UserName, Password);
                        TempData["MessageBody"] = "Your password has been sent to this email address.";
                        ViewBag.Error = TempData["MessageBody"];

                    }
                    else
                    {
                        TempData["MessageBody"] = "This email address does not exist in our records.";
                        ViewBag.Error = TempData["MessageBody"];
                    }
                }

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return View(model);
        }
       

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel changepasswordmodel)
        {
            try
            {
                if (string.IsNullOrEmpty(changepasswordmodel.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Please enter old password.");
                }
                if (string.IsNullOrEmpty(changepasswordmodel.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Please enter new password.");
                }
                if (string.IsNullOrEmpty(changepasswordmodel.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Please enter confirm password.");
                }
                //if (!string.IsNullOrEmpty(changepasswordmodel.NewPassword) && changepasswordmodel.NewPassword.Length < 4)
                //{
                //    ModelState.AddModelError("NewPassword", "Please enter minimum Length 4 in new/confirm password .");

                //}
                if (!string.IsNullOrEmpty(changepasswordmodel.NewPassword) && !string.IsNullOrEmpty(changepasswordmodel.ConfirmPassword))
                {
                    if (changepasswordmodel.NewPassword != changepasswordmodel.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "New password should match confirm password.");
                    }
                }
                int UserId = Convert.ToInt32(Session["UserId"].ToString());

                var user = _UserService.GetUserById(UserId);
                var pasword = SecurityFunction.DecryptString(user.Password);
                if (pasword != changepasswordmodel.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Please enter valid old password.");
                }
                if (ModelState.IsValid)
                {
                    if (user != null)
                    {
                        user.Password = SecurityFunction.EncryptString(changepasswordmodel.NewPassword);
                        _UserService.UpdateUser(user);
                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = "Password changed successfully.";
                        return RedirectToAction("LogOn");
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);
            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            return View(changepasswordmodel);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        private UserModel SetupFormsAuthTicket(string userName, bool persistanceFlag)
        {
            var user = _UserService.GetUserByEmailId(userName);
            Mapper.CreateMap<HomeHelp.Entity.User, HomeHelp.Models.UserModel>();
            HomeHelp.Models.UserModel userModel = Mapper.Map<HomeHelp.Entity.User, HomeHelp.Models.UserModel>(user);

            var userId = userModel.UserId;
            var userData = userId.ToString(CultureInfo.InvariantCulture);
            var authTicket = new FormsAuthenticationTicket(1, //version
                                                        userName, // user name
                                                        DateTime.Now,             //creation
                                                        DateTime.Now.AddMinutes(30), //Expiration
                                                        persistanceFlag, //Persistent
                                                        userData);

            var encTicket = FormsAuthentication.Encrypt(authTicket);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

            return userModel;
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
