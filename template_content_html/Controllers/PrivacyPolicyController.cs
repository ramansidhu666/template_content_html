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
    public class PrivacyPolicyController : BaseController
    {
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        //public IServiceItemService _ServiceItemService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public IRequestService _RequestService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IPrivacyPolicyService _PrivacyPolicyService { get; set; }

        public PrivacyPolicyController(IPrivacyPolicyService PrivacyPolicyService,IRequestService RequestService,ICategoryService CategoryService ,ICustomerService CustomerServices,IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService, IUserRoleService UserRoleService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._PrivacyPolicyService = PrivacyPolicyService;
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

        #region Basic crude oprations of category
        public ActionResult Index(string Name, string operation, string ShowMessage, string MessageBody, string CurrentPageNo)
        {
            UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();
            return View();
        }

        [HttpPost]
        public string GetPolicyData()
        {
            var isExists = _PrivacyPolicyService.GetPrivacyPolicies().Select(c => c.Content).FirstOrDefault();
            return isExists;
        }

        [HttpGet]
        public ActionResult Create()
        {
           
            var isExists = _PrivacyPolicyService.GetPrivacyPolicies().FirstOrDefault();
            Mapper.CreateMap<PrivacyPolicy, PrivacyPolicyModel>();
            var PrivacyPolicies = Mapper.Map<PrivacyPolicy, PrivacyPolicyModel>(isExists);

            return View(PrivacyPolicies);
        }

        [HttpPost]
        public ActionResult Create(PrivacyPolicyModel PrivacyPolicyModel)
        {

            
            try
            {
                if (string.IsNullOrWhiteSpace(PrivacyPolicyModel.Content))
                {
                    ModelState.AddModelError("content", "Please enter content.");
                }
                if (ModelState.IsValid)
                {
                    var isExists = _PrivacyPolicyService.GetPrivacyPolicies().FirstOrDefault();
                    Mapper.CreateMap<PrivacyPolicyModel, PrivacyPolicy>();
                    var PrivacyPolicies = Mapper.Map<PrivacyPolicyModel, PrivacyPolicy>(PrivacyPolicyModel);

                    if (isExists != null)
                    {
                        isExists.Content = PrivacyPolicies.Content;
                        _PrivacyPolicyService.UpdatePrivacyPolicy(isExists);
                    }
                    else
                    {
                        _PrivacyPolicyService.InsertPrivacyPolicy(PrivacyPolicies);
                    }


                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " PrivacyPolicy saved  successfully.";
                    return View(PrivacyPolicyModel);
                }
                else
                {
                    return View(PrivacyPolicyModel);
                }
            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);


            return View(PrivacyPolicyModel);
        }

        #endregion

        
    }
}