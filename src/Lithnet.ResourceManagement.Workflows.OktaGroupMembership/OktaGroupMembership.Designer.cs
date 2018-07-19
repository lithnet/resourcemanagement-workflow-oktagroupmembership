using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace Lithnet.ResourceManagement.Workflows
{
    public partial class OktaGroupMembership
    {
        #region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCode("", "")]
        private void InitializeComponent()
        {
            this.CanModifyActivities = true;
            this.UpdateResource = new Microsoft.ResourceManagement.Workflow.Activities.UpdateResourceActivity();
            this.ResetFactors = new System.Workflow.Activities.CodeActivity();
            this.ReadResource = new Microsoft.ResourceManagement.Workflow.Activities.ReadResourceActivity();
            this.InitializeReadResource = new System.Workflow.Activities.CodeActivity();
            this.CurrentRequest = new Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity();
            // 
            // UpdateResource
            // 
            this.UpdateResource.ActorId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.UpdateResource.ApplyAuthorizationPolicy = false;
            this.UpdateResource.AuthorizationWaitTimeInSeconds = -1;
            this.UpdateResource.Name = "UpdateResource";
            this.UpdateResource.ResourceId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.UpdateResource.UpdateParameters = null;
            // 
            // ResetFactors
            // 
            this.ResetFactors.Name = "ResetFactors";
            this.ResetFactors.ExecuteCode += new System.EventHandler(this.ExecuteCode);
            // 
            // ReadResource
            // 
            this.ReadResource.ActorId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.ReadResource.Name = "ReadResource";
            this.ReadResource.Resource = null;
            this.ReadResource.ResourceId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.ReadResource.SelectionAttributes = null;
            // 
            // InitializeReadResource
            // 
            this.InitializeReadResource.Name = "InitializeReadResource";
            this.InitializeReadResource.ExecuteCode += new System.EventHandler(this.InitializeReadResource_ExecuteCode);
            // 
            // CurrentRequest
            // 
            this.CurrentRequest.CurrentRequest = null;
            this.CurrentRequest.Name = "CurrentRequest";
            // 
            // OktaFactorReset
            // 
            this.Activities.Add(this.CurrentRequest);
            this.Activities.Add(this.InitializeReadResource);
            this.Activities.Add(this.ReadResource);
            this.Activities.Add(this.ResetFactors);
            this.Activities.Add(this.UpdateResource);
            this.Name = "OktaFactorReset";
            this.CanModifyActivities = false;

        }

        #endregion

        private Microsoft.ResourceManagement.Workflow.Activities.UpdateResourceActivity UpdateResource;
        private CodeActivity InitializeReadResource;
        private Microsoft.ResourceManagement.Workflow.Activities.ReadResourceActivity ReadResource;
        private CodeActivity ResetFactors;
        private Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity CurrentRequest;
    }
}
