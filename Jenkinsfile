import static groovy.io.FileType.FILES
import groovy.transform.Field

@Field
ArrayList projectsPaths = []

@Field
ArrayList testProjectsDlls = []

@NonCPS
def setProjectsPaths()
{	
	def filterProjFiles = ~/.*\.csproj$/
	new File(WORKSPACE).traverse(type: groovy.io.FileType.FILES, nameFilter: filterProjFiles) { it ->
		projectsPaths.add(it)
	}
}

@NonCPS
def setTestProjectsDllNames()
{
	for (path in projectsPaths)
	{
		def projContents = new XmlSlurper().parse(path)
		if(projContents.Project.PropertyGroup.IsTestProject.isEmpty())
		{
			def dllName = path.lastIndexOf('.').with {it != -1 ? path.name[0..<it] : path.name}
			testProjectsDlls.add(dllName)
		}
	}
	println ">>> HERE"
	println testProjectsDlls
}

def restoreProjects()
{	
	for (path in projectsPaths)
	{
		bat "nuget restore " + path 
	}
}

def cleanProjects()
{
	for (path in projectsPaths)
	{
		bat "dotnet clean " + path + " /nologo /nr:false /p:configuration=\"release\" /t:clean"
	}
}

def buildProjects()
{
	for (path in projectsPaths)
	{
		bat "dotnet build " + path + " /nologo /nr:false /p:configuration=\"release\" /t:clean;restore;rebuild"
	}
}

pipeline 
{
    agent any

    stages {
        stage ('Clean workspace') 
		{
            steps 
			{
                cleanWs()
            }
        }
        stage ('Git Checkout') 
		{
            steps 
			{
                git branch: 'main', credentialsId: 'f7e62595-1e4d-4a35-8b56-062b1076e919', url: 'https://github.com/SShreider/TestJenkins2.git'
            }
        }
		stage ('Get projects to build') 
		{
			steps 
			{
				script
				{
					setProjectsPaths()
					setTestProjectsDllNames()
				}
			}
		}
		stage('Restore packages') 
		{
			steps 
			{
				bat "dotnet nuget locals all --clear"
				script
				{
					restoreProjects()
				}
			}
		}
        stage('Clean') 
		{
            steps 
			{
                script
				{
					cleanProjects()
				}
            }
        }
        stage('Build') 
		{
            steps 
			{
                script
				{
					buildProjects()
				}
            }
        }
    }
}

