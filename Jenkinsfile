pipeline {
    agent any

    stages {
        stage ('Clean workspace') {
            steps {
                cleanWs()
            }
        }
        stage ('Git Checkout') {
            steps {
                git branch: 'main', credentialsId: 'f7e62595-1e4d-4a35-8b56-062b1076e919', url: 'https://github.com/SShreider/TestJenkins2.git'
            }
        }
		stage('Restore packages') {
			steps {
				bat "dotnet nuget locals all --clear"
				bat "nuget restore ${workspace}\\TestJenkins.sln"
			}
		}
        stage('Clean') {
            steps {
                bat "dotnet clean ${workspace}\\TestJenkins.sln /nologo /nr:false /p:configuration=\"release\" /t:clean"
            }
        }
        stage('Build') {
            steps {
                bat "dotnet build ${workspace}\\TestJenkins.sln /nologo /nr:false /p:configuration=\"release\" /t:clean;restore;rebuild"
            }
        }
    }
}

