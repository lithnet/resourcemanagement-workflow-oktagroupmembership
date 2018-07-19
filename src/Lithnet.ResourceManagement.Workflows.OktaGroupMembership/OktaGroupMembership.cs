using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Xml.Serialization;
using Microsoft.ResourceManagement.WebServices.WSResourceManagement;

namespace Lithnet.ResourceManagement.Workflows
{
    public partial class OktaGroupMembership : SequenceActivity
    {
        // This is the guid for the FIM Service built-in admin account.
        // We will use it to execute each of our workflow activities.
        private static Guid FimServiceGuid = new Guid("e05d1f1b-3d5e-4014-baa6-94dee7d68c89");

        private List<RequestStatusDetail> StatusItems = new List<RequestStatusDetail>();

        internal const string AllowedFactorTypesPropertyName = "AllowedFactorTypes";
        internal const string OktaIdAttributeNamePropertyName = "OktaIdAttributeName";
        internal const string TenantUrlPropertyName = "TenantUrl";
        internal const string GroupIDPropertyName = "GroupID";
        internal const string MembershipOperationPropertyName = "MembershipOperation";

        public static DependencyProperty TenantUrlProperty = DependencyProperty.Register(OktaGroupMembership.TenantUrlPropertyName, typeof(string), typeof(OktaGroupMembership));

