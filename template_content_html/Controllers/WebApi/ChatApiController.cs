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
using HomeHelp.Models;
using AutoMapper;
using HomeHelp.Infrastructure;
using HomeHelp.Core.Infrastructure;
using System.Globalization;

namespace HomeHelp.Controllers.WebApi
{
    [RoutePrefix("Chat")]
    public class ChatApiController : ApiController
    {
        public ICategoryService _CategoryService { get; set; }
        public IRequestService _RequestService { get; set; }
        public IChatService _ChatService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public INotificationService _NotificationService { get; set; }
       
        public ChatApiController(ICategoryService CategoryService,IChatService ChatService,IRequestService RequestService, INotificationService NotificationService, ICustomerService CustomerService)
        {
            this._CategoryService = CategoryService;
            this._ChatService = ChatService;
            this._NotificationService = NotificationService;
            this._RequestService = RequestService;
            this._CustomerService = CustomerService;
           
        }
        //[Route("GetAllChats")]
        //[HttpGet]
        //public HttpResponseMessage GetAllChats()
        //{
        //    var models = new List<ChatModel>();
        //    try
        //    {
        //        var Chats = _ChatService.GetAll();
        //        Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatModel>().ForMember(c => c.CustomersBy, option => option.Ignore()).ForMember(c => c.CustomersTo, option => option.Ignore());
        //        foreach (var Chat in Chats)
        //        {
        //            models.Add(Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatModel>(Chat));
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        // GET api/Chats
        //http://HomeHelp.com/Chats/GetChat?ChatId=12
        //------------
        //[Route("GetChat")]
        //[HttpGet]
        //public HttpResponseMessage GetChat([FromUri]int ChatId)
        //{
        //    try
        //    {
        //        var Chat = _ChatService.GetById(ChatId);
        //        Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>();
        //        HomeHelp.Models.ChatResponseModel ChatModel = Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>(Chat);

        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", ChatModel), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Chat not found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}


        //[Route("GetNewChat")]
        //[HttpGet]
        //public HttpResponseMessage GetNewChat([FromUri]int ChatId,[FromUri] int PageNumber)
        //{
        //    try
        //    {
        //        var model = new List<ChatResponseModel>();
        //        var Chat = _ChatService.GetChats().Where(c => c.ChatId > ChatId).ToList();
        //        if(Chat.Count()>0)
        //        {
        //            foreach (var item in Chat)
        //            {
        //                 Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>();
        //            HomeHelp.Models.ChatResponseModel ChatModel = Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>(item);
        //            //ChatModel.ChatTime = Convert.ToDateTime(ChatModel.DateTimeCreated).ToString("hh:mm tt");
        //            ChatModel.ChatTime = chatTime(ChatModel.DateTimeCreated);
        //            model.Add(ChatModel);
        //            }
        //            var orderByAsc = model.OrderByDescending(o => o.DateTimeCreated).ToList();
        //            int numberOfObjectsPerPage = 10;
        //            var modelsdata = orderByAsc.Skip(numberOfObjectsPerPage * PageNumber).Take(numberOfObjectsPerPage);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No new chat found."), Configuration.Formatters.JsonFormatter);
        //        }
                
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Chat not found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //-----------
        // GET api/Chats
        //http://HomeHelp.com/Chats/GetChatList/{Json}

        //------------
        //[Route("GetUserChatList")]
        //[HttpPost]
        //public HttpResponseMessage GetUserChatList([FromBody]ChatModel ChatModel)
        //{
        //    var models = new List<UserChatListModel>();
        //    try
        //    {
        //        List<int?> ListCustomerIDs = new List<int?>();
        //        ListCustomerIDs.Add(ChatModel.CustomerIdBy);
        //        var Chats = _ChatService.GetAll().Where(x => ListCustomerIDs.Contains(x.CustomerIdBy) || ListCustomerIDs.Contains(x.CustomerIdTo)).OrderByDescending(x => x.DateTimeCreated).ToList();
        //        var CustomerIdList = ((from m in Chats where m.CustomerIdBy == ChatModel.CustomerIdBy select m.CustomerIdTo)
        //                      .Concat(from m in Chats where m.CustomerIdTo == ChatModel.CustomerIdBy select m.CustomerIdBy)).Distinct().ToList();

