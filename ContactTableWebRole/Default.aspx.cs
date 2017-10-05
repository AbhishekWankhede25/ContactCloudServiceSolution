using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ContactLibrary;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ContactTableWebRole
{
    public partial class _Default : Page
    {
        CloudStorageAccount _cloudStorageAccount;
        CloudTableClient _cloudTableClient;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                UpdateContactButton.Visible = false;
                _cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ContactConnectionString"));
                _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();

                CloudTable contactTable = _cloudTableClient.GetTableReference("Contacts");
                contactTable.CreateIfNotExists();
                LogInfo("Got the CloudConnection and the ContactTable");
                ErrorLabel.Text = "Error><br/>";
                ListContacts();
            }
            catch (Exception ex)
            {
                LogError("PageLoad Exception:> " + ex.Message);
            }
        }

        protected void AddNewContactButton_Click(object sender, EventArgs e)
        {
            try
            {
                CloudTable table = _cloudTableClient.GetTableReference("Contacts");
                ContactTable contactTable = new ContactTable(ContactNameTextBox.Text, ContactNumberTextBox.Text)
                {
                    Email = EmailTextBox.Text,
                    ContactType = ContactTypeTextBox.Text
                };
                TableOperation insertOpoerations = TableOperation.Insert(contactTable);
                table.Execute(insertOpoerations);
                LogInfo(string.Format($"Contact: '{ContactNameTextBox.Text}' with the ContactNumber: '{ContactNumberTextBox.Text}', created successfully"));
                ListContacts();
            }
            catch (Exception ex)
            {
                LogError("Add NewContact Exception:> " + ex.Message);
            }
        }

        protected void UpdateContactButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] strArr = ViewState["OldContact"].ToString().Split(new string[] { "|" }, StringSplitOptions.None);
                CloudTable table = _cloudTableClient.GetTableReference("Contacts");
                TableOperation getContactOperations = TableOperation.Retrieve<ContactTable>(strArr[0], strArr[1]);
                TableResult result = table.Execute(getContactOperations);
                ContactTable updateContact = result.Result as ContactTable;
                if (updateContact != null)
                {
                    updateContact.PartitionKey = ContactNameTextBox.Text;
                    updateContact.RowKey = ContactNumberTextBox.Text;
                    updateContact.Email = EmailTextBox.Text;
                    updateContact.ContactType = ContactTypeTextBox.Text;
                    TableOperation updateOperations = TableOperation.Replace(updateContact);
                    table.Execute(updateOperations);
                }
                UpdateContactButton.Visible = false;
                AddNewContactButton.Visible = true;
                ViewState["OldContact"] = null;
                TextClear();
                LogInfo(string.Format($"Contact: '{strArr[0]}' updated successfully"));
                ListContacts();
            }
            catch (Exception ex)
            {
                LogError("UpdateContact Exception:> " + ex.Message);
            }
        }

        protected void CancelNewContactButton_Click(object sender, EventArgs e)
        {

        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            CloudTable table = _cloudTableClient.GetTableReference("Contacts");
            TableQuery<ContactTable> tableQuery = null;
            string strMessage = "";
            try
            {
                if (!string.IsNullOrEmpty(SearchContactNameTextBox.Text) && !string.IsNullOrEmpty(SearchContactNumberTextBox.Text))
                {
                    tableQuery = new TableQuery<ContactTable>().Where(
                        TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SearchContactNameTextBox.Text), TableOperators.And, TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, SearchContactNumberTextBox.Text)
                        ));
                    strMessage = string.Format("Search for ContactName '{0}' and ContactNumber '{1}' completed", SearchContactNameTextBox.Text, SearchContactNumberTextBox.Text);
                }
                else if (!string.IsNullOrEmpty(SearchContactNameTextBox.Text) && string.IsNullOrEmpty(SearchContactNumberTextBox.Text))
                {
                    tableQuery = new TableQuery<ContactTable>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SearchContactNameTextBox.Text));

                    strMessage = string.Format("Search for ContactName '{0}' completed", SearchContactNameTextBox.Text);
                }
                else if (string.IsNullOrEmpty(SearchContactNameTextBox.Text) && !string.IsNullOrEmpty(SearchContactNumberTextBox.Text))
                {
                    tableQuery = new TableQuery<ContactTable>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, SearchContactNumberTextBox.Text));
                    strMessage = string.Format("Search for ContactNumber '{0}' completed", SearchContactNumberTextBox.Text);
                }
                else if (string.IsNullOrEmpty(SearchContactNameTextBox.Text) && string.IsNullOrEmpty(SearchContactNumberTextBox.Text))
                {
                    tableQuery = new TableQuery<ContactTable>();
                    strMessage = "Got all the contacts";
                }
                ContactRepeater.DataSource = table.ExecuteQuery(tableQuery).ToList();
                ContactRepeater.DataBind();
                LogInfo(strMessage);
            }
            catch (Exception ex)
            {
                LogError("SearchContact Exception:> " + ex.Message);
            }
        }

        protected void EditContactButton_Click(object sender, EventArgs e)
        {
            UpdateContactButton.Visible = true;
            AddNewContactButton.Visible = false;
            try
            {
                string[] strArr = ((Button)sender).CommandArgument.Split(new string[] { "|" }, StringSplitOptions.None);
                CloudTable table = _cloudTableClient.GetTableReference("Contacts");
                TableQuery<ContactTable> query = new TableQuery<ContactTable>().Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, strArr[0]), TableOperators.And, TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, strArr[1])));
                ContactTable oldContact = table.ExecuteQuery(query).FirstOrDefault();
                ContactNameTextBox.Text = oldContact.PartitionKey;
                ContactNumberTextBox.Text = oldContact.RowKey;
                EmailTextBox.Text = oldContact.Email;
                ContactTypeTextBox.Text = oldContact.ContactType;
                ViewState["OldContact"] = string.Format($"{oldContact.PartitionKey}|{oldContact.RowKey}|{oldContact.ContactType}|{oldContact.Email}");
                LogInfo(string.Format($"Contact: '{oldContact.PartitionKey}' with the ContactNumber: '{oldContact.RowKey}' ready for editing"));
            }
            catch (Exception ex)
            {
                LogError("EditContact Exception:> " + ex.Message);
            }
        }

        protected void DeleteContactButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] strArr = ((Button)sender).CommandArgument.Split(new string[] { "|" }, StringSplitOptions.None);
                CloudTable table = _cloudTableClient.GetTableReference("Contacts");
                TableOperation getContactOperation = TableOperation.Retrieve<ContactTable>(strArr[0], strArr[1]);
                TableResult result = table.Execute(getContactOperation);
                ContactTable delContact = result.Result as ContactTable;
                if (delContact != null)
                {
                    TableOperation deleteOperations = TableOperation.Delete(delContact);
                    table.Execute(deleteOperations);
                }
                ListContacts();
                LogInfo(string.Format($"Contact: '{delContact.PartitionKey}' with the ContactNumber '{delContact.RowKey}' deleted successfully"));
            }
            catch (Exception ex)
            {
                LogError("DeleteContact Exception:> " + ex.Message);
            }
        }

        protected string GetKey(object dataItem)
        {
            ContactTable contact = dataItem as ContactTable;
            return string.Format("{0}|{1}", contact.PartitionKey, contact.RowKey);
        }

        private void ListContacts()
        {
            try
            {
                TableQuery<ContactTable> query = new TableQuery<ContactTable>();
                CloudTable table = _cloudTableClient.GetTableReference("Contacts");
                ContactRepeater.DataSource = table.ExecuteQuery(query);
                ContactRepeater.DataBind();
                LogInfo("ListContacts, Got All the Contacts from the Table");
            }
            catch (Exception ex)
            {
                LogError("ListContacts Exception:> " + ex.Message);
            }

        }
        private void TextClear()
        {
            ContactNameTextBox.Text = "";
            ContactNumberTextBox.Text = "";
            ContactTypeTextBox.Text = "";
            EmailTextBox.Text = "";
        }

        private void LogInfo(string message)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("{0}, From Machine: {1} and Url: {2}", message, Server.MachineName, Request.UserHostAddress));
            LogInformationListBox.Items.Add(string.Format("{0}, From Machine: {1} and Url: {2}", message, Server.MachineName, Request.UserHostAddress));
        }

        private void LogError(string message)
        {
            System.Diagnostics.Trace.TraceError(string.Format("{0}, From Machine: {1} and Url: {2}", message, Server.MachineName, Request.UserHostAddress));
            ErrorLabel.Text += "Error> " + (string.Format("{0}, From Machine: {1} and Url: {2}", message, Server.MachineName, Request.UserHostAddress)) + "<br/>";
        }

    }
}