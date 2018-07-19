using System.Web.UI.WebControls;
using System.Workflow.ComponentModel;
using Microsoft.IdentityManagement.WebUI.Controls;
using Microsoft.ResourceManagement.Workflow.Activities;

namespace Lithnet.ResourceManagement.Workflows
{
    public class OktaGroupMembershipUI : ActivitySettingsPart
    {
        private const string OktaGroupID = "oktaGroupID";
        private const string TenantUrlID = "controlTenantUrl";
        private const string OktaIdAttributeNameID = "controlOktaIdAttributeName";
        private const string MembershipOperationID = "controlMembershipOperation";

        /// <inheritdoc />
        /// <summary>
        ///  Creates a Table that contains the controls used by the activity UI
        ///  in the Workflow Designer of the FIM portal. Adds that Table to the
        ///  collection of Controls that defines each activity that can be selected
        ///  in the Workflow Designer of the FIM Portal. Calls the base class of 
        ///  ActivitySettingsPart to render the controls in the UI.
        /// </summary>
        protected override void CreateChildControls()
        {
            Table controlLayoutTable = new Table
            {
                Width = Unit.Percentage(100.0),
                BorderWidth = 0,
                CellPadding = 2
            };

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Tenant URL", OktaGroupMembershipUI.TenantUrlID, 400, 0, false, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Specify the full URL of the Okta tenant (Ensure the URL does not contain -admin after the tenant name)"));
            controlLayoutTable.Rows.Add(new TableRow());

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("User Okta ID attribute name", OktaGroupMembershipUI.OktaIdAttributeNameID, 400, 0, false, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Specify the system name of the attribute that contains the user's Okta ID"));
            controlLayoutTable.Rows.Add(new TableRow());

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Okta group ID", OktaGroupMembershipUI.OktaGroupID, 400, 0, true, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Enter the Okta ID of the group to add or remove the member to"));
            controlLayoutTable.Rows.Add(new TableRow());

            controlLayoutTable.Rows.Add(this.AddTableRowDropDown("Membership operation", OktaGroupMembershipUI.MembershipOperationID, 400, "Add"));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Enter the Okta ID of the group to add or remove the member to"));
            controlLayoutTable.Rows.Add(new TableRow());

            this.Controls.Add(controlLayoutTable);
            base.CreateChildControls();
        }

        private TableRow AddTableRowTextBox(string labelText, string controlID, int width, int maxLength, bool multiLine, bool password, string defaultValue)
        {
            TableCell cell;
            TableRow row = new TableRow();

            Label label = new Label();
            label.Text = labelText;
            label.CssClass = this.LabelCssClass;

            cell = new TableCell();
            cell.Controls.Add(label);
            row.Cells.Add(cell);

            TextBox text = new TextBox();
            text.ID = controlID;
            text.CssClass = this.TextBoxCssClass;
            text.Text = defaultValue;

            if (maxLength > 0)
            {
                text.MaxLength = maxLength;
            }

            text.Width = width;

            if (multiLine)
            {
                text.TextMode = TextBoxMode.MultiLine;
                text.Rows = 6;
                text.Wrap = true;
            }

            if (password)
            {
                text.TextMode = TextBoxMode.Password;
            }

            cell = new TableCell();
            cell.Controls.Add(text);
            row.Cells.Add(cell);
            return row;
        }

        private TableRow AddTableRowDropDown(string labelText, string controlID, int width, string defaultValue)
        {
            TableCell cell;
            TableRow row = new TableRow();

            Label label = new Label();
            label.Text = labelText;
            label.CssClass = this.LabelCssClass;

            cell = new TableCell();
            cell.Controls.Add(label);
            row.Cells.Add(cell);

            DropDownList ddl = new DropDownList();
            ddl.ID = controlID;
            ddl.CssClass = this.TextBoxCssClass;
            ddl.Text = defaultValue;
            ddl.Items.Add("Add");
            ddl.Items.Add("Remove");
            
            ddl.Width = width;

            cell = new TableCell();
            cell.Controls.Add(ddl);
            row.Cells.Add(cell);
            return row;
        }

        private TableRow AddTableRowDescription(string labelText)
        {
            TableCell cell;
            TableRow row = new TableRow();

            Label label = new Label();
            label.Text = labelText;
            label.CssClass = this.LabelCssClass;

            cell = new TableCell();
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Controls.Add(label);
            row.Cells.Add(cell);

            return row;
        }

        private string GetText(string textBoxID)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            return textBox.Text ?? string.Empty;
        }

        private string GetDropDownValue(string id)
        {
            DropDownList item = (DropDownList)this.FindControl(id);
            return item.Text ?? string.Empty;
        }

        private void SetText(string textBoxID, string text)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            if (textBox != null)
            {
                textBox.Text = text;
            }
        }

        private void SetDropDownValue(string id, string text)
        {
            DropDownList control = (DropDownList)this.FindControl(id);
            if (control != null)
            {
                control.Text = text;
            }
        }

        private void SetControlReadOnlyOption(string textBoxID, bool readOnly)
        {
            TextBox control = this.FindControl(textBoxID) as TextBox;

            if (control != null)
            {
                control.ReadOnly = readOnly;
                return;
            }

            DropDownList dropDownList = this.FindControl(textBoxID) as DropDownList;

            if (dropDownList != null)
            {
                dropDownList.Enabled = !readOnly;
                return;
            }
        }

        /// <summary>
        /// Called when a user clicks the Save button in the Workflow Designer. 
        /// Returns an instance of the RequestLoggingActivity class that 
        /// has its properties set to the values entered into the text box controls
        /// used in the UI of the activity. 
        /// </summary>
        public override Activity GenerateActivityOnWorkflow(SequentialWorkflow workflow)
        {
            if (!this.ValidateInputs())
            {
                return null;
            }

            return new OktaGroupMembership
            {
                TenantUrl = this.GetText(OktaGroupMembershipUI.TenantUrlID),
                AllowedFactorTypes = this.GetText(OktaGroupMembershipUI.OktaGroupID),
                OktaIdAttributeName = this.GetText(OktaGroupMembershipUI.OktaIdAttributeNameID),
                MembershipOperation = this.GetDropDownValue(OktaGroupMembershipUI.MembershipOperationID) ?? "Add",
            };
        }

        /// <summary>
        /// Called by FIM when the UI for the activity must be reloaded.
        /// It passes us an instance of our workflow activity so that we can
        /// extract the values of the properties to display in the UI.
        /// </summary>
        public override void LoadActivitySettings(Activity activity)
        {
            var resetActivity = activity as OktaGroupMembership;

            if (resetActivity != null)
            {
                this.SetText(OktaGroupMembershipUI.TenantUrlID, resetActivity.TenantUrl);
                this.SetText(OktaGroupMembershipUI.OktaGroupID, resetActivity.AllowedFactorTypes);
                this.SetText(OktaGroupMembershipUI.OktaIdAttributeNameID, resetActivity.OktaIdAttributeName);
                this.SetDropDownValue(OktaGroupMembershipUI.MembershipOperationID, resetActivity.MembershipOperation);
            }
        }

        /// <summary>
        /// Saves the activity settings.
        /// </summary>
        public override ActivitySettingsPartData PersistSettings()
        {
            ActivitySettingsPartData data = new ActivitySettingsPartData();
            data[OktaGroupMembership.AllowedFactorTypesPropertyName] = this.GetText(OktaGroupMembershipUI.OktaGroupID);
            data[OktaGroupMembership.TenantUrlPropertyName] = this.GetText(OktaGroupMembershipUI.TenantUrlID);
            data[OktaGroupMembership.OktaIdAttributeNamePropertyName] = this.GetText(OktaGroupMembershipUI.OktaIdAttributeNameID);
            data[OktaGroupMembership.MembershipOperationPropertyName] = this.GetDropDownValue(OktaGroupMembershipUI.MembershipOperationID);
            return data;
        }

        /// <summary>
        ///  Restores the activity settings in the UI
        /// </summary>
        public override void RestoreSettings(ActivitySettingsPartData data)
        {
            if (data == null)
            {
                return;
            }

            this.SetText(OktaGroupMembershipUI.OktaGroupID, (string)data[OktaGroupMembership.AllowedFactorTypesPropertyName]);
            this.SetText(OktaGroupMembershipUI.TenantUrlID, (string)data[OktaGroupMembership.TenantUrlPropertyName]);
            this.SetText(OktaGroupMembershipUI.OktaIdAttributeNameID, (string)data[OktaGroupMembership.OktaIdAttributeNamePropertyName]);
            this.SetDropDownValue(OktaGroupMembershipUI.MembershipOperationID, (string)data[OktaGroupMembership.MembershipOperationPropertyName]);
        }

        /// <summary>
        ///  Switches the activity between read only and read/write mode
        /// </summary>
        public override void SwitchMode(ActivitySettingsPartMode mode)
        {
            bool readOnly = mode == ActivitySettingsPartMode.View;
            this.SetControlReadOnlyOption(OktaGroupMembershipUI.OktaGroupID, readOnly);
            this.SetControlReadOnlyOption(OktaGroupMembershipUI.TenantUrlID, readOnly);
            this.SetControlReadOnlyOption(OktaGroupMembershipUI.OktaIdAttributeNameID, readOnly);
            this.SetControlReadOnlyOption(OktaGroupMembershipUI.MembershipOperationID, readOnly);
        }

        public override string Title
        {
            get
            {
                return "Okta group membership activity";
            }
        }

        /// <summary>
        ///  In general, this method should be used to validate information entered
        ///  by the user when the activity is added to a workflow in the Workflow
        ///  Designer.
        ///  We could add code to verify that the log file path already exists on
        ///  the server that is hosting the FIM Portal and check that the activity
        ///  has permission to write to that location. However, the code
        ///  would only check if the log file path exists when the
        ///  activity is added to a workflow in the workflow designer. This class
        ///  will not be used when the activity is actually run.
        ///  For this activity we will just return true.
        /// </summary>
        public override bool ValidateInputs()
        {
            return true;
        }
    }
}
