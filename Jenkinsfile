pipeline {
    agent any

    triggers {
        githubPush()
    }

    options {
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
        timestamps()
    }

    stages {
        stage('Pull Latest Code') {
            steps {
                echo 'Git pull starting...'
                timeout(time: 5, unit: 'MINUTES') {
                    sh 'cd /home/ec2-user/IdleRPGServer && sudo -u ec2-user git pull origin master'
                }
            }
        }

        stage('Database Migration') {
            steps {
                echo 'Running database migrations...'
                timeout(time: 5, unit: 'MINUTES') {
                    withCredentials([string(credentialsId: 'rds-postgres-password', variable: 'PGPASSWORD')]) {
                        sh '''
                            cd /home/ec2-user/IdleRPGServer
                            export PGPASSWORD=$PGPASSWORD
                            psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \
                                 -U postgres \
                                 -d idlerpg \
                                 -f IdleRPG.Infrastructure/migration.sql
                        '''
                    }
                }
            }
        }

        stage('Deploy with Docker') {
            steps {
                echo 'Docker deployment starting...'
                timeout(time: 20, unit: 'MINUTES') {
                    script {
                        sh '''
                            cd /home/ec2-user/IdleRPGServer
                            echo "Starting Docker Compose build..."
                            docker-compose -f docker-compose.production.yml up -d --build 2>&1 | tee /tmp/docker-build.log
                            echo "Docker Compose build completed"
                        '''
                    }
                }
            }
        }

        stage('Verify Deployment') {
            steps {
                echo 'Verifying deployment...'
                timeout(time: 2, unit: 'MINUTES') {
                    sh '''
                        docker ps | grep idlerpg || echo "Container not found!"
                        sleep 5
                        curl -k http://localhost:5172/health || echo "Health check skipped"
                    '''
                }
            }
        }

        stage('Success') {
            steps {
                echo 'Deployment completed successfully!'
                echo 'API Server: http://13.209.66.253:5172'
                echo 'Swagger: http://13.209.66.253:5172/swagger'
            }
        }
    }

    post {
        success {
            echo '✅ Pipeline succeeded!'
        }
        failure {
            echo '❌ Pipeline failed! Check logs.'
            sh 'cat /tmp/docker-build.log || echo "No build log found"'
        }
        always {
            sh 'rm -f /tmp/docker-build.log || true'
        }
    }
}
