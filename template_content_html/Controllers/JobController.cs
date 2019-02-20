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
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace HomeHelp.Web.Controllers
{
    public class JobController : BaseController
    {
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public IReviewAndRatingService _ReviewAndRatingService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICustomerPaymentService _CustomerPaymentService { get; set; }
        public IRequestService _RequestService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public IAgencyJobService _AgencyJobService { get; set; }
        public ICustomerLocationService _CustomerLocationService { get; set; }
        public JobController(IReviewAndRatingService ReviewAndRatingService ,IRequestService RequestService, ICategoryService CategoryService, ICustomerService CustomerServices, 
            IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService, IUserRoleService UserRoleService,
            IAgencyIndividualService AgencyIndividualService, IAgencyJobService AgencyJobService, ICustomerLocationService CustomerLocationService, ICustomerPaymentService CustomerPaymentService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._ReviewAndRatingService = ReviewAndRatingService;
            this._CategoryService = CategoryService;
            this._RequestService = RequestService;
            this._UserRoleService = UserRoleService;
            this._UserService = UserService;
            this._CustomerService = CustomerServices;
            this._AgencyIndividualService = AgencyIndividualService;
            this._AgencyJobService = AgencyJobService;
            this._CustomerLocationService = CustomerLocationService;
            this._CustomerPaymentService = CustomerPaymentService;
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

        public ActionResult Index(int? key)
        {
            //UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            //CheckPermission();
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
            List<JobDetails> result = new List<JobDetails>();
            
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {

                var jobs = new List<AgencyJob>();
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                var agencyMembers = _AgencyIndividualService.GetAgencyIndividuals().Where(c => c.AgencyIndividualId == Users.AgencyIndividualId || c.ParentId == Users.AgencyIndividualId).ToList();
                foreach (var item in agencyMembers)
                {
                    var customer = _CustomerService.GetCustomerByUserId(item.UserId);
                    if (customer == null)
                    {
                        jobs = _AgencyJobService.GetAgencyJobs().Where(c => c.AgencyIndividualId == item.AgencyIndividualId).ToList();
                        
                    }
                    else
                    {
                        jobs = _AgencyJobService.GetAgencyJobs().Where(c => c.AgencyIndividualId == customer.CustomerId).ToList();
                    }
                    Mapper.CreateMap<JobRequest, RequestModel>();
                    foreach (var job in jobs)
                    {
                        var jobDetail = new JobDetails();
                        var jobDetails = _RequestService.GetRequests().Where(j => j.JobRequestId == job.JobRequestId).FirstOrDefault();
                        if (jobDetails != null)
                        {
                            var AgencyJobModel = Mapper.Map<JobRequest, RequestModel>(jobDetails);
                            jobDetail.request = AgencyJobModel;
                            var category = _CategoryService.GetCategory(jobDetails.CategoryId);
                            if (category != null)
                            {
                                jobDetail.CategoryName = category.Name;
                            }

                        }
                        result.Add(jobDetail);

                    }
                }
           
                return View(result);
            }
            else
            {
                return RedirectToAction("LogOn","Account");
            }
            
        }
      
        public ActionResult Details(int? key,int JobRequestId)
        {
            //UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            //CheckPermission();
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
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var details = new JobDetails();
                var job = _RequestService.GetRequest(JobRequestId);
                if(job!=null)
                {
                    Mapper.CreateMap<JobRequest, RequestModel>();
                    var model = Mapper.Map<JobRequest, RequestModel>(job);
                    var category = _CategoryService.GetCategory(model.CategoryId);
                    if (category != null)
                    {
                        details.CategoryName = category.Name;
                    }

                    if (job.IsPaid == true)
                    {
                        var customerpaymentdata = _CustomerPaymentService.GetCustomerPaymentByJobRequestId(JobRequestId);
                        Mapper.CreateMap<CustomerPayment, CustomerPaymentModel>();
                        CustomerPaymentModel customerPaymentModel = Mapper.Map<CustomerPayment, CustomerPaymentModel>(customerpaymentdata);
                        details.customerPaymentModel = customerPaymentModel;
                    }
                    List<Reviews> objReview = new List<Reviews>();
                    var reviews =_ReviewAndRatingService.GetReviewAndRatings().Where(r => r.JobRequestId==JobRequestId).ToList();
               
                    float rating = 0;
                    foreach (var review in reviews)
                    {
                        Mapper.CreateMap<ReviewAndRating, Reviews>();
                        Reviews reviewModel = Mapper.Map<ReviewAndRating, Reviews>(review);
                        rating += Convert.ToSingle(reviewModel.Rating);
                        var customer = _CustomerService.GetCustomer(reviewModel.CustomerId);
                        reviewModel.CustomerName = customer.FirstName + " " + customer.LastName;
                        reviewModel.PhotoPath = customer.PhotoPath;
                        reviewModel.ReviewAndRatingId = review.ReviewAndRatingId;
                        objReview.Add(reviewModel);
                    }
                   // DealResponseModel.AverageRating = float.IsNaN(rating / reviews.Count()) ? 0 : (rating / reviews.Count());
                    details.Reviews = objReview;
                    details.TotalReviews = reviews.Count();

                    //START:Get detail of job assign individual

                    AgencyJob agencyJobData = _AgencyJobService.GetAgencyJobByJobRequest(JobRequestId);

                   if (agencyJobData != null)
                   {
                       var agencydata=_AgencyIndividualService.GetAgencyIndividual(agencyJobData.AgencyIndividualId);
                       if (agencydata == null)
                       {
                           var customerData = _CustomerService.GetCustomer(agencyJobData.AgencyIndividualId);
                           Mapper.CreateMap<Customer, CustomerModel>();
                           CustomerModel agencyModel = Mapper.Map<Customer, CustomerModel>(customerData);
                           details.customerModel = agencyModel;
                       }
                       else
                       {
                           Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                           AgencyIndividualModel agencyModel = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(agencydata);
                           details.agencyIndividualModel = agencyModel;
                       }
                   }
                   else
                   {
                       details.agencyIndividualModel = null;
                   }
                   //END:Get detail of job assign individual
                    details.request = model;
                }
                return View(details);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }

        public ActionResult DeleteReviews(int[] ids)
        {
            //UserPermissionAction("Deal", RoleAction.detail.ToString());
            //CheckPermission();
            if (ids == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                List<ReviewAndRating> objReviews = _ReviewAndRatingService.GetReviewAndRatings().Where(r => ids.Contains(r.ReviewAndRatingId)).ToList();

                if (objReviews != null)
                {
                    foreach (var objReview in objReviews)
                    {
                        _ReviewAndRatingService.DeleteReviewAndRating(objReview);
                    }

                   return Json(true,JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                 return Json(true,JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult Pagination(int currentPageNumber)
        {
            Session["CurrentPageNumber"] = currentPageNumber;
            return RedirectToAction("Index");

        }
    }
}