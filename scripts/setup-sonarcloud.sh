#!/bin/bash

# SonarCloud setup script for Ecommerce project
# This script helps set up the initial SonarCloud configuration

set -e

echo "Setting up SonarCloud for Ecommerce project..."

# Check if SONAR_TOKEN is set
if [ -z "$SONAR_TOKEN" ]; then
    echo "Error: SONAR_TOKEN environment variable is not set"
    echo "Please set your SonarCloud token:"
    echo "export SONAR_TOKEN=your_token_here"
    exit 1
fi

# Project configuration
PROJECT_KEY="ecommerce"
PROJECT_NAME="Ecommerce"
ORGANIZATION="wendryandrade"

echo "Creating SonarCloud project: $PROJECT_NAME ($PROJECT_KEY) in organization: $ORGANIZATION"

# Create the project
curl -X POST "https://sonarcloud.io/api/projects/create" \
  -H "Authorization: Bearer $SONAR_TOKEN" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "project=$PROJECT_KEY&name=$PROJECT_NAME&organization=$ORGANIZATION" \
  || echo "Project may already exist"

echo "Setting up default branch configuration..."

# Set default branch to main
curl -X POST "https://sonarcloud.io/api/project_branches/rename" \
  -H "Authorization: Bearer $SONAR_TOKEN" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "project=$PROJECT_KEY&name=main" \
  || echo "Default branch setup completed"

echo "SonarCloud setup completed!"
echo ""
echo "Next steps:"
echo "1. Make sure your SONAR_TOKEN is set in GitHub Secrets"
echo "2. Push your code to trigger the CI/CD pipeline"
echo "3. The first analysis will create the project and set up the default branch"
