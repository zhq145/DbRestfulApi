using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbRestfulApi.Models;
using DbRestfulApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DbRestfulApi.Controllers
{
    [ApiController]
    [Route("api/{module}")]
    public class DynamicController : ControllerBase
    {
        private readonly AppSettings _settings;
        private readonly IDatabaseService _db;

        public DynamicController(Microsoft.Extensions.Options.IOptions<AppSettings> options, IDatabaseService db)
        {
            _settings = options.Value;
            _db = db;
        }

        private ModuleConfig? GetModuleConfig(string module)
        {
            if (_settings.Modules != null && _settings.Modules.TryGetValue(module, out var cfg))
                return cfg;
            return null;
        }

        private Dictionary<string, object> FilterAllowedFields(ModuleConfig mod, Dictionary<string, object> data)
        {
            var allowed = new HashSet<string>(mod.AllowedFields ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var filtered = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in data)
            {
                if (allowed.Contains(kv.Key))
                    filtered[kv.Key] = kv.Value ?? DBNull.Value;
            }
            return filtered;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(string module, int page = 1, int pageSize = 10)
        {
            var mod = GetModuleConfig(module);
            if (mod == null)
                return NotFound(new ApiResponse<object> { Success = false, Code = 4040, Message = "Module not found", Data = null });

            var table = mod.Table ?? module;
            try
            {
                var (items, total) = await _db.ListAsync(table, page, pageSize);
                var paged = new PagedResult { Total = total, Page = page, PageSize = pageSize, Items = items };
                var resp = new ApiResponse<PagedResult> { Success = true, Code = 0, Message = "操作成功", Data = paged };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Code = 5000, Message = ex.Message, Data = null });
            }
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(string module, int id)
        {
            var mod = GetModuleConfig(module);
            if (mod == null)
                return NotFound(new ApiResponse<object> { Success = false, Code = 4040, Message = "Module not found", Data = null });

            try
            {
                var row = await _db.GetAsync(mod.Table ?? module, id);
                var items = row != null ? new[] { row } : Array.Empty<Dictionary<string, object>>();
                var paged = new PagedResult { Total = items.Length, Page = 1, PageSize = items.Length > 0 ? items.Length : 0, Items = items };
                var resp = new ApiResponse<PagedResult> { Success = true, Code = 0, Message = "操作成功", Data = paged };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Code = 5000, Message = ex.Message, Data = null });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(string module, [FromBody] Dictionary<string, object>? payload)
        {
            var mod = GetModuleConfig(module);
            if (mod == null)
                return NotFound(new ApiResponse<object> { Success = false, Code = 4040, Message = "Module not found", Data = null });

            payload ??= new Dictionary<string, object>();
            // validate required
            var missing = (mod.RequiredFields ?? Enumerable.Empty<string>()).Where(r => !payload.ContainsKey(r)).ToList();
            if (missing.Any())
                return BadRequest(new ApiResponse<object> { Success = false, Code = 4001, Message = "Missing required fields: " + string.Join(",", missing), Data = null });

            var data = FilterAllowedFields(mod, payload);
            try
            {
                var id = await _db.AddAsync(mod.Table ?? module, data);
                var resp = new ApiResponse<PagedResult>
                {
                    Success = true,
                    Code = 0,
                    Message = "操作成功",
                    Data = new PagedResult { Total = 1, Page = 1, PageSize = 1, Items = new[] { new Dictionary<string, object> { ["id"] = id } } }
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Code = 5000, Message = ex.Message, Data = null });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(string module, int id, [FromBody] Dictionary<string, object>? payload)
        {
            var mod = GetModuleConfig(module);
            if (mod == null)
                return NotFound(new ApiResponse<object> { Success = false, Code = 4040, Message = "Module not found", Data = null });

            payload ??= new Dictionary<string, object>();
            var data = FilterAllowedFields(mod, payload);
            try
            {
                var ok = await _db.UpdateAsync(mod.Table ?? module, id, data);
                var resp = new ApiResponse<PagedResult>
                {
                    Success = true,
                    Code = 0,
                    Message = "操作成功",
                    Data = new PagedResult { Total = ok ? 1 : 0, Page = 1, PageSize = ok ? 1 : 0, Items = new[] { new Dictionary<string, object> { ["success"] = ok } } }
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Code = 5000, Message = ex.Message, Data = null });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string module, int id)
        {
            var mod = GetModuleConfig(module);
            if (mod == null)
                return NotFound(new ApiResponse<object> { Success = false, Code = 4040, Message = "Module not found", Data = null });

            try
            {
                var ok = await _db.DeleteAsync(mod.Table ?? module, id);
                var resp = new ApiResponse<PagedResult>
                {
                    Success = true,
                    Code = 0,
                    Message = "操作成功",
                    Data = new PagedResult { Total = ok ? 1 : 0, Page = 1, PageSize = ok ? 1 : 0, Items = new[] { new Dictionary<string, object> { ["success"] = ok } } }
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Code = 5000, Message = ex.Message, Data = null });
            }
        }
    }
}
