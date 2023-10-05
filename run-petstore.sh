docker run -d -e SWAGGER_HOST=http://appetizer.openziti.io \
  -e SWAGGER_URL=http://localhost \
  -e SWAGGER_BASE_PATH=/v2 -p 20080:8080 swaggerapi/petstore
