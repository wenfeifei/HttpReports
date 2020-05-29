﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HttpReports.Core.Config;
using HttpReports.Core.Models;
using HttpReports.Dashboard.DTO;
using HttpReports.Dashboard.Models;
using HttpReports.Dashboard.ViewModels;
using HttpReports.Monitor;
using HttpReports.Storage.FilterOptions;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HttpReports.Dashboard.Services
{
    [EnableCors]
    internal class DashboardDataProvideService : IHttpReportsHttpUnit
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHttpReportsStorage _storage;

        private readonly MonitorService _monitorService;

        private readonly QuartzSchedulerService _scheduleService;

        private readonly Localize _localize;

        public DashboardDataProvideService(IServiceProvider serviceProvider,
                                           IHttpContextAccessor contextAccessor,
                                           IHttpReportsStorage storage,
                                           MonitorService monitorService,
                                           QuartzSchedulerService scheduleService,
                                           LocalizeService localizeService)
        {
            _serviceProvider = serviceProvider;
            _contextAccessor = contextAccessor;
            _storage = storage;
            _monitorService = monitorService;
            _scheduleService = scheduleService;
            _localize = localizeService.Current;
        }

        private T RequireService<T>() => _serviceProvider.GetRequiredService<T>();

        [HttpPost]
        public async Task<IActionResult> GetIndexChartData([FromBody]GetIndexDataRequest request)
        {
            var start = (request.Start.IsEmpty() ? DateTime.Now.ToString("yyyy-MM-dd") : request.Start).ToDateTime();
            var end = (request.End.IsEmpty() ? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") : request.End).ToDateTime();

            var nodes = request.Node.IsEmpty() ? null : request.Node.Split(',');

            var topRequest = await _storage.GetUrlRequestStatisticsAsync(new RequestInfoFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                IsAscend = false,
                Take = request.TOP,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            var topError500 = await _storage.GetUrlRequestStatisticsAsync(new RequestInfoFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                IsAscend = false,
                Take = request.TOP,
                StatusCodes = new[] { 500 },
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            var fast = await _storage.GetRequestAvgResponeTimeStatisticsAsync(new RequestInfoFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                IsAscend = true,
                Take = request.TOP,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            var slow = await _storage.GetRequestAvgResponeTimeStatisticsAsync(new RequestInfoFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                IsAscend = false,
                Take = request.TOP,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            var Art = new
            {
                fast = fast.Select(m => new EchartPineDataModel(m.Url, (int)m.Time)),
                slow = slow.Select(m => new EchartPineDataModel(m.Url, (int)m.Time))
            };

            var StatusCode = (await _storage.GetStatusCodeStatisticsAsync(new RequestInfoFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                StatusCodes = new[] { 200, 301, 302, 303, 400, 401, 403, 404, 500, 502, 503 },
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            })).Where(m => true).Select(m => new EchartPineDataModel(m.Code.ToString(), m.Total)).ToArray();

            var ResponseTime = (await _storage.GetGroupedResponeTimeStatisticsAsync(new GroupResponeTimeFilterOption()
            {
                Nodes = nodes,
                StartTime = start,
                EndTime = end,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            })).Where(m => true).Select(m => new EchartPineDataModel(m.Name, m.Total)).ToArray();

            return Json(new HttpResultEntity(1, "ok", new { StatusCode, ResponseTime, topRequest, topError500, Art }));
        }

        [HttpPost]
        public async Task<IActionResult> GetDayStateBar([FromBody]GetIndexDataRequest request)
        {
            var startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0, DateTimeKind.Local).AddDays(-1);
            var endTime = DateTime.Now;

            var nodes = request.Node.IsEmpty() ? null : request.Node.Split(',');

            // 每小时请求次数
            var requestTimesStatistics = await _storage.GetRequestTimesStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Nodes = nodes,
                Type = TimeUnit.Hour,
            });

            //每小时平均处理时间
            var responseTimeStatistics = await _storage.GetResponseTimeStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Nodes = nodes,
                Type = TimeUnit.Hour,
            });

            List<int> timesList = new List<int>();
            List<int> avgList = new List<int>();

            List<int> hours = new List<int>();

            for (int i = 1; i <= 24; i++)
            {
                var start = startTime.AddHours(i).ToString("dd-HH");

                // 每小时请求次数
                var times = requestTimesStatistics.Items.TryGetValue(start, out var tTimes) ? tTimes : 0;
                timesList.Add(times);

                //每小时平均处理时间
                var avg = responseTimeStatistics.Items.TryGetValue(start, out var tAvg) ? tAvg : 0;
                avgList.Add(avg);

                hours.Add(startTime.AddHours(i).ToString("HH").ToInt());
            }

            return Json(new HttpResultEntity(1, "ok", new { timesList, avgList, hours }));
        }

        [HttpPost]
        public async Task<IActionResult> GetMinuteStateBar([FromBody]GetIndexDataRequest request)
        {
            var startTime = DateTime.Now.AddHours(-1).AddSeconds(-DateTime.Now.Second);

            var endTime = DateTime.Now;

            var nodes = request.Node.IsEmpty() ? null : request.Node.Split(',');

            // 每小时请求次数
            var requestTimesStatistics = await _storage.GetRequestTimesStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Nodes = nodes,
                Type = TimeUnit.Minute,
            });

            //每小时平均处理时间
            var responseTimeStatistics = await _storage.GetResponseTimeStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Nodes = nodes,
                Type = TimeUnit.Minute,
            });

            List<int> timesList = new List<int>();
            List<int> avgList = new List<int>();

            List<int> time = new List<int>();

            for (int i = 1; i <= 60; i++)
            {
                var start = startTime.AddMinutes(i).ToString("HH-mm");

                // 每小时请求次数
                var times = requestTimesStatistics.Items.TryGetValue(start, out var tTimes) ? tTimes : 0;
                timesList.Add(times);

                //每小时平均处理时间
                var avg = responseTimeStatistics.Items.TryGetValue(start, out var tAvg) ? tAvg : 0;
                avgList.Add(avg);

                time.Add(startTime.AddMinutes(i).ToString("mm").ToInt());
            }

            return Json(new HttpResultEntity(1, "ok", new { timesList, avgList, time }));
        }

        [HttpPost]
        public async Task<IActionResult> GetLatelyDayChart([FromBody]GetIndexDataRequest request)
        {
            var startTime = DateTime.Now.Date.AddDays(-31);
            var endTime = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            var nodes = request.Node.IsEmpty() ? null : request.Node.Split(',');

            var responseTimeStatistics = await _storage.GetRequestTimesStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Nodes = nodes,
                Type = TimeUnit.Day,
            });

            List<string> time = new List<string>();
            List<int> value = new List<int>();

            var monthDayCount = (endTime - startTime).Days;
            for (int i = 1; i <= monthDayCount; i++)
            {
                var day = startTime.AddDays(i).ToString("yyyy-MM-dd");

                var times = responseTimeStatistics.Items.TryGetValue(day, out var tTimes) ? tTimes : 0;

                time.Add(startTime.AddDays(i).ToString("dd").ToInt().ToString());
                value.Add(times);
            }

            return Json(new HttpResultEntity(1, "ok", new { time, value }));
        }

        [HttpPost]
        public async Task<IActionResult> GetMonthDataByYear([FromBody]GetIndexDataRequest request)
        {
            var startTime = $"{request.Year}-01-01".ToDateTimeOrDefault(() => new DateTime(DateTime.Now.Year, 1, 1));
            var endTime = startTime.AddYears(1);
            var nodes = request.Node?.Split(',');

            var responseTimeStatistics = await _storage.GetRequestTimesStatisticsAsync(new TimeSpanStatisticsFilterOption()
            {
                StartTime = startTime,
                EndTime = endTime,
                Nodes = nodes,
                Type = TimeUnit.Month,
            });

            List<string> time = new List<string>();
            List<int> value = new List<int>();

            string Range = $"{request.Year}-01-{request.Year}-12";

            for (int i = 0; i < 12; i++)
            {
                var month = (i + 1).ToString();

                var times = responseTimeStatistics.Items.TryGetValue(month, out var tTimes) ? tTimes : 0;

                time.Add(month);
                value.Add(times);
            }

            return Json(new HttpResultEntity(1, "ok", new { time, value, Range }));
        }

        /// <summary>
        /// 更改语言
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangeLanguage(string language)
        {
            var localizeService = RequireService<LocalizeService>();

            await localizeService.SetLanguageAsync(language);

            return Json(new HttpResultEntity(1, "ok", null));
        }

        /// <summary>
        /// 获取可用的语言列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAvailableLanguages()
        {
            var localizeService = RequireService<LocalizeService>();
            return Json(new HttpResultEntity(1, "ok", localizeService.Langs));
        }

        /// <summary>
        /// 获取本地化语言
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> GetLocalizeLanguageAsync(string language = null)
        {
            Localize localize = null;
            var localizeService = RequireService<LocalizeService>();

            if (string.IsNullOrEmpty(language))
            {
                language = await _storage.GetSysConfig(BasicConfig.Language);
                if (!localizeService.TryGetLanguage(language, out localize)
                    && _contextAccessor.HttpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
                {
                    var accepts = acceptLanguage.ToString().Split(new[] { ';', ',' }).Where(m => !m.Contains('=')).ToArray();
                    if (accepts.Length > 0)
                    {
                        foreach (var item in accepts)
                        {
                            if (localizeService.TryGetLanguage(item, out localize))
                            {
                                await _storage.SetLanguage(item);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                localizeService.TryGetLanguage(language, out localize);
            }
            localize ??= localizeService.Current;

            return Json(new HttpResultEntity(1, "ok", localize));
        }

        [HttpPost]
        public async Task<IActionResult> GetIndexData([FromBody]GetIndexDataRequest request)
        {
            var start = request.Start.ToDateTime();
            var end = request.End.ToDateTime();

            var result = await _storage.GetIndexPageDataAsync(new IndexPageDataFilterOption()
            {
                Nodes = request.Node.IsEmpty() ? null : request.Node.Split(','),
                StartTime = start,
                EndTime = end,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            return Json(new HttpResultEntity(1, "ok", new
            {
                ART = result.AvgResponseTime.ToInt(),
                Total = result.Total,
                Code404 = result.NotFound,
                Code500 = result.ServerError,
                APICount = result.APICount,
                ErrorPercent = result.ErrorPercent.ToString("0.00%"),
            }));
        }

        [HttpPost]
        public async Task<IActionResult> GetRequestList([FromBody]GetRequestListRequest request)
        {
            var result = await _storage.SearchRequestInfoAsync(new RequestInfoSearchFilterOption()
            {
                TraceId = request.TraceId,
                StatusCodes = request.StatusCode.IsEmpty() ? null : request.StatusCode.Split(',').Select(x => x.ToInt()).ToArray(),
                Nodes = request.Node.IsEmpty() ? null : request.Node.Split(','),
                IP = request.IP,
                Url = request.Url,
                StartTime = request.Start.ToDateTime(),
                EndTime = request.End.TryToDateTime(),
                Page = request.pageNumber,
                PageSize = request.pageSize,
                IsOrderByField = true,
                Field = RequestInfoFields.CreateTime,
                IsAscend = false,
                StartTimeFormat = "yyyy-MM-dd HH:mm:ss",
                EndTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });

            return Json(new { total = result.AllItemCount, rows = result.List });
        }

        [HttpPost]
        public async Task<IActionResult> EditMonitor([FromBody]MonitorJobRequest request)
        {
            string vaild = _monitorService.VaildMonitorJob(request);

            if (!vaild.IsEmpty())
                return Json(new HttpResultEntity(-1, vaild, null));

            IMonitorJob model = _monitorService.GetMonitorJob(request);

            if (request.Id.IsEmpty() || request.Id == "0")
                await _storage.AddMonitorJob(model);
            else
                await _storage.UpdateMonitorJob(model);

            await _scheduleService.UpdateMonitorJobAsync();

            return Json(new HttpResultEntity(1, "ok", null));
        }

        [HttpGet]
        public async Task<IActionResult> GetMonitor(string Id)
        {
            if (Id.IsEmpty() || Id == "0")
                return new NoContentResult();

            var job = await _storage.GetMonitorJob(Id);

            if (job == null)
                return new NoContentResult();

            var request = _monitorService.GetMonitorJobRequest(job);

            return Json(new HttpResultEntity(1, "ok", request));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteJob(string Id)
        {
            await _storage.DeleteMonitorJob(Id);

            await _scheduleService.UpdateMonitorJobAsync();

            return Json(new HttpResultEntity(1, "ok", null));
        }

        [HttpGet]
        public async Task<IActionResult> ChangeJobState(string Id)
        {
            var model = await _storage.GetMonitorJob(Id);

            model.Status = model.Status == 1 ? 0 : 1;

            await _storage.UpdateMonitorJob(model);

            await _scheduleService.UpdateMonitorJobAsync();

            return Json(new HttpResultEntity(1, "ok", null));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccountInfo([FromBody]UpdateAccountRequest request)
        {
            var user = await _storage.GetSysUser(request.Username);

            if (user.Password != request.OldPwd.MD5())
            {
                return Json(new HttpResultEntity(-1, _localize.User_OldPasswordError, null));
            }

            if (request.NewUserName.Length <= 2 || request.NewUserName.Length > 20)
            {
                return Json(new HttpResultEntity(-1, _localize.User_AccountFormatError, null));
            }

            if (request.OldPwd.Length <= 5 || request.OldPwd.Length > 20)
            {
                return Json(new HttpResultEntity(-1, _localize.User_NewPassFormatError, null));
            }

            await _storage.UpdateLoginUser(new SysUser
            {
                Id = user.Id,
                UserName = request.NewUserName,
                Password = request.NewPwd.MD5()
            });

            return Json(new HttpResultEntity(1, _localize.UpdateSuccess, null));
        }

        [HttpGet]
        public async Task<IActionResult> GetTraceList(string Id)
        {
            var parent = await GetGrandParentRequestInfo(Id);

            var tree = await GetRequestInfoTrace(parent.Id);

            return Json(new HttpResultEntity(1, "ok", new List<RequestInfoTrace>() { tree }));
        }

        [HttpGet]
        public async Task<IActionResult> GetRequestInfoDetail(string Id)
        {
            var (requestInfo, requestDetail) = await _storage.GetRequestInfoDetail(Id);

            return Json(new HttpResultEntity(1, "ok", new
            {
                Info = requestInfo,
                Detail = requestDetail
            }));
        }

        private IActionResult Json(object data)
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(data, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                ContentType = "application/json;charset=utf-8",
            };
        }

        private async Task<IRequestInfo> GetGrandParentRequestInfo(string Id)
        {
            var requestInfo = await _storage.GetRequestInfo(Id);

            if (requestInfo.ParentId.IsEmpty())
            {
                return requestInfo;
            }
            else
            {
                return await GetGrandParentRequestInfo(requestInfo.ParentId);
            }
        }

        private async Task<RequestInfoTrace> GetRequestInfoTrace(string Id)
        {
            var requestInfo = await _storage.GetRequestInfo(Id);

            var requestInfoTrace = MapRequestInfo(requestInfo);

            var childs = await _storage.GetRequestInfoByParentId(requestInfo.Id);

            if (childs != null && childs.Count > 0)
            {
                requestInfoTrace.Nodes = new List<RequestInfoTrace>();
            }

            foreach (var item in childs)
            {
                var child = MapRequestInfo(item);

                var trace = await GetRequestInfoTrace(item.Id);

                requestInfoTrace.Nodes.Add(trace);
            }

            return requestInfoTrace;
        }

        private RequestInfoTrace MapRequestInfo(IRequestInfo requestInfo)
        {
            return new RequestInfoTrace
            {
                Id = requestInfo.Id,
                Text = requestInfo.Id,
                Node = requestInfo.Node,
                Url = requestInfo.Url,
                Milliseconds = requestInfo.Milliseconds,
                StatusCode = requestInfo.StatusCode,
                RequestType = requestInfo.RequestType
            };
        }
    }
}