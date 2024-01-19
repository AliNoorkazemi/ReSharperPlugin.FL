using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL;

[ContextAction(
    Group = CSharpContextActions.GroupID,
    Name = nameof(FLContextAction),
    Description = nameof(FLContextAction),
    Priority = -10)]
public class FLContextAction : ContextActionBase
{
    private readonly ICSharpContextActionDataProvider _provider;

    public FLContextAction(ICSharpContextActionDataProvider provider)
    {
        _provider = provider;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
        ICSharpTypeDeclaration typeDeclaration = _provider.GetSelectedElement<ICSharpTypeDeclaration>();
        if (typeDeclaration is null)
            return null;
        
        var dialogHost = solution.GetComponent<IDialogHost>();

        BeTextControl testCaseTextControl;

        return textControl =>
        {
            var codeContent = textControl.Document.GetText();
            
            dialogHost.Show(
                getDialog: lt => BeControls.GetDialog(
                        dialogContent: testCaseTextControl = BeControls.GetTextControl(isReadonly:false, id : "testCase"),
                        title: "Test Case",
                        id: "testCase")
                    .WithOkButton(lt, () =>
                    {
                        var input = testCaseTextControl.Text.Value;

                        var testCases = ExtractTestCases(input).ToList();

                        var faultLocalizationRunner = new FaultLocalizationRunner(testCases, codeContent);

                        var faultLocalizationReport = faultLocalizationRunner.GetFaultLocalizationReport();
                    
                        MessageBox.ShowInfo(faultLocalizationReport);
                    })
                    .WithCancelButton(lt),
                parentLifetime: Lifetime.Eternal);
        };
    }

    private static IEnumerable<TestCase> ExtractTestCases(string input)
    {
        var testCases = input.Split(new[] { "\n#$#\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var testCase in testCases)
        {
            var outputIndex = testCase.IndexOf("output :", StringComparison.InvariantCultureIgnoreCase);
            
            var inputPart = testCase.Substring(7, outputIndex - 7).Trim();
            var outputPart = testCase[(outputIndex + 8)..].Trim();

            yield return new TestCase
            {
                Input = inputPart,
                Output = outputPart
            };
        }
    }

    public override string Text => "FL";
    public override bool IsAvailable(IUserDataHolder cache)
    {
        return _provider.SelectedElement is not null;
    }
}