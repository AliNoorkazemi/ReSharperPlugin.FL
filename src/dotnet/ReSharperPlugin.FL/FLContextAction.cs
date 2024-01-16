using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
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
    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
        return textControl =>
        {
            Console.WriteLine("hello");
        };
    }

    public override string Text => "FL";
    public override bool IsAvailable(IUserDataHolder cache)
    {
        return true;
    }
}