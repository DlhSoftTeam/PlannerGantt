using DlhSoft.Web.UI.WebControls;
using DlhSoft.Windows.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlannerGantt
{
    public partial class DefaultPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var accessToken = Session["AccessToken"] as string;
            if (accessToken == null)
                throw new InvalidOperationException();
            var today = DateTime.Today;
            var continuousSchedule = new Schedule { WorkingWeekStart = DayOfWeek.Sunday, WorkingWeekFinish = DayOfWeek.Saturday, WorkingDayStart = TimeOfDay.MinValue, WorkingDayFinish = TimeOfDay.MaxValue };
            var knownUserNames = new Dictionary<string, string>();
            using (var client = new HttpClient())
            {
                using (var plansRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/planner/plans"))
                {
                    plansRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    plansRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var plansResponse = client.SendAsync(plansRequest).Result)
                    {
                        var plansJson = plansResponse.Content.ReadAsStringAsync().Result;
                        var plans = JObject.Parse(plansJson)["value"];
                        var bucketNames = new Dictionary<string, string>();
                        foreach (var plan in plans)
                        {
                            using (var bucketsRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/planner/plans/" + plan["id"].Value<string>() + "/buckets"))
                            {
                                bucketsRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                bucketsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                                using (var bucketsResponse = client.SendAsync(bucketsRequest).Result)
                                {
                                    var bucketsJson = bucketsResponse.Content.ReadAsStringAsync().Result;
                                    var buckets = JObject.Parse(bucketsJson)["value"];
                                    foreach (var bucket in buckets)
                                        bucketNames.Add(bucket["id"].Value<string>(), bucket["name"].Value<string>());
                                }
                            }
                            using (var tasksRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/planner/plans/" + plan["id"].Value<string>() + "/tasks"))
                            {
                                tasksRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                tasksRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                                using (var tasksResponse = client.SendAsync(tasksRequest).Result)
                                {
                                    var tasksJson = tasksResponse.Content.ReadAsStringAsync().Result;
                                    var tasks = JObject.Parse(tasksJson)["value"];
                                    if (tasks.Any())
                                    {
                                        var planName = plan["title"].Value<string>();
                                        GanttChartView.Items.Add(new GanttChartItem { Content = planName, Indentation = 0, IsExpanded = true });
                                        string previousBucketName = null;
                                        foreach (var task in tasks.OrderBy(t => bucketNames[t["bucketId"].Value<string>()]).ThenBy(t => t["startDateTime"].Value<DateTime?>() ?? (t["dueDateTime"].Value<DateTime?>() ?? today.AddDays(1)).AddDays(-1)).ThenBy(t => t["title"].Value<string>()))
                                        {
                                            var bucketName = bucketNames[task["bucketId"].Value<string>()];
                                            if (bucketName != previousBucketName)
                                            {
                                                GanttChartView.Items.Add(new GanttChartItem { Content = bucketName, Indentation = 1, IsExpanded = true });
                                                previousBucketName = bucketName;
                                            }
                                            var name = task["title"].Value<string>();
                                            var start = task["startDateTime"].Value<DateTime?>()?.Date ?? today;
                                            var finish = task["dueDateTime"].Value<DateTime?>()?.Date ?? start.AddDays(1);
                                            if (finish < start)
                                                start = finish.AddDays(-1);
                                            var isCompleted = task["completedDateTime"].Value<DateTime?>() != null;
                                            var assignedToId = (task["assignments"].First as JProperty)?.Name;
                                            string assignedTo;
                                            if (assignedToId != null)
                                            {
                                                if (!knownUserNames.TryGetValue(assignedToId, out assignedTo))
                                                {
                                                    using (var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/users/" + assignedToId))
                                                    {
                                                        userRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                                        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                                                        using (var userResponse = client.SendAsync(userRequest).Result)
                                                        {
                                                            var userJson = userResponse.Content.ReadAsStringAsync().Result;
                                                            var user = JObject.Parse(userJson);
                                                            assignedTo = user["displayName"] != null ? user["displayName"].Value<string>() : null;
                                                        }
                                                    }
                                                    knownUserNames.Add(assignedToId, assignedTo);
                                                }
                                            }
                                            else
                                            {
                                                assignedTo = null;
                                            }
                                            GanttChartView.Items.Add(new GanttChartItem
                                            {
                                                Content = name,
                                                Indentation = 2,
                                                Start = start,
                                                Finish = finish,
                                                CompletedFinish = isCompleted ? finish : start,
                                                AssignmentsContent = assignedTo,
                                                Schedule = continuousSchedule
                                            });
                                            if (GanttChartView.TimelineStart > start)
                                                GanttChartView.TimelineStart = start;
                                            if (GanttChartView.TimelineFinish < finish)
                                                GanttChartView.TimelineFinish = finish;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                GanttChartView.Columns[(int)ColumnType.Content].Header = "Plans / Tasks";
                GanttChartView.Columns[(int)ColumnType.Start].Width = 100;
                GanttChartView.Columns[(int)ColumnType.Finish].Width = 100;
                GanttChartView.Columns[(int)ColumnType.Milestone].IsVisible = false;
                GanttChartView.InitializingClientCode = "control.settings.dateTimeFormatter = control.settings.dateFormatter;";
                GanttChartView.InitializingClientCode += @"
                    initializeGanttChartTheme(control.settings, 'Generic-bright');
                    initializeGanttChartTemplates(control.settings, 'Generic-bright');";
            }
        }
    }
}
