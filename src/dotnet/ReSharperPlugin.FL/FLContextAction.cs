using System;
using System.IO;
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
using ReSharperPlugin.FL.Compilation;
using ReSharperPlugin.FL.Instrumentation;

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

        var code = string.Empty;
        BeTextControl inputTextControl;
        BeTextControl codeTextControl;

        dialogHost.Show(
            getDialog: lt => BeControls.GetDialog(
                    dialogContent: inputTextControl = BeControls.GetTextControl(isReadonly:false, id : "input"),
                    title: "input",
                    id: "input")
                .WithOkButton(lt, () =>
                {
                    var input = inputTextControl.Text.Value;

                    var output = ExecuteCode(code, input);
                    MessageBox.ShowInfo(output);
                })
                .WithCancelButton(lt),
            parentLifetime: Lifetime.Eternal);
        
        dialogHost.Show(
            getDialog: lt => BeControls.GetDialog(
                    dialogContent: codeTextControl = BeControls.GetTextControl(isReadonly:false, id : "code"),
                    title: "code",
                    id: "code")
                .WithOkButton(lt, () =>
                {
                    code = codeTextControl.Text.Value;
                })
                .WithCancelButton(lt),
            parentLifetime: Lifetime.Eternal);

        return _ =>
        {
        };
    }

    private static string ExecuteCode(string code, string input)
    {
        var compiler = new Compiler();
        var emitResult = compiler.Compile(code);

        if (emitResult.Success)
        {
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);
            
            var runSuccessfully = compiler.Run(input);

            if (runSuccessfully)
            {
                var outputConsole = outputWriter.ToString();
                return InstrumentFactory.GetActualOutput(outputConsole);
            }

            return "RunTime error";
        }

        return "Compile error";
    }

    public override string Text => "FL";
    public override bool IsAvailable(IUserDataHolder cache)
    {
        return _provider.SelectedElement is not null;
    }
}