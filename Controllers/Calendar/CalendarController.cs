using EskomCalendarApi.Models.Calendar;
using EskomCalendarApi.Services.Calendar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EskomCalendarApi.Controllers.Calendar
{
    [ApiController]
    [Route("api/[controller]/")]
    public class CalendarController : ControllerBase
    {
        private readonly ILogger<CalendarController> _logger;
        private readonly ICalendarService _calendarService;

        public CalendarController(ILogger<CalendarController> logger, ICalendarService calendarService)
        {
            _logger = logger;
            _logger.LogInformation("Init CalendarController");
            _calendarService = calendarService;
        }

        [HttpGet("Get")]
        [SwaggerOperation(Summary = "Return the ICS file for the given calendarName")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetCalendar(string calendarName)
        {
            var res = await _calendarService.GetCalendarData(calendarName);
            return Ok(res);
        }

        [HttpGet("GetCalendarSuburbs")]
        [SwaggerOperation(Summary = "Return a list of suburbs for the given calendarName")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetCalendarSuburbs(string calendarName)
        {
            try
            {
                var res = await _calendarService.GetCalendarSuburbs(calendarName);
                return Ok(res);
            }
            catch (CalendarSuburbsNotImplementedException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetMachineFriendlyInfo")]
        [ProducesResponseType(typeof(MachineDataDto), 200)]
        [SwaggerOperation(Summary = "Return the data from the MachineFriendly.csv (Max 1000 at a time)")]
        public async Task<IActionResult> GetMachineFriendlyInfo(int lastRecord, [Range(1, 1000, ErrorMessage = "Value must be less than or equal to 1000")] int recordsToRetrieve)
        {
            var res = await _calendarService.GetMachineData(lastRecord, recordsToRetrieve);
            return Ok(res);
        }

        [HttpGet("GetDataByArea")]
        [SwaggerOperation(Summary = "Get all data for a specific area (Max 1000 at a time)")]
        [ProducesResponseType(typeof(MachineDataDto), 200)]
        public async Task<IActionResult> GetDataByArea(string areaName, int lastRecord = 0, [Range(1, 1000, ErrorMessage = "Value must be less than or equal to 1000")] int recordsToRetrieve = 1000)
        {
            var res = await _calendarService.GetDataByArea(areaName, lastRecord, recordsToRetrieve);
            return Ok(res);
        }

        [HttpGet("GetCertainDayInArea")]
        [ProducesResponseType(typeof(MachineDataDto), 200)]
        [SwaggerOperation(Summary = "Get all data for a certain day in a certain area")]
        public async Task<IActionResult> CertainDayInArea(string areaName, DateTime? startDate, DateTime? endDate)
        {
            var myStartDate = startDate ?? DateTime.Today;
            var myEndDate = endDate ?? myStartDate;
            var res = await _calendarService.GetDataByAreaDateTime(areaName, myStartDate, myEndDate);
            return Ok(res);
        }

        [HttpGet("GetDistinctAreas")]
        [ProducesResponseType(typeof(MachineDataGroupedDto), 200)]
        [SwaggerOperation(Summary = "Get the distinct area's for a specified 'province'")]
        public async Task<IActionResult> GetAreaDataGrouped(string areaName)
        {
            var myStartDate = DateTime.Today;
            var myEndDate = myStartDate.AddDays(10);
            var res = await _calendarService.GetDataByAreaDateTime(areaName, myStartDate, myEndDate);
            var data = res.data
                        .Select(s => new MyMachineDataGrouped() { area_name = s.area_name, province = s.province, block = s.block })
                        .GroupBy(x => new { x.province, x.block, x.area_name })
                        .Select(grp => grp.First())
                        .OrderBy(o=>o.province)
                        .ThenBy(o => o.area_name)
                        .ToList();

            var m = new MachineDataGroupedDto() { data = data };
            return Ok(m);
        }

        [HttpGet("GetAssetByCalendarName")]
        [ProducesResponseType(typeof(Asset), 200)]
        [SwaggerOperation(Summary = "Get the selected calendar AssetInfo")]
        public async Task<IActionResult> GetAssetDataByCalendarName(string calendarname)
        {
            var res = await _calendarService.GetAssetDataByCalendarName(calendarname);
            return Ok(res);
        }

    }

}
