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

# Configure for non-root user
RUN chmod +x env.sh && \
    chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

# Switch to non-root user
USER nginx

# Expose port (Railway uses $PORT, typically 8080)
EXPOSE 8080
EXPOSE 8081

# Start nginx with env substitution
CMD ["/bin/sh", "-c", "sh /usr/share/nginx/html/env.sh && nginx -g 'daemon off;'"]
