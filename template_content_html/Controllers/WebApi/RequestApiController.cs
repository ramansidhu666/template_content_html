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

namespace HomeHelp.Controllers.WebApi
{
    [RoutePrefix("Request")]
    public class RequestApiController : ApiController
    {
        public ICustomerPaymentService _CustomerPaymentService { get; set; }
        public IUserService _UserService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IRequestService _RequestService { get; set; }
        public IReviewAndRatingService _ReviewAndRatingService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public IAgencyJobService _AgencyJobService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public RequestApiController(ICustomerPaymentService CustomerPaymentService,IReviewAndRatingService ReviewAndRatingService,INotificationService NotificationService,
            IRequestService RequestService, ICategoryService CategoryService, ICustomerService CustomerService, IUserService UserService, IUserRoleService UserRoleService,
            IAgencyJobService AgencyJobService, IAgencyIndividualService AgencyIndividualService)
        {
            this._CustomerPaymentService = CustomerPaymentService;
            this._ReviewAndRatingService = ReviewAndRatingService;
            this._CategoryService = CategoryService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._UserRoleService = UserRoleService;
            this._RequestService = RequestService;
            this._NotificationService = NotificationService;
            this._AgencyJobService = AgencyJobService;
            this._AgencyIndividualService = AgencyIndividualService;
        }


