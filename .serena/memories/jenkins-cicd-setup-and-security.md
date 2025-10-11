# Jenkins CI/CD Setup on EC2 - Production-Ready Security Considerations

## User Context
- **Current Environment**: Learning/Development
- **Important**: User is studying with **production environment in mind**
- **Security Priority**: User wants to implement proper security practices even in dev environment

## EC2 Environment
- **Instance Type**: t3.micro (1GB RAM, 1 vCPU)
- **Public IP**: 13.209.66.253 (changes on reboot - need Elastic IP for prod)
- **OS**: Amazon Linux 2023
- **Services Running**: Jenkins (8080), Docker, PostgreSQL, Redis

## Jenkins Setup Summary

### Successfully Completed
1. **Jenkins Installation**: Installed on EC2, running on port 8080
2. **tmpfs Issue Resolution**: Redirected temp directory to /var/lib/jenkins/tmp
3. **Git SSH Authentication**: Set up SSH keys for GitHub access
4. **Permission Configuration**: jenkins user in ec2-user group with limited sudo

### Current Configuration Files

#### `/etc/systemd/system/jenkins.service.d/override.conf`
```ini
[Service]
Environment="JAVA_OPTS=-Djava.io.tmpdir=/var/lib/jenkins/tmp"
```

#### `/etc/sudoers.d/jenkins` (via visudo)
```
jenkins ALL=(ec2-user) NOPASSWD: /usr/bin/git
```

#### SSH Keys
- **Location**: `/home/ec2-user/.ssh/id_ed25519`
- **Type**: ED25519 (modern, secure)
- **Passphrase**: None (required for automation)
- **GitHub**: Public key registered

#### Jenkins Pipeline Script
```groovy
pipeline {
    agent any
    stages {
        stage('Pull Latest Code') {
            steps {
                echo 'Git pull starting...'
                sh 'cd /home/ec2-user/IdleRPGServer && sudo -u ec2-user git pull origin master'
            }
        }
        stage('Deploy with Docker') {
            steps {
                echo 'Docker deployment starting...'
                sh 'cd /home/ec2-user/IdleRPGServer && docker-compose -f docker-compose.prod.yml up -d --build'
            }
        }
        stage('Success') {
            steps {
                echo 'Deployment completed successfully!'
                echo 'API Server: https://13.209.66.253:7122'
            }
        }
    }
    post {
        success { echo 'Pipeline succeeded!' }
        failure { echo 'Pipeline failed! Check logs.' }
    }
}
```

## Security Analysis

### ⚠️ HIGH RISK - Requires Production Hardening

#### 1. Docker Group Membership (CRITICAL)
```bash
sudo usermod -a -G docker jenkins
```
**Risk**: docker group = near-root privileges
**Attack Vector**: 
```bash
docker run -v /:/host -it alpine chroot /host
# → Full root access to EC2 filesystem
```

**Production Solutions**:
- **Option A**: Use Docker rootless mode
- **Option B**: Use dedicated CI/CD tools (GitHub Actions, GitLab CI)
- **Option C**: Implement Docker socket proxy with restricted permissions
- **Current**: Acceptable for dev, NOT for production

#### 2. Public Jenkins Exposure
- **Current**: http://13.209.66.253:8080 accessible from internet
- **Risk**: Jenkins can execute arbitrary code, target for attacks

**Production Solutions**:
```bash
# Security Group: Restrict to specific IPs
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxx \
  --protocol tcp \
  --port 8080 \
  --cidr YOUR_IP/32

# Or: Use VPN/Bastion host
# Or: Use reverse proxy with HTTPS + authentication
```

#### 3. No HTTPS on Jenkins
- **Current**: HTTP only (port 8080)
- **Risk**: Credentials/tokens sent in plaintext

**Production Solutions**:
- Use nginx reverse proxy with Let's Encrypt SSL
- Or use Jenkins built-in HTTPS with proper certificates

### ✅ LOW RISK - Acceptable with Caveats

#### 1. Limited Sudo for Git
```
jenkins ALL=(ec2-user) NOPASSWD: /usr/bin/git
```
**Status**: Relatively safe, only git command allowed
**Improvement**: Could restrict to specific git operations/directories

#### 2. SSH Key without Passphrase
**Status**: Required for automation, acceptable if key is protected
**Mitigation**: Ensure proper file permissions (600)

### ✅ GOOD Security Practices

1. **SSH over HTTPS**: Using SSH keys for GitHub (better than HTTPS tokens)
2. **File Permissions**: 775 on project directory, limited access
3. **Git Safe Directory**: Properly configured
4. **Service Isolation**: Jenkins runs as dedicated user

## Production Migration Checklist

When moving to production:

- [ ] **Elastic IP**: Assign static IP to EC2
- [ ] **Security Groups**: Restrict Jenkins port to VPN/office IP only
- [ ] **HTTPS**: Set up SSL/TLS for Jenkins (nginx reverse proxy recommended)
- [ ] **Authentication**: Enable Jenkins security, strong passwords, consider 2FA
- [ ] **Docker Security**: 
  - [ ] Remove jenkins from docker group
  - [ ] Implement Docker rootless OR
  - [ ] Use Docker socket proxy (tecnativa/docker-socket-proxy)
- [ ] **Secrets Management**: Use AWS Secrets Manager or HashiCorp Vault
- [ ] **Monitoring**: CloudWatch alarms for failed logins, unusual activity
- [ ] **Backups**: Regular Jenkins home directory backups
- [ ] **Updates**: Regular security updates for Jenkins, plugins, OS

## Docker Rootless Alternative

For production, consider Docker rootless mode:

```bash
# Install rootless Docker for jenkins user
curl -fsSL https://get.docker.com/rootless | sh

# Update Jenkins to use rootless Docker
# No docker group membership needed
```

## Recommended Architecture for Production

```
Internet → CloudFront/ALB (HTTPS) → EC2 Security Group (restricted) 
  → Nginx Reverse Proxy (SSL termination, auth) 
  → Jenkins (localhost:8080 only)
  → Docker Rootless OR Docker Socket Proxy
  → Application Containers
```

## Current Status
- Jenkins running and accessible
- Git pull working via SSH
- **Next**: Need to add jenkins to docker group OR implement rootless Docker
- **Decision Point**: Choose security model before proceeding

## Notes
- User wants to learn production-grade practices even in dev environment
- Balance between learning convenience and security best practices
- Document all security trade-offs for educational purposes
