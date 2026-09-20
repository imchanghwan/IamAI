// PM2 placeholder for future deployment. Not used yet; the server is not implemented.
module.exports = {
  apps: [{ name: 'iamai-server', script: 'server/dist/index.js', env: { NODE_ENV: 'production' } }],
};
