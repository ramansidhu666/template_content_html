using AutoMapper;
using HomeHelp.Controllers;
using HomeHelp.Core.Infrastructure;
using HomeHelp.Entity;
using HomeHelp.Infrastructure;
using HomeHelp.Models;
using HomeHelp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HomeHelp.Web.Controllers
{
    public class AgencyController : BaseController
    {
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        //public IServiceItemService _ServiceItemService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public IRequestService _RequestService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public IAgencyJobService _AgencyJobService { get; set; }
        public ICustomerLocationService _CustomerLocationService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public AgencyController(IRequestService RequestService, ICategoryService CategoryService, ICustomerService CustomerServices, 
            IUserService UserService, IRoleService RoleService, IFormService FormService, IRoleDetailService RoleDetailService, IUserRoleService UserRoleService,
            IAgencyIndividualService AgencyIndividualService, IAgencyJobService AgencyJobService, ICustomerLocationService CustomerLocationService, INotificationService NotificationService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._CategoryService = CategoryService;
            this._RequestService = RequestService;
            this._UserRoleService = UserRoleService;
            this._UserService = UserService;
            this._CustomerService = CustomerServices;
            this._AgencyIndividualService = AgencyIndividualService;
            this._AgencyJobService = AgencyJobService;
            this._CustomerLocationService = CustomerLocationService;
            this._NotificationService = NotificationService;
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
            List<AgencyIndividualModel> AgencyIndividualModels = new List<AgencyIndividualModel>();
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
               
                var myIndividuals = _AgencyIndividualService.GetAgencyIndividualByParentId(Users.AgencyIndividualId);
                Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                foreach (var User in myIndividuals)
                {
                    var cc = _CustomerService.GetCustomerByUserId(User.UserId);
                    var inProgressJob = _RequestService.GetProgressRequest(cc.CustomerId, EnumValue.GetEnumDescription(EnumValue.JobStatus.InProgress));
                    var _User = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(User);
                    if (inProgressJob.Count > 0)
                    {
                        foreach (var item in inProgressJob)
                        {
                            if (cc.CustomerId == item.CustomerIdTo)
                            {
                                _User.inProgress = true;
                                //Mapper.CreateMap<JobRequest, RequestModel>();
                                //var _requestData = Mapper.Map<JobRequest, RequestModel>(item);
                                _User.jobRequestId = item.CustomerIdTo;
                                
                            }
                            else
                            {
                                _User.inProgress = false;
                            }
                        }
                    }
                    AgencyIndividualModels.Add(_User);

                }
                return View(AgencyIndividualModels);
            }
            else
            {
                return RedirectToAction("LogOn","Account");
            }
            
        }

        public ActionResult EditProfile()
        {
            //UserPermissionAction("Category", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            //CheckPermission();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
           
            AgencyIndividualModel AgencyIndividualModels = new AgencyIndividualModel();
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                AgencyIndividualModels = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(Users);
                //AgencyIndividualModels.Add(_User);
                return View(AgencyIndividualModels);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }

        [HttpPost]
        public ActionResult UpdateProfile([Bind(Include = "AgencyIndividualId,EmailId,FullName,WorkRate,ContactNumber,Address,Latitude,Longitude")] 
            AgencyIndividualModel model, HttpPostedFileBase file)
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
                    string name ="";
                    if (file != null && file.ContentLength > 0)
                    {
                        try
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                name = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                                string path = Path.Combine(Server.MapPath("~/AgencyImage"), name);
                                file.SaveAs(path);
                            }

                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        }

                    }
                    var existingUser = _UserService.GetUserByEmailId(model.EmailId);
                    if (existingUser != null)
                    {

                        existingUser.FirstName = model.FullName;
                        existingUser.LastUpdatedOn = DateTime.Now;
                        _UserService.UpdateUser(existingUser);

                        var agencyIndividualDetail=_AgencyIndividualService.GetAgencyIndividualById(model.AgencyIndividualId);
                        if (agencyIndividualDetail != null)
                        {
                            agencyIndividualDetail.FullName = model.FullName;
                            agencyIndividualDetail.Address = model.Address;
                            agencyIndividualDetail.ContactNumber = model.ContactNumber;
                            agencyIndividualDetail.Latitude = model.Latitude;
                            agencyIndividualDetail.Longitude = model.Longitude;
                            agencyIndividualDetail.LastUpdatedOn = DateTime.Now;
                            agencyIndividualDetail.WorkRate = model.WorkRate;
                            agencyIndividualDetail.PhotoPath = name==""?"":CommonCls.GetURL() + "/AgencyImage/"+name;
                            _AgencyIndividualService.UpdateAgencyIndividual(agencyIndividualDetail);
                            TempData["MessageBody"] = "Registeration done.";
                            ViewBag.Error = TempData["MessageBody"];
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["MessageBody"] = "User not found.";
                            ViewBag.Error = TempData["MessageBody"];
                            return View(model);
                        }
                       
                    }
                    else
                    {

                        TempData["MessageBody"] = "User not found.";
                        ViewBag.Error = TempData["MessageBody"];
                        return View(model);
                    }
                }
                else
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                    var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
                    TempData["MessageBody"] = "Please fill the required fields.";
                    ViewBag.Error = TempData["MessageBody"];
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);
                return View(model);
            }

        }
       
        public ActionResult AddIndividual(int? key)
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
            List<AgencyIndividualModel> AgencyIndividualModels = new List<AgencyIndividualModel>();
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                var myIndividuals = _AgencyIndividualService.GetNotAddedAgencyIndividuals();
                Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                foreach (var User in myIndividuals)
                {
                    var _User = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(User);
                    AgencyIndividualModels.Add(_User);

                }
                return View(AgencyIndividualModels);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }
      
        public ActionResult NewRequest(int? key)
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
            List<AgencyJobModel> AgencyJobModels = new List<AgencyJobModel>();
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var newRequest = new List<AgencyJob>();
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                var agencyMembers = _AgencyIndividualService.GetAgencyIndividuals().Where(c => c.AgencyIndividualId == Users.AgencyIndividualId || c.ParentId == Users.AgencyIndividualId).ToList();
                foreach (var item in agencyMembers)
                {
                    var customer = _CustomerService.GetCustomerByUserId(item.UserId);
                    if (customer == null)
                    {
                        newRequest = _AgencyJobService.GetAgencyNewRequest(item.AgencyIndividualId);

                    }
                    else
                    {
                        newRequest = _AgencyJobService.GetAgencyNewRequest(customer.CustomerId);
                    }
                    Mapper.CreateMap<AgencyJob, AgencyJobModel>();
                    foreach (var item1 in newRequest)
                    {
                        Mapper.CreateMap<JobRequest, RequestModel>();
                        var jobRequestDetail = _RequestService.GetRequest(item1.JobRequestId);
                        var _JobRequest = Mapper.Map<JobRequest, RequestModel>(jobRequestDetail);
                        var category = _CategoryService.GetCategory(_JobRequest.CategoryId);
                        if (category != null)
                        {
                            _JobRequest.CategoryName = category.Name;
                        }
                        var _User = Mapper.Map<AgencyJob, AgencyJobModel>(item1);
                        _User.requestModel = _JobRequest;
                        AgencyJobModels.Add(_User);

                    }
                }
                
                //newRequest = _AgencyJobService.GetAgencyNewRequest(Users.AgencyIndividualId);
               
                return View(AgencyJobModels);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }
      
        public ActionResult DoneRequest(int? key)
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
            List<AgencyJobModel> AgencyJobModels = new List<AgencyJobModel>();
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                var completeResquest = _AgencyJobService.GetAgencyDoneRequest(Users.AgencyIndividualId);
                Mapper.CreateMap<AgencyJob, AgencyJobModel>();
                foreach (var item in completeResquest)
                {
                    Mapper.CreateMap<JobRequest, RequestModel>();
                    var jobRequestDetail = _RequestService.GetRequest(item.JobRequestId);
                    var _JobRequest = Mapper.Map<JobRequest, RequestModel>(jobRequestDetail);
                    var _User = Mapper.Map<AgencyJob, AgencyJobModel>(item);
                    _User.requestModel = _JobRequest;
                    AgencyJobModels.Add(_User);

                }
                return View(AgencyJobModels);
            }
            else
            {   
                return RedirectToAction("LogOn", "Account");
            }

        }
        public ActionResult RequestDetail(int id,Guid agencyJobId,int categoryId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            Session["AssignJobId"] = agencyJobId;
            Mapper.CreateMap<JobRequest, RequestModel>();
            var jobRequestDetail = _RequestService.GetRequest(id);
            var _JobRequest = Mapper.Map<JobRequest, RequestModel>(jobRequestDetail);
        
            int UserId = Convert.ToInt32(Session["UserId"].ToString());
            if (UserId > 0)
            {
                List<AgencyIndividualModel> AgencyIndividualModels = new List<AgencyIndividualModel>();
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(UserId);
                var myIndividuals = _AgencyIndividualService.GetAgencyIndividualCategoryByParentId(Users.AgencyIndividualId, categoryId);
                Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                foreach (var User in myIndividuals)
                {
                    var _User = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(User);
                    AgencyIndividualModels.Add(_User);

                }
                _JobRequest.agencyIndividualModel = AgencyIndividualModels;
               
            }
            return View(_JobRequest);
        }

        public ActionResult AssignRequest(int? key)
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
            int id = Convert.ToInt32(Session["UserId"].ToString());
            if (id > 0)
            {
                var Users = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                var myIndividuals = _AgencyIndividualService.GetAgencyIndividualByParentId(Users.AgencyIndividualId);
                Mapper.CreateMap<AgencyIndividual, AgencyIndividualModel>();
                foreach (var User in myIndividuals)
                {
                    var _User = Mapper.Map<AgencyIndividual, AgencyIndividualModel>(User);
                    AgencyIndividualModels.Add(_User);

                }
                return View(AgencyIndividualModels);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        [HttpPost]
        public ActionResult AssignRequest(Guid assignedId, int jobRequestId)
        {
            string UserMessage = "";
            string Flag = "";
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (assignedId == null || jobRequestId == 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {   
                RequestModel requestModel = new RequestModel();
                Mapper.CreateMap<JobRequest, RequestModel>();
                var jobRequestDetail = _RequestService.GetRequest(jobRequestId);
                var customerToData=_AgencyIndividualService.GetAgencyIndividual(assignedId);
                var data=_CustomerService.GetCustomerByUserId(customerToData.UserId);
                int id = Convert.ToInt32(Session["UserId"].ToString());
                if (id > 0)
                {
                    var assignedBy = _AgencyIndividualService.GetAgencyIndividualByUserId(id);
                    UserMessage = "You have new job request by " + assignedBy.FullName;
                    Flag = "NewRequest";
                    Notification notification = new Notification();
                    notification.CustomerIdBy = assignedBy.AgencyIndividualId;

                    //notification.CustomerIdTo = data.CustomerId;
                    notification.CustomerIdTo = data.CustomerId;

                    notification.SourceId = jobRequestDetail.JobRequestId;
                    notification.UserMessage = UserMessage;
                    //notification.NotificationType = Convert.ToString(EnumValue.NotificationType.Request); 
                    notification.Flag = Flag;
                    notification.DeviceType = data.DeviceType;
                    notification.NotificationStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.New);
                    _NotificationService.InsertNotification(notification);

                    string Message = "{\"flag\":\"" + Flag + "\",\"JobRequestId\":\"" + jobRequestDetail.JobRequestId + "\",\"UserMessage\":\"" + UserMessage + "\"}";

                    if (data.ApplicationId != null && data.ApplicationId != "")
                    {

                        if (data.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                        {
                            //Send Notification another Andriod
                            CommonCls.SendFCM_Notifications(data.ApplicationId, Message, true);
                        }
                        else
                        {
                            string Msg = UserMessage;

                            CommonCls.TestSendFCM_Notifications(data.ApplicationId, Message, Msg, true);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("LogOn", "Account");
                }
                jobRequestDetail.CustomerIdTo = data.CustomerId;
                var assignedJob = _AgencyJobService.GetAgencyJobByJobRequest(jobRequestDetail.JobRequestId);
                assignedJob.AgencyIndividualId = data.CustomerId;
                if(jobRequestDetail.JobStatus==EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined))
                {
                    jobRequestDetail.RequestStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.New);
                    jobRequestDetail.JobStatus = EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending);
                    assignedJob.Status = EnumValue.GetEnumDescription(EnumValue.JobStatus.Pending);
                    
                }
               
                _RequestService.UpdateRequest(jobRequestDetail);
                _AgencyJobService.UpdateAgencyJob(assignedJob);
               

                return Json(true, JsonRequestBehavior.AllowGet);
            }
                        
        }

        public ActionResult TrackJob(Guid jobRequestId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            //var agencyIndividual=_AgencyIndividualService.GetAgencyIndividual(jobRequestId);
            Mapper.CreateMap<CustomerLocation, CustomerLocationModel>();
            //var jobRequestDetail = _CustomerService.GetCustomerByUserId(agencyIndividual.UserId);
            var customerLocation = _CustomerLocationService.GetCustomerLocationByCustomerId(jobRequestId);
            var _JobRequest = Mapper.Map<CustomerLocation, CustomerLocationModel>(customerLocation);
            _JobRequest.CustomerIdTo = jobRequestId;
            return View(_JobRequest);
        }
        public ActionResult UpdateTrackJob(Guid jobRequestId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            //var agencyIndividual = _AgencyIndividualService.GetAgencyIndividual(jobRequestId);
            Mapper.CreateMap<CustomerLocation, CustomerLocationModel>();
            //var jobRequestDetail = _CustomerService.GetCustomerByUserId(agencyIndividual.UserId);
            var customerLocation = _CustomerLocationService.GetCustomerLocationByCustomerId(jobRequestId);
            var _JobRequest = Mapper.Map<CustomerLocation, CustomerLocationModel>(customerLocation);
            _JobRequest.CustomerIdTo = jobRequestId;
            return Json(_JobRequest, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddIndividual(Guid individualId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int id = Convert.ToInt32(Session["UserId"].ToString());

            AgencyIndividual individualData = _AgencyIndividualService.GetAgencyIndividual(individualId);

            var agencyData = _AgencyIndividualService.GetAgencyIndividualByUserId(id);

            Boolean result = CommonCls.SendMailOfAddIndividual(individualData.FullName, individualId, individualData.EmailId, agencyData.FullName, agencyData.AgencyIndividualId, agencyData.ContactNumber);
            if (result == true)
            {
                individualData.IsInvited = true;
                _AgencyIndividualService.UpdateAgencyIndividual(individualData);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult RemoveIndividual(Guid id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            AgencyIndividual individualData = _AgencyIndividualService.GetAgencyIndividual(id);
            if (individualData !=null)
            {
                individualData.ParentId = new Guid();
                individualData.IsInvited = false;
                _AgencyIndividualService.UpdateAgencyIndividual(individualData);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {   
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult RejectRequest(int jobRequestId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            JobRequest jobRequest = _RequestService.GetRequest(jobRequestId);
            if (jobRequest != null)
            {
                jobRequest.RequestStatus = EnumValue.GetEnumDescription(EnumValue.RequestStatus.Declined);
                _RequestService.UpdateRequest(jobRequest);

                AgencyJob agencyjob = _AgencyJobService.GetAgencyJobByJobRequest(jobRequestId);
                if (agencyjob != null)
                {
                    _AgencyJobService.DeleteAgencyJob(agencyjob);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult AcceptInvitation()
        {
            if (Request.QueryString != null & Request.QueryString.Count > 0)
            {
                var queryStrings = HttpUtility.UrlDecode(Request.QueryString.ToString());
                var arrQueryStrings = queryStrings.Split('&');
                var individualId = arrQueryStrings[0];
                var agencyId = arrQueryStrings[1];

                AgencyIndividual individualData = _AgencyIndividualService.GetAgencyIndividual(new Guid(individualId));
                try
                {
                    if (individualData != null)
                    {
                        individualData.ParentId = new Guid(agencyId);
                        _AgencyIndividualService.UpdateAgencyIndividual(individualData);
                        TempData["ShowMessage"] = true;
                        TempData["MessageBody"] = "Invitation Accepted.";
                        return View();
                    }

                }
                catch (Exception ex)
                {
                    ErrorLogging.LogError(ex);
                    TempData["ShowMessage"] = false;
                    return View();
                }
            }
            TempData["ShowMessage"] = false;
            TempData["MessageBody"] = "Invitation Accepted.";
            return View();

        }
        [HttpGet]
        public ActionResult Delete(Guid id)
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
                    _AgencyIndividualService.DeleteAgencyIndividual(objAgencyIndividual);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully Deleted.";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult Unblock(Guid id)
        {
            ViewBag.currentPageNumber = ViewBag.currentPageNumber;
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
                    objAgencyIndividual.IsActive = true;
                    _AgencyIndividualService.UpdateAgencyIndividual(objAgencyIndividual);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Account successfully activated.";
                    CommonCls.SendMailOfAccountIsActive(objAgencyIndividual.FullName, objAgencyIndividual.EmailId, "activated");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");

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


                    return RedirectToAction("Index");

                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult Pagination(int currentPageNumber)
        {
            Session["CurrentPageNumber"] = currentPageNumber;
            return RedirectToAction("Index");

        }
    }
}