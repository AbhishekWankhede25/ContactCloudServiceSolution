using Microsoft.WindowsAzure.Storage.Table;

namespace ContactLibrary
{
    public class ContactTable: TableEntity
    {
        public ContactTable() { }

        public ContactTable(string contactName, string contactNumber)
        {
            PartitionKey = contactName;
            RowKey = contactNumber;
        }

        public string ContactType { get; set; }
        public string Email { get; set; }
    }
}