        //        Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatModel>().ForMember(c => c.CustomersBy, option => option.Ignore()).ForMember(c => c.CustomersTo, option => option.Ignore());
        //        foreach (var customerId in CustomerIdList)
        //        {
        //            if (customerId != null)
        //            {
        //                List<int?> CustomerIds = new List<int?>();
        //                CustomerIds.Add(ChatModel.CustomerIdBy);
        //                CustomerIds.Add(customerId);


        //                var chatList = Chats.Where(x => CustomerIds.Contains(x.CustomerIdBy) && CustomerIds.Contains(x.CustomerIdTo)).OrderByDescending(x => x.DateTimeCreated).ToList();

        //                //var chat = Chats.Where(x => CustomerIds.Contains(x.CustomerIdBy) && CustomerIds.Contains(x.CustomerIdTo)).OrderByDescending(x => x.DateTimeCreated).FirstOrDefault();
        //                if (chatList.Count() > 0)
        //                {
        //                    bool IsSkipRec = true;
        //                    Chat chat = new Chat();
        //                    foreach (var chatItem in chatList)
        //                    {
        //                        chat = chatItem;
        //                        if (chatItem.CustomerIdBy == ChatModel.CustomerIdBy && Convert.ToBoolean(chatItem.IsDeletedCustomerBy))
        //                        {
        //                            IsSkipRec = true;
        //                        }
        //                        else if (chatItem.CustomerIdTo == ChatModel.CustomerIdBy && Convert.ToBoolean(chatItem.IsDeletedCustomerTo))
        //                        {
        //                            IsSkipRec = true;
        //                        }
        //                        else
        //                        {
        //                            IsSkipRec = false;
        //                             break;
        //                        }
        //                        }

        //                        //if (chat.CustomerIdBy == ChatModel.CustomerIdBy && Convert.ToBoolean(chat.IsDeletedCustomerBy))
        //                        //{
        //                        //    IsSkipRec = true;
        //                        //}
        //                        //else if (chat.CustomerIdTo == ChatModel.CustomerIdBy && Convert.ToBoolean(chat.IsDeletedCustomerTo))
        //                        //{
        //                        //    IsSkipRec = true;
        //                        //}

        //                        if (!IsSkipRec)
        //                        {
        //                            var customer = _CustomerService.GetCustomer(Convert.ToInt32(customerId));
        //                            UserChatListModel userChatListModel = new UserChatListModel();
        //                            userChatListModel.CustomerIdTo = customerId;
        //                            userChatListModel.CustomerName = customer.Name;
        //                            userChatListModel.PhotoPath = customer.PhotoPath;
        //                            userChatListModel.ChatContent = chat.ChatContent;
        //                           // userChatListModel.ChatTime = Convert.ToDateTime(chat.DateTimeCreated).ToString("hh:mm tt");
        //                            userChatListModel.ChatTime = chatTime(chat.DateTimeCreated);
        //                            userChatListModel.IsRead = chat.IsRead;
        //                            userChatListModel.ChatId = chat.ChatId;
        //                            userChatListModel.CountMessage = Chats.Where(x => CustomerIds.Contains(x.CustomerIdBy) && CustomerIds.Contains(x.CustomerIdTo)).Count();
        //                            models.Add(userChatListModel);
        //                        }
        //                   // }

