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
    [RoutePrefix("Customer")]
    public class CategoryApiController : ApiController
    {
        public IUserService _UserService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        // public INotificationService _NotificationService { get; set; }
        public CategoryApiController(IAgencyIndividualService AgencyIndividualService,ICategoryService CategoryService,ICustomerService CustomerService, IUserService UserService, IUserRoleService UserRoleService)
        {
            this._AgencyIndividualService = AgencyIndividualService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._UserRoleService = UserRoleService;
            this._CategoryService = CategoryService;
            //this._NotificationService = NotificationService;
        }

        [Route("GetAllCategories")]
        [HttpGet]   
        public HttpResponseMessage GetAllCategories()
        {
            try
            {
                var categories = _CategoryService.GetCategories();
                var models = new List<categoryResponseModule>();
                Mapper.CreateMap<HomeHelp.Entity.Category, HomeHelp.Models.categoryResponseModule>();
                foreach (var category in categories)
                {
                    var categoryModel = Mapper.Map<HomeHelp.Entity.Category, HomeHelp.Models.categoryResponseModule>(category);
                    models.Add(categoryModel);
                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again."), Configuration.Formatters.JsonFormatter);
            }

        }

        //[Route("GetServiceProviderByCategory")]
        //[HttpGet]
        //public HttpResponseMessage GetServiceProviderByCategory([FromUri] int CategoryId, [FromUri] int PageNumber)
        //{
        //    try
        //    {
        //        var category = _CategoryService.GetCategory(CategoryId);
        //        if (category != null)
        //        {
        //            var customerList = new List<UserCustomerModel>();
        //            var customers = _CustomerService.GetCustomers().Where(c => c.CategoryId == CategoryId && c.IsActive == true && c.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider)).ToList();
        //            foreach (var customer in customers)
        //            {
        //                Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>();
        //                UserCustomerModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>(customer);
        //                CustomerResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32( customer.CategoryId)).Name;
        //                customerList.Add(CustomerResponseModel);
                        
        //            }

        //            int numberOfObjectsPerPage = 10;
        //            var modelsdata = customerList.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);

        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "category not found."), Configuration.Formatters.JsonFormatter);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        [Route("GetServiceProviderByCategory")]
        [HttpGet]
        public HttpResponseMessage GetServiceProviderByCategory([FromUri] int CategoryId, [FromUri] int PageNumber)
        {
            try
            {
                var category = _CategoryService.GetCategory(CategoryId);
                if (category != null)
                {
                    var customerList = new List<NearByModel>();
                    var customers = _AgencyIndividualService.GetAgencyIndividuals().Where(a => a.IsActive == true).ToList();
                    foreach (var customer in customers)
                    {
                        Mapper.CreateMap<HomeHelp.Entity.AgencyIndividual, HomeHelp.Models.NearByModel>();
                        HomeHelp.Models.NearByModel NewResponseModel = Mapper.Map<HomeHelp.Entity.AgencyIndividual, HomeHelp.Models.NearByModel>(customer);
                        var agencyMembers = new List<int?>();
                        if (!customer.IsAgency)
                        {
                            NewResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(customer.CategoryId)).Name;
                        }
                        else
                        {
                            //string categoryId = "";
                            //string categoryName = "";

                             agencyMembers = _AgencyIndividualService.GetAgencyIndividuals().Where(c => c.ParentId == customer.AgencyIndividualId).Select(c => c.CategoryId).ToList();
                            //foreach (var item in agencyMembers.Distinct())
                            //{
                            //    categoryId = categoryId + "," + item;
                            //    var name = _CategoryService.GetCategory(Convert.ToInt32(item)).Name;
                            //    categoryName = categoryName + "," + name;
                            //}
                            //NewResponseModel.CategoryId = categoryId.TrimEnd(',').TrimStart(',');
                            //NewResponseModel.CategoryName = categoryName.TrimEnd(',').TrimStart(',');
                            NewResponseModel.CategoryId = CategoryId.ToString();
                            NewResponseModel.CategoryName = _CategoryService.GetCategory(CategoryId).Name;
                        }

                        if ((!customer.IsAgency && customer.ParentId == new Guid() && customer.CategoryId == CategoryId) || agencyMembers.Distinct().Contains(CategoryId))
                                {
                                    if (customer.IsAgency)
                                    {
                                        NewResponseModel.CustomerId = customer.AgencyIndividualId;
                                        NewResponseModel.CompanyName = customer.FullName;
                                        NewResponseModel.FirstName = customer.FullName;
                                        NewResponseModel.MobileNumber = customer.ContactNumber;
                                        NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.Agency);
                                        NewResponseModel.CustomerId = customer.AgencyIndividualId;

                                    }
                                    else
                                    {
                                        var cust = _CustomerService.GetCustomers().Where(c => c.UserId == customer.UserId).FirstOrDefault();
                                        if (cust != null)
                                        {
                                            NewResponseModel.PhotoPath = cust.PhotoPath;
                                            NewResponseModel.WorkRate = cust.WorkRate;
                                            NewResponseModel.FirstName = cust.FirstName;
                                            NewResponseModel.LastName = cust.LastName;
                                            NewResponseModel.FacebookId = cust.FacebookId;
                                            NewResponseModel.DeviceSerialNo = cust.DeviceSerialNo;
                                            NewResponseModel.ApplicationId = cust.ApplicationId;
                                            NewResponseModel.DeviceType = cust.DeviceType;
                                            NewResponseModel.CategoryId = cust.CategoryId.ToString();
                                            NewResponseModel.CustomerId = cust.CustomerId;
                                        }
                                      
                                        NewResponseModel.MobileNumber = customer.ContactNumber;
                                        NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider);
                                    }
                                    customerList.Add(NewResponseModel);
                                }

                    }

                    int numberOfObjectsPerPage = 10;
                    var modelsdata = customerList.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "category not found."), Configuration.Formatters.JsonFormatter);
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