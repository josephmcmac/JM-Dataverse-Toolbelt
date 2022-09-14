﻿using JosephM.Application.Modules;
using JosephM.CodeGenerator.CSharp;
using JosephM.CodeGenerator.JavaScriptOptions;

namespace JosephM.CodeGenerator.Xrm
{
    [DependantModule(typeof(CSharpModule))]
    [DependantModule(typeof(JavaScriptOptionsModule))]
    public class CodeGeneratorModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }
    }
}