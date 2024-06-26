import groovy.transform.Field
 
@Field
final String GitRepo = 'https://github.com/SShreider/TestJenkins2'

@Field
final String GitRepoCredentials = 'f7e62595-1e4d-4a35-8b56-062b1076e919'
 
@Field
ArrayList projectsPaths = []

@Field
ArrayList testProjectsDlls = []
 
def setProjectsPaths()
{	
    def files = findFiles(glob: '**/*.csproj')

    for (fileName in files)
	{
        projectsPaths.add(fileName)
	}
}
 
def setTestProjectsDllNames()
{
	for (path in projectsPaths)
	{
        def projectName = path.name
		if(projectName.indexOf('.Test.') > -1)
		{
			def fileName = projectName.lastIndexOf('.').with {it != -1 ? projectName[0..<it] : projectName}
			def filePath = path.path
            filePath = filePath.lastIndexOf('\\').with {it != -1 ? filePath[0..<it]: filePath}
			testProjectsDlls.add(filePath + '\\bin\\Release\\net8.0\\' + fileName + '.dll')
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
                git branch: 'main', credentialsId: GitRepoCredentials, url: GitRepo
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
