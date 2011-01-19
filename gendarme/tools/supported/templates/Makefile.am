EXTRA_DIST = README README.vsnet \
	AssemblyRule.cs AssemblyRule.vstemplate \
	AssemblyTest.cs AssemblyTest.vstemplate \
	TypeRule.cs TypeRule.vstemplate \
	TypeTest.cs TypeTest.vstemplate \
	MethodRule.cs MethodRule.vstemplate \
	MethodTest.cs MethodTest.vstemplate \
	Templates.RuleProject.csproj Templates.RuleProject.vstemplate \
	Templates.TestProject.csproj Templates.TestProject.vstemplate

GENDARME_ICON = ../../../swf-wizard-runner/Resources/gendarme.ico

vsnet-item-assembly-rule:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp AssemblyRule.cs temp/AssemblyRule.cs
	cp AssemblyRule.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Assembly Rule.zip" temp/*
	rm -rf temp/*

vsnet-item-assembly-test:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp AssemblyTest.cs temp/AssemblyTest.cs
	cp AssemblyTest.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Assembly Rule Test.zip" temp/*
	rm -rf temp/*

vsnet-item-method-rule:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp MethodRule.cs temp/MethodRule.cs
	cp MethodRule.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Method Rule.zip" temp/*
	rm -rf temp/*

vsnet-item-method-test:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp MethodTest.cs temp/MethodTest.cs
	cp MethodTest.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Method Rule Test.zip" temp/*
	rm -rf temp/*

vsnet-item-type-rule:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp TypeRule.cs temp/TypeRule.cs
	cp TypeRule.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Type Rule.zip" temp/*
	rm -rf temp/*

vsnet-item-type-test:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp TypeTest.cs temp/TypeTest.cs
	cp TypeTest.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ItemTemplates\Visual C#\Gendarme Items\Type Rule Test.zip" temp/*
	rm -rf temp/*

vsnet-item-templates: vsnet-item-assembly-rule vsnet-item-assembly-test \
	vsnet-item-method-rule vsnet-item-method-test \
	vsnet-item-type-rule vsnet-item-type-test

vsnet-project-rules:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp Templates.RuleProject.csproj temp/Templates.RuleProject.csproj
	cp Templates.RuleProject.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ProjectTemplates\Visual C#\Gendarme Projects\Rule Assembly.zip" temp/*
	rm -rf temp/*

vsnet-project-tests:
	mkdir -p temp
	cp $(GENDARME_ICON) temp/__TemplateIcon.ico
	cp Templates.TestProject.csproj temp/Templates.TestProject.csproj
	cp Templates.TestProject.vstemplate temp/MyTemplate.vstemplate
	zip -j "Templates\ProjectTemplates\Visual C#\Gendarme Projects\Rule Test Assembly.zip" temp/*
	rm -rf temp/*

vsnet-project-templates: vsnet-project-rules vsnet-project-tests

vsnet: vsnet-item-templates vsnet-project-templates
	zip gendarme-vsnet-templates.zip README.vsnet \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Assembly Rule.zip" \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Assembly Rule Test.zip" \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Method Rule.zip" \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Method Rule Test.zip" \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Type Rule.zip" \
		"Templates\ItemTemplates\Visual C#\Gendarme Items\Type Rule Test.zip" \
		"Templates\ProjectTemplates\Visual C#\Gendarme Projects\Rule Assembly.zip" \
		"Templates\ProjectTemplates\Visual C#\Gendarme Projects\Rule Test Assembly.zip"
	rmdir temp

gendarme-vsnet-templates.zip: vsnet

all: gendarme-vsnet-templates.zip

clean:
	rm -f *.zip
	rm -rf temp

