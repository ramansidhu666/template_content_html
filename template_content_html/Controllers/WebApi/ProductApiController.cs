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
using HomeHelp.Entity;
using HomeHelp.Services;
using Newtonsoft.Json;
using AutoMapper;
using HomeHelp.Models;
using HomeHelp.Infrastructure;
using HomeHelp.Core.Infrastructure;
using System.IO;
using System.Drawing;
using System.Configuration;
using System.Net.Mail;
using System.Web.Configuration;
using HomeHelp.Data;
using OilAndGas.Core.UtilityManager;

namespace HomeHelp.Controllers.WebApi
{
    [RoutePrefix("Product")]
    public class ProductApiController : ApiController
    {
        public IUserService _UserService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IRequestService _RequestService { get; set; }
        public IReviewAndRatingService _ReviewAndRatingService { get; set; }
         public INotificationService _NotificationService { get; set; }
         public IProductService _ProductService { get; set; }
         public IPurchaseService _PurchaseService { get; set; }
         public ProductApiController(IPurchaseService PurchaseService,IProductService ProductService,INotificationService NotificationService, IReviewAndRatingService ReviewAndRatingService, IRequestService RequestService, ICategoryService CategoryService, ICustomerService CustomerService, IUserService UserService, IUserRoleService UserRoleService)
        {
            this._PurchaseService = PurchaseService;
            this._ProductService = ProductService;
            this._ReviewAndRatingService = ReviewAndRatingService;
            this._RequestService = RequestService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._UserRoleService = UserRoleService;
            this._CategoryService = CategoryService;
            this._NotificationService = NotificationService;
        }

      
        [Route("GetProductList")]
        [HttpGet]
         public HttpResponseMessage GetProductList( [FromUri] int PageNumber)
        {
            try
            {
                //if (ServiceProviderId == 0)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ServiceProvider Id is blank"), Configuration.Formatters.JsonFormatter);
                //}
               
                //var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                //if (Customer != null)
                //{
                    //if (Customer.IsActive)
                    //{
                        var products = _ProductService.GetProducts().ToList();
                        if (products.Count() > 0)
                        {
                            var list = new List<ProductResponseModel>();
                            foreach (var product in products)
                            {
                                Mapper.CreateMap<Product, ProductResponseModel>();
                                list.Add(Mapper.Map<Product, ProductResponseModel>(product));

                            }
                            int numberOfObjectsPerPage = 10;
                            var modelsdata = list.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "no products found."), Configuration.Formatters.JsonFormatter);
                        }
                //    }
                //    else
                //    {
                //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                //    }
                //}
                //else
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                //}
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("PurchaseProduct")]
        [HttpPost]
        public HttpResponseMessage PurchaseProduct(PurchaseModel PurchaseModel) 
        {
            try
            {
                if (PurchaseModel.ProductId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Product Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.CustomerId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.Size == ""||PurchaseModel.Size==null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Size is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.Size != EnumValue.GetEnumDescription(EnumValue.ProductSizes.L) && PurchaseModel.Size != EnumValue.GetEnumDescription(EnumValue.ProductSizes.M) && PurchaseModel.Size != EnumValue.GetEnumDescription(EnumValue.ProductSizes.XL) && PurchaseModel.Size != EnumValue.GetEnumDescription(EnumValue.ProductSizes.XXL))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Size is wrong."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.Quantity == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Product Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.Latitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.Longitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (PurchaseModel.PaymentId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Payment Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(PurchaseModel.CustomerId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var products = _ProductService.GetProducts().Where(p=>p.ProductId==PurchaseModel.ProductId).FirstOrDefault();
                        if (products!=null)
                        {
                            Mapper.CreateMap<PurchaseModel, Purchase>();
                            var purchase = Mapper.Map<PurchaseModel, Purchase>(PurchaseModel);
                            _PurchaseService.InsertPurchase(purchase);
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully purchased."), Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "no products found."), Configuration.Formatters.JsonFormatter);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }



    }
}