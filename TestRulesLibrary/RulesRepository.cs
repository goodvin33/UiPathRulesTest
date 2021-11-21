using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer;
using UiPath.Studio.Activities.Api.Analyzer.Rules;
using UiPath.Studio.Analyzer.Models;

namespace TestRulesLibrary
{
    public class RulesRepository : IRegisterAnalyzerConfiguration
    {
        public void Initialize(IAnalyzerConfigurationService workflowAnalyzerConfigService)
        {
            if (workflowAnalyzerConfigService.HasFeature("WorkflowAnalyzerV4"))
                return;
            var forbiddenStringRule = new Rule<IActivityModel>("TestRule", "DE-USG-001", InspectTestRule);
            forbiddenStringRule.DefaultErrorLevel = System.Diagnostics.TraceLevel.Error;
            forbiddenStringRule.Parameters.Add("test_parameters", new Parameter()
            {
                DefaultValue = "testDefaultValue",
                Key = "test_parametrs",
                LocalizedDisplayName = "testDisplayName",
                

            });

            workflowAnalyzerConfigService.AddRule<IActivityModel>(forbiddenStringRule);

        }

        private InspectionResult InspectTestRule(IActivityModel activityToInspect, Rule configuredRule)
        {
            var configuredstring = configuredRule.Parameters["test_parameters"].Value;
            if (string.IsNullOrWhiteSpace(configuredstring))
            {
                return new InspectionResult() { HasErrors = false };
            }    

            if(activityToInspect.Variables.Count == 0)
            {
                return new InspectionResult() { HasErrors = false };
            }

            var messageList = new List<InspectionMessage>();

            foreach (var variable in activityToInspect.Variables)
            {
                if(variable.DisplayName.Contains(configuredstring))
                {
                    messageList.Add(new InspectionMessage()
                    {
                        Message = $"Variable {variable.DisplayName} contains an illegal string: {configuredstring}"
                    });
                }    
            }

            if (messageList.Count > 0)
                return new InspectionResult()
                {
                    HasErrors = true,
                    InspectionMessages = messageList,
                    RecommendationMessage = "Fix your test project",
                    ErrorLevel = configuredRule.ErrorLevel
                };
            return new InspectionResult() { HasErrors = false };
        }

    }
}
