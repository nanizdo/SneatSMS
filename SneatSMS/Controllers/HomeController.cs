using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SneatSMS.Models;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SneatSMS.Controllers
{
    public class GPSToken
    {
        public string is_authenticated { get; set; }
        public string jwtoken { get; set; }
    }
    public class DataTableParameter
    {
        public int draw { get; set; }
    }
    public class dataMsg
    {
        public string datetime { get; set; }
        public string clientphonenumber { get; set; }
        public string clientname { get; set; }
        public string message { get; set; }
        public string itenerary { get; set; }
        public string duration { get; set; }
        public string smsstatus { get; set; }
        public string emailstatus { get; set; }
        public string matricule { get; set; }
        public string kilodep { get; set; }
        public string kiloarr { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            return View();
        }

        public string Table(DataTableParameter parameter)
        {
            string query = "Select date_time,client_phone_number,client_name,message,itenerary,duration,sms_status,email_status,matricule,kilo_dep,kilo_arr from SMS_History order by date_time desc";
            string connectionString = System.IO.File.ReadAllText("Config.txt");

            // var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DB1"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        List<dataMsg> list = new List<dataMsg>();
                        while (reader.Read())
                        {
                            dataMsg dataMsg = new dataMsg();
                            dataMsg.datetime = reader.GetValue(0).ToString();
                            dataMsg.clientphonenumber = reader.GetValue(1).ToString();
                            dataMsg.clientname = reader.GetValue(2).ToString();
                            dataMsg.message = reader.GetValue(3).ToString();
                            dataMsg.itenerary = reader.GetValue(4).ToString();
                            dataMsg.duration = reader.GetValue(5).ToString();
                            dataMsg.smsstatus = reader.GetValue(6).ToString();
                            dataMsg.emailstatus = reader.GetValue(7).ToString();
                            dataMsg.matricule = reader.GetValue(8).ToString();
                            dataMsg.kilodep = reader.GetValue(9).ToString();
                            dataMsg.kiloarr = reader.GetValue(10).ToString();
                            list.Add(dataMsg);
                        }
                        var messages = new
                        {
                            draw = parameter.draw,
                            recordsTotal = list.Count,
                            recordsFiletred = list.Count,
                            data = list
                        };
                        string Json = JsonConvert.SerializeObject(messages);
                        return Json;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public async Task<string> GPS(string matricule)
        {
            try
            {
                var client = new RestClient("http://api.activegps.net:8090/authenticate");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("username", "API");
                request.AddParameter("password", "@ct1v32o22!");
                var response = await client.ExecuteTaskAsync(request);


                GPSToken token = JsonConvert.DeserializeObject<GPSToken>(response.Content);


                //string tokenString = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJBZG1pbmlzdHJhdGV1ciBBZG1pbmlzdHJhdGV1ciIsImlkX2J1Ijo3MjIxNTgyNzEyNjkzODA5NzE4LCJpZF9jbGllbnQiOjIxNzczNjYzOTksImlkX3VzZXIiOjM2MTY0NTM1OTg5NzE4MzU0OTIsImV4cCI6MTY1NDA5OTEyOSwiaWF0IjoxNjU0MDgxMTI5LCJ0eXBlX2J1IjoxfQ.rRbv5YnTX1hoTvOgmz4BeHWDjxwSOdcTkuFIc4Fvm49G_Exydhfg0B-jtRf-SXS66oHFQxFml20bJj3kpHorUQ";

                client = new RestClient("http://api.activegps.net:8090/flotteInfos/registration/" + matricule);

                request = new RestRequest(Method.GET);
                request.AddHeader("authorization", token.jwtoken);
                // Action<string> callback=null;
                // response = client.Execute(request);

                var x = await client.ExecuteTaskAsync(request);
                string  resultGPS = x.Content;
                //resultsGPS = JObject.Parse(x.Content);
                return resultGPS;
            }
            catch (Exception ex)
            {
                //resultsGPS = JObject.Parse("There's a problem with the ActiveGPS API");
                throw;
            }
        }

        public string dataByState()
        {

            string queryString = "select itenerary,Count(id) as bla from SMS_History group by itenerary order by bla desc";
            string connectionString = System.IO.File.ReadAllText("Config.txt");
            Dictionary<string,int> countData = new Dictionary<string,int>(); 
            List<string> itenerary = new List<string>();
            List<int> count = new List<int>();
            // var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DB1"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                itenerary.Add(reader.GetString(0).ToString());
                            }
                            catch (Exception)
                            {
                                itenerary.Add("Error String!");
                            }

                            try
                            {
                                count.Add(reader.GetInt32(1));
                            }
                            catch (Exception)
                            {
                                count.Add(0);
                            }

                            if(!string.IsNullOrEmpty(reader.GetString(0)))

                            countData.Add(reader.GetString(0).ToString(),reader.GetInt32(1));
                        }
                    }
                }
                catch (Exception)
                { throw; }

            }

            string filePath = "data iteneray.csv"; // replace with your file path
            Dictionary<string, string> stateData = new Dictionary<string, string>();

            // read data from CSV file
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(',');
                    stateData[line[0]] = line[1].Trim();
                }
            }

            var resultDict = countData
    .GroupBy(x => stateData[x.Key])
    .ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
            string a = "hello";
            var jsonArray = resultDict.Select(x => new object[] { x.Key, x.Value }).ToArray();

            // serialize array to JSON string
            string jsonString = JsonConvert.SerializeObject(jsonArray);


            return jsonString;
        }
        public string last7()
        {

            string queryString = "select COALESCE(t1.bla, t2.bla ) as bla,sum(t1.x) as succes ,sum(t2.y) as failed from (select cast(date_time as date ) as bla,count(message) as x from SMS_History where CAST(date_time as date) between dateadd(day, -7, GETDATE()) and GETDATE() and sms_status like '%ok%'   group by  cast(date_time as date ) ) t1 full outer join(select cast(date_time as date ) as bla,count(message) as y from SMS_History where CAST(date_time as date) between dateadd(day, -7, GETDATE()) and GETDATE() and sms_status like '%failed%' group by  cast(date_time as date ) ) t2 on (t1.bla = t2.bla)group by t1.bla,t2.bla";
            string connectionString = System.IO.File.ReadAllText("Config.txt");
            List<string> dat = new List<string>();
            List<int> env = new List<int>();
            List<int> ech = new List<int>();
            // var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DB1"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string datetime = (reader.GetDateTime(0) != null) ? reader.GetDateTime(0).ToString("dd/MM/yyyy") : "date";
                            try
                            {
                                dat.Add(reader.GetDateTime(0).ToString("dd/MM/yyyy"));
                            }
                            catch (Exception)
                            {
                                dat.Add("Error DATE!");
                            }

                            try
                            {
                                env.Add(reader.GetInt32(1));
                            }
                            catch (Exception)
                            {
                                env.Add(0);
                            }
                            try
                            {
                                ech.Add(reader.GetInt32(2));
                            }
                            catch (Exception)
                            {
                                ech.Add(0);
                            }

                        }
                    }
                }
                catch (Exception )
                { throw; }

            }
            var last7 = new
            {
                date = dat,
                env = env,
                ech = ech,

            };
            string Json = JsonConvert.SerializeObject(last7);
            return Json;
        }
        public IActionResult Stats()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}