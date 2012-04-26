﻿using System;
using System.Collections.Generic;
using System.Linq;
using AgentMulder.ReSharper.Domain.Registrations;
using AgentMulder.ReSharper.Domain.Search;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Services.CSharp.StructuralSearch;
using JetBrains.ReSharper.Psi.Services.CSharp.StructuralSearch.Placeholders;
using JetBrains.ReSharper.Psi.Services.StructuralSearch;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentMulder.Containers.CastleWindsor.Patterns
{
    internal sealed class WindsorContainerRegisterPattern : RegistrationBase
    {
        private readonly RegistrationBase[] argumentsPatterns;

        private static readonly IStructuralSearchPattern pattern =
            new CSharpStructuralSearchPattern("$container$.Register($arguments$)",
                                              new ExpressionPlaceholder("container", "Castle.Windsor.IWindsorContainer",
                                                                        false),
                                              new ArgumentPlaceholder("arguments", -1, -1)); // any number of arguments

        public WindsorContainerRegisterPattern(params RegistrationBase[] argumentsPatterns)
            : base(pattern)
        {
            this.argumentsPatterns = argumentsPatterns;
        }

        public override IEnumerable<IComponentRegistration> GetComponentRegistrations(ITreeNode parentElement)
        {
            IStructuralMatcher matcher = CreateMatcher();
            IStructuralMatchResult match = matcher.Match(parentElement);

            IEnumerable<IInvocationExpression> invocationExpressions =
                match.GetMatchedElementList("arguments").Cast<ICSharpArgument>()
                    .Select(argument => argument.Value as IInvocationExpression);

            foreach (RegistrationBase argumentPattern in argumentsPatterns)
            {
                foreach (var expression in invocationExpressions)
                {
                    foreach (var registration in argumentPattern.GetComponentRegistrations(expression))
                    {
                        yield return registration;
                    }
                }
            }
        }
    }
}