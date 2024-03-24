import static groovy.io.FileType.FILES
import groovy.transform.Field
import org.apache.commons.io.FilenameUtils

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
		if(projContents.PropertyGroup.IsTestProject.text() == "true")
		{
			def filename = path.name.lastIndexOf('.').with {it != -1 ? path.name[0..<it] : path.name}
			def filePath = path.getParent()
			testProjectsDlls.add(filePath + '\\bin\\Release\\net8.0\\' + filename + '.dll')
		}
	}
}

def restoreProjects()
{	
	for (path in projectsPaths)
	{
		bat 'nuget restore ' + path 
	}
}

def cleanProjects()
{
	for (path in projectsPaths)
	{
		bat 'dotnet clean ' + path + ' /nologo /nr:false /p:configuration=\"release\" /t:clean'
	}
}

def buildProjects()
{
	for (path in projectsPaths)
	{
		bat 'dotnet build ' + path + ' /nologo /nr:false /p:configuration=\"release\" /t:clean;restore;rebuild'
	}
}

def runTests()
{	
	testProjectsDlls.eachWithIndex { dllName, index ->
		bat 'dotnet test ' + dllName + ' --logger \"xunit;LogFilePath=' + WORKSPACE + '/TestResults/1.0.0.' + BUILD_NUMBER + '/'\" --configuration release --collect \"XPlat Code Coverage\"'
		powershell '''
			$file = Get-ChildItem -Path \"$env:WORKSPACE/TestResults/*/results.xml\"
			$file | Rename-Item -NewName testcoverage.coverage
		'''
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
				bat 'dotnet nuget locals all --clear'
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
		stage('Archive binaries')
		{
			steps
			{
				archiveArtifacts artifacts: '**/bin/Release/net8.0/*.dll', followSymlinks: false
			}
		}
		stage('Run unit tests')
		{
			steps
			{
				script
				{
					runTests()
				}
			}
		}
    }
}