        [Route("SaveRequest")]
        [HttpPost]
        public HttpResponseMessage SaveRequest([FromBody]RequestModel RequestModel)
        {
            string UserMessage = "";
            string Flag = "";
            int JobRequestId = 0;
            try
            {
                List<JobRequest> model = new List<JobRequest>();

                if (RequestModel.CustomerIdBy == new Guid() || RequestModel.CustomerIdTo == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
                }

                ////Change to
                //if (RequestModel.CustomerIdBy == new Guid())
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
                //}

                var CustomerBy = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdBy).FirstOrDefault();
                if (CustomerBy == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerBy.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerBy should be customer."), Configuration.Formatters.JsonFormatter);
                }
                var IndividualTo = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdTo).FirstOrDefault();
                //Change To
                if (IndividualTo == null)
                {
                    var CustomerTo = _AgencyIndividualService.GetAgencyIndividual(RequestModel.CustomerIdTo);
                    if (CustomerTo == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
                //if (CustomerTo.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerTo should be customer."), Configuration.Formatters.JsonFormatter);
                //}
                if (RequestModel.FirstName == null || RequestModel.FirstName == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "First Name is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.Latitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.Longitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.LastName == null || RequestModel.LastName == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Last Name is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.EmailId == null || RequestModel.EmailId == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Email Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.Address == null || RequestModel.Address == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Address is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.ZipCode == null || RequestModel.ZipCode == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Zip Code is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.Description == null || RequestModel.Description == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Description is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.MobileNumber == null || RequestModel.MobileNumber == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Mobile No is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.NumberOfHours == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Number Of Hours is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.CategoryId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Category Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                //List<int> ids = new List<int>();
                //ids.Add(RequestModel.CustomerIdBy);
                //ids.Add(RequestModel.CustomerIdTo);
                //var requestFound = _RequestService.GetRequests().Where(r => ids.Contains(r.CustomerIdBy) && ids.Contains(r.CustomerIdTo)).FirstOrDefault();
                //if (requestFound == null)
                //{
                    Mapper.CreateMap<RequestModel, JobRequest>();
                    var request = Mapper.Map<RequestModel, JobRequest>(RequestModel);
                    request.RequestStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.New);
                    request.JobStatus = EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending);
                    request.IsPaid = false;
                    if (IndividualTo == null)
                    {
                        request.CustomerIdTo = new Guid();
                    }
                    else{
                         request.CustomerIdTo = RequestModel.CustomerIdTo;
                    }
                   
                    TimeZoneInfo tz = TimeZoneInfo.Local;
                    request.CreatedOn = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                    request.JobStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                    _RequestService.InsertRequest(request);
                    JobRequestId = request.JobRequestId;

              

                    if (IndividualTo != null)
                    {
                        UserMessage = "You have new job request by" + CustomerBy.FirstName+" " +CustomerBy.LastName;
                        Flag = "NewRequest";
                        Notification notification = new Notification();
                        notification.CustomerIdBy = request.CustomerIdBy;
                        notification.CustomerIdTo = IndividualTo.CustomerId;
                        notification.SourceId = request.JobRequestId;
                        notification.UserMessage = UserMessage;
                        //notification.NotificationType = Convert.ToString(EnumValue.NotificationType.Request); 
                        notification.Flag = Flag;
                        notification.DeviceType = IndividualTo.DeviceType;
                        notification.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.New);
                        _NotificationService.InsertNotification(notification);

                        string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + request.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                        if (IndividualTo.ApplicationId != null && IndividualTo.ApplicationId != "")
                        {

                            if (IndividualTo.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                            {
                                //Send Notification another Andriod
                                CommonCls.SendFCM_Notifications(IndividualTo.ApplicationId, Message, true);
                            }
                            else
                            {
                                string Msg = UserMessage;

                                CommonCls.TestSendFCM_Notifications(IndividualTo.ApplicationId, Message, Msg, true);
                            }
                        }
                    }
                    else
                    {
                        AgencyJobModel agencyJobModel = new AgencyJobModel();
                        agencyJobModel.AgencyIndividualId = RequestModel.CustomerIdTo;
                        agencyJobModel.JobRequestId = JobRequestId;
                        agencyJobModel.Status = EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending);
                        request.CreatedOn = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                        request.JobStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                        Mapper.CreateMap<AgencyJobModel, AgencyJob>();
                        var agencyJobRequest = Mapper.Map<AgencyJobModel, AgencyJob>(agencyJobModel);
                        _AgencyJobService.InsertAgencyJob(agencyJobRequest);
                    }
                    
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "JobRequestId: " + JobRequestId), Configuration.Formatters.JsonFormatter);
                
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "please try again."), Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("AcceptDeclineRequest")]
        [HttpPost]
        public HttpResponseMessage AcceptDeclineRequest([FromBody]RequestModel RequestModel)
        {
            try
            {
                List<JobRequest> model = new List<JobRequest>();
                if (RequestModel.CustomerIdBy == new Guid() || RequestModel.CustomerIdTo == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (RequestModel.JobRequestId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "JobRequest Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var CustomerBy = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdBy).FirstOrDefault();
                if (CustomerBy == null)
                {
                    //return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    if (CustomerBy.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerBy should be service provider."), Configuration.Formatters.JsonFormatter);
                    }
                }
                
                var CustomerTo = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdTo).FirstOrDefault();
                if (CustomerTo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    if (CustomerTo.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerTo should be customer."), Configuration.Formatters.JsonFormatter);
                    }
                }
                
                if (RequestModel.CustomerIdBy == new Guid() || RequestModel.CustomerIdTo == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
                }

                if (RequestModel.RequestStatus == null || RequestModel.RequestStatus == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Status is blank."), Configuration.Formatters.JsonFormatter);
                }
                else if ((RequestModel.RequestStatus != EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) && RequestModel.RequestStatus != EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined)))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Status is incorrect."), Configuration.Formatters.JsonFormatter);
                }
               
                var requestFound = _RequestService.GetRequests().Where(r => r.CustomerIdBy == RequestModel.CustomerIdTo && r.CustomerIdTo == RequestModel.CustomerIdBy && r.JobRequestId == RequestModel.JobRequestId).FirstOrDefault();
                if (requestFound != null)
                {
                    if (requestFound.JobStatus == Convert.ToString((int)EnumValue.RequestStatus.Accepted))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Request already accepted."), Configuration.Formatters.JsonFormatter);
                    }
                    else if (requestFound.JobStatus == Convert.ToString((int)EnumValue.RequestStatus.Declined))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Request is already declined."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        if(RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined)){
                            requestFound.CustomerIdTo = new Guid();
                        }
                        requestFound.RequestStatus = RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) ? EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) : EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
                        requestFound.JobStatus = RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) ? EnumValue.GetEnumDescription(EnumValue.RequestStatus.New) : EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
                        _RequestService.UpdateRequest(requestFound);

                        var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(requestFound.JobRequestId);
                        if (agencyJob != null)
                        {
                            agencyJob.Status = RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) ? EnumValue.GetEnumDescription(EnumValue.RequestStatus.New) : EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
                            _AgencyJobService.UpdateAgencyJob(agencyJob);
                        }
                        var notificationFound = _NotificationService.GetNotifications().Where(n => n.SourceId == requestFound.JobRequestId).FirstOrDefault();
                        if (RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted))
                        {
                            notificationFound.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted);
                        }
                        else
                        {
                            notificationFound.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
                        }
                        _NotificationService.UpdateNotification(notificationFound);

                        if (CustomerBy != null)
                        {
                            string UserMessage = CustomerBy.FirstName + " " + CustomerBy.LastName + " has " + RequestModel.RequestStatus + " your request.";
                            string Flag = "AcceptDeclineRequest";
                            string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + RequestModel.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                            if (CustomerTo.ApplicationId != null && CustomerTo.ApplicationId != "")
                            {

                                if (CustomerTo.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                                {
                                    //Send Notification another Andriod
                                    CommonCls.SendFCM_Notifications(CustomerTo.ApplicationId, Message, true);
                                }
                                else
                                {
                                    string Msg = UserMessage;

                                    CommonCls.TestSendFCM_Notifications(CustomerTo.ApplicationId, Message, Msg, true);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Request" + " " + RequestModel.RequestStatus + " " + "successfully."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No request exists."), Configuration.Formatters.JsonFormatter);

                }

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "please try again."), Configuration.Formatters.JsonFormatter);
            }

        }

        //[Route("AcceptDeclineRequest")]
        //[HttpPost]
        //public HttpResponseMessage AcceptDeclineRequest([FromBody]RequestModel RequestModel)
        //{
        //    try
        //    {
        //        List<JobRequest> model = new List<JobRequest>();
        //        if (RequestModel.CustomerIdBy == new Guid() || RequestModel.CustomerIdTo == new Guid())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
        //        }
        //        if (RequestModel.JobRequestId == 0 )
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "JobRequest Id is blank."), Configuration.Formatters.JsonFormatter);
        //        }


        //        var CustomerBy = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdTo).FirstOrDefault();
        //        if (CustomerBy == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        if (CustomerBy.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer).ToString())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerTo should be Customer."), Configuration.Formatters.JsonFormatter);
        //        }



        //        var IndividualTo = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdBy).FirstOrDefault();
        //        if (IndividualTo == null)
        //        {
        //            var CustomerTo = _AgencyIndividualService.GetAgencyIndividual(RequestModel.CustomerIdBy);
        //            if (CustomerTo == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //            }
        //        }
        //        else
        //        {
        //            if (IndividualTo.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerTo should be service provider."), Configuration.Formatters.JsonFormatter);
        //            }
        //        }
        //        //var CustomerTo = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdTo).FirstOrDefault();
        //        //if (CustomerTo == null)
        //        //{
        //        //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //        //}
        //        //if (CustomerTo.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
        //        //{
        //        //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerTo should be customer."), Configuration.Formatters.JsonFormatter);
        //        //}
        //        if (RequestModel.CustomerIdBy == new Guid() || RequestModel.CustomerIdTo == new Guid())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
        //        }

        //        if (RequestModel.RequestStatus == null || RequestModel.RequestStatus == "")
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Status is blank."), Configuration.Formatters.JsonFormatter);
        //        }
        //        else if ((RequestModel.RequestStatus != EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) && RequestModel.RequestStatus != EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined)))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Status is incorrect."), Configuration.Formatters.JsonFormatter);
        //        }
        //        var CustomerByMember = _CustomerService.GetCustomers().Where(c => c.CustomerId == RequestModel.CustomerIdBy).FirstOrDefault();
        //        if (CustomerByMember == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerIdBy or customerIdTo is blank."), Configuration.Formatters.JsonFormatter);
        //        }

        //        var requestFound = _RequestService.GetRequests().Where(r => r.CustomerIdBy == RequestModel.CustomerIdTo && r.CustomerIdTo == RequestModel.CustomerIdBy && r.JobRequestId == RequestModel.JobRequestId).FirstOrDefault();
        //        if (requestFound != null)
        //        {
        //            if (requestFound.JobStatus == Convert.ToString((int)EnumValue.RequestStatus.Accepted))
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Request already accepted."), Configuration.Formatters.JsonFormatter);
        //            }
        //            else if (requestFound.JobStatus == Convert.ToString((int)EnumValue.RequestStatus.Declined))
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Request is already declined."), Configuration.Formatters.JsonFormatter);
        //            }
        //            else
        //            {
        //                requestFound.RequestStatus = RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) ? EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) : EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
        //                requestFound.JobStatus = RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted) ? EnumValue.GetEnumDescription(EnumValue.RequestStatus.New) : EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
        //                _RequestService.UpdateRequest(requestFound);

        //                var notificationFound = _NotificationService.GetNotifications().Where(n => n.SourceId == requestFound.JobRequestId ).FirstOrDefault();
        //                if (RequestModel.RequestStatus == EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted))
        //                {
        //                    notificationFound.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.Accepted);
        //                }
        //                else
        //                {
        //                    notificationFound.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
        //                }
        //                _NotificationService.UpdateNotification(notificationFound);

        //                if (IndividualTo != null)
        //                {
        //                    string UserMessage = IndividualTo.FirstName + " " + IndividualTo.LastName + " has " + RequestModel.RequestStatus + " your request.";
        //                    string Flag = "AcceptDeclineRequest";
        //                    string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + RequestModel.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

        //                    if (CustomerBy.ApplicationId != null && CustomerBy.ApplicationId != "")
        //                    {

        //                        if (CustomerBy.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
        //                        {
        //                            //Send Notification another Andriod
        //                            CommonCls.SendFCM_Notifications(CustomerBy.ApplicationId, Message, true);
        //                        }
        //                        else
        //                        {
        //                            string Msg = UserMessage;

        //                            CommonCls.TestSendFCM_Notifications(CustomerBy.ApplicationId, Message, Msg, true);
        //                        }
        //                    }
        //                }
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Request" + " " + RequestModel.RequestStatus + " " + "successfully."), Configuration.Formatters.JsonFormatter);
        //            }
        //        }
        //        else
        //        {

        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No request exists."), Configuration.Formatters.JsonFormatter);

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "please try again."), Configuration.Formatters.JsonFormatter);
        //    }

        //}

        //[Route("GetRequestListByMember")]
        //[HttpGet]
        //public HttpResponseMessage GetRequestListByMember([FromUri] int CustomerId)
        //{

        //    try
        //    {
        //        var requestList = new List<requestDetailsModel>();
        //        var customer = _CustomerService.GetCustomer(CustomerId);
        //        if (customer == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        var requests = _RequestService.GetRequests().Where(r => r.CustomerIdBy == CustomerId).ToList();
        //        //if (requests.Count() > 0)
        //        //{
        //            foreach (var item in requests)
        //            {
        //                Mapper.CreateMap<Request, requestDetailsModel>();
        //                requestDetailsModel RequestModel = Mapper.Map<Request, requestDetailsModel>(item);
        //                RequestModel.CustomerId = item.CustomerIdTo;
        //                string stringValue = Enum.GetName(typeof(EnumValue.RequestStatus), Convert.ToInt32(item.Status));
        //                RequestModel.Status = stringValue;
        //                RequestModel.Name = _CustomerService.GetCustomers().Where(c => c.CustomerId == item.CustomerIdTo).Select(c => c.Name).FirstOrDefault();
        //                requestList.Add(RequestModel);
        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", requestList), Configuration.Formatters.JsonFormatter);
        //        //}
        //        //else
        //        //{
        //        //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Request not found."), Configuration.Formatters.JsonFormatter);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        //[Route("GetRequestListToTrainer")]
        //[HttpGet]
        //public HttpResponseMessage GetRequestListToTrainer([FromUri] int CustomerId)
        //{

        //    try
        //    {
        //        var requestList = new List<requestDetailsModel>();
        //        var customer = _CustomerService.GetCustomer(CustomerId);
        //        if (customer == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        var requests = _RequestService.GetRequests().Where(r => r.CustomerIdTo == CustomerId).ToList();
        //        //if (requests.Count() > 0)
        //        //{
        //            foreach (var item in requests)
        //            {
        //                Mapper.CreateMap<Request, requestDetailsModel>();
        //                requestDetailsModel RequestModel = Mapper.Map<Request, requestDetailsModel>(item);
        //                RequestModel.CustomerId = item.CustomerIdBy;
        //                string stringValue = Enum.GetName(typeof(EnumValue.RequestStatus), Convert.ToInt32(item.Status));
        //                RequestModel.Status = stringValue;
        //                RequestModel.Name = _CustomerService.GetCustomers().Where(c => c.CustomerId == item.CustomerIdBy).Select(c => c.Name).FirstOrDefault();
        //                requestList.Add(RequestModel);
        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", requestList), Configuration.Formatters.JsonFormatter);
        //        //}
        //        //else
        //        //{
        //        //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Request not found."), Configuration.Formatters.JsonFormatter);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
        //    }
        //}




        //[Route("GetRequestNotificationList")]
        //[HttpGet]
        //public HttpResponseMessage GetRequestNotificationList([FromUri] int CustomerId)
        //{

        //    try
        //    {
        //        var customer = _CustomerService.GetCustomer(CustomerId);
        //        if (customer == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        List<int?> ListCustomerIDs = new List<int?>();
        //        ListCustomerIDs.Add(CustomerId);

        //        var notificationList = new List<RequestNotificationResponse>();
        //        var notifications = _NotificationService.GetNotifications().Where(x => ListCustomerIDs.Contains(x.CustomerIdBy) || ListCustomerIDs.Contains(x.CustomerIdTo)).OrderByDescending(x => x.NotificationDateTime).ToList();

        //        if (notifications == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "notifications not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {

        //            foreach (var notification in notifications)
        //            {
        //                Mapper.CreateMap<Notification, RequestNotificationResponse>();
        //                RequestNotificationResponse RequestNotificationResponse = Mapper.Map<Notification, RequestNotificationResponse>(notification);
        //                var customerBy = _CustomerService.GetCustomer(RequestNotificationResponse.CustomerIdBy);
        //                RequestNotificationResponse.Name = customerBy.Name;
        //                RequestNotificationResponse.PhotoPath = customerBy.PhotoPath;
        //                if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Request) && RequestNotificationResponse.NotificationStatus == Convert.ToString(EnumValue.NotificationStatus.New))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Request);
        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Post))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Post);

        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Comment))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Comment);
        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Like))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Like);
        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Tag))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Tag);
        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else if (RequestNotificationResponse.NotificationType == Convert.ToString(EnumValue.NotificationType.Appointment) && RequestNotificationResponse.NotificationStatus == Convert.ToString(EnumValue.NotificationStatus.New))
        //                {
        //                    RequestNotificationResponse.NotificationType = Convert.ToString(EnumValue.NotificationType.Appointment);
        //                    RequestNotificationResponse.Comment = _AppointmentService.GetAppointment(notification.SourceId).Comment;
        //                    notificationList.Add(RequestNotificationResponse);
        //                }
        //                else
        //                {

        //                }

        //                RequestNotificationResponse.Time = chatTime(notification.NotificationDateTime);

        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", notificationList), Configuration.Formatters.JsonFormatter);

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //public string chatTime(DateTime? chatTime)
        //{
        //    TimeSpan span = (DateTime.Now - Convert.ToDateTime(chatTime));
        //    string[] year = CalculateDifference(Convert.ToDateTime(chatTime), DateTime.Now).Split(',');
        //    string result = "";
        //    if (year[0] == "0")
        //    {
        //        if (year[1] == "0")
        //        {
        //            if (year[2] == "0")
        //            {
        //                string[] difference = (String.Format("{0} days, {1} hours, {2} minutes, {3} seconds", span.Days, span.Hours, span.Minutes, span.Seconds)).Split(',');

        //                if (difference[1] == " 0 hours" && difference[2] == " 0 minutes")
        //                {
        //                    result = "just now.";
        //                }
        //                else if (difference[2] == " 0 minutes")
        //                {
        //                    result = difference[1] + " ago.";
        //                }
        //                else if (difference[1] == " 0 hours")
        //                {
        //                    result = difference[2] + " ago.";
        //                }
        //                else
        //                {
        //                    result = difference[1] + " ago.";
        //                }

        //            }
        //            else
        //            {
        //                result = year[2] + " days ago.";
        //            }
        //        }
        //        else
        //        {
        //            result = year[1] + " months ago.";
        //        }
        //    }
        //    else
        //    {
        //        result = year[0] + " year ago.";
        //    }
        //    return result;
        //}

        //public string CalculateDifference(DateTime Bday, DateTime Cday)
        //{
        //    int Years;
        //    int Months;
        //    int Days;
        //    if ((Cday.Year - Bday.Year) > 0 ||
        //       (((Cday.Year - Bday.Year) == 0) &&
        //       ((Bday.Month < Cday.Month) ||
        //       ((Bday.Month == Cday.Month) &&
        //       (Bday.Day <= Cday.Day)))))
        //    {

        //        int DaysInBdayMonth = DateTime.DaysInMonth(Bday.Year, Bday.Month);
        //        int DaysRemain = Cday.Day + (DaysInBdayMonth - Bday.Day);

        //        if (Cday.Month > Bday.Month)
        //        {
        //            Years = Cday.Year - Bday.Year;
        //            Months = Cday.Month - (Bday.Month + 1) + Math.Abs(DaysRemain / DaysInBdayMonth);
        //            Days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
        //        }
        //        else if (Cday.Month == Bday.Month)
        //        {
        //            if (Cday.Day >= Bday.Day)
        //            {
        //                Years = Cday.Year - Bday.Year;
        //                Months = 0;
        //                Days = Cday.Day - Bday.Day;
        //            }
        //            else
        //            {
        //                Years = (Cday.Year - 1) - Bday.Year;
        //                Months = 11;
        //                Days = DateTime.DaysInMonth(Bday.Year, Bday.Month) - (Bday.Day - Cday.Day);

        //            }
        //        }
        //        else
        //        {
        //            Years = (Cday.Year - 1) - Bday.Year;
        //            Months = Cday.Month + (11 - Bday.Month) + Math.Abs(DaysRemain / DaysInBdayMonth);
        //            Days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
        //        }
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Birthday date must be earlier than current date");
        //    }
        //    return Years + "," + Months + "," + Days;
        //}

        [Route("GetJobByID")]
        [HttpGet]
        public HttpResponseMessage GetCustomerByID([FromUri] int JobId)
        {
            try
            {
                if (JobId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var job = _RequestService.GetRequest(JobId);

                if (job != null)
                {
                    Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.CompletedJObRequestResponseModel>();
                    CompletedJObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.CompletedJObRequestResponseModel>(job);
                    //var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                    //if(agencyJob!=null)
                    //{
                    //    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                    //    if(cust!=null)
                    //    {
                    //        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                    //        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                    //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                    //        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                    //    }
                    //    else
                    //    {
                    //        var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                    //        JObRequestResponseModel.CustomerToName = agency.FullName;
                    //        JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                    //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                    //        JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                    //    }
                    //}

                    var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                    if (agencyJob != null)
                    {

                        var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                        if (agency == null)
                        {
                            var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                            if (cust != null)
                            {
                                JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            }
                        }
                        else
                        {
                            JObRequestResponseModel.CustomerToName = agency.FullName;
                            JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                            JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                            JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                        }


                    }
                    else
                    {
                        var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                        if (cust != null)
                        {
                            JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                        }
                    }
                    
                    //JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                    var review = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.JobRequestId == job.JobRequestId).ToList();
                    if (review.Count() > 0)
                    {
                        foreach (var item in review)
                        {
                            var customer = _CustomerService.GetCustomer(item.CustomerId);
                            if (customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                            {
                                var custReviews = new ServiceProviderModel();
                                custReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                custReviews.CustomerId = item.CustomerId;
                                custReviews.CustomerImage = customer.PhotoPath;
                                custReviews.Rating = item.Rating;
                                custReviews.Review = item.Review;
                                JObRequestResponseModel.CustomerReviews = custReviews;
                            }
                            else
                            {
                                var serviceProviderReviews = new ServiceProviderModel();
                                serviceProviderReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                serviceProviderReviews.CustomerId = item.CustomerId;
                                serviceProviderReviews.CustomerImage = customer.PhotoPath;
                                serviceProviderReviews.Rating = item.Rating;
                                serviceProviderReviews.Review = item.Review;
                                JObRequestResponseModel.ServiceProviderReviews = serviceProviderReviews;
                            }

                        }

                    }
                    //jobList.Add(JObRequestResponseModel);
                    //Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>();
                    //JObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>(job);
                    //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                    //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                    //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;

                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", JObRequestResponseModel), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetJobList")]
        [HttpGet]
        public HttpResponseMessage GetJobList([FromUri] Guid CustomerId, [FromUri] string JobStatus, [FromUri] int PageNumber)
        {
            try
            {
                if (CustomerId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (JobStatus == "" || JobStatus == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Status is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (JobStatus != EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending) && JobStatus != EnumValue.GetEnumDescription(EnumValue.JobStatus.New) && JobStatus != EnumValue.GetEnumDescription(EnumValue.JobStatus.InProgress) && JobStatus != EnumValue.GetEnumDescription(EnumValue.JobStatus.Completed))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Status is wrong."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(CustomerId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobList = new List<JObListResponseModel>();
                        var jobs = _RequestService.GetRequests().Where(r => r.JobStatus == JobStatus && r.CustomerIdBy == CustomerId).ToList();
                        foreach (var job in jobs)
                        {
                            Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObListResponseModel>();
                            JObListResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObListResponseModel>(job);

                            
                            var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            if (agencyJob != null)
                            {
                               
                                    var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                                    if (agency == null)
                                    {
                                        var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                                        if (cust != null)
                                        {
                                            JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                            JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                            JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                            JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                        }
                                    }
                                    else
                                    {
                                        JObRequestResponseModel.CustomerToName = agency.FullName;
                                        JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                                        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                                        JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                                    }
                                    
                               
                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                                if (cust != null)
                                {
                                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                    JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                }
                            }
                            //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).LastName;
                            //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            var review = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.JobRequestId == job.JobRequestId).ToList();
                            if (review.Count()>0)
                            {
                                foreach (var item in review)
                                {
                                    var customer = _CustomerService.GetCustomer(item.CustomerId);
                                    if (customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                                    {
                                        var custReviews = new ServiceProviderModel();
                                        custReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                        custReviews.CustomerId = item.CustomerId;
                                        custReviews.CustomerImage = customer.PhotoPath;
                                        custReviews.Rating = item.Rating;
                                        custReviews.Review = item.Review;
                                        JObRequestResponseModel.CustomerReviews = custReviews;
                                    }
                                    else
                                    {
                                        var serviceProviderReviews = new ServiceProviderModel();
                                        serviceProviderReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                        serviceProviderReviews.CustomerId = item.CustomerId;
                                        serviceProviderReviews.CustomerImage = customer.PhotoPath;
                                        serviceProviderReviews.Rating = item.Rating;
                                        serviceProviderReviews.Review = item.Review;
                                        JObRequestResponseModel.ServiceProviderReviews = serviceProviderReviews;
                                    }
                                    
                                }
                                
                            }
                            jobList.Add(JObRequestResponseModel);
                        }
                        int numberOfObjectsPerPage = 10;
                        var modelsdata = jobList.Skip(numberOfObjectsPerPage * PageNumber).OrderByDescending(c=>c.CreatedOn).Take(numberOfObjectsPerPage);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("StartJob")]
        [HttpGet]
        public HttpResponseMessage StartJob([FromUri] int JobId, [FromUri] Guid CustomerId)
        {
            try
            {
                if (JobId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Id is blank."), Configuration.Formatters.JsonFormatter);
                }

                TimeZoneInfo tz = TimeZoneInfo.Local;  
                var Job = _RequestService.GetRequest(JobId);
                if (Job != null)
                {
                    Job.JobStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                    Job.JobStatus = EnumValue.GetEnumDescription(EnumValue.JobStatus.InProgress);
                    _RequestService.UpdateRequest(Job);
                    var agencyJob = _AgencyJobService.GetAgencyJobs().Where(c => c.JobRequestId == JobId).FirstOrDefault();
                    if(agencyJob!=null)
                    {
                        agencyJob.Status = EnumValue.GetEnumDescription(EnumValue.JobStatus.InProgress);
                        _AgencyJobService.UpdateAgencyJob(agencyJob);
                    }
                    string ApplicationId = "";
                    string DeviceType = "";
                    if (Job.CustomerIdBy == CustomerId)
                    {
                        ApplicationId = _CustomerService.GetCustomer(Job.CustomerIdTo).ApplicationId;
                        DeviceType = _CustomerService.GetCustomer(Job.CustomerIdTo).DeviceType;
                    }
                    else
                    {
                        ApplicationId = _CustomerService.GetCustomer(Job.CustomerIdBy).ApplicationId;
                        DeviceType = _CustomerService.GetCustomer(Job.CustomerIdBy).DeviceType;
                    }
                    var customer = _CustomerService.GetCustomer(CustomerId);
                    string UserMessage = customer.FirstName + " " + customer.LastName + " has started a job.";
                    string Flag = "StartJob";
                    string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + Job.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                    if (ApplicationId != null && ApplicationId != "")
                    {

                        if (DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                        {
                            //Send Notification another Andriod
                            CommonCls.SendFCM_Notifications(ApplicationId, Message, true);
                        }
                        else
                        {
                            string Msg = UserMessage;

                            CommonCls.TestSendFCM_Notifications(ApplicationId, Message, Msg, true);
                        }
                    }
                        
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Job started successfully."), Configuration.Formatters.JsonFormatter);

                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("StopJob")]
        [HttpGet]
        public HttpResponseMessage StopJob([FromUri] int JobId,[FromUri] Guid CustomerId)
        {
            try
            {
                if (JobId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                TimeZoneInfo tz = TimeZoneInfo.Local;  
                var Job = _RequestService.GetRequest(JobId);
                if (Job != null)
                {
                    Job.JobEndDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                    Job.JobStatus = EnumValue.GetEnumDescription(EnumValue.JobStatus.Completed);
                    _RequestService.UpdateRequest(Job);
                    var agencyJob = _AgencyJobService.GetAgencyJobs().Where(c => c.JobRequestId == JobId).FirstOrDefault();
                    if (agencyJob != null)
                    {
                        agencyJob.Status = EnumValue.GetEnumDescription(EnumValue.JobStatus.Completed);
                        _AgencyJobService.UpdateAgencyJob(agencyJob);
                    }
                    string ApplicationId = "";
                    string DeviceType = "";
                    if (Job.CustomerIdBy == CustomerId)
                    {
                        ApplicationId = _CustomerService.GetCustomer(Job.CustomerIdTo).ApplicationId;
                        DeviceType = _CustomerService.GetCustomer(Job.CustomerIdTo).DeviceType;
                    }
                    else
                    {
                        ApplicationId = _CustomerService.GetCustomer(Job.CustomerIdBy).ApplicationId;
                        DeviceType = _CustomerService.GetCustomer(Job.CustomerIdBy).DeviceType;
                    } 
                    var customer = _CustomerService.GetCustomer(CustomerId);
                    string UserMessage = customer.FirstName + " " + customer.LastName + " has stopped a job.";
                    string Flag = "StopJob";
                    string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + Job.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                    if (ApplicationId != null && ApplicationId != "")
                    {

                        if (DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                        {
                            //Send Notification another Andriod
                            CommonCls.SendFCM_Notifications(ApplicationId, Message, true);
                        }
                        else
                        {
                            string Msg = UserMessage;

                            CommonCls.TestSendFCM_Notifications(ApplicationId, Message, Msg, true);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Job stoped successfully."), Configuration.Formatters.JsonFormatter);  
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetNewJobList")]
        [HttpGet]
        public HttpResponseMessage GetNewJobList([FromUri] Guid ServiceProviderId)
        {
            try
            {
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobList = new List<JObRequestResponseModel>();
                        var jobStatus = EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending);
                        var jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId && r.JobStatus == jobStatus).ToList();
                        foreach (var job in jobs)
                        {
                            Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>();
                            JObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>(job);

                            var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            if (agencyJob != null)
                            {

                                var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                                if (agency == null)
                                {
                                    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                                    if (cust != null)
                                    {
                                        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                    }
                                }
                                else
                                {
                                    JObRequestResponseModel.CustomerToName = agency.FullName;
                                    JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                                    JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                                }


                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                                if (cust != null)
                                {
                                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                    JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                }
                            }
                            //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            jobList.Add(JObRequestResponseModel);
                        }
                        //int numberOfObjectsPerPage = 10;
                        //var modelsdata = jobList.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", jobList.OrderByDescending(c => c.CreatedOn)), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetInProgressJobList")]
        [HttpGet]
        public HttpResponseMessage GetInProgressJobList([FromUri] Guid ServiceProviderId)
        {
            try
            {
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobList = new List<JObRequestResponseModel>();
                        var jobStatus=EnumValue.GetEnumDescription(EnumValue.JobStatus.InProgress);
                        var jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId && r.JobStatus == jobStatus).ToList();
                        foreach (var job in jobs)
                        {
                            Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>();
                            JObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>(job);


                            var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            if (agencyJob != null)
                            {

                                var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                                if (agency == null)
                                {
                                    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                                    if (cust != null)
                                    {
                                        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                    }
                                }
                                else
                                {
                                    JObRequestResponseModel.CustomerToName = agency.FullName;
                                    JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                                    JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                                }


                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                                if (cust != null)
                                {
                                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                    JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                }
                            }

                            //var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            //if (agencyJob != null)
                            //{
                            //    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                            //    if (cust != null)
                            //    {
                            //        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            //    }
                            //    else
                            //    {
                            //        var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                            //        JObRequestResponseModel.CustomerToName = agency.FullName;
                            //        JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                            //    }
                            //}
                            //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            jobList.Add(JObRequestResponseModel);
                        }
                        //int numberOfObjectsPerPage = 10;
                        //var modelsdata = jobList.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", jobList.OrderByDescending(c => c.CreatedOn)), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetRequestedJobList")]
        [HttpGet]
        public HttpResponseMessage GetRequestedJobList([FromUri] Guid ServiceProviderId)
        {
            try
            {
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobList = new List<JObRequestResponseModel>();
                        var jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId && r.JobStatus == EnumValue.GetEnumDescription(EnumValue.JobStatus.New)).ToList();
                        foreach (var job in jobs)
                        {
                            Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>();
                            JObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.JObRequestResponseModel>(job);


                            var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            if (agencyJob != null)
                            {

                                var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                                if (agency == null)
                                {
                                    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                                    if (cust != null)
                                    {
                                        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                    }
                                }
                                else
                                {
                                    JObRequestResponseModel.CustomerToName = agency.FullName;
                                    JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                                    JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                                }


                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                                if (cust != null)
                                {
                                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                    JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                }
                            }
                            //var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            //if (agencyJob != null)
                            //{
                            //    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                            //    if (cust != null)
                            //    {
                            //        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            //    }
                            //    else
                            //    {
                            //        var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                            //        JObRequestResponseModel.CustomerToName = agency.FullName;
                            //        JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                            //    }
                            //}
                            //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            jobList.Add(JObRequestResponseModel);
                        }
                        //int numberOfObjectsPerPage = 10;
                        //var modelsdata = jobList.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", jobList.OrderByDescending(c => c.CreatedOn)), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetCompletedJobList")]
        [HttpGet]
        public HttpResponseMessage GetCompletedJobList([FromUri] Guid ServiceProviderId, [FromUri] int PageNumber)
        {
            try
            {
                if (ServiceProviderId ==new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobList = new List<CompletedJObRequestResponseModel>();
                        var jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId && r.JobStatus == EnumValue.GetEnumDescription(EnumValue.JobStatus.Completed)).ToList();
                        foreach (var job in jobs)
                        {
                            Mapper.CreateMap<HomeHelp.Entity.JobRequest, HomeHelp.Models.CompletedJObRequestResponseModel>();
                            CompletedJObRequestResponseModel JObRequestResponseModel = Mapper.Map<HomeHelp.Entity.JobRequest, HomeHelp.Models.CompletedJObRequestResponseModel>(job);

                            var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            if (agencyJob != null)
                            {

                                var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                                if (agency == null)
                                {
                                    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                                    if (cust != null)
                                    {
                                        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                    }
                                }
                                else
                                {
                                    JObRequestResponseModel.CustomerToName = agency.FullName;
                                    JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                                    JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                                }


                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomer(job.CustomerIdTo);
                                if (cust != null)
                                {
                                    JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                                    JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                                    JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                                    JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                                }
                            }
                            //var agencyJob = _AgencyJobService.GetAgencyJobByJobRequest(job.JobRequestId);
                            //if (agencyJob != null)
                            //{
                            //    var cust = _CustomerService.GetCustomer(agencyJob.AgencyIndividualId);
                            //    if (cust != null)
                            //    {
                            //        JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //        JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            //    }
                            //    else
                            //    {
                            //        var agency = _AgencyIndividualService.GetAgencyIndividualById(agencyJob.AgencyIndividualId);
                            //        JObRequestResponseModel.CustomerToName = agency.FullName;
                            //        JObRequestResponseModel.CustomerToImage = agency.PhotoPath;
                            //        JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(job.CategoryId).Name;
                            //        JObRequestResponseModel.CustomerIdByImage = agency.PhotoPath;
                            //    }
                            //}
                            //JObRequestResponseModel.CustomerToName = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).FirstName + " " + _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).LastName;
                            //JObRequestResponseModel.CustomerToImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).PhotoPath;
                            //JObRequestResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdTo).CategoryId)).Name;
                            //JObRequestResponseModel.CustomerIdByImage = _CustomerService.GetCustomer(JObRequestResponseModel.CustomerIdBy).PhotoPath;
                            var review = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.JobRequestId == job.JobRequestId).ToList();
                            if (review.Count() > 0)
                            {
                                foreach (var item in review)
                                {
                                    var customer = _CustomerService.GetCustomer(item.CustomerId);
                                    if (customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                                    {
                                        var custReviews = new ServiceProviderModel();
                                        custReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                        custReviews.CustomerId = item.CustomerId;
                                        custReviews.CustomerImage = customer.PhotoPath;
                                        custReviews.Rating = item.Rating;
                                        custReviews.Review = item.Review;
                                        JObRequestResponseModel.CustomerReviews = custReviews;
                                    }
                                    else
                                    {
                                        var serviceProviderReviews = new ServiceProviderModel();
                                        serviceProviderReviews.CustomerName = customer.FirstName + " " + customer.LastName;
                                        serviceProviderReviews.CustomerId = item.CustomerId;
                                        serviceProviderReviews.CustomerImage = customer.PhotoPath;
                                        serviceProviderReviews.Rating = item.Rating;
                                        serviceProviderReviews.Review = item.Review;
                                        JObRequestResponseModel.ServiceProviderReviews = serviceProviderReviews;
                                    }

                                }

                            }
                            jobList.Add(JObRequestResponseModel);
                        }
                        int numberOfObjectsPerPage = 10;
                        var modelsdata = jobList.Skip(numberOfObjectsPerPage * PageNumber).OrderByDescending(c => c.JobStartDate).Take(numberOfObjectsPerPage);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Try again later."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("SavePayment")]
        [HttpPost]
        public HttpResponseMessage SavePayment([FromBody]CustomerPaymentModel CustomerPaymentModel)
        {
          
            try
            {
                List<CustomerPaymentModel> model = new List<CustomerPaymentModel>();
                if (CustomerPaymentModel.CustomerId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerId is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerPaymentModel.PaidAmount == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "PaidAmount is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerPaymentModel.PaymentId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "PaymentId is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerPaymentModel.ServiceproviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ServiceproviderId is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerPaymentModel.JobRequestId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "JobRequest Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var job = _RequestService.GetRequests().Where(j => j.JobRequestId == CustomerPaymentModel.JobRequestId).FirstOrDefault();
                if (job == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "job not found."), Configuration.Formatters.JsonFormatter);
                }
                var CustomerBy = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerPaymentModel.CustomerId).FirstOrDefault();
                if (CustomerBy == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }

                var Serviceprovider = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerPaymentModel.ServiceproviderId).FirstOrDefault();
                if (Serviceprovider == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Serviceprovider not found."), Configuration.Formatters.JsonFormatter);
                }
                var paymentExists = _CustomerPaymentService.GetCustomerPayments().Where(c => c.JobRequestId == CustomerPaymentModel.JobRequestId).FirstOrDefault();
                if(paymentExists==null)
                {
                    Mapper.CreateMap<CustomerPaymentModel, CustomerPayment>();
                    var payment = Mapper.Map<CustomerPaymentModel, CustomerPayment>(CustomerPaymentModel);
                    _CustomerPaymentService.InsertCustomerPayment(payment);
                    job.IsPaid = true;
                    _RequestService.UpdateRequest(job);

                    string UserMessage = "Payment has been done by " + CustomerBy.FirstName + " " + CustomerBy.LastName + ".";

                    string NotificationType = "chat";
                    string Flag = "NewPayment";

                    //Save notification in Table
                    Notification Notification = new Notification();
                    Notification.NotificationId = 0; //New Case
                    //Notification.ClientId = _Event.ClientID; Save it as NULL
                    Notification.CustomerIdBy = CustomerPaymentModel.CustomerId;
                    Notification.CustomerIdTo = job.CustomerIdTo;
                    //Notification.EventID = _Event.EventID; Save it as NULL
                    Notification.UserMessage = UserMessage;
                    Notification.NotificationType = NotificationType;
                    Notification.Flag = Flag;
                    Notification.DeviceType = CustomerBy.DeviceType;
                    _NotificationService.InsertNotification(Notification);
                    //        //End : Save notification in Table
                    string Message = "{\"flag\":\"" + Flag + "\",\"UserMessage\":\"" + UserMessage + "\",\"JobRequestId\":\"" + CustomerPaymentModel.JobRequestId + "\"}";

                     var customerTo = _CustomerService.GetCustomer(job.CustomerIdTo);
                    
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
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Payment saved successfully."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Payment already exists."), Configuration.Formatters.JsonFormatter);
                }
                ////Notification notification = new Notification();
                ////notification.CustomerIdBy = request.CustomerIdBy;
                ////notification.CustomerIdTo = request.CustomerIdTo;
                ////notification.SourceId = request.JobRequestId;
                ////notification.UserMessage = UserMessage;
                //////notification.NotificationType = Convert.ToString(EnumValue.NotificationType.Request); 
                ////notification.Flag = Flag;
                ////notification.DeviceType = CustomerTo.DeviceType;
                ////notification.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.New);
                ////_NotificationService.InsertNotification(notification);

                //if (CustomerTo.ApplicationId != null && CustomerTo.ApplicationId != "")
                //{

                //    //Send Notification another Andriod
                //    string Message = "{\"flag\":\"" + Flag + "\",\"RequestID\":\"" + request.RequestId + "\",\"CustomerById\":\"" + RequestModel.CustomerIdBy + "\",\"CustomerByName\":\"" + CustomerBy.Name + "\",\"CustomerByPic\":\"" + CustomerBy.PhotoPath + "\",\"RequestTime\":\"" + request.CreatedDate + "\"}";
                //    CommonCls.SendGCM_Notifications(CustomerTo.ApplicationId, Message, true);

                //}


                
               
               
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "please try again."), Configuration.Formatters.JsonFormatter);
            }

        }


        [Route("GetTotalAmount")]
        [HttpGet]
        public HttpResponseMessage GetTotalAmount([FromUri] int JobId,[FromUri] Guid ServiceProviderId)
        {
            try
            {
                int? amount = 0;
                if (JobId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ServiceProvider Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var job = _RequestService.GetRequest(JobId);

                if (job != null)
                {
                    var customer = _CustomerService.GetCustomer(ServiceProviderId);
                    if(customer==null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                    }
                        else if(customer.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "wrong ServiceProviderId."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        amount = customer.WorkRate * job.NumberOfHours;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", amount), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Job not found."), Configuration.Formatters.JsonFormatter);
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