import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 50 }, // Ramp up to 50 users
    { duration: '1m', target: 50 },  // Stay at 50 users for 1m
    { duration: '30s', target: 0 },  // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
  },
};

export default function () {
  // Simulate API latency check
  let res = http.get('http://localhost:5000/api/health'); // Assume health endpoint
  check(res, {
    'status was 200': (r) => r.status == 200,
    'transaction time OK': (r) => r.timings.duration < 200,
  });

  // Simulate Streaming start
  // let streamRes = http.get('http://localhost:5000/api/videos/stream/123');
  // check(streamRes, { 'stream started successfully': (r) => r.status === 200 || r.status === 206 });

  sleep(1);
}
