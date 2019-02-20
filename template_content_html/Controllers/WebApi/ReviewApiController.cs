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
    [RoutePrefix("Reviews")]
    public class ReviewApiController : ApiController
    {
        public IUserService _UserService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IRequestService _RequestService { get; set; }
        public IReviewAndRatingService _ReviewAndRatingService { get; set; }
         public INotificationService _NotificationService { get; set; }
        public ReviewApiController(INotificationService NotificationService ,IReviewAndRatingService ReviewAndRatingService, IRequestService RequestService, ICategoryService CategoryService, ICustomerService CustomerService, IUserService UserService, IUserRoleService UserRoleService)
        {
            this._ReviewAndRatingService = ReviewAndRatingService;
            this._RequestService = RequestService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._UserRoleService = UserRoleService;
            this._CategoryService = CategoryService;
            this._NotificationService = NotificationService;
        }

        [Route("SaveReviewAndRating")]
        [HttpPost]
        public HttpResponseMessage SaveReviewAndRating([FromBody]ReviewAndRatingModel ReviewAndRatingModel)
        {
            try
            {
                if (ReviewAndRatingModel.CustomerId ==new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }

                if (ReviewAndRatingModel.JobRequestId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "JobRequest Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (ReviewAndRatingModel.Rating == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Rating is blank."), Configuration.Formatters.JsonFormatter);
                }
                //if (ReviewAndRatingModel.Rating > 5)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Rating cant be greater than 5."), Configuration.Formatters.JsonFormatter);
                //}
                


 
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == ReviewAndRatingModel.CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer != null)
                {
                    var job = _RequestService.GetRequest(ReviewAndRatingModel.JobRequestId);
                    if (job == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "job not found."), Configuration.Formatters.JsonFormatter);
                    }
                    if (job.JobStatus != EnumValue.GetEnumDescription(EnumValue.JobStatus.Completed))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "job is not completed yet."), Configuration.Formatters.JsonFormatter);
                    }
                    var jobFound = _ReviewAndRatingService.GetReviewAndRatings().Where(f => f.CustomerId == ReviewAndRatingModel.CustomerId && f.JobRequestId == ReviewAndRatingModel.JobRequestId).FirstOrDefault();
                    if (jobFound == null)
                    {
                        Mapper.CreateMap<ReviewAndRatingModel, ReviewAndRating>();
                        ReviewAndRating reviewAndRating = Mapper.Map<ReviewAndRatingModel, ReviewAndRating>(ReviewAndRatingModel);
                        _ReviewAndRatingService.InsertReviewAndRating(reviewAndRating);

                        var customerBy = _CustomerService.GetCustomer(ReviewAndRatingModel.CustomerId);
                        string UserMessage = "You have new reviews.";

                        string NotificationType = "reviews";
                        string Flag = "NewReviews";

                        //Save notification in Table
                        Notification Notification = new Notification();
                        Notification.NotificationId = 0; //New Case
                        //Notification.ClientId = _Event.ClientID; Save it as NULL
                        Notification.CustomerIdBy = customerBy.CustomerId;
                        //Notification.CustomerIdTo = customerTo.CustomerId;
                        //Notification.EventID = _Event.EventID; Save it as NULL
                        Notification.UserMessage = UserMessage;
                        Notification.NotificationType = NotificationType;
                        Notification.Flag = Flag;
                        Notification.DeviceType = customerBy.DeviceType;
                        _NotificationService.InsertNotification(Notification);
                        //        //End : Save notification in Table
                        string ApplicationId="";
                        string DeviceType = "";
                        if(job.CustomerIdBy==customerBy.CustomerId)
                        {
                            ApplicationId = _CustomerService.GetCustomer(job.CustomerIdTo).ApplicationId;
                            DeviceType = _CustomerService.GetCustomer(job.CustomerIdTo).DeviceType;
                        }
                        else
                        {
                            ApplicationId = _CustomerService.GetCustomer(job.CustomerIdBy).ApplicationId;
                            DeviceType = _CustomerService.GetCustomer(job.CustomerIdBy).DeviceType;
                        }
                        string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + ReviewAndRatingModel.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

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
                        
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Reviews saved successfully."), Configuration.Formatters.JsonFormatter);

                    }
                    else
                    {
                        jobFound.Rating = (ReviewAndRatingModel.Rating != 0 ? ReviewAndRatingModel.Rating : jobFound.Rating);
                        jobFound.Review = (ReviewAndRatingModel.Review != null && ReviewAndRatingModel.Review != "") ? ReviewAndRatingModel.Review : jobFound.Review;
                        _ReviewAndRatingService.UpdateReviewAndRating(jobFound);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Reviews updated successfully."), Configuration.Formatters.JsonFormatter);
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
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }
        [Route("GetReviewsByJob")]
        [HttpGet]
        public HttpResponseMessage GetReviewsByJob([FromUri] Guid ServiceProviderId, [FromUri] int PageNumber)
        {
            try
            {
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ServiceProvider Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var jobs = new List<JobRequest>();
                        if(Customer.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer))
                        {
                             jobs = _RequestService.GetRequests().Where(r => r.CustomerIdBy == ServiceProviderId).ToList();
                        }
                        else
                        {
                            jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId).ToList();
                        }
                       
                        if (jobs.Count() > 0)
                        {
                            List<Reviews> obj = new List<Reviews>();

                            foreach (var job in jobs)
                            {
                                var reviews = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.JobRequestId == job.JobRequestId && r.CustomerId!=ServiceProviderId).FirstOrDefault();
                                if (reviews != null)
                                {
                                   Mapper.CreateMap<ReviewAndRating, Reviews>();
                                    Reviews reviewModel = Mapper.Map<ReviewAndRating, Reviews>(reviews);

                                    var customerCommented = _CustomerService.GetCustomer(reviewModel.CustomerId);

                                    reviewModel.CustomerName = customerCommented.FirstName + " " + customerCommented.LastName;
                                    reviewModel.PhotoPath = customerCommented.PhotoPath;
                                    obj.Add(reviewModel);
                                }
                            }


                            int numberOfObjectsPerPage = 10;
                            var modelsdata = obj.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "no job found."), Configuration.Formatters.JsonFormatter);
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


        [Route("GetReviewsByJobProvider")]
        [HttpGet]
        public HttpResponseMessage GetReviewsByJobProvider([FromUri] Guid ServiceProviderId, [FromUri] int PageNumber)
        {
            try
            {
                if (ServiceProviderId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ServiceProvider Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                var Customer = _CustomerService.GetCustomer(ServiceProviderId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        if(Customer.CustomerType!=EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Provide service provider id."), Configuration.Formatters.JsonFormatter);
                        }
                        var jobs = _RequestService.GetRequests().Where(r => r.CustomerIdTo == ServiceProviderId).ToList();
                        if (jobs.Count() > 0)
                        {
                            List<ReviewsServiceProvider> obj = new List<ReviewsServiceProvider>();

                            foreach (var job in jobs)
                            {
                                var reviews = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.CustomerId == ServiceProviderId && r.JobRequestId==job.JobRequestId).FirstOrDefault();

                                if (reviews != null)
                                {
                                    Mapper.CreateMap<ReviewAndRating, ReviewsServiceProvider>();
                                    ReviewsServiceProvider reviewModel = Mapper.Map<ReviewAndRating, ReviewsServiceProvider>(reviews);

                                    var customerCommented = _CustomerService.GetCustomer(reviews.CustomerId);
                                    var customerCommentedTo = _CustomerService.GetCustomer(job.CustomerIdBy);
                                    reviewModel.ReviewedToCustomerId = customerCommentedTo.CustomerId;
                                    reviewModel.ReviewedToCustomerName = customerCommentedTo.FirstName + " " + customerCommentedTo.LastName;
                                    reviewModel.ReviewedToPhotoPath = customerCommentedTo.PhotoPath;
                                    reviewModel.ReviewedByCustomerId = reviews.CustomerId;
                                    reviewModel.ReviewedByCustomerName = customerCommented.FirstName + " " + customerCommented.LastName;
                                    reviewModel.ReviewedByPhotoPath = customerCommented.PhotoPath;
                                    obj.Add(reviewModel);
                                }
                            }


                            int numberOfObjectsPerPage = 10;
                            var modelsdata = obj.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "no job found."), Configuration.Formatters.JsonFormatter);
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
        //[Route("GetReviewsByDeal")]
        //[HttpGet]
        //public HttpResponseMessage GetReviewsByDeal([FromUri] int DealId, [FromUri] int PageNumber)
        //{
        //    try
        //    {
        //        if (DealId == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Deal Id is blank"), Configuration.Formatters.JsonFormatter);
        //        }

        //        var deals = _DealsService.GetDeal(DealId);
        //        if (deals != null)
        //        {
        //            var reviews = _ReviewAndRatingService.GetReviewAndRatings().Where(r => r.DealId == DealId).ToList();
        //            List<Reviews> objReview = new List<Reviews>();

        //            float rating = 0;
        //            foreach (var review in reviews)
        //            {
        //                Mapper.CreateMap<SuperDolarClub.Entity.ReviewAndRating, SuperDolarClub.Models.Reviews>();
        //                SuperDolarClub.Models.Reviews reviewModel = Mapper.Map<SuperDolarClub.Entity.ReviewAndRating, SuperDolarClub.Models.Reviews>(review);
        //                rating += reviewModel.Rating;
        //                var customer = _CustomerService.GetCustomer(reviewModel.CustomerId);
        //                reviewModel.CustomerName = customer.FirstName + " " + customer.LastName;
        //                reviewModel.PhotoPath = customer.PhotoPath;
        //                objReview.Add(reviewModel);
        //            }
        //            int numberOfObjectsPerPage = 10;
        //            var modelsdata = objReview.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "no deal found."), Configuration.Formatters.JsonFormatter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "no deal found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}


    }
}