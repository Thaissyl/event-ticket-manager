#!/bin/bash
# Generate TypeScript types from OpenAPI spec

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Generating TypeScript types from OpenAPI spec...${NC}"

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
OUTPUT_DIR="$PROJECT_ROOT/src/frontend/src/api/types"
SPEC_FILE="$PROJECT_ROOT/src/backend/EventTickets.API/bin/Debug/net8.0/openapi.json"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Check if API is running, if not start it temporarily to generate spec
if [ ! -f "$SPEC_FILE" ]; then
    echo -e "${BLUE}OpenAPI spec not found. Starting API to generate spec...${NC}"
    cd "$PROJECT_ROOT/src/backend/EventTickets.API"

    # Build and run API in background to generate spec
    dotnet build --no-restore > /dev/null 2>&1

    # Generate OpenAPI spec using dotnet-openapi tool or fetch from running API
    # For now, we'll use Swagger UI endpoint when API is running
    echo -e "${GREEN}API built. Start the API with 'dotnet run' in src/backend/EventTickets.API${NC}"
    echo -e "${GREEN}Then run: curl http://localhost:5000/swagger/v1/swagger.json -o $SPEC_FILE${NC}"
    echo -e "${GREEN}Then run this script again.${NC}"
    exit 0
fi

# Check if openapi-typescript is installed
if ! command -v openapi-typescript &> /dev/null; then
    echo -e "${BLUE}Installing openapi-typescript...${NC}"
    npm install -g openapi-typescript
fi

# Generate TypeScript types
echo -e "${BLUE}Generating TypeScript types...${NC}"
openapi-typescript "$SPEC_FILE" -o "$OUTPUT_DIR/api-schema.ts"

echo -e "${GREEN}TypeScript types generated successfully!${NC}"
echo -e "${GREEN}Output: $OUTPUT_DIR/api-schema.ts${NC}"
