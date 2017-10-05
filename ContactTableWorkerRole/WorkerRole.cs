using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

using ContactLibrary;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;

namespace ContactTableWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        CloudStorageAccount _storageAccount;
        CloudTableClient _tableClient;

        public override void Run()
        {
            Trace.TraceInformation("ContactTableWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ContactConnectionString"));
            _tableClient = _storageAccount.CreateCloudTableClient();
            _tableClient.GetTableReference("Contacts").CreateIfNotExists();

            Trace.TraceInformation("From ContactTableWorkerRole OnStart(), Got the ContactConnectionString, Created the Contact Table if not exists");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("ContactTableWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("ContactTableWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            try
            {
                CloudQueueClient queueClient = _storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference("addcontactqueue");
                queue.CreateIfNotExists();
                Trace.TraceInformation("From ContactWorkerRoel RunAsync(), Got the Queue Connection and Created the 'addcontactqueue' if not exists");
                while (!cancellationToken.IsCancellationRequested)
                {
                    Trace.TraceInformation("Working, from the RunAsync() inside the while");
                    CloudQueueMessage message = queue.GetMessage();
                    if (queue.Exists())
                    {
                        if (message != null)
                        {
                            ContactTable contactTable = JsonConvert.DeserializeObject<ContactTable>(message.AsString);
                            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
                            CloudTable table = tableClient.GetTableReference("Contacts");
                            TableOperation insertOperation = TableOperation.Insert(contactTable);
                            table.Execute(insertOperation);
                            Trace.TraceInformation(string.Format($"Contact: {contactTable.PartitionKey} with the ContactNumber: {contactTable.RowKey} created successfully"));
                            queue.DeleteMessage(message);
                            Trace.TraceInformation(string.Format($"Deleted the Contact: {contactTable.PartitionKey} with the ContactNumber: {contactTable.RowKey} from the Queue"));
                        }
                    }
                    await Task.Delay(20000);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