        [Description("The URL of the Okta tenant")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string TenantUrl
        {
            get
            {
                return (string)this.GetValue(OktaGroupMembership.TenantUrlProperty);
            }

            set
            {
                this.SetValue(OktaGroupMembership.TenantUrlProperty, value);
            }
        }

        private string ApiKey { get; set; }

        public static DependencyProperty AllowedFactorTypesProperty = DependencyProperty.Register(OktaGroupMembership.AllowedFactorTypesPropertyName, typeof(string), typeof(OktaGroupMembership));

        [Description("Enter the types of providers that this activity can reset. Leave blank to allow the activity to reset all factor types. Otherwise, use the format {provider}/{type} and separate each entry on a new line. For example, OKTA/push or YUBICO/token:hardware")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string AllowedFactorTypes
        {
            get
            {
                return (string)this.GetValue(OktaGroupMembership.AllowedFactorTypesProperty);
            }

            set
            {
                this.SetValue(OktaGroupMembership.AllowedFactorTypesProperty, value);
            }
        }

        public static DependencyProperty OktaIdAttributeNameProperty = DependencyProperty.Register(OktaGroupMembership.OktaIdAttributeNamePropertyName, typeof(string), typeof(OktaGroupMembership));

        [Description("The name of the attribute that contains the Okta ID of the resource")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string OktaIdAttributeName
        {
            get
            {
                return (string)this.GetValue(OktaGroupMembership.OktaIdAttributeNameProperty);
            }

            set
            {
                this.SetValue(OktaGroupMembership.OktaIdAttributeNameProperty, value);
            }
        }

        public static DependencyProperty OktaGroupIDProperty = DependencyProperty.Register(OktaGroupMembership.GroupIDPropertyName, typeof(string), typeof(OktaGroupMembership));

        [Description("The ID of the Okta group")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string OktaGroupId
        {
            get
            {
                return (string)this.GetValue(OktaGroupMembership.OktaGroupIDProperty);
            }

            set
            {
                this.SetValue(OktaGroupMembership.OktaGroupIDProperty, value);
            }
        }




        public static DependencyProperty MembershipOperationProperty = DependencyProperty.Register(OktaGroupMembership.MembershipOperationPropertyName, typeof(string), typeof(OktaGroupMembership));

        [Description("The type of operation to perform")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string MembershipOperation
        {
            get
            {
                return (string)this.GetValue(OktaGroupMembership.MembershipOperationProperty);
            }

            set
            {
                this.SetValue(OktaGroupMembership.MembershipOperationProperty, value);
            }
        }


        public OktaGroupMembership()
        {
            this.InitializeComponent();
        }

        private void InitializeReadResource_ExecuteCode(object sender, EventArgs e)
        {
            // Set the Actor ID for the Read Activity to the FIM Admin GUID
            this.ReadResource.ActorId = FimServiceGuid;

            // Set the Resource to retrieve as the currently requested object.
            // Note, you could also set this to the target ID of the containing workflow
            this.ReadResource.ResourceId = this.CurrentRequest.CurrentRequest.Target.GetGuid();

            // Set the selection parameters as the date attribute
            this.ReadResource.SelectionAttributes = new string[] { this.OktaIdAttributeName };
        }

        private void ExecuteCode(object sender, EventArgs e)
        {
            try
            {
                // Get the object that was read using the Read Activity Resource into a ResourceType object
                // In a real-world workflow, you would check that anything was returned at all.

                if (this.ReadResource == null)
                {
                    throw new InvalidOperationException("The ReadResource object was null");
                }

                if (this.ReadResource.Resource == null)
                {
                    throw new InvalidOperationException("The Resource provided to the workflow was null");
                }

                ResourceType resource = this.ReadResource.Resource;

                // Get the Okta ID from the resource object

                string oktaID = resource[this.OktaIdAttributeName] as string;

                if (oktaID == null)
                {
                    throw new InvalidOperationException("The user had a null oktaID");
                }

                this.ApiKey = ConfigurationManager.AppSettings["okta-api-key"] ?? ConfigurationManager.AppSettings["okta-factor-reset-api-key"];

                if (this.ApiKey == null)
                {
                    throw new InvalidOperationException("The Okta API key was not present in the app.config file");
                }

                this.PerformOperation(oktaID);
                this.UpdateStatusItems();
            }
            catch (Exception ex)
            {
                this.AddMessage(DetailLevel.Error, string.Format("Unexpected error resetting factors\r\n{0}", ex));
                this.UpdateStatusItems();
                throw;
            }

            if (this.StatusItems.Any(t => t.DetailLevel == DetailLevel.Error))
            {
                throw new ApplicationException(string.Format("One or more errors occurred processing the workflow\r\n{0}", string.Join("\r\n", this.StatusItems.Select(t => t.Message).ToArray())));
            }
        }

        private void PerformOperation(string oktaID)
        {
            try
            {
                Trace.WriteLine(string.Format("Performing member '{0}' on group '{1}' for user {2}", this.MembershipOperation, this.OktaGroupId, oktaID));

                WebClient client = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Authorization", string.Format("SSWS {0}", this.ApiKey));

                Uri uri = new Uri(new Uri(this.TenantUrl), string.Format("/api/v1/groups/{0}/users/{1}", this.OktaGroupId, oktaID));

                string operation = string.Equals(this.MembershipOperation, "delete", StringComparison.InvariantCultureIgnoreCase) ? "DELETE" : "PUT";

                byte[] result = client.UploadValues(uri, operation, new NameValueCollection());

                if (result == null || result.Length == 0)
                {
                    Trace.WriteLine("Membership operation completed");
                }
                else
                {
                    string result2 = Encoding.Default.GetString(result);
                    Trace.WriteLine(string.Format("Membership operation completed with message: {0}", result2));
                }

                this.AddMessage(DetailLevel.Information, string.Format("Completed member '{0}' on group '{1}' for user {2}", this.MembershipOperation, this.OktaGroupId, oktaID));
            }
            catch (Exception ex)
            {
                this.AddMessage(DetailLevel.Error, string.Format("Failed to perform member '{0}' on group '{1}' for user {2}\r\n{3}", this.MembershipOperation, this.OktaGroupId, oktaID, ex));
            }
        }

        private void AddMessage(DetailLevel level, string message)
        {
            Trace.WriteLine(string.Format("{0}: {1}", level, message));
            this.StatusItems.Add(new RequestStatusDetail(level, message));
        }

        private void UpdateStatusItems()
        {
            if (this.StatusItems.Count == 0)
            {
                return;
            }

            List<UpdateRequestParameter> updates = new List<UpdateRequestParameter>();

            foreach (RequestStatusDetail item in this.StatusItems)
            {
                UpdateRequestParameter updateRequestParameter = new UpdateRequestParameter();
                updateRequestParameter.PropertyName = "RequestStatusDetail";
                updateRequestParameter.Value = SerializeObject(item);
                updateRequestParameter.Mode = UpdateMode.Insert;
                updates.Add(updateRequestParameter);
            }

            this.UpdateResource.UpdateParameters = updates.ToArray();
            this.UpdateResource.ActorId = FimServiceGuid;
            this.UpdateResource.ResourceId = this.CurrentRequest.CurrentRequest.ObjectID.GetGuid();
        }

        private static string SerializeObject(object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
    }
}