        //                }
        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //--------------
        // GET api/Chats
        //http://HomeHelp.com/Chats/GetChatList/{Json}
        [Route("GetChatList")]
        [HttpPost]
        public HttpResponseMessage GetChatList([FromBody]ChatModel ChatModel)
        {
            var userList = new UserChatModel();
            var models = new List<ChatsModel>();
            try
            {
                int count = 0;
                List<Guid?> ListCustomerIDs = new List<Guid?>();
                ListCustomerIDs.Add(ChatModel.CustomerIdBy);
                ListCustomerIDs.Add(ChatModel.CustomerIdTo);

                var Chats = _ChatService.GetAll().Where(x => ListCustomerIDs.Contains(x.CustomerIdBy) && ListCustomerIDs.Contains(x.CustomerIdTo)).ToList();

                var Results = (from m in Chats where m.CustomerIdBy == ChatModel.CustomerIdBy && m.CustomerIdTo == ChatModel.CustomerIdTo select m)
                              .Concat(from m in Chats where m.CustomerIdBy == ChatModel.CustomerIdTo && m.CustomerIdTo == ChatModel.CustomerIdBy select m)
                              .OrderBy(x => x.DateTimeCreated);


                Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatsModel>().ForMember(c => c.CustomersBy, option => option.Ignore()).ForMember(c => c.CustomersTo, option => option.Ignore());
                foreach (var Chat in Results)
                {

                    bool IsSkipRec = false;
                    if (Chat.CustomerIdBy == ChatModel.CustomerIdBy && Convert.ToBoolean(Chat.IsDeletedCustomerBy))
                    {
                        IsSkipRec = true;
                    }
                    else if (Chat.CustomerIdTo == ChatModel.CustomerIdBy && Convert.ToBoolean(Chat.IsDeletedCustomerTo))
                    {
                        IsSkipRec = true;
                    }

                    if (!IsSkipRec)
                    {
                        //When Our Customer ID (By) will match with Others CustomerID (To) then Update Read Message status
                        if (Chat.CustomerIdTo == ChatModel.CustomerIdBy)
                        {
                            //Update the Chat For IsRead Field true, It will Update by Trigger in SQL Table
                            _ChatService.Update(new HomeHelp.Entity.Chat
                            {
                                ChatId = Chat.ChatId,
                                CustomerIdBy = Chat.CustomerIdBy,
                                CustomerIdTo = Chat.CustomerIdTo,
                                ChatContent = Chat.ChatContent,
                                DateTimeCreated = Chat.DateTimeCreated,
                                IsDeletedCustomerBy = Chat.IsDeletedCustomerBy,
                                IsDeletedCustomerTo = Chat.IsDeletedCustomerTo,
                                IsRead = true
                            });
                            //End : Update the Chat For IsRead Field true

                            //Set the Read Message Status for List
                            Chat.IsRead = true;
                        }
                        Models.ChatsModel chatModel = Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatsModel>(Chat);
                        string sqlFormattedDate = Chat.DateTimeCreated.HasValue ? Chat.DateTimeCreated.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : "";

                        chatModel.DateTimeCreated = sqlFormattedDate;
                        //chatModel.ChatTime = chatTime(Chat.DateTimeCreated);
                        
                
                       // chatModel.ChatTime = Convert.ToDateTime(Chat.DateTimeCreated).ToString("hh:mm tt");
                        //Set Customer By
                        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerDetailModel>();
                        HomeHelp.Entity.Customer customersBy = _CustomerService.GetCustomer(Chat.CustomerIdBy);
                        Models.UserCustomerDetailModel customerByModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerDetailModel>(customersBy);
                        //End : Set Customer By
                        chatModel.CustomersBy = customerByModel;
                        //Set Customer To
                        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerDetailModel>();
                        HomeHelp.Entity.Customer customersTo = _CustomerService.GetCustomer(Chat.CustomerIdTo);
                        Models.UserCustomerDetailModel customerToModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerDetailModel>(customersTo);
                        //End : Set Customer To
                        chatModel.CustomersTo = customerToModel;
                        count++;
                        
                        models.Add(chatModel);
                    }
                }
               
                int numberOfObjectsPerPage = 10;
                var modelsdata = models.OrderByDescending(c=>c.DateTimeCreated).Skip(numberOfObjectsPerPage * ChatModel.PageNumber).Take(numberOfObjectsPerPage).ToList();
                //userList.CountMessage = count;
                //userList.chatList = modelsdata;
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
            }
        }
        public string chatTime(DateTime? chatTime)
        {
            TimeSpan span = (DateTime.Now - Convert.ToDateTime(chatTime));
            string[] year = CalculateDifference(Convert.ToDateTime(chatTime), DateTime.Now).Split(',');
            string result = ""; 
            if (year[0] == "0")
            {
                if (year[1] == "0")
                {
                    if (year[2] == "0")
                    {
                        string[] difference = (String.Format("{0} days, {1} hours, {2} minutes, {3} seconds", span.Days, span.Hours, span.Minutes, span.Seconds)).Split(',');

                        if (difference[1] == " 0 hours" && difference[2] == " 0 minutes")
                        {
                            result = "just now.";
                        }
                        else if (difference[2] == " 0 minutes")
                        {
                            result = difference[1] + " ago.";
                        }
                        else if (difference[1] == " 0 hours")
                        {
                            result = difference[2] + " ago.";
                        }
                        else
                        {
                            result = difference[1] + " ago.";
                        }

                    }
                    else
                    {
                        result = year[2] + " days ago.";
                    }
                }
                else
                {
                    result = year[1] + " months ago.";
                }
            }
            else
            {
                result = year[0] + " year ago.";
            }
            return result;
        }


