using System;
using JetBrains.Application.Progress;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Extensions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model.UIAutomation;
using JetBrains.TextControl;
using JetBrains.Util;

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

        BeTextBox textBox;

        dialogHost.Show(
            getDialog: lt => BeControls.GetDialog(
                    dialogContent: textBox = BeControls.GetTextBox(lt),
                    title: "title",
                    id: "Fl-id")
                .WithOkButton(lt, () => MessageBox.ShowInfo($"Okay {textBox.GetText()}"))
                .WithCancelButton(lt),
            parentLifetime: Lifetime.Eternal);

        return textControl =>
        {
            int caretOffset = _provider.CaretOffset;
            string generatedGuid = Guid.NewGuid().ToString().ToUpper();
            string textToInsert = $"\"{generatedGuid}\";";
        
            using (WriteLockCookie.Create())
            {
                textControl.Document.InsertText(caretOffset, textToInsert);
                textControl.Caret.MoveTo(caretOffset + textToInsert.Length, CaretVisualPlacement.DontScrollIfVisible);
            }
        };
    }

    public override string Text => "FL";
    public override bool IsAvailable(IUserDataHolder cache)
    {
        return _provider.SelectedElement is not null;
    }
}