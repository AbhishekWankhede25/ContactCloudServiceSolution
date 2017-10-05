<%@ Page Title="Contact Book Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ContactTableWebRole._Default" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

        <h1>Azure Table Storage Contact Table...</h1>

    <div class="row">
        <div class="col-sm-12 col-md-12 col-lg-12" style="border: solid Black 2px">
            <asp:Panel ID="LogInfoPanel" runat="server">
                <p style="text-decoration: underline overline">Log Information...</p>
                <div>
                    <asp:ListBox ID="LogInformationListBox" runat="server" height="90px" style="min-width:100%"></asp:ListBox>
                </div>
                <div>
                    <asp:Label ID="ErrorLabel" runat="server" /></div>
            </asp:Panel>
        </div>
    </div>
    <br />
    <div class="row">
        <div class="col-md-5">
            <asp:Panel runat="server" ID="AddContactPanel" Height="100%" BorderStyle="Solid" BorderColor="Brown">
                <p style="text-decoration: Underline Overline">New Contact...</p>
                <div>
                    <label class="labelSize">ContactName:</label>
                    <asp:TextBox runat="server" ID="ContactNameTextBox" />
                </div>
                <div>
                    <label class="labelSize">ContactNumber:</label>
                    <asp:TextBox runat="server" ID="ContactNumberTextBox" />
                </div>
                <div>
                    <label class="labelSize">ContactType:</label>
                    <asp:TextBox runat="server" ID="ContactTypeTextBox" />
                </div>
                <div>
                    <label class="labelSize">Email:</label>
                    <asp:TextBox runat="server" ID="EmailTextBox" />
                </div>
                <div>
                    <asp:Button CssClass="btn btn-primary btn-sm" ID="AddNewContactButton" Text="Add New Contact" runat="server" OnClick="AddNewContactButton_Click" />&nbsp;
                    <asp:Button CssClass="btn btn-primary btn-sm" ID="UpdateContactButton" Text="Update Contact" runat="server" OnClick="UpdateContactButton_Click" />&nbsp;
                    <asp:Button CssClass="btn btn-primary btn-sm" ID="CancelNewContactButton" Text="Cancel Add New Contact" runat="server" OnClick="CancelNewContactButton_Click" />&nbsp;
                </div>
            </asp:Panel>
        </div>

        <div class="col-md-5">
            <asp:Panel ID="ContactSearchPanel" runat="server" Height="100%" BorderStyle="Solid" BorderColor="DarkBlue">
                <p style="text-decoration: underline overline">Enter the Contact Name and Contact Number or anyone of the two........</p>
                <div>
                    <label class="labelSize">ContactName:</label>
                    <asp:TextBox runat="server" ID="SearchContactNameTextBox" />
                </div>
                <div>
                    <label class="labelSize">ContactNumber:</label>
                    <asp:TextBox runat="server" ID="SearchContactNumberTextBox" />
                </div>
                <div>
                    <asp:Button CssClass="btn btn-primary btn-sm" ID="SearchButton" Text="Search Contact" runat="server" OnClick="SearchButton_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>
    <br />
    <div class="row">
         <div class="col-md-7">
            <asp:Panel runat="server" ID="ContactDetailsPanel">
                <p style="text-decoration: underline overline">Contact details........</p>
                <asp:Repeater ID="ContactRepeater" runat="server" EnableViewState="false">
                    <HeaderTemplate>
                        <table border="1" style="border-color: ThreeDDarkShadow; padding: 5px;">
                            <thead>
                                <th>Contact Name</th>
                                <th>Contact Number</th>
                                <th>Contact Type</th>
                                <th>Email</th>
                                <th>Operation</th>
                            </thead>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <%#Eval("PartitionKey")%>
                            </td>
                            <td>
                                <%#Eval("RowKey") %>
                            </td>
                            <td>
                                <%#Eval("ContactType") %>
                            </td>
                            <td>
                                <%#Eval("Email") %>
                            </td>
                            <td>
                                <asp:Button ID="EditContactButton" runat="server" CommandArgument="<%#GetKey(Container.DataItem) %>" OnClick="EditContactButton_Click" Text="Edit" />
                                <asp:Button ID="DeleteContactButton" runat="server" CommandArgument="<%#GetKey(Container.DataItem) %>" OnClick="DeleteContactButton_Click" Text="Delete" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
