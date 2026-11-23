FROM node:24-alpine AS build

WORKDIR /app

RUN chown node:node /app

USER node

COPY --chown=node:node package*.json ./

RUN npm ci --ignore-scripts --no-audit --no-fund

COPY --chown=node:node . .

RUN npm run build

FROM nginx/nginx-unprivileged:stable-alpine AS production

COPY --chown=101:101 nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build --chown=101:101 /app/dist /usr/share/nginx/html

USER 101:101

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
