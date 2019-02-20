using AutoMapper;
using Onlo.Controllers;
using Onlo.Entity;
using Onlo.Models;
using Onlo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Onlo.Web.Controllers
{
    public class VIPsetupController : BaseController
    {
        public IVIPsetupService _VIPsetupService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IServiceItemService _ServiceItemService { get; set; }
        public ICustomerServices _CustomerServices { get; set; }
        public ICompanyService _CompanyService { get; set; }
        public IBranchService _BranchService { get; set; }
        public ICustomerServicesService _CustomerServicesService { get; set; }
        public IProductService _ProductService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public VIPsetupController(IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, IServiceItemService ServiceItemService, ICustomerServices CustomerServices, ICompanyService CompanyService, IBranchService BranchService, ICustomerServicesService CustomerServicesService, IProductService ProductService, ICategoryService CategoryService,IVIPsetupService VIPsetupService)
            : base( UserService, RoleService, FormService, RoleDetailService, UserRoleService,VIPsetupService)
        {
            this._VIPsetupService = VIPsetupService;
        }
        // GET: VIPsetup
        public ActionResult Index()
        {
            var VIPsetups = _VIPsetupService.GetVIPsetups().Where(v=>v.Flag==false).ToList();
            var models = new List<VIPsetupModel>();
            Mapper.CreateMap<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>();

            VIPsetups.ForEach(c => { models.Add(Mapper.Map<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>(c)); });
            return View(models);
            //List<VIPsetupModel> VIPsetupModels = new List<VIPsetupModel>();
            //var VIPsetups = _VIPsetupService.GetVIPsetups();
            //Mapper.CreateMap<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>();

            //foreach (var VIPsetup in VIPsetups)
            //{
            //    var _VIPsetup = Mapper.Map<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>(VIPsetup);
            //    VIPsetupModels.Add(VIPsetup);

            //}
            //return View(VIPsetupModels);
        }

        public ActionResult Delete(int VIPsetupId)
        {
           
                var VIP = _VIPsetupService.GetVIPsetups().Where(x => x.VIPsetupId == VIPsetupId).FirstOrDefault();
                if (VIP != null)
                {
                    //Mapper.CreateMap<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>();
                    //var _VIPsetup = Mapper.Map<Onlo.Entity.VIPsetup, Onlo.Models.VIPsetupModel>(VIP);
                    //_VIPsetupService.DeleteVIPsetup(VIP);
                    // delete VIPsetup
                    //VIPsetup VIPsetup = new VIPsetup();
                    VIP.Flag = true;
                    _VIPsetupService.UpdateVIPsetup(VIP);
                }
            
            return RedirectToAction("Index");
        }
    }
}