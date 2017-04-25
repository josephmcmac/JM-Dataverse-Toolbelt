Notes

1. The  JosephM.Xrm.SolutionTemplate project which contains a soluton template is added as an Asset
	It should be set to not build. If it is built it appears to conflict with this projects build
	In its project properties VSIX tab it should have the checklboxes cleared as it is only a project template build and not an actual VSIX
	Need to ensure the template projects are located in the projects filesystem folder
	Its .vstemplate item has the property "category" set to XRM
	Its .vstemplate references this projects assembly including the public token and version
	This projects assembly must be added to itself as an asset so the above project wizard works

2. Manual steps to configure the project included
	Added this attribute to the XrmPackage class     [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	Use same signing key or need to regenerate publictoken in .template