        public string CalculateDifference(DateTime Bday, DateTime Cday)
        {
             int Years;
         int Months;
         int Days;
            if ((Cday.Year - Bday.Year) > 0 ||
               (((Cday.Year - Bday.Year) == 0) &&
               ((Bday.Month < Cday.Month) ||
               ((Bday.Month == Cday.Month) &&
               (Bday.Day <= Cday.Day)))))
            {

                int DaysInBdayMonth = DateTime.DaysInMonth(Bday.Year, Bday.Month);
                int DaysRemain = Cday.Day + (DaysInBdayMonth - Bday.Day);

                if (Cday.Month > Bday.Month)
                {
                    Years = Cday.Year - Bday.Year;
                    Months = Cday.Month - (Bday.Month + 1) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    Days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
                else if (Cday.Month == Bday.Month)
                {
                    if (Cday.Day >= Bday.Day)
                    {
                        Years = Cday.Year - Bday.Year;
                        Months = 0;
                        Days = Cday.Day - Bday.Day;
                    }
                    else
                    {
                       Years = (Cday.Year - 1) - Bday.Year;
                       Months = 11;
                       Days = DateTime.DaysInMonth(Bday.Year, Bday.Month) - (Bday.Day - Cday.Day);

                    }
                }
                else
                {
                    Years = (Cday.Year - 1) - Bday.Year;
                    Months = Cday.Month + (11 - Bday.Month) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    Days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
            }
            else
            {
                throw new ArgumentException("Birthday date must be earlier than current date");
            }
            return Years+ ","+Months + ","+Days;
        }

        [Route("GetRecentChatListByServiceProvider")]
        [HttpGet]
        public HttpResponseMessage GetRecentChatListByServiceProvider([FromUri]Guid CustomerId, [FromUri] int PageNumber)
        {

            var models = new List<RecentChatCustomerResponseModel>();
            try
            {
                var jobRequests = _RequestService.GetRequests().Where(c => c.CustomerIdTo == CustomerId).Select(c=>c.CustomerIdBy).ToList();
                foreach (var jobRequest in jobRequests.Distinct())
                {
                    var user = _CustomerService.GetCustomer(jobRequest);
                    Mapper.CreateMap<Customer, RecentChatCustomerResponseModel>();
                    var customer = Mapper.Map<Customer, RecentChatCustomerResponseModel>(user);
                    var message = _ChatService.GetChats().Where(c => c.CustomerIdBy == jobRequest || c.CustomerIdTo == jobRequest).OrderByDescending(c => c.DateTimeCreated).Take(1).FirstOrDefault();
                    if(message!=null)
                    {
                        string sqlFormattedDate = message.DateTimeCreated.HasValue? message.DateTimeCreated.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"): "";

                        customer.ChatTime = sqlFormattedDate;
                        customer.Message = message.ChatContent;
                    }
                    else
                    {
                        customer.Message ="";
                    }
                    
                    models.Add(customer);
                }
                int numberOfObjectsPerPage = 10;
                var modelsdata = models.Skip(numberOfObjectsPerPage * PageNumber).OrderByDescending(c => c.ChatTime).Take(numberOfObjectsPerPage);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetRecentChatListByCustomer")]
        [HttpGet]
        public HttpResponseMessage GetRecentChatListByCustomer([FromUri]Guid CustomerId, [FromUri] int PageNumber)
        {

            var models = new List<RecentChatServiceProviderResponseModel>();
            try
            {
                var jobRequests = _RequestService.GetRequests().Where(c => c.CustomerIdBy == CustomerId).Select(c => c.CustomerIdTo).ToList();
                foreach (var jobRequest in jobRequests.Distinct())
                {
                    var user = _CustomerService.GetCustomer(jobRequest);
                    Mapper.CreateMap<Customer, RecentChatServiceProviderResponseModel>();
                    var customer = Mapper.Map<Customer, RecentChatServiceProviderResponseModel>(user);
                    if(customer!=null)
                    {
                        var message = _ChatService.GetChats().Where(c => c.CustomerIdBy == jobRequest || c.CustomerIdTo == jobRequest).OrderByDescending(c => c.DateTimeCreated).Take(1).FirstOrDefault();

                        if (message != null)
                        {
                            string sqlFormattedDate = message.DateTimeCreated.HasValue ? message.DateTimeCreated.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : "";

                            customer.ChatTime = sqlFormattedDate;
                            customer.Message = message.ChatContent;
                        }
                        else
                        {
                            customer.Message = "";
                        }

                        //customer.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(_CustomerService.GetCustomer(jobRequest).CategoryId)).Name;
                        models.Add(customer);
                    }
                    
                }
                int numberOfObjectsPerPage = 10;
                var modelsdata = models.Skip(numberOfObjectsPerPage * PageNumber).OrderByDescending(c => c.ChatTime).Take(numberOfObjectsPerPage);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", modelsdata), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
            }
        }


        //// GET api/Chats
        ////http://HomeHelp.com/Chats/GetChatList/{Json}
        //[Route("GetChatListUnread")]
        //[HttpPost]
        //public HttpResponseMessage GetChatListUnread([FromBody]ChatModel ChatModel)
        //{
        //    var models = new List<ChatModel>();
        //    try
        //    {
        //        List<int?> ListCustomerIDs = new List<int?>();
        //        ListCustomerIDs.Add(ChatModel.CustomerIdBy);
        //        ListCustomerIDs.Add(ChatModel.CustomerIdTo);

        //        var Chats = _ChatService.GetAll().Where(x => ListCustomerIDs.Contains(x.CustomerIdBy) && ListCustomerIDs.Contains(x.CustomerIdTo)).ToList();

        //        var Results = (from m in Chats where m.CustomerIdBy == ChatModel.CustomerIdBy && m.CustomerIdTo == ChatModel.CustomerIdTo && m.IsRead == false select m)
        //                      .Concat(from m in Chats where m.CustomerIdBy == ChatModel.CustomerIdTo && m.CustomerIdTo == ChatModel.CustomerIdBy && m.IsRead == false select m)
        //                      .OrderBy(x => x.DateTimeCreated);

        //        Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatModel>().ForMember(c => c.CustomersBy, option => option.Ignore()).ForMember(c => c.CustomersTo, option => option.Ignore());
        //        foreach (var Chat in Results)
        //        {

        //            bool IsSkipRec = false;
        //            if (Chat.CustomerIdBy == ChatModel.CustomerIdBy && Convert.ToBoolean(Chat.IsDeletedCustomerBy))
        //            {
        //                IsSkipRec = true;
        //            }
        //            else if (Chat.CustomerIdTo == ChatModel.CustomerIdBy && Convert.ToBoolean(Chat.IsDeletedCustomerTo))
        //            {
        //                IsSkipRec = true;
        //            }
        //            if (!IsSkipRec)
        //            {
        //                //When Our Customer ID (By) will match with Others CustomerID (To) then Update Read Message status
        //                if ((Chat.CustomerIdTo == ChatModel.CustomerIdBy) && (Chat.IsRead == false))
        //                {
        //                    //Update the Chat For IsRead Field true, It will Update by Trigger in SQL Table
        //                    _ChatService.Update(new HomeHelp.Entity.Chat
        //                    {
        //                        ChatId = Chat.ChatId,
        //                        CustomerIdBy = Chat.CustomerIdBy,
        //                        CustomerIdTo = Chat.CustomerIdTo,
        //                        ChatContent = Chat.ChatContent,
        //                        DateTimeCreated = Chat.DateTimeCreated,
        //                        IsDeletedCustomerBy = Chat.IsDeletedCustomerBy,
        //                        IsDeletedCustomerTo = Chat.IsDeletedCustomerTo,
        //                        IsRead = true
        //                    });
        //                    //End : Update the Chat For IsRead Field true

        //                    //Set the Read Message Status for List
        //                    Chat.IsRead = true;
        //                    Models.ChatModel chatModel = Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatModel>(Chat);
        //                    chatModel.DateTimeCreatedStr = CommonCls.ConvertDate(Convert.ToDateTime(Chat.DateTimeCreated));
        //                    chatModel.ChatTime = Convert.ToDateTime(Chat.DateTimeCreated).ToString("hh:mm tt");
        //                    //DateTime ut = DateTime.SpecifyKind(Convert.ToDateTime(Chat.DateTimeCreated), DateTimeKind.Utc);
        //                    chatModel.ChatTimeServer = DateTime.SpecifyKind(Convert.ToDateTime(Chat.DateTimeCreated), DateTimeKind.Utc).ToString("hh:mm tt");
        //                    ////Set Customer By
        //                    //Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
        //                    //HomeHelp.Entity.Customer customersBy = _CustomerService.GetCustomer(Convert.ToInt32(Chat.CustomerIdBy));
        //                    //Models.CustomerModel customerByModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(customersBy);
        //                    ////End : Set Customer By
        //                    //chatModel.CustomersBy = customerByModel;
        //                    ////Set Customer To
        //                    //Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
        //                    //HomeHelp.Entity.Customer customersTo = _CustomerService.GetCustomer(Convert.ToInt32(Chat.CustomerIdTo));
        //                    //Models.CustomerModel customerToModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(customersTo);
        //                    ////End : Set Customer To
        //                    //chatModel.CustomersTo = customerToModel;
        //                    models.Add(chatModel);
        //                }
        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        //// GET api/Chats
        ////http://HomeHelp.com/Chats/GetChatNotificationList/{Json}
        //[Route("GetChatNotificationList")]
        //[HttpPost]
        //public HttpResponseMessage GetChatNotificationList([FromBody]ChatModel ChatModel)
        //{
        //    var models = new List<ChatModel>();
        //    try
        //    {
        //        var Chats = _ChatService.GetAll().ToList();

        //        //Sample Query
        //        //from m in Chats
        //        //where (from c in Chats where c.CustomerIdBy==91 select c.CustomerIdTo).Distinct().Contains(m.CustomerIdBy)
        //        //&& m.CustomerIdTo==91 && m.IsRead==false
        //        //group m by new{m.CustomerIdBy} into g
        //        //select new {CustomerIdBy=g.Key.CustomerIdBy,Count=g.Count()}


        //        var Results = (from m in Chats
        //                       where (from c in Chats where c.CustomerIdBy == ChatModel.CustomerIdBy select c.CustomerIdTo).Distinct().Contains(m.CustomerIdBy)
        //                       && m.CustomerIdTo == ChatModel.CustomerIdBy && m.IsRead == false
        //                       group m by new { m.CustomerIdBy } into g
        //                       select new { CustomerIdBy = g.Key.CustomerIdBy, Count = g.Count() });


        //        foreach (var Chat in Results)
        //        {

        //            models.Add(new ChatModel
        //            {
        //                ChatId = 0,
        //                CustomerIdBy = Chat.CustomerIdBy,
        //                CustomerIdTo = ChatModel.CustomerIdBy,
        //                ChatContent = "",
        //                CountMessage = Chat.Count
        //            });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", models), Configuration.Formatters.JsonFormatter);
        //    }
        //}
         //POST api/Chats
        //http://HomeHelp.com/Chats/SaveChat/{Json}
        [Route("SaveChat")]
        [HttpPost]
        public HttpResponseMessage SaveChat([FromBody]ChatModel chatModel)
        {
            string ChatID = "-1";
            try
            {
                if(chatModel.CustomerIdBy==new Guid())
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerId By is blank."), Configuration.Formatters.JsonFormatter);
            }
                if (chatModel.CustomerIdTo == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CustomerId To is blank."), Configuration.Formatters.JsonFormatter);
                }
                var customerTo = _CustomerService.GetCustomer(chatModel.CustomerIdTo);
                if(customerTo==null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer To not found."), Configuration.Formatters.JsonFormatter);
                }
                var customerBy = _CustomerService.GetCustomer(chatModel.CustomerIdBy);
                if (customerBy == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer By not found."), Configuration.Formatters.JsonFormatter);
                }
                if (chatModel.ChatContent != null && chatModel.ChatContent != "")
                {
                    List<Guid?> ListCustomerIDs = new List<Guid?>();
                    ListCustomerIDs.Add(chatModel.CustomerIdBy);
                    ListCustomerIDs.Add(chatModel.CustomerIdTo);

                    
                    var isFriend = _RequestService.GetRequests().Where(x => ListCustomerIDs.Contains(x.CustomerIdBy) && ListCustomerIDs.Contains(x.CustomerIdTo)).FirstOrDefault();
                    if (isFriend!=null)
                    {
                        
                        Mapper.CreateMap<HomeHelp.Models.ChatModel, HomeHelp.Entity.Chat>();
                        HomeHelp.Entity.Chat chat = Mapper.Map<HomeHelp.Models.ChatModel, HomeHelp.Entity.Chat>(chatModel);
                        if (chatModel.ChatId <= 0)
                        {
                            TimeZoneInfo tz = TimeZoneInfo.Local;
                            chat.DateTimeCreated = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, tz);
                            _ChatService.Insert(chat); //Save Operation
                            ChatID = chat.ChatId.ToString();
                            
                        }

                       // Code For GCM & Iphone
                        //var Customer = _CustomerService.GetCustomer(Convert.ToInt32(chatModel.CustomerIdTo));
                        //if (Customer != null)
                        //{
                        //    var Chats = _ChatService.GetAll().ToList();
                        //    var Results = (from m in Chats
                        //                   where m.CustomerIdTo == chatModel.CustomerIdTo && m.IsRead == false
                        //                   group m by new { m.CustomerIdTo } into g
                        //                   select new
                        //                   {
                        //                       CustomerIdBy = g.Key.CustomerIdTo,
                        //                       ChatContent = (from c in Chats
                        //                                      where c.CustomerIdTo == chatModel.CustomerIdTo && c.IsRead == false
                        //                                      orderby c.DateTimeCreated descending
                        //                                      select c.ChatContent).FirstOrDefault(),
                        //                       Count = g.Count()
                        //                   }).ToList();

                        //    var models = new List<ChatModel>();

                        //    var UserMessage = "";
                        //    bool NotificationStatus = true;
                        //    if (Results.Count() >= 0)
                        //    {
                        //        int MessageCount = Convert.ToInt32(Results.Sum(x => x.Count).ToString()) + 1;
                       string UserMessage = "You have new message.";

                        string NotificationType = "chat";
                        string Flag = "NewChatMessage";

                        //Save notification in Table
                        Notification Notification = new Notification();
                        Notification.NotificationId = 0; //New Case
                        //Notification.ClientId = _Event.ClientID; Save it as NULL
                        Notification.CustomerIdBy = customerBy.CustomerId;
                        Notification.CustomerIdTo = customerTo.CustomerId;
                        //Notification.EventID = _Event.EventID; Save it as NULL
                        Notification.UserMessage = UserMessage;
                        Notification.NotificationType = NotificationType;
                        Notification.Flag = Flag;
                        Notification.DeviceType = customerBy.DeviceType;
                        _NotificationService.InsertNotification(Notification);
                        //        //End : Save notification in Table
                    
                       // string Message = "{\"flag\":\"" + Flag + "\",\"message\":\"" + UserMessage + "\",\"ChatContent\":\"" + chatModel.ChatContent + "\"}";


                        string Message = "{ \"ChatId\": \"" + ChatID + "\",      \"CustomerIdBy\": \"" + customerBy.CustomerId                  + "\", \"CustomerIdTo\": \"" + customerTo.CustomerId + "\",      \"ChatContent\": \"" + chatModel.ChatContent
                            + "\",      \"flag\": \"" + Flag + "\",      \"IsRead\": \"" + chat.IsRead + "\",     \"IsDeletedCustomerBy\":\"" + chat.IsDeletedCustomerBy
                            + "\",      \"IsDeletedCustomerTo\": \"" + chat.IsDeletedCustomerTo + "\",     \"DateTimeCreated\": \""
                            + chat.DateTimeCreated + "\",      \"CustomersBy\": { \"CustomerId\": \"" + customerBy.CustomerId
                            + "\",       \"FirstName\": \"" + customerBy.FirstName + "\",        \"LastName\": \"" + customerBy.LastName
                            + "\",       \"PhotoPath\": \"" +  customerBy.PhotoPath + "\"      }, \"CustomersTo\": { \"CustomerId\": \"" + customerTo.CustomerId + "\",       \"FirstName\": \"" + customerTo.FirstName + "\",      \"LastName\": \"" + customerTo.LastName + "\",       \"PhotoPath\": \"" + customerTo.PhotoPath + "\"     }     }";


                        if (customerTo.ApplicationId != null && customerTo.ApplicationId != "")
                        {

                            if (customerTo.DeviceType == EnumValue.GetEnumDescription(EnumValue.DeviceType.Android))
                            {
                                //Send Notification another Andriod
                                CommonCls.SendFCM_Notifications(customerTo.ApplicationId, Message, true);
                            }
                            else
                            {

                                string Msg = customerTo.FirstName + " " + customerTo.LastName + " : " + chat.ChatContent;
                                
                                CommonCls.TestSendFCM_Notifications(customerTo.ApplicationId, Message, Msg, true);
                                //CommonCls.TestSendFCM_Notifications(customerTo.ApplicationId, Message, true);
                                }
                        }
                        Mapper.CreateMap<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>();
                        HomeHelp.Models.ChatResponseModel ChatModel = Mapper.Map<HomeHelp.Entity.Chat, HomeHelp.Models.ChatResponseModel>(chat);
                        string sqlFormattedDate = chat.DateTimeCreated.HasValue ? chat.DateTimeCreated.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : "";

                        ChatModel.DateTimeCreated = sqlFormattedDate;

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", ChatModel), Configuration.Formatters.JsonFormatter);
                    }
                   else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "they are not associated."), Configuration.Formatters.JsonFormatter);
                    }
                    

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Chat content is blank."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later"), Configuration.Formatters.JsonFormatter);
            }
        }
        // DELETE api/Chats/5
        //[Route("ClearChat")]
        //[HttpPost]
        //public HttpResponseMessage ClearChat([FromBody]ChatClearModel chatClearModel)
        //{
        //    try
        //    {
        //        var Chat = _ChatService.GetById(chatClearModel.ChatId);

        //        if (Chat != null)
        //        {
        //            if (Chat.CustomerIdBy == chatClearModel.CustomerId)
        //            {
        //                Chat.IsDeletedCustomerBy = true;
        //            }
        //            else if (Chat.CustomerIdTo == chatClearModel.CustomerId)
        //            {
        //                Chat.IsDeletedCustomerTo = true;
        //            }
        //            _ChatService.Delete(Chat);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Chat is deleted successfully."), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotImplemented, CommonCls.CreateMessage("error", "Chat not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.NotImplemented, CommonCls.CreateMessage("error", "Chat is not deleted."), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //[Route("ClearAllChat")]
        //[HttpPost]
        //public HttpResponseMessage ClearAllChat([FromBody]ChatModel chatClearModel)
        //{
        //    try
        //    {
        //        var Chats = _ChatService.GetChats().Where(x => x.CustomerIdBy == chatClearModel.CustomerIdBy && x.CustomerIdTo == chatClearModel.CustomerIdTo);
        //        foreach (var Chat in Chats)
        //        {
        //            if (Chat != null)
        //            {
        //                if (Chat.CustomerIdBy == chatClearModel.CustomerIdBy)
        //                {
        //                    Chat.IsDeletedCustomerBy = true;
        //                }
        //                else if (Chat.CustomerIdTo == chatClearModel.CustomerIdTo)
        //                {
        //                    Chat.IsDeletedCustomerTo = true;
        //                }
        //                _ChatService.Delete(Chat);
        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Chat is deleted successfully."), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.NotImplemented, CommonCls.CreateMessage("error", "Chat is not deleted."), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //// DELETE api/Chats/5
        ////http://HomeHelp.com/Chats/DeleteChat?ChatId=12
        //[Route("DeleteChat")]
        //[HttpGet]
        //public HttpResponseMessage DeleteChat([FromUri]int ChatId)
        //{
        //    try
        //    {
        //        var Chat = _ChatService.GetById(ChatId);
        //        _ChatService.Delete(Chat);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Chat is deleted successfully."), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.NotImplemented, CommonCls.CreateMessage("error", "Chat is not deleted."), Configuration.Formatters.JsonFormatter);
        //    }
        //}
        //public string SaveImage(string Base64String)
        //{
        //    string fileName = Guid.NewGuid() + ".png";
        //    Image image = CommonCls.Base64ToImage(Base64String);
        //    var subPath = HttpContext.Current.Server.MapPath("~/ChatPhoto");
        //    var path = Path.Combine(subPath, fileName);
        //    image.Save(path, System.Drawing.Imaging.ImageFormat.Png);

        //    string URL = CommonCls.GetURL() + "/ChatPhoto/" + fileName;
        //    return URL;
        //}
    }
}
