using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PollingService
{
    public partial class Service1 : ServiceBase
    {
        public static Service1 ServiceInstance = new Service1();

        public Service1()
        {
            InitializeComponent();
        }

        private static void Main(string[] args)
        {
            if (args.Select(x => x.ToLower()).Contains("-cmdline"))
            {
                Console.WriteLine("Starting service...");
                var service = Service1.ServiceInstance;
                //Console.WriteLine("service properties service.AutoLog:{0},service.CanHandlePowerEvent:{1},service.CanHandleSessionChangeEvent:{2},service.CanPauseAndContinue:{3},service.CanRaiseEvents:{4},service.CanShutdown:{5},service.CanStop:{6}", service.AutoLog, service.CanHandlePowerEvent, service.CanHandleSessionChangeEvent, service.CanPauseAndContinue, service.CanRaiseEvents, service.CanShutdown, service.CanStop);
                service.OnStart(args);
                Console.WriteLine("Service started, press Enter to continue.");
                Console.ReadLine();
                Console.WriteLine("Stopping service...");
                service.OnStop();
                Console.WriteLine("Service stopped");
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendEventNotification);
            aTimer.Interval = 60000;
            aTimer.Enabled = true;
        }

        protected override void OnStop()
        {
        }


        private void SendEventNotification(object sender, EventArgs e)
        {
            try
            {
                List<ItemChange> events = itemChangeLookUp();
                triggerEventPerSubscription(events);
            }
            catch (Exception ex)
            {
                EventLog.Log = "Application";
                EventLog.Source = ServiceName;
                EventLog.WriteEntry("PollingService" + ex.Message, EventLogEntryType.Error);
            }
        }

        private void triggerEventPerSubscription(List<ItemChange> events)
        {
            foreach (ItemChange itemChangeEvent in events)
            {
                SendNotification(itemChangeEvent, itemChangeEvent.DeliveryAddress);
                string message =
                    string.Format(
                        "PollingService.TriggerEventPerSubscription: Notification sent for item {0} of eventType 
                {
                    1
                }
                ", itemChangeEvent.CustomerId, itemChangeEvent.EventType);
                EventLog.Log = "Application";
                EventLog.Source = ServiceName;
                EventLog.WriteEntry(message);
            }
        }

        private List<ItemChange> itemChangeLookUp()
        {
            EventLog.Log = "Application";
            EventLog.Source = ServiceName;
            EventLog.WriteEntry("Polling for Item Change");
            List<ItemChange> itemChangeList = new List<ItemChange>();
            string connectionString = "Data Source=.;Initial Catalog=Northwind;Integrated Security=true";

            // Provide the query string with a parameter placeholder.
            string queryString = "Proc_RetrieveEventRecords";

            // Specify the parameter value.
            int paramValue = -50;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TimeSince", paramValue);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ItemChange item = new ItemChange(reader["CustomerID"].ToString(),
                            Int32.Parse(reader["EventType"].ToString()),
                            reader[14].ToString(), reader["DeliveryUrl"].ToString(), reader["CompanyName"].ToString(),
                            reader["ContactName"].ToString(), reader["ContactTitle"].ToString(),
                            reader["Address"].ToString(),
                            reader["City"].ToString(), reader["Region"].ToString(), reader["Country"].ToString(),
                            reader["PostalCode"].ToString(),
                            reader["Phone"].ToString(), reader["Fax"].ToString());
                        itemChangeList.Add(item);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    EventLog.Log = "Application";
                    EventLog.Source = ServiceName;
                    EventLog.WriteEntry("PollingService : ItemChangeLookup " + ex.Message, EventLogEntryType.Error);
                }
            }
            string message = string.Format("{0} items changes", itemChangeList.Count);
            EventLog.Log = "Application";
            EventLog.Source = ServiceName;
            EventLog.WriteEntry(message);

            return itemChangeList;
        }
    }


    class ItemChange
    {
        public ItemChange(string customerID, int eventType, string s, string deliveryUr
            l, string s1, string toString2, string s2, string toString3, string s3, string toString4, string s4, 
                string toString5, string s5, string toString6)
        {
            //TODO:
        }
    }

}
