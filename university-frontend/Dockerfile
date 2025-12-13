# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . ./
RUN npm run build

# Runtime stage
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Copy built files
COPY --from=build /app/dist/university-frontend/browser ./

# Copy nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy env script
COPY env.sh ./

# Make script executable
RUN chmod +x env.sh

# Expose port (Railway uses $PORT, typically 8080)
EXPOSE 8080

# Start nginx with env substitution
# Railway: Run env.sh first, then start nginx
CMD ["/bin/sh", "-c", "sh /usr/share/nginx/html/env.sh && nginx -g 'daemon off;'"]
