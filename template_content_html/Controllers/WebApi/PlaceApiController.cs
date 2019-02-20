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
    [RoutePrefix("Place")]
    public class PlaceApiController : ApiController
    {

        public ICustomerService _CustomerService { get; set; }
        public ICityService _CityService { get; set; }
        public ICountryService _CountryService { get; set; }
        public IStateService _StateService { get; set; }
        public INotificationService _NotificationService { get; set; }

        public PlaceApiController(IPackageService PackageService,ICityService CityService,ICountryService CountryService,IStateService StateService, INotificationService NotificationService, ICustomerService CustomerService)
        {

            this._NotificationService = NotificationService;

            this._CustomerService = CustomerService;

            this._CityService = CityService;
            this._StateService = StateService;
            this._CountryService = CountryService;
        }


        //Get api/State/States
        [Route("GetAllStates")]
        [HttpGet]
        public HttpResponseMessage GetAllStates([FromUri] int CountryId)
        {
            if(CountryId==0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Country id is blank."), Configuration.Formatters.JsonFormatter);
            }
            var country = _CountryService.GetCountry(CountryId);
            if(country==null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong counrty id."), Configuration.Formatters.JsonFormatter);
            }
            var states = _StateService.GetStates().Where(s=>s.CountryID==CountryId);
            if(states!=null)
            {
                var models = new List<StateModel>();
                Mapper.CreateMap<Friendlier.Entity.State, Friendlier.Models.StateModel>();
                foreach (var state in states)
                {
                    models.Add(Mapper.Map<Friendlier.Entity.State, Friendlier.Models.StateModel>(state));

                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No state found."), Configuration.Formatters.JsonFormatter);
            }
           
        }


        [Route("GetAllCities")]
        [HttpGet]
        public HttpResponseMessage GetAllCities([FromUri] int StateId)
        {
            if (StateId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "State id is blank."), Configuration.Formatters.JsonFormatter);
            }
            var State = _StateService.GetState(StateId);
            if (State == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong State id."), Configuration.Formatters.JsonFormatter);
            }
            var cities = _CityService.GetCities().Where(s => s.StateID == StateId);
            if (cities != null)
            {
                var models = new List<CityResponseModel>();
                Mapper.CreateMap<Friendlier.Entity.City, Friendlier.Models.CityResponseModel>();
                foreach (var city in cities)
                {
                    models.Add(Mapper.Map<Friendlier.Entity.City, Friendlier.Models.CityResponseModel>(city));

                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No city found."), Configuration.Formatters.JsonFormatter);
            }

        }
        //Get api/Country/Country
        [Route("GetAllCountry")]
        [HttpGet]
        public IHttpActionResult GetAllCountry()
        {
            var Country = _CountryService.GetCountries();
            var models = new List<CountryModel>();
            Mapper.CreateMap<Friendlier.Entity.Country, Friendlier.Models.CountryModel>();
            foreach (var country in Country)
            {
                models.Add(Mapper.Map<Friendlier.Entity.Country, Friendlier.Models.CountryModel>(country));

            }

            return Json(models);
        }
    }
}