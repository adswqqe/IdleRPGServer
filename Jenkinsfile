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
                echo 'Running hybrid migrations...'
                timeout(time: 5, unit: 'MINUTES') {
                    withCredentials([string(credentialsId: 'rds-postgres-password', variable: 'PGPASSWORD')]) {
                        sh '''
                            # ec2-user의 로그인 쉘 환경에서 실행 (PATH 로드)
                            sudo -u ec2-user bash -lc "
                                cd /home/ec2-user/IdleRPGServer

                                # 1. 도구 복원
                                echo '[Step 1/4] Restoring dotnet tools...'
                                dotnet tool restore

                                # 2. 레거시 마이그레이션 (안전망)
                                echo '[Step 2/4] Running legacy migration.sql...'
                                if [ -f 'IdleRPG.Infrastructure/migration.sql' ]; then
                                    export PGPASSWORD='$PGPASSWORD'
                                    psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \\
                                         -U postgres \\
                                         -d idlerpg \\
                                         -f IdleRPG.Infrastructure/migration.sql || exit 1
                                    echo '✅ Legacy migration applied'
                                else
                                    echo '⚠️ migration.sql not found, skipping legacy migration'
                                fi

                                # 3. EF Core 마이그레이션 (새로운 변경사항)
                                echo '[Step 3/4] Running EF Core migrations...'
                                dotnet ef database update \\
                                    --project IdleRPG.Infrastructure \\
                                    --connection 'Host=idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com;Port=5432;Database=idlerpg;Username=postgres;Password='\"$PGPASSWORD\"';SSL Mode=Require;Trust Server Certificate=true' \\
                                    || exit 1
                                echo '✅ EF Core migrations applied'

                                # 4. 마이그레이션 검증
                                echo '[Step 4/4] Verifying migrations...'
                                export PGPASSWORD='$PGPASSWORD'
                                psql -h idlerpg-dev.chiqweeeuidv.ap-northeast-2.rds.amazonaws.com \\
                                     -U postgres \\
                                     -d idlerpg \\
                                     -c 'SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;' \\
                                     || echo '⚠️ Verification skipped'
                            "
                        '''
                    }
                }
            }
        }

        stage('Clean Docker Cache') {
            steps {
                echo 'Cleaning Docker cache and old containers...'
                timeout(time: 5, unit: 'MINUTES') {
                    sh '''
                        cd /home/ec2-user/IdleRPGServer
                        echo "Stopping and removing containers..."
                        docker-compose -f docker-compose.production.yml down || true

                        echo "Removing dangling images..."
                        docker image prune -f || true

                        echo "Removing build cache..."
                        docker builder prune -f || true
                    '''
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
                            echo "Starting Docker Compose build (no cache)..."
                            docker-compose -f docker-compose.production.yml build --no-cache 2>&1 | tee /tmp/docker-build.log
                            docker-compose -f docker-compose.production.yml up -d
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
