using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArkitechDataApi.Services;
using ArkitechDataApi.Models;
using MongoDB.Bson;
using System.ComponentModel;

namespace ArkitechDataApi.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController : ControllerBase
    {
        private readonly IDataService _dataService;

        public DataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// GET /data/all?limit=100&amp;topic=xyz
        /// Returns up to “limit” latest DataItem documents, optionally filtered by “topic.”
        /// Instead of returning raw DataItem (with BsonDocument), we map each Payload to primitives.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllData(
            [FromQuery] int limit = 100,
            [FromQuery] string? topic = null
        )
        {
            if (limit <= 0 || limit > 1000)
            {
                return BadRequest("`limit` must be between 1 and 1000.");
            }

            try
            {

                var dataItems = await _dataService.GetLatestItemsAsync(limit, topic);
                var dtoList = dataItems.Select(dataItem =>
                {
                    var payloadDict = ConvertBsonDocumentToDictionary(dataItem.Payload);
                    return new LatestDataDto
                    {
                        Topic = dataItem.Topic,
                        Timestamp = dataItem.Timestamp,
                        Payload = payloadDict
                    };
                }).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /data/latest?topic=xyz
        /// Returns a single LatestDataDto (topic, timestamp, payload as primitives).
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestItem([FromQuery] string? topic = null)
        {
            try
            {
                // 1) Retrieve the raw DataItem
                var dataItem = await _dataService.GetLatestItemAsync(topic);
                if (dataItem == null)
                    return NotFound();

                // 2) Convert BsonDocument → Dictionary<string, object>
                var payloadDict = ConvertBsonDocumentToDictionary(dataItem.Payload);

                // 3) Build the DTO
                var dto = new LatestDataDto
                {
                    Topic = dataItem.Topic,
                    Timestamp = dataItem.Timestamp,
                    Payload = payloadDict
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /data/timerange?start=2025-06-01T00:00:00&amp;end=2025-06-01T23:59:59&amp;topic=xyz
        /// Mirrors Python’s get_items_by_timerange(...)
        /// (This endpoint returns List<DataItem> directly, because the DataItem model may already use
        /// a CLR‐friendly payload type. If not, you can similarly map here.)
        /// </summary>
        [HttpGet("timerange")]
        public async Task<IActionResult> GetByTimeRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromQuery]
            [DefaultValue("arkitech/ships/vessel1/crew_quarters/co2")]
             string? topic = null
        )
        {
            if (end < start)
            {
                return BadRequest("`end` must be later than or equal to `start`.");
            }

            try
            {
                var items = await _dataService.GetItemsByTimeRangeAsync(start, end, topic);

                // Map to DTOs with primitive payloads
                var dtoList = items.Select(dataItem =>
                {
                    var payloadDict = ConvertBsonDocumentToDictionary(dataItem.Payload);
                    return new LatestDataDto
                    {
                        Topic = dataItem.Topic,
                        Timestamp = dataItem.Timestamp,
                        Payload = payloadDict
                    };
                }).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        /// <summary>
        /// Helper: Recursively converts a BsonDocument (and nested BsonArray) into
        /// Dictionary<string, object> (and nested List<object>) of CLR primitives.
        /// </summary>
        private static Dictionary<string, object> ConvertBsonDocumentToDictionary(BsonDocument doc)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var element in doc)
            {
                dict[element.Name] = ConvertBsonValue(element.Value);
            }

            return dict;
        }

        /// <summary>
        /// Helper: Converts a single BsonValue into a CLR primitive or nested structure.
        /// </summary>
        private static object ConvertBsonValue(BsonValue value)
        {
            switch (value.BsonType)
            {
                case BsonType.Boolean:
                    return value.IsBoolean ? value.AsBoolean : false;

                case BsonType.Int32:
                    return value.IsInt32 ? value.AsInt32 : 0;

                case BsonType.Int64:
                    return value.IsInt64 ? value.AsInt64 : 0L;

                case BsonType.Double:
                    return value.IsDouble ? value.AsDouble :
                           value.IsString && double.TryParse(value.AsString, out var d) ? d : 0.0;

                case BsonType.String:
                    return value.AsString;

                case BsonType.DateTime:
                    return value.ToUniversalTime();

                case BsonType.Document:
                    return ConvertBsonDocumentToDictionary(value.AsBsonDocument);

                case BsonType.Array:
                    return value.AsBsonArray.Select(ConvertBsonValue).ToList();

                case BsonType.Decimal128:
                    return Decimal128.ToDecimal(value.AsDecimal128);

                case BsonType.ObjectId:
                    return value.AsObjectId.ToString();

                default:
                    return value.ToString();
            }
        }
    }
}
