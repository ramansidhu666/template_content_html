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
using Onlo.Entity;
using Onlo.Services;
using Newtonsoft.Json;
using Onlo.Models;
using AutoMapper;
using Onlo.Infrastructure;
using Onlo.Core.Infrastructure;

namespace Onlo.Controllers.WebApi
{
    [RoutePrefix("Comment")]
    public class CommentApiController : ApiController
    {
        public ITasksService _TasksService { get; set; }
        public ITaskCustomerService _TaskCustomerService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ITaskFileService _TaskFileService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public ITaskCustomerFileService _TaskCustomerFileService { get; set; }
        public ICommentService _CommentService { get; set; }

        public CommentApiController(ITasksService TasksService, INotificationService NotificationService, ITaskCustomerService TaskCustomerService, ICustomerService CustomerService, ITaskFileService TaskFileService, ITaskCustomerFileService TaskCustomerFileService, ICommentService CommentService)
        {
            this._TasksService = TasksService;
            this._NotificationService = NotificationService;
            this._TaskCustomerService = TaskCustomerService;
            this._CustomerService = CustomerService;
            this._TaskFileService = TaskFileService;
            this._TaskCustomerFileService = TaskCustomerFileService;
            this._CommentService = CommentService;
        }
        [Route("SaveComment")]
        [HttpPost]
        public HttpResponseMessage SaveComment([FromBody]CommentModel CommentModel)
        {
            var taskAssignedToIds = new List<NotifyModel>();
            string UserMessage = "";
            string NotificationType = "";
            string setFlag = "";
            try
            {
                if (CommentModel.TaskId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Task Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CommentModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CommentModel.CommentMessage == null && CommentModel.CommentMessage == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Comment Message is blank."), Configuration.Formatters.JsonFormatter);
                }

                Mapper.CreateMap<Onlo.Models.CommentModel, Onlo.Entity.Comment>();
                Onlo.Entity.Comment Comment = Mapper.Map<Onlo.Models.CommentModel, Onlo.Entity.Comment>(CommentModel);
                Comment.CustomerId = CommentModel.CustomerId;
                Comment.CommentMessage = CommentModel.CommentMessage;
                Comment.TaskId = CommentModel.TaskId;
                _CommentService.InsertComment(Comment);

                var Task = _TasksService.GetTask(CommentModel.TaskId);
                var customer = _CustomerService.GetCustomer(CommentModel.CustomerId);
                Onlo.Entity.Customer CustomerBy = _CustomerService.GetCustomer(Convert.ToInt32(CommentModel.CustomerId));
                
                if (CustomerBy.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Teacher))
                {
                    var taskAssignedToStudent = _TaskCustomerService.GetTaskCustomers().Where(t => t.TaskID == CommentModel.TaskId).ToList();
                    var ids = taskAssignedToStudent.Select(t => t.CustomerId).ToList();

                    foreach (var item in ids)
                    {
                        NotifyModel notify = new NotifyModel();
                        notify.AssignId = item;

                        taskAssignedToIds.Add(notify);
                    }

                    foreach (var item in ids)
                    {
                        NotifyModel notifyy = new NotifyModel();
                        var taskAssignedToParent = _CustomerService.GetCustomers().Where(c => c.ParentId == item).FirstOrDefault();
                        if (taskAssignedToParent != null)
                        {
                            notifyy.AssignId = taskAssignedToParent.CustomerId;

                            taskAssignedToIds.Add(notifyy);
                        }

                    }
                    if (Task.TaskType == EnumValue.GetEnumDescription(EnumValue.DietType.Diet))
                    {
                        UserMessage = "You have new comment on diet - " + Task.TaskName + ".";
                        NotificationType = "Teacher_CommentDiet";
                        setFlag = "Teacher_DietComment";
                    }
                    else
                    {
                        UserMessage = "You have new comment on task - " + Task.TaskName + ".";
                        NotificationType = "Teacher_CommentTask";
                        setFlag = "Teacher_TaskComment";
                    }
                }
                else if (CustomerBy.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Student))
                {
                    NotifyModel notify = new NotifyModel();
                    var taskAssignedToParent1 = _CustomerService.GetCustomers().Where(c => c.ParentId == CommentModel.CustomerId).FirstOrDefault();
                    if (taskAssignedToParent1 != null)
                    {
                        notify.AssignId = taskAssignedToParent1.CustomerId;
                        taskAssignedToIds.Add(notify);
                    }

                    NotifyModel notifyy = new NotifyModel();
                    var taskAssignedToTeacher1 = _CustomerService.GetCustomers().Where(c => c.CustomerId == customer.ParentId).FirstOrDefault();
                    notifyy.AssignId = taskAssignedToTeacher1.CustomerId;
                    taskAssignedToIds.Add(notifyy);
                    if (Task.TaskType == EnumValue.GetEnumDescription(EnumValue.DietType.Diet))
                    {
                        UserMessage = "You have new comment on diet - " + Task.TaskName + ".";
                        NotificationType = "Student_CommentDiet";
                        setFlag = "Student_DietComment";
                    }
                    else
                    {
                        UserMessage = "You have new comment on task - " + Task.TaskName + ".";
                        NotificationType = "Student_CommentTask";
                        setFlag = "Student_TaskComment";
                    }

                }
                else if (CustomerBy.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.Parent))
                {
                    NotifyModel notify = new NotifyModel();
                    var taskAssignedToStudent2 = _CustomerService.GetCustomers().Where(c => c.CustomerId == customer.ParentId).FirstOrDefault();
                    notify.AssignId = taskAssignedToStudent2.CustomerId;
                    taskAssignedToIds.Add(notify);
                    NotifyModel notifyy = new NotifyModel();
                    var taskAssignedToTeacher = _CustomerService.GetCustomers().Where(c => c.CustomerId == taskAssignedToStudent2.ParentId).FirstOrDefault();
                    if (taskAssignedToTeacher != null)
                    {
                        notifyy.AssignId = taskAssignedToTeacher.CustomerId;

                        taskAssignedToIds.Add(notifyy);
                    }
                    if (Task.TaskType == EnumValue.GetEnumDescription(EnumValue.DietType.Diet))
                    {
                        UserMessage = "You have new comment on diet - " + Task.TaskName + ".";
                        NotificationType = "Parent_CommentDiet";
                        setFlag = "Parent_DietComment";
                    }
                    else
                    {
                        UserMessage = "You have new comment on task - " + Task.TaskName + ".";
                        NotificationType = "Parent_CommentTask";
                        setFlag = "Parent_TaskComment";
                    }

                }
                foreach (var taskAssignedToId in taskAssignedToIds)
                {
                   
                    string ApplicationId = _CustomerService.GetCustomerApplicationIds(Convert.ToInt32(taskAssignedToId.AssignId), EnumValue.GetEnumDescription(EnumValue.DeviceType.Android)) + ",";

                    if (ApplicationId != null && ApplicationId != "'',")
                    {
                        Notification Notification = new Notification();
                       
                        Notification.CustomerId = taskAssignedToId.AssignId;
                        Notification.TaskID = Task.TaskId;
                        Notification.UserMessage = UserMessage;
                        Notification.NotificationType = NotificationType;
                        Notification.Flag = setFlag;
                        Notification.DeviceType = EnumValue.GetEnumDescription(EnumValue.DeviceType.Android);
                        _NotificationService.InsertNotification(Notification);
                        string Message = "";
                       
                        if (Task.TaskType == EnumValue.GetEnumDescription(EnumValue.DietType.Diet))
                        {
                            Message = "{\"flag\":\"" + setFlag + "\",\"TaskId\":\"" + Task.TaskId + "\",\"DietName\":\"" + Task.TaskName + "\",\"CommentMsg\":\"" + CommentModel.CommentMessage + "\",\"Name\":\"" + CustomerBy.Name + "\",\"PhotoPath\":\"" + CustomerBy.PhotoPath + "\",\"Id\":\"" + CustomerBy.CustomerId + "\"}";
                        }
                        else
                        {
                           Message = "{\"flag\":\"" + setFlag + "\",\"TaskId\":\"" + Task.TaskId + "\",\"CommentMsg\":\"" + CommentModel.CommentMessage + "\",\"DietName\":\"" + Task.TaskName + "\",\"Name\":\"" + CustomerBy.Name + "\",\"PhotoPath\":\"" + CustomerBy.PhotoPath + "\",\"Id\":\"" + CustomerBy.CustomerId + "\"}";
                        }

                        CommonCls.SendGCM_Notifications(ApplicationId, Message, true);

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Comment added successfully."), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("GetCommentsById")]
        [HttpGet]
        public HttpResponseMessage GetCommentsById([FromUri] int CustomerId, int TaskId, int PageNumber)
        {
            var models = new List<CommentModel>();
            try
            {
                var Comments = _CommentService.GetComments().Where(c => c.TaskId == TaskId).ToList();
                if (Comments.Count() > 0)
                {
                    foreach (var Comment in Comments)
                    {
                        Mapper.CreateMap<Onlo.Entity.Comment, Onlo.Models.CommentModel>();
                        Onlo.Models.CommentModel Model = Mapper.Map<Onlo.Entity.Comment, Onlo.Models.CommentModel>(Comment);
                        var customerDetail = _CustomerService.GetCustomer(Comment.CustomerId);
                        Model.Name = customerDetail.Name;
                        Model.PhotoPath = customerDetail.PhotoPath;
                        Model.CustomerType = customerDetail.CustomerType;
                        models.Add(Model);
                    }
                    var CommentOrderByAsc = models.OrderByDescending(o => o.CreatedOn).ToList();
                    int numberOfObjectsPerPage = 15;
                    var modelsdata = CommentOrderByAsc.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Comment not found."), Configuration.Formatters.JsonFormatter);
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