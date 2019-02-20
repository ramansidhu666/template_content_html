using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Friendlier.Entity;
using Friendlier.Services;
using Newtonsoft.Json;
using Friendlier.Models;
using AutoMapper;
using Friendlier.Infrastructure;
using Friendlier.Core.Infrastructure;

namespace Friendlier.Controllers.WebApi
{
    [RoutePrefix("Credit")]
    public class CreditApiController : ApiController
    {

        public ICustomerService _CustomerService { get; set; }
        public IPackageService _PackageService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public ICreditService _CreditService { get; set; }

        public CreditApiController(ICreditService CreditService ,IPackageService PackageService, INotificationService NotificationService, ICustomerService CustomerService)
        {
            this._CreditService = CreditService;
            this._NotificationService = NotificationService;

            this._CustomerService = CustomerService;

            this._PackageService = PackageService;
        }

        [Route("GetAllPackages")]
        [HttpGet]
        public HttpResponseMessage GetAllPackages()
        {
            var models = new List<PackageModel>();
            try
            {
                var Packages = _PackageService.GetPackages();
                Mapper.CreateMap<Friendlier.Entity.Package, Friendlier.Models.PackageModel>();
                foreach (var Package in Packages)
                {
                    Friendlier.Models.PackageModel PackageModel = Mapper.Map<Friendlier.Entity.Package, Friendlier.Models.PackageModel>(Package);
                    models.Add(PackageModel);

                }
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("SaveCredit")]
        [HttpPost]
        public HttpResponseMessage SaveCredit([FromBody]CreditModel CreditModel)
        {
            try
            {
                if (CreditModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (CreditModel.Credits == null || CreditModel.Credits == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Credit is blank"), Configuration.Formatters.JsonFormatter);
                }
               
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CreditModel.CustomerId).FirstOrDefault();
                if (customer != null)
                {
                    var credits = _CreditService.GetCredits().Where(c => c.CustomerId == CreditModel.CustomerId).FirstOrDefault();
                    if(credits==null)
                    {
                        Mapper.CreateMap<CreditModel, Credit>();
                        var credit = Mapper.Map<CreditModel, Credit>(CreditModel);
                        _CreditService.InsertCredit(credit);
                        Mapper.CreateMap<Credit, CreditResponseModel>();
                        CreditResponseModel CreditResponseModel = Mapper.Map<Credit, CreditResponseModel>(credit);

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CreditResponseModel), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        credits.Credits=(CreditModel.Credits!=null|| CreditModel.Credits!="")?CreditModel.Credits:credits.Credits;
                        _CreditService.UpdateCredit(credits);
                        Mapper.CreateMap<Credit, CreditResponseModel>();
                        CreditResponseModel CreditResponseModel = Mapper.Map<Credit, CreditResponseModel>(credits);

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CreditResponseModel), Configuration.Formatters.JsonFormatter);
                    }
                  

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer id is wrong."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("GetCreditCustomerId")]
        [HttpGet]
        public HttpResponseMessage GetCreditCustomerId([FromUri] int CustomerId)
        {
            try
            {
                var models = new List<CreditResponseModel>();
                var customer = _CustomerService.GetCustomer(CustomerId);
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                var Credits = _CreditService.GetCredits().Where(l => l.CustomerId == CustomerId).ToList();
                if (Credits.Count() > 0)
                {
                    foreach (var Credit in Credits)
                    {
                        Mapper.CreateMap<Friendlier.Entity.Credit, Friendlier.Models.CreditResponseModel>();
                        CreditResponseModel CreditResponseModel = Mapper.Map<Friendlier.Entity.Credit, Friendlier.Models.CreditResponseModel>(Credit);
                        
                        models.Add(CreditResponseModel);

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Credit not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }
    }
}