import static groovy.io.FileType.FILES
import groovy.transform.Field
import org.apache.commons.io.FilenameUtils
 
@Field
final String GitRepo = 'https://github.com/SShreider/TestJenkins2'

@Field
final String GitRepoCredentials = 'f7e62595-1e4d-4a35-8b56-062b1076e919'
 
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
	for (dllName in testProjectsDlls)
	{
		bat 'dotnet test ' + dllName + ' --logger \"trx;LogFileName=' + WORKSPACE + '/TestResults/1.0.0.' + BUILD_NUMBER + '/tests_result.trx\" --configuration release'
		powershell '''
			$file = Get-ChildItem -Path \"$env:WORKSPACE/TestResults/*/tests_result.trx\"
			$destinationFolder = \"$env:WORKSPACE/TestResults\"
			Copy-Item $file -Destination $destinationFolder
		'''
	}
}
 
def runCoverage()
{	
	for (dllName in testProjectsDlls)
	{
		bat 'dotnet test ' + dllName + ' --framework net8.0 --no-build --verbosity normal --collect:"XPlat Code Coverage" --logger trx --results-directory coverage_tmp'
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
                git branch: 'master', credentialsId: GitRepoCredentials, url: GitRepo
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
		stage('Archive binaries')
		{
			steps
			{
				archiveArtifacts artifacts: '**/bin/Release/net8.0/*.dll', followSymlinks: false
				archiveArtifacts artifacts: '**/bin/Release/net8.0/*.exe', followSymlinks: false
			}
		}
		stage('Run coverage')
		{
			steps
			{
				script
				{
					runCoverage()
				}
			}
			post 
			{
				always 
				{
					script {
						recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'TestResults/coverage.cobertura.xml']], id: 'cobertura', name: 'Cobertura Coverage', qualityGates: [[metric: 'LINE', threshold: 50.0]])
					}
				}
			}
		}
    }
	post {
        always {
			xunit (tools: [ MSTest(pattern: 'TestResults/tests_result.trx') ], skipPublishingChecks: false)
        }
    }
}
