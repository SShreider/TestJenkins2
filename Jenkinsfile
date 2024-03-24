import static groovy.io.FileType.FILES
import groovy.transform.Field
import org.apache.commons.io.FilenameUtils

@Field
ArrayList projectsPaths = []

@Field
ArrayList testProjectsDlls = []

@Field
ArrayList testProjectsPublishedDlls = []

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

@NonCPS
def setTestProjectsPublishedDllNames()
{
	for (path in projectsPaths)
	{
		def projContents = new XmlSlurper().parse(path)
		if(projContents.PropertyGroup.IsTestProject.text() == "true")
		{
			def filename = path.name.lastIndexOf('.').with {it != -1 ? path.name[0..<it] : path.name}
			def filePath = path.getParent()
			testProjectsPublishedDlls.add(filePath + '\\bin\\Release\\net8.0\\publish\\' + filename + '.dll')
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

def publishProjects()
{
	for (path in projectsPaths)
	{
		bat 'dotnet publish ' + path + ' /nologo /nr:false /p:configuration=\"release\" /t:clean;restore;rebuild'
	}
}

def runTests()
{	
	for (dllName in testProjectsDlls)
	{
		bat 'dotnet test ' + dllName + ' --logger \"xunit;LogFilePath=' + WORKSPACE + '/TestResults/1.0.0.' + BUILD_NUMBER + '/tests_result.xml\" --configuration release'
		powershell '''
			$file = Get-ChildItem -Path \"$env:WORKSPACE/TestResults/*/tests_result.xml\"
			$destinationFolder = \"$env:WORKSPACE/TestResults\"
			Copy-Item $file -Destination $destinationFolder
		'''
	}
}

def runCoverage()
{	
	for (dllName in testProjectsPublishedDlls)
	{
		bat 'dotnet test ' + dllName + ' --collect \"XPlat Code Coverage\" --results-directory ' + WORKSPACE + '/coverage_tmp'
		powershell '''
			$file = Get-ChildItem -Path \"$env:WORKSPACE/coverage_tmp/*/coverage.cobertura.xml\"
			$destinationFolder = \"$env:WORKSPACE/TestResults\"
			Copy-Item $file -Destination $destinationFolder
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
					setTestProjectsPublishedDllNames()
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
		stage('Publish dlls and run coverage')
		{
			steps
			{
				script
				{
					publishProjects()
					runCoverage()
				}
			}
		}
		stage('Store all binaries, logs and test results')
		{
			steps
			{
				post {
						archiveArtifacts artifacts: '**/bin/Release/net8.0/*.dll', followSymlinks: false
				}
			}
		}
    }
}

