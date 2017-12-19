using Microsoft.EnterpriseManagement.ServiceManager.Application.Common;

namespace Swisscard.MS.Helpers
{
    public class ConfigItemRelatedItemsConfiguration : IRelatedItemsConfiguration
    {
        public ConfigItemRelatedItemsConfiguration ()
        {
            this.ProjectionComponentExpanderSettingsList.Add(new WorkItemProjectionComponentExpanderSettings("ImpactedWorkItems", ApplicationCommonResources.WIAffectingCI));
            this.ProjectionComponentExpanderSettingsList.Add(new WorkItemProjectionComponentExpanderSettings("RelatedWIs", ApplicationCommonResources.WorkItemsText));
            this.ProjectionComponentExpanderSettingsList.Add(new WorkItemProjectionComponentExpanderSettings("RelatedCIs", "RelatedCIsSource", ApplicationCommonResources.ConfigItemsText));
            this.ProjectionComponentExpanderSettingsList.Add(new WorkItemProjectionComponentExpanderSettings("RelatedKnowledgeArticles", ApplicationCommonResources.KnowledgeArticlesText));
        }
    }
}
